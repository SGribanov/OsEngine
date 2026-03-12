#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.MachineLearning.Onnx;
using OsEngine.Market.Connectors;
using OsEngine.Market.Servers;
using OsEngine.Market.Servers.Tester;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.TechSamples;
using Xunit;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public sealed class OnnxModelRuntimeTests
{
    private const string ModelABase64 =
        "CA0SBWNvZGV4OrUBCiEKCGZlYXR1cmVzCgFXEgptYXRtdWxfb3V0IgZNYXRNdWwKGwoKbWF0bXVsX291dAoBQhIFc2NvcmUiA0FkZBISY29kZXhfbGluZWFyX21vZGVsKhsIBAgBEAEiEM3MTD+amZm+mpkZP83MTD5CAVcqDQgBEAEiBM3MTD1CAUJaGgoIZmVhdHVyZXMSDgoMCAESCAoCCAEKAggEYhcKBXNjb3JlEg4KDAgBEggKAggBCgIIAUIECgAQDQ==";

    private const string ModelBBase64 =
        "CA0SBWNvZGV4OrUBCiEKCGZlYXR1cmVzCgFXEgptYXRtdWxfb3V0IgZNYXRNdWwKGwoKbWF0bXVsX291dAoBQhIFc2NvcmUiA0FkZBISY29kZXhfbGluZWFyX21vZGVsKhsIBAgBEAEiEAAAAL/NzMw+MzMzv83MzL1CAVcqDQgBEAEiBArXo7xCAUJaGgoIZmVhdHVyZXMSDgoMCAESCAoCCAEKAggEYhcKBXNjb3JlEg4KDAgBEggKAggBCgIIAUIECgAQDQ==";

    [Fact]
    public void LoadAndRun_WithLinearModel_ShouldReturnExpectedScoreAndMetadata()
    {
        using OnnxModelFileScope scope = new();
        string modelPath = scope.WriteModel("linear-a.onnx", ModelABase64);
        using OnnxModelRuntime runtime = new();

        runtime.Load(modelPath);
        OnnxInferenceResult result = runtime.Run(OnnxTensorInput.Create("features", new[] { 1f, 2f, 1f, 2f }, 1, 4));

        Assert.True(runtime.IsLoaded);
        Assert.Equal(Path.GetFullPath(modelPath), runtime.ModelPath);
        Assert.Contains("features", runtime.InputNames);
        Assert.Contains("score", runtime.OutputNames);
        Assert.Equal(1.25f, result.GetOutput("score").GetScalar<float>(), 3);
    }

    [Fact]
    public void Reload_AfterModelFileReplacement_ShouldUseNewWeights()
    {
        using OnnxModelFileScope scope = new();
        string modelPath = scope.WriteModel("linear-reload.onnx", ModelABase64);
        using OnnxModelRuntime runtime = new();

        runtime.Load(modelPath);
        float initialScore = runtime
            .Run(OnnxTensorInput.Create("features", new[] { 1f, 2f, 1f, 2f }, 1, 4))
            .GetOutput("score")
            .GetScalar<float>();

        scope.OverwriteModel(modelPath, ModelBBase64);
        runtime.Reload();

        float reloadedScore = runtime
            .Run(OnnxTensorInput.Create("features", new[] { 1f, 2f, 1f, 2f }, 1, 4))
            .GetOutput("score")
            .GetScalar<float>();

        Assert.Equal(1.25f, initialScore, 3);
        Assert.Equal(-0.62f, reloadedScore, 3);
    }

    [Fact]
    public void Run_WithoutLoad_ShouldThrowInvalidOperationException()
    {
        using OnnxModelRuntime runtime = new();

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            runtime.Run(OnnxTensorInput.Create("features", new[] { 1f, 2f, 1f, 2f }, 1, 4)));

        Assert.Contains("no model has been loaded", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_WithMissingModel_ShouldThrowFileNotFoundException()
    {
        using OnnxModelRuntime runtime = new();

        FileNotFoundException error = Assert.Throws<FileNotFoundException>(() =>
            runtime.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) + ".onnx")));

        Assert.Contains(".onnx", error.FileName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dispose_AfterLoad_ShouldRejectFurtherOperations()
    {
        using OnnxModelFileScope scope = new();
        string modelPath = scope.WriteModel("linear-dispose.onnx", ModelABase64);
        OnnxModelRuntime runtime = new();
        runtime.Load(modelPath);

        runtime.Dispose();

        Assert.Throws<ObjectDisposedException>(() => runtime.Reload());
        Assert.Throws<ObjectDisposedException>(() => _ = runtime.InputNames);
    }

    [Fact]
    public void OnnxInferenceSample_WithPositiveSignal_ShouldOpenLongPosition()
    {
        using OnnxModelFileScope scope = new();
        string modelPath = scope.WriteModel("linear-robot.onnx", ModelABase64);
        OnnxInferenceSample robot = new("CodexOnnxRobot", StartProgram.IsTester);

        SetStringParameter(robot, "Regime", "On");
        SetDecimalParameter(robot, "Volume", 1m);
        SetDecimalParameter(robot, "SignalThreshold", 0.05m);
        SetStringParameter(robot, "ModelPath", modelPath);
        SetStringParameter(robot, "InputTensorName", "features");
        SetStringParameter(robot, "OutputTensorName", "score");

        List<Candle> candles = CreatePositiveSignalCandles();
        BotTabSimple tab = PrepareTabForSignalTest(robot, candles);

        InvokePrivate(robot, "OnCandleFinished", candles);

        Position position = Assert.Single(tab.PositionsAll ?? new List<Position>());
        Assert.Equal(Side.Buy, position.Direction);

        robot.Delete();
    }

    private static void SetStringParameter(BotPanel bot, string parameterName, string value)
    {
        StrategyParameterString parameter = Assert.IsType<StrategyParameterString>(bot.Parameters.Find(p => p.Name == parameterName));
        parameter.ValueString = value;
    }

    private static void SetDecimalParameter(BotPanel bot, string parameterName, decimal value)
    {
        StrategyParameterDecimal parameter = Assert.IsType<StrategyParameterDecimal>(bot.Parameters.Find(p => p.Name == parameterName));
        parameter.ValueDecimal = value;
    }

    private static BotTabSimple PrepareTabForSignalTest(BotPanel bot, List<Candle> candles, decimal priceStep = 1m)
    {
        BotTabSimple tab = Assert.Single(bot.TabsSimple);
        Security security = new()
        {
            Name = "ONNX_TEST",
            NameClass = "ONNX_CLASS",
            PriceStep = priceStep,
            PriceStepCost = priceStep,
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

    private static List<Candle> CreatePositiveSignalCandles()
    {
        DateTime start = new DateTime(2026, 3, 12, 10, 0, 0, DateTimeKind.Utc);

        return new List<Candle>
        {
            CreateCandle(start, 100m, 101m, 99m, 100m, 50m),
            CreateCandle(start.AddMinutes(1), 100m, 102m, 100m, 101m, 60m),
            CreateCandle(start.AddMinutes(2), 101m, 104m, 101m, 103m, 70m),
            CreateCandle(start.AddMinutes(3), 103m, 105m, 103m, 104m, 80m),
            CreateCandle(start.AddMinutes(4), 104m, 107m, 104m, 106m, 90m)
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

    private static void SetPrivateField(object instance, string fieldName, object? value)
    {
        FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Field {fieldName} not found.");

        field.SetValue(instance, value);
    }

    private sealed class OnnxModelFileScope : IDisposable
    {
        private readonly string _directoryPath;

        public OnnxModelFileScope()
        {
            _directoryPath = Path.Combine(Path.GetTempPath(), "osengine-onnx-tests", Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
            Directory.CreateDirectory(_directoryPath);
        }

        public string WriteModel(string fileName, string base64Payload)
        {
            string path = Path.Combine(_directoryPath, fileName);
            File.WriteAllBytes(path, Convert.FromBase64String(base64Payload));
            return path;
        }

        public void OverwriteModel(string path, string base64Payload)
        {
            File.WriteAllBytes(path, Convert.FromBase64String(base64Payload));
        }

        public void Dispose()
        {
            if (Directory.Exists(_directoryPath))
            {
                Directory.Delete(_directoryPath, recursive: true);
            }
        }
    }
}
