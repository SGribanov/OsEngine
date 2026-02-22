#nullable enable

using System.Globalization;
using OsEngine.Market.Servers.Entity;
using Xunit;

namespace OsEngine.Tests;

public class ServerParameterPersistenceTests
{
    [Fact]
    public void ServerParameterDecimal_LoadFromStr_ShouldParseInvariantDecimal()
    {
        ServerParameterDecimal parameter = new ServerParameterDecimal();

        parameter.LoadFromStr("Decimal^Leverage^1234.56");

        Assert.Equal("Leverage", parameter.Name);
        Assert.Equal(1234.56m, parameter.Value);
    }

    [Fact]
    public void ServerParameterDecimal_LoadFromStr_ShouldParseCommaDecimal_OnNonRuCurrentCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo testCulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = testCulture;
            CultureInfo.CurrentUICulture = testCulture;

            ServerParameterDecimal parameter = new ServerParameterDecimal();
            parameter.LoadFromStr("Decimal^Leverage^1234,56");

            Assert.Equal(1234.56m, parameter.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}
