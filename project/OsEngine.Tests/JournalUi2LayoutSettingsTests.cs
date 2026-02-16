using System;
using System.Reflection;
using OsEngine.Journal;
using Xunit;

namespace OsEngine.Tests;

public class JournalUi2LayoutSettingsTests
{
    [Fact]
    public void ParseLegacyLayoutSettings_ShouldSupportLineBasedFormat()
    {
        MethodInfo parseMethod = typeof(JournalUi2).GetMethod(
            "ParseLegacyLayoutSettings",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method ParseLegacyLayoutSettings not found.");

        object settings = parseMethod.Invoke(
            null,
            new object[] { "True\nAbsolute\nFalse\nTrue\nFalse\nBTC\n" })!;
        Assert.NotNull(settings);

        Assert.True(GetBool(settings, "LeftPanelIsHide"));
        Assert.Equal("Absolute", GetString(settings, "ProfitType"));
        Assert.False(GetBool(settings, "VisibleEquityLine"));
        Assert.True(GetBool(settings, "VisibleLongLine"));
        Assert.False(GetBool(settings, "VisibleShortLine"));
        Assert.Equal("BTC", GetString(settings, "Benchmark"));
    }

    private static bool GetBool(object settings, string propertyName)
    {
        PropertyInfo property = settings.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property {propertyName} not found.");
        return (bool)property.GetValue(settings)!;
    }

    private static string GetString(object settings, string propertyName)
    {
        PropertyInfo property = settings.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property {propertyName} not found.");
        return (string)property.GetValue(settings)!;
    }
}
