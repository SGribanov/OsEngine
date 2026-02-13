# Отчет по изменениям Codex

Дата фиксации: 2026-02-13

## Измененные файлы

1. `project/OsEngine/Journal/Internal/PositionController.cs`
- Добавлена потокобезопасная активация фонового воркера через `lock`.
- Повторный запуск воркера блокируется, если задача уже активна.
- `WatcherHome` переведен с `async void` на `async Task`.
- `Activate()` вызывается только вне режима `StartProgram.IsOsOptimizer`.

2. `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`
- Расчет максимального количества тестов разделен на оценочный (`estimatedMaxTests`) и динамический прогресс.
- В `StartOptimizeFazeInSample` добавлен параметр `inSampleBotsCount`, максимум прогресса увеличивается по фактическому старту.
- В `StartOptimizeFazeOutOfSample` добавлено увеличение максимума по числу отчетов `in-sample`.
- Прогноз оставшегося времени пересчитан на основе `_countAllServersEndTest` (а не количества накопленных замеров).
- Обновление ETA теперь начинается после накопления замеров не меньше числа потоков.

3. `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
- Защита от преждевременного завершения оптимизации: событие окончания игнорируется, пока прогресс-бар(ы) не достигли максимума.
- Добавлена проверка активного состояния прогресса (prime + worker statuses).
- При обнаружении незавершенного прогресса `_testIsEnd` сбрасывается в `false`.

4. `project/OsEngine/Robots/TechSamples/CustomChartInParamWindowSample.cs`
- UI-инициализация вкладки/графика отключена в режиме `StartProgram.IsOsOptimizer`.
- Добавлены `null`-проверки для `_host`, `_chart` и `MainWindow.GetDispatcher`.
- Поток отрисовки запускается только когда UI доступен.

5. `project/OsEngine/Robots/TechSamples/CustomTableInTheParamWindowSample.cs`
- Добавлена защита от `null` для `_tab`, `_tableDataGrid`, `TabsScreener`, `MainWindow.GetDispatcher`.
- UI-таблица параметров создается только вне `StartProgram.IsOsOptimizer`.
- Логирование ошибок переведено на `SendNewLogMessage(...)` без зависимости от `_tab`.
- Логика обновления движения и входа в позицию переведена на данные `Lines`, уменьшена зависимость от UI-таблицы.
- При отсутствии таблицы добавление/сохранение строк выполняется в модель (`Lines`) без падения.

## Сводка diff
- Изменено файлов: 5
- Добавлено строк: 202
- Удалено строк: 61
