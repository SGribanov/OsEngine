#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Market.Connectors;
using OsEngine.Market.Servers;
using OsEngine.Market.Servers.Tester;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots;
using Xunit;

namespace OsEngine.Tests;

[Collection("ImpulsV1ScriptBot")]
public sealed class ImpulsV1ScriptTests
{
    [Fact]
    public void ImpulsV1Script_ShouldCompileAsScript_AndExposeSecondMomentumParameter()
    {
        using ImpulsV1ScriptScope scope = new();
        BotPanel bot = scope.CreateBot("CodexImpulsV1ScriptCompile");

        Assert.True(bot.IsScript);
        Assert.Contains(bot.Parameters, parameter => parameter.Name == "Second Momentum Length");

        bot.Delete();
    }

    [Fact]
    public void ImpulsV1Script_ShouldBlockLong_WhenSecondMomentumDoesNotConfirm()
    {
        using ImpulsV1ScriptScope scope = new();
        BotPanel bot = scope.CreateBot("CodexImpulsV1ScriptLongBlocked");

        ConfigureSignalParameters(bot, momentumLength: 1, secondMomentumLength: 2);

        List<Candle> candles = CreateBullishCandlesWithoutSecondMomentumConfirmation();
        BotTabSimple tab = PrepareTabForSignalTest(bot, candles);

        InvokePrivate(bot, "OnCandleFinished", candles);

        Assert.Empty(tab.PositionsAll ?? new List<Position>());

        bot.Delete();
    }

    [Fact]
    public void ImpulsV1Script_ShouldOpenLong_WhenBothMomentumValuesRiseAboveZero()
    {
        using ImpulsV1ScriptScope scope = new();
        BotPanel bot = scope.CreateBot("CodexImpulsV1ScriptLongAllowed");

        ConfigureSignalParameters(bot, momentumLength: 1, secondMomentumLength: 2);

        List<Candle> candles = CreateBullishCandlesWithDoubleMomentumConfirmation();
        BotTabSimple tab = PrepareTabForSignalTest(bot, candles);

        InvokePrivate(bot, "OnCandleFinished", candles);

        Position position = Assert.Single(tab.PositionsAll ?? new List<Position>());
        Assert.Equal(Side.Buy, position.Direction);
        Assert.Equal("ImpulseLong", position.SignalTypeOpen);

        bot.Delete();
    }

    [Fact]
    public void ImpulsV1Script_ShouldOpenShort_WhenBothMomentumValuesFallBelowZero()
    {
        using ImpulsV1ScriptScope scope = new();
        BotPanel bot = scope.CreateBot("CodexImpulsV1ScriptShortAllowed");

        ConfigureSignalParameters(bot, momentumLength: 1, secondMomentumLength: 2);

        List<Candle> candles = CreateBearishCandlesWithDoubleMomentumConfirmation();
        BotTabSimple tab = PrepareTabForSignalTest(bot, candles);

        InvokePrivate(bot, "OnCandleFinished", candles);

        Position position = Assert.Single(tab.PositionsAll ?? new List<Position>());
        Assert.Equal(Side.Sell, position.Direction);
        Assert.Equal("ImpulseShort", position.SignalTypeOpen);

        bot.Delete();
    }

    private static void ConfigureSignalParameters(BotPanel bot, int momentumLength, int secondMomentumLength)
    {
        GetPrivateField<StrategyParameterString>(bot, "_regime").ValueString = "On";
        GetPrivateField<StrategyParameterInt>(bot, "_momentumLength").ValueInt = momentumLength;
        GetPrivateField<StrategyParameterInt>(bot, "_secondMomentumLength").ValueInt = secondMomentumLength;
        GetPrivateField<StrategyParameterInt>(bot, "_averageVolumeLength").ValueInt = 1;
        GetPrivateField<StrategyParameterInt>(bot, "_atrLength").ValueInt = 1;
        GetPrivateField<StrategyParameterDecimal>(bot, "_volumeExcessPercent").ValueDecimal = 0m;
        GetPrivateField<StrategyParameterDecimal>(bot, "_bodyAtrMultiplier").ValueDecimal = 0.1m;
    }

    private static BotTabSimple PrepareTabForSignalTest(BotPanel bot, List<Candle> candles)
    {
        BotTabSimple tab = Assert.Single(bot.TabsSimple);
        Security security = new()
        {
            Name = "CODEx_TEST",
            NameClass = "CODEx_CLASS",
            PriceStep = 1m,
            PriceStepCost = 1m,
            Lot = 1m,
            DecimalsVolume = 0
        };
        Portfolio portfolio = new()
        {
            Number = "GodMode",
            ValueCurrent = 100000m,
            ServerUniqueName = "Tester"
        };

        tab.Security = security;
        tab.Portfolio = portfolio;

        ConnectorCandles connector = tab.Connector;
        SetPrivateField(connector, "_securityName", security.Name);
        SetPrivateField(connector, "_securityClass", security.NameClass);
        SetPrivateField(connector, "_bestBid", candles[^1].Close);
        SetPrivateField(connector, "_bestAsk", candles[^1].Close + security.PriceStep);
        SetPrivateField(connector, "_mySeries", new CandleSeries(connector.TimeFrameBuilder, security, StartProgram.IsTester)
        {
            CandlesAll = candles
        });

        TesterServer testerServer = (TesterServer)RuntimeHelpers.GetUninitializedObject(typeof(TesterServer));
        SetPrivateField(testerServer, "_serverConnectStatus", ServerConnectStatus.Connect);
        SetPrivateField(testerServer, "_serverTime", candles[^1].TimeStart);
        SetPrivateField(testerServer, "OrdersActive", new List<Order>());
        SetPrivateField(connector, "_myServer", testerServer);

        return tab;
    }

    private static List<Candle> CreateBullishCandlesWithoutSecondMomentumConfirmation()
    {
        DateTime start = new DateTime(2026, 3, 8, 10, 0, 0, DateTimeKind.Utc);

        return new List<Candle>
        {
            CreateCandle(start, 100m, 105m, 95m, 100m, 50m),
            CreateCandle(start.AddMinutes(1), 103m, 105m, 99m, 105m, 60m),
            CreateCandle(start.AddMinutes(2), 104m, 111m, 101m, 106m, 100m),
            CreateCandle(start.AddMinutes(3), 107m, 113m, 107m, 113m, 200m)
        };
    }

    private static List<Candle> CreateBullishCandlesWithDoubleMomentumConfirmation()
    {
        DateTime start = new DateTime(2026, 3, 8, 10, 0, 0, DateTimeKind.Utc);

        return new List<Candle>
        {
            CreateCandle(start, 100m, 105m, 95m, 100m, 50m),
            CreateCandle(start.AddMinutes(1), 101m, 107m, 97m, 103m, 60m),
            CreateCandle(start.AddMinutes(2), 103m, 110m, 100m, 106m, 100m),
            CreateCandle(start.AddMinutes(3), 107m, 113m, 107m, 113m, 200m)
        };
    }

    private static List<Candle> CreateBearishCandlesWithDoubleMomentumConfirmation()
    {
        DateTime start = new DateTime(2026, 3, 8, 10, 0, 0, DateTimeKind.Utc);

        return new List<Candle>
        {
            CreateCandle(start, 110m, 115m, 105m, 110m, 50m),
            CreateCandle(start.AddMinutes(1), 109m, 113m, 103m, 107m, 60m),
            CreateCandle(start.AddMinutes(2), 107m, 110m, 100m, 104m, 100m),
            CreateCandle(start.AddMinutes(3), 103m, 103m, 97m, 97m, 200m)
        };
    }

    private static Candle CreateCandle(DateTime timeStart, decimal open, decimal high, decimal low, decimal close, decimal volume)
    {
        return new Candle
        {
            TimeStart = timeStart,
            Open = open,
            High = high,
            Low = low,
            Close = close,
            Volume = volume
        };
    }

    private static void InvokePrivate(object instance, string methodName, params object[] args)
    {
        MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Method {methodName} not found.");

        method.Invoke(instance, args);
    }

    private static T GetPrivateField<T>(object instance, string fieldName)
    {
        FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field {fieldName} not found.");

        return (T)(field.GetValue(instance)
            ?? throw new InvalidOperationException($"Field {fieldName} is null."));
    }

    private static void SetPrivateField(object instance, string fieldName, object? value)
    {
        FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field {fieldName} not found.");

        field.SetValue(instance, value);
    }

    private sealed class ImpulsV1ScriptScope : IDisposable
    {
        private readonly string _currentDirectory;
        private readonly string _engineDirectory;
        private readonly string _customDirectory;
        private readonly string _robotsDirectory;
        private readonly string _scriptPath;
        private readonly string _backupPath;
        private readonly bool _customDirectoryExisted;
        private readonly bool _robotsDirectoryExisted;
        private readonly bool _scriptExisted;

        public ImpulsV1ScriptScope()
        {
            _currentDirectory = Directory.GetCurrentDirectory();
            _engineDirectory = Path.Combine(_currentDirectory, "Engine");
            _customDirectory = Path.Combine(_currentDirectory, "Custom");
            _robotsDirectory = Path.Combine(_customDirectory, "Robots");
            _scriptPath = Path.Combine(_robotsDirectory, "ImpulsV1.cs");
            _backupPath = _scriptPath + ".codex.bak";

            _customDirectoryExisted = Directory.Exists(_customDirectory);
            _robotsDirectoryExisted = Directory.Exists(_robotsDirectory);
            _scriptExisted = File.Exists(_scriptPath);

            Directory.CreateDirectory(_robotsDirectory);

            if (_scriptExisted)
            {
                File.Copy(_scriptPath, _backupPath, overwrite: true);
            }

            File.Copy(GetSourceScriptPath(), _scriptPath, overwrite: true);
            ClearBotFactoryCaches();
        }

        public BotPanel CreateBot(string instanceName)
        {
            ClearBotFactoryCaches();

            BotPanel bot = BotFactory.GetStrategyForName("ImpulsV1", instanceName, StartProgram.IsTester, true)
                ?? throw new InvalidOperationException("ImpulsV1 script bot was not created.");

            return bot;
        }

        public void Dispose()
        {
            ClearBotFactoryCaches();

            if (_scriptExisted)
            {
                if (File.Exists(_backupPath))
                {
                    File.Copy(_backupPath, _scriptPath, overwrite: true);
                    File.Delete(_backupPath);
                }
            }
            else
            {
                if (File.Exists(_scriptPath))
                {
                    File.Delete(_scriptPath);
                }
            }

            if (!_robotsDirectoryExisted
                && Directory.Exists(_robotsDirectory)
                && !Directory.EnumerateFileSystemEntries(_robotsDirectory).Any())
            {
                Directory.Delete(_robotsDirectory);
            }

            if (!_customDirectoryExisted
                && Directory.Exists(_customDirectory)
                && !Directory.EnumerateFileSystemEntries(_customDirectory).Any())
            {
                Directory.Delete(_customDirectory);
            }

            if (Directory.Exists(_engineDirectory))
            {
                foreach (string file in Directory.EnumerateFiles(_engineDirectory, "CodexImpulsV1Script*", SearchOption.TopDirectoryOnly))
                {
                    TryDeleteFile(file);
                }
            }
        }

        private static void ClearBotFactoryCaches()
        {
            ClearStaticDictionary("_folderFileCache");
            ClearStaticDictionary("_compiledBotTypesCache");
        }

        private static void ClearStaticDictionary(string fieldName)
        {
            FieldInfo field = typeof(BotFactory).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Field {fieldName} not found.");

            object dictionary = field.GetValue(null)
                ?? throw new InvalidOperationException($"Field {fieldName} is null.");

            MethodInfo clearMethod = dictionary.GetType().GetMethod("Clear")
                ?? throw new InvalidOperationException($"Field {fieldName} does not expose Clear().");

            clearMethod.Invoke(dictionary, null);
        }

        private static string GetSourceScriptPath()
        {
            DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "project", "OsEngine", "bin", "Debug", "Custom", "Robots", "ImpulsV1.cs");

                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not find source ImpulsV1.cs in repository tree.");
        }

        private static void TryDeleteFile(string path)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    return;
                }
                catch (IOException) when (i < 4)
                {
                    System.Threading.Thread.Sleep(50);
                }
            }
        }
    }
}

[CollectionDefinition("ImpulsV1ScriptBot", DisableParallelization = true)]
public sealed class ImpulsV1ScriptCollectionDefinition
{
}
