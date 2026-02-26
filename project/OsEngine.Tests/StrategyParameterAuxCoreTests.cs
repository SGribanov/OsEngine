#nullable enable

using System;
using System.Windows.Forms;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class StrategyParameterAuxCoreTests
{
    [Fact]
    public void TimeOfDay_ComparisonOperators_ShouldBehaveAsExpected()
    {
        TimeOfDay left = new TimeOfDay { Hour = 10, Minute = 15, Second = 20, Millisecond = 300 };
        TimeOfDay right = new TimeOfDay { Hour = 10, Minute = 15, Second = 20, Millisecond = 200 };
        DateTime dt = new DateTime(2024, 1, 1, 10, 15, 20, 250);

        Assert.True(left > right);
        Assert.True(right < left);
        Assert.True(left != right);
        Assert.False(left == right);

        Assert.True(left > dt);
        Assert.True(right < dt);
        Assert.True(right != dt);
        Assert.False(left == dt);
    }

    [Fact]
    public void StrategyParameterTimeOfDay_ArithmeticOperators_ShouldUseTimeSpan()
    {
        StrategyParameterTimeOfDay a = new StrategyParameterTimeOfDay("A", 1, 30, 0, 0);
        StrategyParameterTimeOfDay b = new StrategyParameterTimeOfDay("B", 0, 45, 0, 0);

        TimeSpan sum = a + b;
        TimeSpan diff = a - b;
        double ratio = a / b;

        Assert.Equal(new TimeSpan(1, 30, 0), a.TimeSpan);
        Assert.Equal(new TimeSpan(0, 45, 0), b.TimeSpan);
        Assert.Equal(new TimeSpan(2, 15, 0), sum);
        Assert.Equal(new TimeSpan(0, 45, 0), diff);
        Assert.True(ratio > 1.99 && ratio < 2.01);
    }

    [Fact]
    public void StrategyParameterButton_Click_ShouldRaiseEvent()
    {
        StrategyParameterButton button = new StrategyParameterButton("Run");
        int clicks = 0;
        button.UserClickOnButtonEvent += () => clicks++;

        button.Click();
        button.Click();

        Assert.Equal(2, clicks);
        Assert.StartsWith("Run#", button.GetStringToSave(), StringComparison.Ordinal);
        Assert.Equal(StrategyParameterType.Button, button.Type);
    }

    [Fact]
    public void StrategyParameterCheckBox_ShouldRoundTripAndRaiseValueChange()
    {
        StrategyParameterCheckBox parameter = new StrategyParameterCheckBox("UseRisk", true);
        int changes = 0;
        parameter.ValueChange += () => changes++;

        parameter.CheckState = CheckState.Checked;
        parameter.CheckState = CheckState.Unchecked;

        Assert.Equal(1, changes);

        string[] saved = parameter.GetStringToSave().Split('#');
        StrategyParameterCheckBox loaded = new StrategyParameterCheckBox("UseRisk", false);
        loaded.LoadParamFromString(saved);

        Assert.Equal(CheckState.Unchecked, loaded.CheckState);
        Assert.Equal(StrategyParameterType.CheckBox, loaded.Type);
    }
}
