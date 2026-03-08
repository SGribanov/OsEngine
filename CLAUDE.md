# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

OsEngine is an open-source algorithmic trading platform written in C# / WPF. It provides a complete suite for automated stock and crypto exchange trading: bot station (live trading), backtesting, strategy optimization, and historical data downloading. The codebase is bilingual (Russian/English comments and UI via localization).

## Build & Run

```bash
# Solution file
project/OsEngine.sln

# Canonical verification entrypoint (preferred)
powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify-dotnet.ps1

# Restore
dotnet restore project/OsEngine.sln

# Build (required after any change)
dotnet build project/OsEngine/OsEngine.csproj --configuration Release

# Test (required after any change)
dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Release

# Run (WPF desktop app, Windows only)
dotnet run --project project/OsEngine/OsEngine.csproj
```

All `restore/build/test` commands must be executed in host context (outside sandbox).
Prefer the repository script `tools/verify-dotnet.ps1` for local verification because it serializes restore/build/test and shuts down build servers before and after the run to reduce recurring WPF/generated-file lock noise.

- SDK baseline: `.NET 10`
- Language baseline: `C# 14`
- Target framework: `net10.0-windows` (WPF + WinForms interop)
- Platform: `x64` only
- Output goes to `project/OsEngine/bin/Debug/` or `bin/Release/` (no TFM subfolder appended)
- Many DLL dependencies live directly in `bin/Debug/` and are referenced by HintPath (not NuGet)
- Test project exists: `project/OsEngine.Tests/OsEngine.Tests.csproj` (xUnit)

## Engineering Baseline (Mandatory)

This baseline is mandatory for this repository and should be treated as default guidance for .NET projects unless a project-specific policy says otherwise.

- Prioritize reliability and security over convenience and speed of delivery.
- Apply modern .NET/C# best practices only when they are reasonable for the context and improve maintainability, runtime efficiency, or memory allocation profile.
- Prefer explicit, deterministic, and culture-safe behavior in protocol/serialization/boundary code.
- For every code change, run build and tests before moving to the next task.
- Always run package restore, build, and tests outside sandbox (host context).
- Aim to cover every changed method/function/module with tests; if full coverage is not feasible, document the gap and risk explicitly.

## Architecture

### Application Modes (`Entity/StartProgram.cs`)

The app launches from `MainWindow` and spawns one of these modes:
- **IsOsTrader** - Live trading bot station (`OsTrader/OsTraderMaster.cs`)
- **IsTester** - Backtesting via exchange emulator (`Market/Servers/Tester/`)
- **IsOsOptimizer** - Parameter optimization (`OsOptimizer/OptimizerMaster.cs`)
- **IsOsData** - Historical data downloader (`OsData/OsDataMaster.cs`)
- **IsOsConverter** - Tick-to-candle converter (`OsConverter/OsConverterMaster.cs`)

### Core Layers

**Market/Servers/** - Exchange connectivity layer. Each exchange has its own subfolder (e.g., `Binance/Spot/`, `Bybit/`, `OKX/`). The pattern:
- `AServer` (abstract class) provides the server lifecycle, event routing, order management, and data storage
- `IServerRealization` - interface each exchange connector must implement (Connect, Subscribe, SendOrder, GetCandleData, etc.)
- Each exchange folder contains a server class (extends `AServer`) that wires parameters, and a realization class (implements `IServerRealization`) with the actual REST/WebSocket logic
- `ServerMaster.cs` in `Market/` is the factory that creates and manages all server instances

**OsTrader/Panels/** - Robot framework:
- `BotPanel` - abstract base class for all trading robots. Constructor takes `(string name, StartProgram startProgram)`
- `BotTabSimple` - primary trading tab for a single security (candles, indicators, order execution, journal)
- `BotTabScreener` - multi-security tab
- `BotTabPair` - pair trading tab
- `BotTabPolygon` - currency arbitrage tab
- `BotTabIndex`, `BotTabCluster`, `BotTabNews`, `BotTabOptions` - specialized tabs

**Robots/** - 300+ built-in trading strategies organized by category:
- `Trend/`, `CounterTrend/`, `MarketMaker/`, `Patterns/`, `Screeners/`, `PairArbitrage/`, `IndexArbitrage/`, `CurrencyArbitrage/`, `Grids/`, `High Frequency/`, etc.
- `BotFactory.cs` discovers robots via reflection using the `[Bot("Name")]` attribute or by Roslyn-compiling scripts from `Custom/Robots/Scripts/`

**Indicators/** - Technical indicator framework:
- `Aindicator` - abstract base for all indicators. Override `OnStateChange()` and `OnProcess()`
- `IndicatorsFactory.cs` discovers indicators via `[Indicator("Name")]` attribute or compiles scripts from `Custom/Indicators/Scripts/`
- Built-in indicators are individual `.cs` files in this folder (e.g., `AC.cs`)

**Candles/** - Candle construction subsystem:
- `ACandlesSeriesRealization` - abstract base for candle series types
- Built-in types in `Series/`: Simple (time-based), Tick, Range, Delta, HeikenAshi, Revers, Volume-adaptive, etc.
- `CandleFactory.cs` manages creation of candle series

### Key Entity Types (`Entity/`)

- `Security` - tradeable instrument
- `Portfolio` / `PositionOnBoard` - account/position tracking
- `Order` / `MyTrade` / `Trade` - order and trade types
- `Position` - strategy-level position with entry/exit tracking
- `MarketDepth` - order book
- `Candle` - OHLCV candle
- `StrategyParameter` - robot parameter system

### Registration Pattern for New Components

**Robots**: Decorate class with `[Bot("MyRobotName")]` attribute. The class must extend `BotPanel` and have constructor `(string name, StartProgram startProgram)`. `BotFactory` discovers it automatically via assembly reflection. Alternatively, place a `.cs` script in `Custom/Robots/Scripts/` for Roslyn compilation.

**Indicators**: Decorate class with `[Indicator("MyIndicatorName")]` attribute. The class must extend `Aindicator`. Alternatively, place scripts in `Custom/Indicators/Scripts/`.

**Exchange Connectors**: Add a new folder under `Market/Servers/`, create a server class extending `AServer` and a realization class implementing `IServerRealization`, then register the `ServerType` enum value and add the creation case in `ServerMaster.cs`.

### Other Subsystems

- **Language/** - Localization system (`OsLocalization` class). Supports Russian (`Ru`) and English (`Eng`). UI strings are accessed via `OsLocalization.{Category}.{Key}`
- **Logging/** - Log system with email, SMS, Telegram, and Webhook notification channels
- **Journal/** - Trade journal for position tracking and P&L analysis
- **Charts/** - Candlestick charting (`CandleChart/`) and cluster charts (`ClusterChart/`)
- **Alerts/** - Price/indicator alert system
- **Layout/** - Window positioning and sticky borders system
- **PrimeSettings/** - Global application settings
- **Attributes/** - Parameter binding system using `[Parameter]`, `[Label]`, `[Button]` attributes for auto-generating robot UI

## Coding Conventions

- Comments and variable names mix Russian and English; public API comments typically have both languages
- License header is present in every source file
- Settings are persisted to text files in the `Engine/` directory at runtime (not a database)
- WPF XAML + code-behind pattern for all UI; no MVVM framework
- Each UI component typically has a paired `.xaml` / `.xaml.cs` file
- Decimal is used for all price/volume values (not double)
