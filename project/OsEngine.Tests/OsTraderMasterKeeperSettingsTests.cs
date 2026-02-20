#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OsEngine.OsTrader;
using Xunit;

namespace OsEngine.Tests;

public class OsTraderMasterKeeperSettingsTests
{
    [Fact]
    public void ParseLegacyBotKeeperSettings_ShouldSupportLineBasedFormat()
    {
        MethodInfo parseMethod = typeof(OsTraderMaster).GetMethod(
            "ParseLegacyBotKeeperSettings",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method ParseLegacyBotKeeperSettings not found.");

        object settings = parseMethod.Invoke(null, new object[] { "BotA@TypeA@False@NameA\nBotB@TypeB@True@NameB\n" })!;
        Assert.NotNull(settings);

        PropertyInfo botSettingsProperty = settings.GetType().GetProperty("BotSettings", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Property BotSettings not found.");

        IEnumerable<string> values = (IEnumerable<string>)botSettingsProperty.GetValue(settings)!;
        List<string> list = values.ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal("BotA@TypeA@False@NameA", list[0]);
        Assert.Equal("BotB@TypeB@True@NameB", list[1]);
    }
}
