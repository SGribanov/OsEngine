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
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using NumberGenFileScope scope = new NumberGenFileScope();

        SetStaticField("_numberDealForRealTrading", 123);
        SetStaticField("_numberOrderForRealTrading", 456);
        InvokePrivate("Save");

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        SetStaticField("_numberDealForRealTrading", 0);
        SetStaticField("_numberOrderForRealTrading", 0);
        InvokePrivate("Load");

        Assert.Equal(123, (int)GetStaticField("_numberDealForRealTrading"));
        Assert.Equal(456, (int)GetStaticField("_numberOrderForRealTrading"));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using NumberGenFileScope scope = new NumberGenFileScope();
        File.WriteAllLines(scope.SettingsPath, new[] { "7", "8" });

        SetStaticField("_numberDealForRealTrading", 0);
        SetStaticField("_numberOrderForRealTrading", 0);
        InvokePrivate("Load");

        Assert.Equal(7, (int)GetStaticField("_numberDealForRealTrading"));
        Assert.Equal(8, (int)GetStaticField("_numberOrderForRealTrading"));
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

    private sealed class NumberGenFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;

        public NumberGenFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "NumberGen.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _settingsFileExisted = File.Exists(SettingsPath);
            if (_settingsFileExisted)
            {
                File.Copy(SettingsPath, _settingsBackup, overwrite: true);
            }
            else if (File.Exists(_settingsBackup))
            {
                File.Delete(_settingsBackup);
            }
        }

        public string SettingsPath { get; }

        public void Dispose()
        {
            if (_settingsFileExisted)
            {
                if (File.Exists(_settingsBackup))
                {
                    File.Copy(_settingsBackup, SettingsPath, overwrite: true);
                    File.Delete(_settingsBackup);
                }
            }
            else
            {
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }

                if (File.Exists(_settingsBackup))
                {
                    File.Delete(_settingsBackup);
                }
            }

            if (!_engineDirExisted
                && Directory.Exists(_engineDirPath)
                && !Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
            {
                Directory.Delete(_engineDirPath);
            }
        }
    }
}
