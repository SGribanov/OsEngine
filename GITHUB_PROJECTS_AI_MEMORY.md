# GitHub Projects AI Memory Setup

Дата проверки: 2026-02-27  
Репозиторий: `SGribanov/OsEngine`  
Проект: `Tasks` (`https://github.com/users/SGribanov/projects/2`)

## 1. Предусловия

Установлен GitHub CLI:

```powershell
gh --version
```

Важно: если в окружении задан `GITHUB_TOKEN`, `gh` будет использовать его вместо сохраненных credential-ов.

Очистка переменной:

```powershell
# Текущая сессия
Remove-Item Env:GITHUB_TOKEN -ErrorAction SilentlyContinue

# Постоянные переменные
[Environment]::SetEnvironmentVariable("GITHUB_TOKEN", $null, "User")
[Environment]::SetEnvironmentVariable("GITHUB_TOKEN", $null, "Machine")
```

## 2. Авторизация и scope `project`

```powershell
gh auth login
gh auth refresh --hostname github.com -s project
gh auth status
```

Ожидаемо в `gh auth status`:
- активный аккаунт `SGribanov`
- тип credential: `keyring`
- scopes включают `project`, `repo`, `read:org`

## 3. Проверка/создание проекта `Tasks`

Проверка:

```powershell
gh project list --owner SGribanov
```

Создание (если нет):

```powershell
gh project create --owner SGribanov --title "Tasks"
```

## 4. Проверка обязательных полей

```powershell
gh project field-list 2 --owner SGribanov --format json
```

Текущее состояние проекта `Tasks`:
- поле `Status` с опциями `Todo`, `In Progress`, `Done`
- системные поля (`Title`, `Assignees`, `Labels`, `Repository` и др.) присутствуют

## 5. Создание issue и добавление в проект

```powershell
$issueUrl = gh issue create --repo SGribanov/OsEngine --title "Название задачи" --body "Описание"
gh project item-add 2 --owner SGribanov --url $issueUrl
```

Проверка:

```powershell
gh project item-list 2 --owner SGribanov
```

## 6. Воспроизводимый быстрый сценарий (новая машина)

```powershell
Remove-Item Env:GITHUB_TOKEN -ErrorAction SilentlyContinue
gh auth login
gh auth refresh --hostname github.com -s project
gh project list --owner SGribanov
```

Дальше:

```powershell
$issueUrl = gh issue create --repo SGribanov/OsEngine --title "Test issue" --body "Smoke test"
gh project item-add 2 --owner SGribanov --url $issueUrl
gh project item-list 2 --owner SGribanov
```

## 7. Авто-добавление issues в проект `Tasks`

В репозитории добавлен workflow:

`/.github/workflows/add-issues-to-project.yml`

Он срабатывает на `issues.opened` и `issues.reopened` и добавляет issue в:

`https://github.com/users/SGribanov/projects/2`

### Что нужно настроить один раз в GitHub

1. Создать классический PAT (или fine-grained token с эквивалентными правами), у которого есть доступ к:
- `project`
- `repo`

2. Добавить секрет в репозиторий:
- Name: `ADD_TO_PROJECT_PAT`
- Value: PAT из шага 1

Путь в UI:
- `Settings` -> `Secrets and variables` -> `Actions` -> `New repository secret`

### Проверка

1. Создать новую issue в `SGribanov/OsEngine`
2. Открыть проект `Tasks`
3. Убедиться, что карточка появилась автоматически в `Todo`

Примечание: в workflow включен fallback на `github.token`, если `ADD_TO_PROJECT_PAT` не задан.

## 8. Рабочий цикл задач в `Tasks`

### Статусы и смысл

- `Todo`: задача согласована и готова к старту, но работа еще не начата
- `In Progress`: по задаче ведется активная разработка (есть ветка/PR или активные изменения)
- `Done`: задача завершена и проверена, issue закрыта

### Правила переходов

- `Todo -> In Progress`
- Когда исполнитель начал работу
- Или автоматически при открытии PR (для связанной issue)

- `In Progress -> Done`
- При merge PR, закрывающем issue
- Или при ручном закрытии issue, если работа завершена без PR

- `Done -> In Progress`
- Если issue переоткрыли и нужно доработать

### Ответственность

- Инициатор задачи: корректная постановка issue (цель, шаги, критерии)
- Исполнитель: своевременный перевод в `In Progress`, обновление прогресса
- Ревьюер/мейнтейнер: подтверждение завершения и перевод в `Done`

### Минимальный операционный ритм

- Раз в день проверять `Todo` и брать следующую задачу в работу
- После merge/close в тот же день синхронизировать статус в проекте
- Не оставлять закрытые issue в `Todo`/`In Progress`

## 9. Шаблоны новых задач (Issue Templates)

Добавлены шаблоны:

- `/.github/ISSUE_TEMPLATE/feature_task.yml`
- `/.github/ISSUE_TEMPLATE/bug_report.yml`
- `/.github/ISSUE_TEMPLATE/config.yml`

Что это дает:

- единый формат постановки задач
- обязательные поля: контекст, критерии готовности, шаги проверки
- меньше неполных/непроверяемых задач

## 10. Минимальная приоритизация задач

Для задач используются labels:

- `priority:critical` — блокирующая задача, берем немедленно
- `priority:high` — берем в ближайшую итерацию
- `priority:medium` — плановая работа
- `priority:low` — можно отложить

Правила применения:

- у каждой рабочей issue должен быть ровно один `priority:*` label
- если приоритет не задан, по умолчанию считать `priority:medium`
- изменение приоритета фиксировать комментарием в issue
