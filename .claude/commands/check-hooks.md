Проверить работу session hooks (сохранение и восстановление state/context/dialog):

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/self-check.ps1
```

Оставить временные артефакты проверки для ручного просмотра:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/self-check.ps1 -KeepArtifacts
```
