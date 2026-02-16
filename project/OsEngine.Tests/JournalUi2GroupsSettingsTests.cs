using System;
using System.Collections;
using System.Reflection;
using OsEngine.Journal;
using Xunit;

namespace OsEngine.Tests;

public class JournalUi2GroupsSettingsTests
{
    [Fact]
    public void ParseLegacyJournalGroupsSettings_ShouldSupportLineBasedFormat()
    {
        MethodInfo parseMethod = typeof(JournalUi2).GetMethod(
            "ParseLegacyJournalGroupsSettings",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method ParseLegacyJournalGroupsSettings not found.");

        object settings = parseMethod.Invoke(
            null,
            new object[] { "BotA&Group1&1.5&True\nBotB&Group2&2.0&False\n" })!;
        Assert.NotNull(settings);

        PropertyInfo groupsProperty = settings.GetType().GetProperty("Groups", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Property Groups not found.");

        IList groups = (IList)groupsProperty.GetValue(settings)!;
        Assert.Equal(2, groups.Count);

        object first = groups[0]!;
        Assert.Equal("BotA", GetString(first, "BotName"));
        Assert.Equal("Group1", GetString(first, "BotGroup"));
        Assert.Equal(1.5m, GetDecimal(first, "Mult"));
        Assert.True(GetBool(first, "IsOn"));

        object second = groups[1]!;
        Assert.Equal("BotB", GetString(second, "BotName"));
        Assert.Equal("Group2", GetString(second, "BotGroup"));
        Assert.Equal(2.0m, GetDecimal(second, "Mult"));
        Assert.False(GetBool(second, "IsOn"));
    }

    private static string GetString(object item, string propertyName)
    {
        PropertyInfo property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property {propertyName} not found.");
        return (string)property.GetValue(item)!;
    }

    private static decimal GetDecimal(object item, string propertyName)
    {
        PropertyInfo property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property {propertyName} not found.");
        return (decimal)property.GetValue(item)!;
    }

    private static bool GetBool(object item, string propertyName)
    {
        PropertyInfo property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property {propertyName} not found.");
        return (bool)property.GetValue(item)!;
    }
}
