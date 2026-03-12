# Tester Smoke Seed

Этот набор файлов подготавливает минимальное состояние `Engine\*` для сценария `tester-start-smoke.sample.json`.

Что входит:
- `TestServer.txt` с рабочим локальным источником истории для Tester;
- `SettingsTesterKeeper.txt` для автоматической загрузки бота `FractalBreakthrough`;
- файлы `123*`, которые восстанавливают настройки бота и его вкладки.

Что не входит:
- сами исторические данные свечей;
- `SecurityTestSettings.txt`, потому что он лежит рядом с локальным каталогом истории, а не в `Engine\`.

Требование:
- локальный путь истории из `TestServer.txt` должен существовать на машине;
- если путь отличается, обновите `TestServer.txt` в этом каталоге и путь в `restoreFiles` у `project/OsEngine.TesterAutomation/Scenarios/tester-start-smoke.sample.json`.
