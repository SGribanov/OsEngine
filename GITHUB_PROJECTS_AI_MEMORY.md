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
