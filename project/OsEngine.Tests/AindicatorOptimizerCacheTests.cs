#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using OsEngine.Entity;
using OsEngine.Indicators;
using OsEngine.OsOptimizer.OptEntity;
using Xunit;

namespace OsEngine.Tests;

public class AindicatorOptimizerCacheTests
{
    [Fact]
    public void Init_InOptimizerMode_ShouldPrimeParameterHashBeforeFirstCacheContextBuild()
    {
        using OptimizerIndicatorCacheScope scope = new();
        OptimizerCacheTestIndicator indicator = CreateOptimizerIndicator("CodexAindicatorPrime");
        CountingIndicatorParameter parameter = indicator.CountingParameterInstance;

        Assert.False(GetOptimizerParameterHashDirty(indicator));
        Assert.Equal(1, parameter.GetStringToSaveCalls);

        parameter.ResetCounter();

        bool prepared = indicator.TryPrepareOptimizerCacheContext(
            BuildCandles(6),
            out IndicatorCache cache,
            out IndicatorCacheKey cacheKey);

        Assert.True(prepared);
        Assert.Same(scope.Cache, cache);
        Assert.Equal(ExpectedParameterHash(parameter.SerializedValue), cacheKey.ParametersHash);
        Assert.Equal(0, parameter.GetStringToSaveCalls);
    }

    [Fact]
    public void TryPrepareOptimizerCacheContext_AfterParameterChange_ShouldRebuildSnapshotOnce()
    {
        using OptimizerIndicatorCacheScope scope = new();
        OptimizerCacheTestIndicator indicator = CreateOptimizerIndicator("CodexAindicatorDirty");
        CountingIndicatorParameter parameter = indicator.CountingParameterInstance;
        List<Candle> candles = BuildCandles(6);

        parameter.ResetCounter();
        parameter.SetSerializedValue("Length#21#");

        Assert.True(GetOptimizerParameterHashDirty(indicator));

        bool prepared = indicator.TryPrepareOptimizerCacheContext(
            candles,
            out IndicatorCache firstCache,
            out IndicatorCacheKey firstKey);

        Assert.True(prepared);
        Assert.Same(scope.Cache, firstCache);
        Assert.False(GetOptimizerParameterHashDirty(indicator));
        Assert.Equal(1, parameter.GetStringToSaveCalls);
        Assert.Equal(ExpectedParameterHash(parameter.SerializedValue), firstKey.ParametersHash);

        parameter.ResetCounter();

        bool preparedAgain = indicator.TryPrepareOptimizerCacheContext(
            candles,
            out IndicatorCache secondCache,
            out IndicatorCacheKey secondKey);

        Assert.True(preparedAgain);
        Assert.Same(scope.Cache, secondCache);
        Assert.Equal(0, parameter.GetStringToSaveCalls);
        Assert.Equal(firstKey, secondKey);
    }

    [Fact]
    public void Process_WithSharedOptimizerCache_ShouldHitOnSecondIndicator()
    {
        using OptimizerIndicatorCacheScope scope = new();
        List<Candle> candles = BuildCandles(8);
        OptimizerCacheTestIndicator first = CreateOptimizerIndicator("CodexAindicatorFirst");
        OptimizerCacheTestIndicator second = CreateOptimizerIndicator("CodexAindicatorSecond");

        first.Process(candles);

        IndicatorCacheStatistics afterFirst = scope.Cache.GetStatisticsSnapshot();
        Assert.Equal(candles.Count, first.OnProcessCalls);
        Assert.Equal(0, afterFirst.Hits);
        Assert.Equal(1, afterFirst.Misses);
        Assert.Equal(1, afterFirst.Writes);

        second.Process(candles);

        IndicatorCacheStatistics afterSecond = scope.Cache.GetStatisticsSnapshot();
        Assert.Equal(0, second.OnProcessCalls);
        Assert.Equal(first.DataSeries[0].Values, second.DataSeries[0].Values);
        Assert.Equal(1, afterSecond.Hits);
        Assert.Equal(1, afterSecond.Misses);
        Assert.Equal(1, afterSecond.Writes);
    }

    private static OptimizerCacheTestIndicator CreateOptimizerIndicator(string name)
    {
        OptimizerCacheTestIndicator indicator = new();
        indicator.Init(name, StartProgram.IsOsOptimizer);
        indicator.StartProgram = StartProgram.IsOsOptimizer;
        return indicator;
    }

    private static bool GetOptimizerParameterHashDirty(Aindicator indicator)
    {
        FieldInfo field = typeof(Aindicator).GetField("_optimizerParameterHashDirty", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Field _optimizerParameterHashDirty not found.");
        return (bool)(field.GetValue(indicator) ?? false);
    }

    private static string ExpectedParameterHash(string serializedValue)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(serializedValue);
            return hash.ToString("X8", CultureInfo.InvariantCulture);
        }
    }

    private static List<Candle> BuildCandles(int count)
    {
        List<Candle> candles = new(count);
        DateTime start = new(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        for (int i = 0; i < count; i++)
        {
            decimal basis = 100m + i;
            candles.Add(new Candle
            {
                TimeStart = start.AddMinutes(i),
                Open = basis,
                High = basis + 1m,
                Low = basis - 1m,
                Close = basis + 0.5m,
                Volume = 10m + i
            });
        }

        return candles;
    }

    private sealed class OptimizerIndicatorCacheScope : IDisposable
    {
        public OptimizerIndicatorCacheScope()
        {
            Cache = new IndicatorCache(maxEntries: 8);
            Aindicator.SetOptimizerIndicatorCache(Cache);
        }

        public IndicatorCache Cache { get; }

        public void Dispose()
        {
            Aindicator.ClearOptimizerIndicatorCache();
        }
    }

    private sealed class OptimizerCacheTestIndicator : Aindicator
    {
        public CountingIndicatorParameter CountingParameterInstance { get; private set; } = null!;

        public int OnProcessCalls { get; private set; }

        public override void OnStateChange(IndicatorState state)
        {
            if (state != IndicatorState.Configure)
            {
                return;
            }

            CountingParameterInstance = new CountingIndicatorParameter("Length#14#");
            AttachParameter(CountingParameterInstance);
            CreateSeries("Main", Color.Red, IndicatorChartPaintType.Line, true);
        }

        public override void OnProcess(List<Candle> source, int index)
        {
            OnProcessCalls++;
            DataSeries[0].Values[index] = source[index].Close;
        }

        private void AttachParameter(IndicatorParameter parameter)
        {
            FieldInfo field = typeof(Aindicator).GetField("_parameters", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("Field _parameters not found.");
            List<IndicatorParameter> parameters = (List<IndicatorParameter>)(field.GetValue(this)
                ?? throw new InvalidOperationException("_parameters list is null."));

            MethodInfo valueChangeHandler = typeof(Aindicator).GetMethod("Parameter_ValueChange", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("Method Parameter_ValueChange not found.");

            parameter.ValueChange += (Action)Delegate.CreateDelegate(typeof(Action), this, valueChangeHandler);
            parameters.Add(parameter);
        }
    }

    private sealed class CountingIndicatorParameter : IndicatorParameter
    {
        public CountingIndicatorParameter(string serializedValue)
        {
            SerializedValue = serializedValue;
        }

        public string SerializedValue { get; private set; }

        public int GetStringToSaveCalls { get; private set; }

        public void ResetCounter()
        {
            GetStringToSaveCalls = 0;
        }

        public void SetSerializedValue(string serializedValue)
        {
            if (string.Equals(SerializedValue, serializedValue, StringComparison.Ordinal))
            {
                return;
            }

            SerializedValue = serializedValue;
            ValueChange?.Invoke();
        }

        public override string Name => "Counting";

        public override string GetStringToSave()
        {
            GetStringToSaveCalls++;
            return SerializedValue;
        }

        public override void LoadParamFromString(string[] save)
        {
            SerializedValue = string.Join("#", save);
        }

        public override IndicatorParameterType Type => IndicatorParameterType.String;

        public override event Action? ValueChange;
    }
}
