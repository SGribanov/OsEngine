#nullable enable

using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsOptimizer.OptEntity;
using System.Reflection;
using Xunit;

namespace OsEngine.Tests;

public class BotPanelOptimizerMethodHashTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(-7)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void BuildOptimizerMethodCacheParameterHash_IntOverload_ShouldMatchParamsPath(int value)
    {
        string fastPath = BotPanelOptimizerMethodHashAccessor.BuildInt(value);
        string paramsPath = BotPanelOptimizerMethodHashAccessor.BuildParams(value);

        Assert.Equal(paramsPath, fastPath);
        Assert.Equal(8, fastPath.Length);
    }

    [Fact]
    public void BuildOptimizerMethodCacheParameterHash_EmptyInput_ShouldReturnStableSeedHash()
    {
        string hash = BotPanelOptimizerMethodHashAccessor.BuildParams();
        Assert.Equal("00000011", hash);
    }

    [Fact]
    public void BuildOptimizerMethodCacheParameterHash_NullInput_ShouldReturnStableHash()
    {
        string first = BotPanelOptimizerMethodHashAccessor.BuildParams((object?)null);
        string second = BotPanelOptimizerMethodHashAccessor.BuildParams((object?)null);

        Assert.Equal(first, second);
        Assert.Equal(8, first.Length);
    }

    [Fact]
    public void BuildOptimizerMethodCacheParameterHash_IntOverload_ShouldReuseCachedStringInstance()
    {
        string first = BotPanelOptimizerMethodHashAccessor.BuildInt(42);
        string second = BotPanelOptimizerMethodHashAccessor.BuildInt(42);

        Assert.Same(first, second);
    }

    [Fact]
    public void BuildOptimizerMethodCacheParameterHash_IntOverload_FastCacheBoundaries_ShouldStayStable()
    {
        string fastBoundaryFirst = BotPanelOptimizerMethodHashAccessor.BuildInt(4095);
        string fastBoundarySecond = BotPanelOptimizerMethodHashAccessor.BuildInt(4095);
        string fallbackBoundaryFirst = BotPanelOptimizerMethodHashAccessor.BuildInt(4096);
        string fallbackBoundarySecond = BotPanelOptimizerMethodHashAccessor.BuildInt(4096);

        Assert.Equal(BotPanelOptimizerMethodHashAccessor.BuildParams(4095), fastBoundaryFirst);
        Assert.Equal(BotPanelOptimizerMethodHashAccessor.BuildParams(4096), fallbackBoundaryFirst);
        Assert.Same(fastBoundaryFirst, fastBoundarySecond);
        Assert.Same(fallbackBoundaryFirst, fallbackBoundarySecond);
    }

    [Fact]
    public void OptimizerMethodKeyParametersHashHasher_SameContentDifferentInstance_ShouldRetargetThreadStaticSource()
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
        Type botPanelType = typeof(BotPanel);
        MethodInfo method = botPanelType.GetMethod("GetOptimizerMethodKeyParametersHashHashed", flags)!;
        FieldInfo sourceField = botPanelType.GetField("_optimizerMethodKeyParametersHashSource", flags)!;
        FieldInfo hashedField = botPanelType.GetField("_optimizerMethodKeyParametersHashHashed", flags)!;

        sourceField.SetValue(null, null);
        hashedField.SetValue(null, default(OrdinalHashedString));

        string first = new string("A1B2C3D4".ToCharArray());
        string second = new string("A1B2C3D4".ToCharArray());

        OrdinalHashedString firstHashed = (OrdinalHashedString)method.Invoke(null, [first])!;
        OrdinalHashedString secondHashed = (OrdinalHashedString)method.Invoke(null, [second])!;

        Assert.NotSame(first, second);
        Assert.Equal(firstHashed, secondHashed);
        Assert.Same(second, sourceField.GetValue(null));
    }

    private sealed class BotPanelOptimizerMethodHashAccessor : BotPanel
    {
        private BotPanelOptimizerMethodHashAccessor() : base("BotPanelOptimizerMethodHashAccessor", StartProgram.IsTester)
        {
        }

        internal static string BuildInt(int value)
        {
            return BuildOptimizerMethodCacheParameterHash(value);
        }

        internal static string BuildParams(params object?[] parts)
        {
            object[] normalizedParts = new object[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                normalizedParts[i] = parts[i]!;
            }

            return BuildOptimizerMethodCacheParameterHash(normalizedParts);
        }
    }
}
