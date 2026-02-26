#nullable enable

using System;
using System.Globalization;
using System.Windows.Forms;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class StrategyParameterNumericTimeCoreTests
{
    [Fact]
    public void StrategyParameterInt_ShouldRaiseValueChange_AndRoundTripSaveLoad()
    {
        StrategyParameterInt source = new StrategyParameterInt("Len", 10, 1, 20, 1);
        int changes = 0;
        source.ValueChange += () => changes++;

        source.ValueInt = 10;
        source.ValueInt = 11;
        source.ValueInt = 12;

        Assert.Equal(2, changes);

        string[] saved = source.GetStringToSave().Split('#');
        StrategyParameterInt loaded = new StrategyParameterInt("Len", 1, 1, 1, 1);
        loaded.LoadParamFromString(saved);

        Assert.Equal(12, loaded.ValueInt);
        Assert.Equal(10, loaded.ValueIntDefolt);
        Assert.Equal(1, loaded.ValueIntStart);
        Assert.Equal(20, loaded.ValueIntStop);
        Assert.Equal(1, loaded.ValueIntStep);
    }

    [Fact]
    public void StrategyParameterDecimal_ShouldRaiseValueChange_AndRoundTripSaveLoad()
    {
        StrategyParameterDecimal source = new StrategyParameterDecimal("Step", 0.5m, 0.1m, 1.0m, 0.1m);
        int changes = 0;
        source.ValueChange += () => changes++;

        source.ValueDecimal = 0.5m;
        source.ValueDecimal = 0.7m;

        Assert.Equal(1, changes);

        string[] saved = source.GetStringToSave().Split('#');
        StrategyParameterDecimal loaded = new StrategyParameterDecimal("Step", 1m, 1m, 2m, 1m);
        loaded.LoadParamFromString(saved);

        Assert.Equal(0.7m, loaded.ValueDecimal);
        Assert.Equal(0.5m, loaded.ValueDecimalDefolt);
        Assert.Equal(0.1m, loaded.ValueDecimalStart);
        Assert.Equal(1.0m, loaded.ValueDecimalStop);
        Assert.Equal(0.1m, loaded.ValueDecimalStep);
    }

    [Fact]
    public void StrategyParameterBool_ShouldRaiseValueChange_AndRoundTripSaveLoad()
    {
        StrategyParameterBool source = new StrategyParameterBool("Enabled", false);
        int changes = 0;
        source.ValueChange += () => changes++;

        source.ValueBool = false;
        source.ValueBool = true;
        source.ValueBool = false;

        Assert.Equal(2, changes);

        string[] saved = source.GetStringToSave().Split('#');
        StrategyParameterBool loaded = new StrategyParameterBool("Enabled", true);
        loaded.LoadParamFromString(saved);

        Assert.False(loaded.ValueBool);
        Assert.False(loaded.ValueBoolDefolt);
    }

    [Fact]
    public void TimeOfDay_LoadFromString_ShouldReportChangeOnlyWhenNeeded()
    {
        TimeOfDay value = new TimeOfDay
        {
            Hour = 10,
            Minute = 15,
            Second = 20,
            Millisecond = 30
        };

        bool changed = value.LoadFromString("10:15:20:30");
        bool changedSecond = value.LoadFromString("11:15:20:30");

        Assert.False(changed);
        Assert.True(changedSecond);
        Assert.Equal(11, value.Hour);
        Assert.Equal("11:15:20:30", value.ToString());
    }

    [Fact]
    public void StrategyParameterTimeOfDay_ShouldRoundTripAndSupportComparisons()
    {
        StrategyParameterTimeOfDay parameter = new StrategyParameterTimeOfDay("Start", 9, 30, 0, 0);

        DateTime now = new DateTime(2024, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified);
        Assert.True(parameter < now);
        Assert.False(parameter > now);

        string[] saved = parameter.GetStringToSave().Split('#');
        StrategyParameterTimeOfDay loaded = new StrategyParameterTimeOfDay("Start", 0, 0, 0, 0);
        int changes = 0;
        loaded.ValueChange += () => changes++;
        loaded.LoadParamFromString(saved);

        Assert.Equal(1, changes);
        Assert.Equal(new TimeSpan(9, 30, 0), loaded.TimeSpan);
    }

    [Fact]
    public void StrategyParameterDecimalCheckBox_ShouldRoundTripAndRaiseValueChange()
    {
        StrategyParameterDecimalCheckBox parameter = new StrategyParameterDecimalCheckBox(
            name: "Risk",
            value: 1.5m,
            start: 1.0m,
            stop: 3.0m,
            step: 0.5m,
            isChecked: true);

        int changes = 0;
        parameter.ValueChange += () => changes++;

        parameter.ValueDecimal = 2.0m;
        parameter.CheckState = CheckState.Unchecked;

        Assert.Equal(2, changes);

        string[] saved = parameter.GetStringToSave().Split('#');
        StrategyParameterDecimalCheckBox loaded = new StrategyParameterDecimalCheckBox(
            name: "Risk",
            value: 0m,
            start: 0m,
            stop: 1m,
            step: 1m,
            isChecked: false);
        loaded.LoadParamFromString(saved);

        Assert.Equal(2.0m, loaded.ValueDecimal);
        Assert.Equal(1.5m, loaded.ValueDecimalDefolt);
        Assert.Equal(1.0m, loaded.ValueDecimalStart);
        Assert.Equal(3.0m, loaded.ValueDecimalStop);
        Assert.Equal(0.5m, loaded.ValueDecimalStep);
        Assert.Equal(CheckState.Unchecked, loaded.CheckState);
    }
}
