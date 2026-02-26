#nullable enable

using System;
using System.Collections.Generic;
using OsEngine.Logging;
using OsEngine.OsOptimizer;
using OsEngine.OsOptimizer.OptEntity;
using Xunit;

namespace OsEngine.Tests;

public class PhaseCalculatorCoreTests
{
    [Fact]
    public void CalculatePhases_ShouldReturnNull_WhenDatesAreInvalid()
    {
        PhaseCalculator calculator = new PhaseCalculator();
        List<string> logs = new List<string>();
        calculator.LogMessageEvent += (m, t) => logs.Add($"{t}:{m}");

        List<OptimizerFaze>? result = calculator.CalculatePhases(
            timeStart: DateTime.MinValue,
            timeEnd: DateTime.UtcNow,
            iterationCount: 2,
            percentOnFiltration: 20m,
            lastInSample: false);

        Assert.Null(result);
        Assert.NotEmpty(logs);
    }

    [Fact]
    public void CalculatePhases_ShouldReturnExpectedSequence_WhenLastInSampleFalse()
    {
        PhaseCalculator calculator = new PhaseCalculator();

        DateTime start = new DateTime(2024, 1, 1);
        DateTime end = new DateTime(2024, 1, 31);

        List<OptimizerFaze>? phases = calculator.CalculatePhases(
            timeStart: start,
            timeEnd: end,
            iterationCount: 2,
            percentOnFiltration: 20m,
            lastInSample: false);

        Assert.NotNull(phases);
        Assert.Equal(4, phases!.Count);
        Assert.Equal(OptimizerFazeType.InSample, phases[0].TypeFaze);
        Assert.Equal(OptimizerFazeType.OutOfSample, phases[1].TypeFaze);
        Assert.Equal(OptimizerFazeType.InSample, phases[2].TypeFaze);
        Assert.Equal(OptimizerFazeType.OutOfSample, phases[3].TypeFaze);
        Assert.True(phases[0].Days > 0);
        Assert.True(phases[1].Days > 0);
    }

    [Fact]
    public void CalculatePhases_ShouldReturnOnlyInSample_WhenLastInSampleTrue_AndSingleIteration()
    {
        PhaseCalculator calculator = new PhaseCalculator();

        List<OptimizerFaze>? phases = calculator.CalculatePhases(
            timeStart: new DateTime(2024, 1, 1),
            timeEnd: new DateTime(2024, 2, 15),
            iterationCount: 1,
            percentOnFiltration: 30m,
            lastInSample: true);

        Assert.NotNull(phases);
        Assert.Single(phases!);
        Assert.Equal(OptimizerFazeType.InSample, phases[0].TypeFaze);
        Assert.True(phases[0].Days > 0);
    }

    [Fact]
    public void CalculatePhases_ShouldReturnEmpty_WhenForwardPercentIsZero()
    {
        PhaseCalculator calculator = new PhaseCalculator();
        List<(string Message, LogMessageType Type)> logs = new List<(string, LogMessageType)>();
        calculator.LogMessageEvent += (m, t) => logs.Add((m, t));

        List<OptimizerFaze>? phases = calculator.CalculatePhases(
            timeStart: new DateTime(2024, 1, 1),
            timeEnd: new DateTime(2024, 1, 31),
            iterationCount: 3,
            percentOnFiltration: 0m,
            lastInSample: false);

        Assert.NotNull(phases);
        Assert.Empty(phases!);
        Assert.Contains(logs, x => x.Type == LogMessageType.Error);
    }
}
