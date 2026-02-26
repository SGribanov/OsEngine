#nullable enable

using System;
using System.Collections.Generic;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class StrategyParameterStringCoreTests
{
    [Fact]
    public void Constructor_WithCollection_ShouldAddDefaultValue_WhenMissing()
    {
        List<string> values = new List<string> { "A", "B" };

        StrategyParameterString parameter = new StrategyParameterString(
            name: "Mode",
            value: "C",
            collection: values);

        Assert.Contains("C", parameter.ValuesString);
        Assert.Equal("C", parameter.ValueString);
        Assert.Equal(StrategyParameterType.String, parameter.Type);
    }

    [Fact]
    public void Constructor_WithNullValue_ShouldNormalizeToEmptyString()
    {
        StrategyParameterString parameter = new StrategyParameterString("Mode", null!);

        Assert.Equal(string.Empty, parameter.ValueString);
        Assert.Single(parameter.ValuesString);
        Assert.Equal(string.Empty, parameter.ValuesString[0]);
    }

    [Fact]
    public void ValueString_ShouldRaiseValueChange_OnlyWhenChanged()
    {
        StrategyParameterString parameter = new StrategyParameterString("Mode", "A");
        int changes = 0;
        parameter.ValueChange += () => changes++;

        parameter.ValueString = "A";
        parameter.ValueString = "B";
        parameter.ValueString = "B";
        parameter.ValueString = "C";

        Assert.Equal(2, changes);
    }

    [Fact]
    public void SaveAndLoad_ShouldRoundTripValue_AndSkipEmptyCollectionItems()
    {
        StrategyParameterString source = new StrategyParameterString(
            name: "Mode",
            value: "Fast",
            collection: new List<string> { "Fast", "Slow" });

        string save = source.GetStringToSave();
        string[] parts = save.Split('#');

        // emulate legacy tail with empty entries
        string[] withEmpty = new[] { parts[0], parts[1], parts[2], "", "Turbo", "" };

        StrategyParameterString loaded = new StrategyParameterString("X", "Y");
        loaded.LoadParamFromString(withEmpty);

        Assert.Equal("Fast", loaded.ValueString);
        Assert.Contains("Fast", loaded.ValuesString);
        Assert.Contains("Turbo", loaded.ValuesString);
        Assert.DoesNotContain(string.Empty, loaded.ValuesString);
    }
}
