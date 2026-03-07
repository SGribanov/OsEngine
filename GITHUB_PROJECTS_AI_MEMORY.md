# GitHub Projects AI Memory (Portable)

Дата обновления: 2026-03-07

Этот документ сделан переносимым между аккаунтами Codex/Claude Code.
Он не содержит жестких привязок к конкретному owner, project number или локальному пути пользователя.

## 1. Политика досок

- Одна доска GitHub Project v2 на один репозиторий.
- Имя доски по умолчанию = имя репозитория.
- Допускаются override-переменные репозитория:
  - `PROJECT_BOARD_NUMBER` (приоритет 1);
  - `PROJECT_BOARD_TITLE` (приоритет 2).
- Если override не задан, используется доска с названием текущего репозитория.

## 2. Быстрый bootstrap (owner/repo/board autodetect)

```powershell
$repoFull = gh repo view --json nameWithOwner --jq .nameWithOwner
$owner, $repo = $repoFull -split '/'

$boardNumber = if ($env:PROJECT_BOARD_NUMBER) { [int]$env:PROJECT_BOARD_NUMBER } else { $null }
$boardTitle = if ($env:PROJECT_BOARD_TITLE) { $env:PROJECT_BOARD_TITLE } else { $repo }

gh project list --owner $owner --format json
```

Если доска не найдена:

```powershell
gh project create --owner $owner --title $boardTitle
gh project list --owner $owner --format json
```

Проверка полей:

```powershell
# Подставьте номер доски
gh project field-list <project-number> --owner $owner --format json
```

Обязательные статусы поля `Status`:
- `Todo`
- `In Progress`
- `Done`

## 3. Добавление issue в доску

```powershell
$issueUrl = gh issue create --repo $repoFull --title "Task title" --body "Task body"
gh project item-add <project-number> --owner $owner --url $issueUrl
```

Проверка:

```powershell
gh project item-list <project-number> --owner $owner --format json
```

## 4. Единый источник статуса

- Единственный источник прогресса: Project field `Status`.
- `Issue open/closed` = факт завершения, а не отдельная прогресс-шкала.
- Labels используются как метаданные (приоритет/тип), не как параллельный workflow-state.
- `AI-CONTEXT` не должен дублировать статус `Todo/In Progress/Done`.

## 5. AI-CONTEXT (handoff/resume)

Использовать один блок контекста на issue:

```markdown
<!-- AI-CONTEXT:START -->
## Context
**Done:** ...
**Next:** ...
**Resume:** ...
<!-- AI-CONTEXT:END -->
```

- На паузе/завершении обновлять существующий блок, не создавать дубликаты.
- На старте сессии читать этот блок в первую очередь.

## 6. GitHub auth (portable)

Предпочтительно:

```powershell
gh auth status
```

Fallback, если keyring недоступен:

```powershell
$tokenPath = Join-Path $HOME 'gh_pat.txt'
if (Test-Path $tokenPath) { $env:GH_TOKEN = (Get-Content $tokenPath -Raw).Trim() }
```

## 7. Учет главного реестра `C:\Repos`

- Главный репозиторий `C:\Repos` хранит индекс child-репозиториев в `REPOSITORIES.md`.
- Перед началом работ полезно проверить, что текущий child-репозиторий корректно отражен в реестре:

```powershell
rg -n "\| <LocalFolderName> \|" C:\Repos\REPOSITORIES.md
```

- Если появился новый top-level repo в `C:\Repos`, обновить реестр в parent-репозитории:

```powershell
cd C:\Repos
git registry-refresh
```

Обновление реестра коммитится отдельным commit в parent-репозитории.

## 8. Репозиторий `OsEngine` (текущая фактическая привязка)

- Для `SGribanov/OsEngine` сейчас используется доска:
  - `OsEngine` — `https://github.com/users/SGribanov/projects/6`
- Эта привязка является текущим состоянием, но automation построен так, чтобы не зависеть от hardcoded `#6`.
