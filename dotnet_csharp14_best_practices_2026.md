# .NET 10 / C# 14 Best Practices (2026 Snapshot)

Дата фиксации: `2026-03-07`

## Baseline

1. Использовать `.NET 10 (LTS)` как основную production-базу для долгоживущих модулей.
2. Для проектов `net10.0*` ориентироваться на `C# 14` (или default от TFM), не форсировать язык выше поддерживаемого TFM.
3. Не использовать preview-фичи языка/SDK без явного продуктового запроса.

## Concurrency

1. В новом коде предпочитать `System.Threading.Lock` + `lock`/`EnterScope()`.
2. Не допускать `await` внутри критической секции.
3. В коде с несколькими lock-объектами соблюдать единый порядок захвата.

## Memory & Hot Path

1. Для синхронных hot-path API предпочитать `Span<T>`/`ReadOnlySpan<T>`.
2. Для async-границ и долгоживущих буферов применять `Memory<T>`/`ReadOnlyMemory<T>`.
3. При использовании `IMemoryOwner<T>` явно управлять ownership/dispose.
4. Для read-mostly lookup-структур рассматривать `FrozenDictionary`/`FrozenSet`.

## Serialization

1. Для performance-critical и AOT-sensitive сериализации использовать `System.Text.Json` source generation:
   `JsonSerializerContext`, `[JsonSerializable]`, `TypeInfoResolver`.
2. Выбирать generation mode осознанно:
   `Serialization` для fast path сериализации,
   `Metadata` для универсальных сценариев.

## Time & Testability

1. Для бизнес-логики времени использовать `TimeProvider`.
2. В unit-тестах использовать `FakeTimeProvider`.
3. Избегать прямой зависимости от `DateTime.Now/UtcNow` в тестируемых сервисах.

## Code Analysis

1. Анализаторы включены по умолчанию для .NET 5+; держать их включенными.
2. Настраивать `AnalysisMode` и severity через `dotnet_diagnostic.*.severity`.
3. Не понижать performance/reliability правила в hot-path без документированного обоснования.

## C# 14 Features Usage Policy

1. Применять `field`-backed properties для валидации без ручного backing-field, когда это упрощает код.
2. Использовать null-conditional assignment для безопасных кратких мутаций.
3. Extension members и implicit span conversions применять только там, где это повышает читаемость и не ухудшает поддержку.
4. Не внедрять новые синтаксические возможности "ради новизны".

## Sources (official)

1. https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview
2. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14
3. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning
4. https://learn.microsoft.com/en-us/dotnet/api/system.threading.lock?view=net-10.0
5. https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines
6. https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation
7. https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview
8. https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options
9. https://learn.microsoft.com/en-us/dotnet/api/system.collections.frozen.frozendictionary-2?view=net-10.0
