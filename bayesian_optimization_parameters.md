# Bayesian Optimization Parameters (OsEngine)

Этот документ описывает параметры, которые пользователь задает для режима `OptimizationMethod = Bayesian`.

## Где задаются
- UI: `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
- Настройки/дефолты: `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
- Runtime-валидация: `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`
- Алгоритм: `project/OsEngine/OsOptimizer/OptEntity/BayesianOptimizationStrategy.cs`

## Параметры

| Параметр | Что означает | Допустимый диапазон | Значение по умолчанию |
|---|---|---|---|
| `BayesianInitialSamples` | Сколько комбинаций проверить на стартовом (первичном) шаге перед итеративным выбором кандидатов | `>= 1` | `20` |
| `BayesianMaxIterations` | Бюджет последующих итераций после стартовой выборки | `>= 1` | `100` |
| `BayesianBatchSize` | Сколько кандидатов брать за одну итерацию | `>= 1` | `5` |
| `BayesianAcquisitionMode` | Режим функции выбора следующего кандидата (`Ucb`, `ExpectedImprovement`, `Greedy`) | Только значения enum | `Ucb` |
| `BayesianAcquisitionKappa` | Коэффициент exploration/exploitation для `Ucb`/`ExpectedImprovement` | `>= 0` | `0.25` |
| `BayesianUseTailPass` | Включить дополнительный «хвостовой» проход на конце (exploitation-tail) | `true/false` | `true` |
| `BayesianTailSharePercent` | Доля tail-pass от `MaxIterations` | `1..50` | `20` |

## Как именно работает в коде
- Плановый бюджет проверок: `InitialSamples + MaxIterations` (и не больше числа доступных комбинаций).
- `BatchSize` ограничивает размер одной итерации.
- `TailPass` работает только если:
  - `BayesianUseTailPass = true`
  - режим не `Greedy`
  - `MaxIterations >= 4`
- Для `ExpectedImprovement` хвостовая доля немного уменьшается: фактически используется `TailSharePercent - 5` (но не меньше 1).
- `TailSharePercent` в стратегии дополнительно жестко ограничивается в `1..50`.

## Смысл режимов `BayesianAcquisitionMode`
- `Ucb`: приоритет кандидатов по `mean + kappa * uncertainty`.
- `ExpectedImprovement`: приоритет по ожидаемому улучшению над текущим лучшим.
- `Greedy`: берет по текущей оценке (`mean`) без компоненты неопределенности.

## Важный нюанс по `Kappa`
Перед применением `kappa` масштабируется по целевой метрике:
- `PositionCount`: `kappa * 0.5`
- `MaxDrawDawn`: `kappa * 1.3`
- `SharpRatio`: `kappa * 1.2`
- `ProfitFactor`/`PayOffRatio`: `kappa * 0.8`
- прочие: без изменения

Это значит, что одинаковое значение `kappa` на разных метриках ведет себя по-разному.

## Что считается невалидным
- `InitialSamples <= 0`, `MaxIterations <= 0`, `BatchSize <= 0`
- `Kappa < 0`
- `TailSharePercent` вне `1..50` (UI не пропустит, settings/strategy дополнительно ограничат)
- невалидный enum в `BayesianAcquisitionMode`

## Рекомендации по стартовым настройкам
Единственного «оптимального» набора нет: зависит от размера сетки параметров, шума метрики и времени теста.

Практичные пресеты:

1. Быстрый прогон (черновой)
- `InitialSamples = 10`
- `MaxIterations = 40`
- `BatchSize = 3`
- `AcquisitionMode = Ucb`
- `Kappa = 0.20`
- `UseTailPass = true`
- `TailSharePercent = 15`

2. Сбалансированный (рекомендуемый старт)
- `InitialSamples = 20`
- `MaxIterations = 100`
- `BatchSize = 5`
- `AcquisitionMode = Ucb`
- `Kappa = 0.25`
- `UseTailPass = true`
- `TailSharePercent = 20`

3. Более тщательный поиск
- `InitialSamples = 30..50`
- `MaxIterations = 150..300`
- `BatchSize = 5..10`
- `AcquisitionMode = ExpectedImprovement`
- `Kappa = 0.15..0.35`
- `UseTailPass = true`
- `TailSharePercent = 20..30`

## Значения по умолчанию в проекте
- `BayesianInitialSamples = 20`
- `BayesianMaxIterations = 100`
- `BayesianBatchSize = 5`
- `BayesianAcquisitionMode = Ucb`
- `BayesianAcquisitionKappa = 0.25`
- `BayesianUseTailPass = true`
- `BayesianTailSharePercent = 20`

Источник: `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`.
