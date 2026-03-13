#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class NumberGenPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistToml_AndLoadRoundTrip()
    {
        using StructuredSettingsFileScope scope = new StructuredSettingsFileScope(Path.Combine("Engine", "NumberGen.toml"));

        SetStaticField("_numberDealForRealTrading", 123);
        SetStaticField("_numberOrderForRealTrading", 456);
        InvokePrivate("Save");

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("NumberDealForRealTrading = 123", content);

        SetStaticField("_numberDealForRealTrading", 0);
        SetStaticField("_numberOrderForRealTrading", 0);
        InvokePrivate("Load");

        Assert.Equal(123, (int)GetStaticField("_numberDealForRealTrading"));
        Assert.Equal(456, (int)GetStaticField("_numberOrderForRealTrading"));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        using StructuredSettingsFileScope scope = new StructuredSettingsFileScope(Path.Combine("Engine", "NumberGen.toml"));
        File.WriteAllLines(scope.LegacyTxtPath, new[] { "7", "8" });

        SetStaticField("_numberDealForRealTrading", 0);
        SetStaticField("_numberOrderForRealTrading", 0);
        InvokePrivate("Load");

        Assert.Equal(7, (int)GetStaticField("_numberDealForRealTrading"));
        Assert.Equal(8, (int)GetStaticField("_numberOrderForRealTrading"));

        InvokePrivate("Save");
        Assert.True(File.Exists(scope.CanonicalPath));
        Assert.Contains("NumberOrderForRealTrading = 8", File.ReadAllText(scope.CanonicalPath));
    }

    private static void InvokePrivate(string methodName)
    {
        MethodInfo method = typeof(NumberGen).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method not found: " + methodName);
        method.Invoke(null, null);
    }

    private static object GetStaticField(string fieldName)
    {
        FieldInfo field = typeof(NumberGen).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Field not found: " + fieldName);
        return field.GetValue(null)!;
    }

    private static void SetStaticField(string fieldName, object value)
    {
        FieldInfo field = typeof(NumberGen).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Field not found: " + fieldName);
        field.SetValue(null, value);
    }
}
