#nullable enable

using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
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
