#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections;
using System.Reflection;
using OsEngine.OsOptimizer;
using Xunit;

namespace OsEngine.Tests;

public class OptimizerMasterPersistenceTests
{
    [Fact]
    public void ParseLegacyStandardParameters_ShouldSupportLineBasedFormat()
    {
        MethodInfo parseMethod = typeof(OptimizerMaster).GetMethod(
            "ParseLegacyStandardParameters",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method ParseLegacyStandardParameters not found.");

        object settings = parseMethod.Invoke(null, new object[] { "P1#1\nP2#2\n" })!;
        Assert.NotNull(settings);

        PropertyInfo parameterLinesProperty = settings.GetType().GetProperty("ParameterLines", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Property ParameterLines not found.");
        IList lines = (IList)parameterLinesProperty.GetValue(settings)!;

        Assert.Equal(2, lines.Count);
        Assert.Equal("P1#1", lines[0]);
        Assert.Equal("P2#2", lines[1]);
    }

    [Fact]
    public void ParseLegacyStandardParametersOnOff_ShouldSupportLineBasedFormat()
    {
        MethodInfo parseMethod = typeof(OptimizerMaster).GetMethod(
            "ParseLegacyStandardParametersOnOff",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method ParseLegacyStandardParametersOnOff not found.");

        object settings = parseMethod.Invoke(null, new object[] { "True\nFalse\nTrue\n" })!;
        Assert.NotNull(settings);

        PropertyInfo parametersOnProperty = settings.GetType().GetProperty("ParametersOn", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Property ParametersOn not found.");
        IList values = (IList)parametersOnProperty.GetValue(settings)!;

        Assert.Equal(3, values.Count);
        Assert.True((bool)values[0]!);
        Assert.False((bool)values[1]!);
        Assert.True((bool)values[2]!);
    }
}
