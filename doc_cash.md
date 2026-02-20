# Кэш в оптимизаторе OsEngine (indicator + internal methods)

Документ описывает, как работает кэш расчетов в оптимизаторе, как формируются ключи, и как писать роботов с учетом кэша.

## 1. Что кэшируется

- Кэш индикаторов: результаты `Aindicator.ProcessAll(List<Candle>)`.
- Кэш внутренних методов робота: результаты детерминированных расчетных методов, которые разработчик явно оборачивает в API `BotPanel`.

Кэш активен только в режиме оптимизатора и только при включенной опции `UseIndicatorCache` в настройках оптимизатора.

## 2. Быстрая mind-map схема

```text
OptimizerExecutor (старт прогона)
  -> читает UseIndicatorCache
  -> создает IndicatorCache + OptimizerMethodCache
  -> прокидывает их в статические точки доступа
       Aindicator.SetOptimizerIndicatorCache(...)
       BotPanel.SetOptimizerMethodCache(...)

Робот/индикатор в прогоне
  -> вызов расчета (индикатор или internal method)
  -> формирование cache key
  -> TryGet(key)
       -> HIT: вернуть готовое значение
       -> MISS: посчитать -> Set(key, value) -> вернуть

OptimizerExecutor (финиш прогона)
  -> снимает статистику HIT/MISS/WRITE/EVICT
  -> логирует
  -> очищает кэши
```

## 3. Ключи кэша

### 3.1 Индикаторный кэш

Ключ учитывает:
- инструмент (`security/ticker`);
- таймфрейм;
- имя/тип индикатора;
- параметры индикатора;
- характеристики свечного окна (временные границы, размер, fingerprint данных, источник серии).

Это защищает от ложных попаданий между разными инструментами/параметрами/окнами данных.

### 3.2 Кэш внутренних методов робота

Ключ `OptimizerMethodCacheKey` включает:
- `securityName`;
- `timeframeTicks`;
- `calculationName` (уникальное имя вашего метода);
- `parametersHash` (хэш параметров метода через `BuildOptimizerMethodCacheParameterHash(...)`);
- границы и размер свечного окна;
- fingerprint свечей;
- `resultTypeName`.

Важно: для внутренних методов вы сами контролируете `calculationName` и `parametersHash`, поэтому коллизии обычно решаются дисциплиной именования.

## 4. Как писать внутренние методы с кэшем

Используйте API из `BotPanel`:

- `BuildOptimizerMethodCacheParameterHash(params object[] parts)`
- `GetOrCreateOptimizerMethodCacheValue<T>(...)`

Минимальный шаблон:

```csharp
string parametersHash = BuildOptimizerMethodCacheParameterHash(length, threshold);

decimal value = GetOrCreateOptimizerMethodCacheValue(
    _tab,
    "MyRobot.MyDeterministicCalc",
    parametersHash,
    candles,
    () =>
    {
        // чистый детерминированный расчет
        return ComputeValue(candles, length, threshold);
    });
```

## 5. Пример, добавленный в проект

Робот: `project/OsEngine/Robots/TechSamples/CustomDataInIndicatorSample.cs`

Что добавлено:
- параметр `Custom average length`;
- внутренний метод `GetCachedHalfCloseAverage(List<Candle>)`;
- метод обернут в `GetOrCreateOptimizerMethodCacheValue(...)` c `calculationName = "CustomDataInIndicatorSample.HalfCloseAverage"`;
- `parametersHash` строится через `BuildOptimizerMethodCacheParameterHash(actualLength)`.

## 6. Внешние индикаторы: как раньше + авто-кэш

Вызов внешних индикаторов остается прежним (через `CreateCandleIndicator`, `ProcessAll`, `Reload` и т.д.).
Если кэш оптимизатора включен и индикатор детерминированный (`IsDeterministicForOptimizerCache == true`), кэш применяется автоматически.

## 7. Что изменилось для автора роботов

- Для внешних индикаторов: почти ничего, поведение прозрачное.
- Для внутренних методов: кэш включается только там, где вы явно вызвали API `GetOrCreateOptimizerMethodCacheValue`.
- По сути это и есть «принудительное включение» для конкретных методов: метод попадет в кэш только если вы его обернули.

## 8. Ограничения и рекомендации

- Кэшируйте только детерминированные функции от входных данных и параметров.
- Не кэшируйте методы с побочными эффектами (логирование действий, изменение состояния позиций и т.п.).
- Если возвращаете изменяемые коллекции/объекты, используйте `cloneValue`, чтобы не повредить кэшированный экземпляр.
- Делайте `calculationName` стабильным и уникальным (`ClassName.MethodName`).
- В `parametersHash` включайте все параметры, влияющие на результат.

## 9. Диагностика

При завершении оптимизационного прогона `OptimizerExecutor` пишет статистику кэшей в лог:
- Hits
- Misses
- Writes
- Evictions
- Entries

По этим метрикам можно оценить реальный эффект от кэширования.
