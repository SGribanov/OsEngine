#nullable enable

using System;
using OsEngine.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class IndicatorParameterCoreTests
{
    [Fact]
    public void DoDefault_ShouldRestoreDefaultValue_ForIntParameter()
    {
        IndicatorParameterInt parameter = new IndicatorParameterInt("Length", 10);
        parameter.ValueInt = 42;

        parameter.DoDefault();

        Assert.Equal(10, parameter.ValueInt);
    }

    [Fact]
    public void Bind_ShouldSynchronizeValues_ForSameType()
    {
        IndicatorParameterInt left = new IndicatorParameterInt("A", 1);
        IndicatorParameterInt right = new IndicatorParameterInt("B", 5);

        left.Bind(right);

        Assert.Equal(5, left.ValueInt);

        right.ValueInt = 7;
        Assert.Equal(7, left.ValueInt);

        left.ValueInt = 9;
        Assert.Equal(9, right.ValueInt);
    }

    [Fact]
    public void Bind_ShouldThrow_ForDifferentTypes()
    {
        IndicatorParameterInt left = new IndicatorParameterInt("A", 1);
        IndicatorParameterString right = new IndicatorParameterString("B", "x");

        Assert.Throws<Exception>(() => left.Bind(right));
    }

    [Fact]
    public void ValueChange_ShouldFireOnlyWhenValueActuallyChanged()
    {
        IndicatorParameterBool parameter = new IndicatorParameterBool("UseFilter", false);
        int changeCount = 0;
        parameter.ValueChange += () => changeCount++;

        parameter.ValueBool = false;
        parameter.ValueBool = true;
        parameter.ValueBool = true;
        parameter.ValueBool = false;

        Assert.Equal(2, changeCount);
    }
}
