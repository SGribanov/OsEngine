#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Text.Json;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class SettingsManagerTests
{
    [Fact]
    public void SaveAndLoad_ShouldRoundTripToml_WhenTomlPathRequested()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-settings-" + Guid.NewGuid());
        Directory.CreateDirectory(root);

        try
        {
            string path = Path.Combine(root, "settings.toml");
            TestSettings source = new TestSettings { Name = "alpha", Value = 42 };

            SettingsManager.Save(path, source);
            TestSettings? loaded = SettingsManager.Load(path, new TestSettings());
            string content = File.ReadAllText(path);

            Assert.NotNull(loaded);
            Assert.Equal("alpha", loaded!.Name);
            Assert.Equal(42, loaded.Value);
            Assert.Contains("Name = \"alpha\"", content);
            Assert.Contains("Value = 42", content);
            Assert.True(File.Exists(path + ".bak") == false || File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void Load_ShouldUseLegacyJsonCompanion_WhenTomlPathMissing()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-settings-" + Guid.NewGuid());
        Directory.CreateDirectory(root);

        try
        {
            string path = Path.Combine(root, "settings.toml");
            string legacyJsonPath = Path.Combine(root, "settings.json");
            string legacyJson = JsonSerializer.Serialize(new TestSettings { Name = "beta", Value = 7 });
            File.WriteAllText(legacyJsonPath, legacyJson);

            TestSettings? loaded = SettingsManager.Load(path, new TestSettings());

            Assert.NotNull(loaded);
            Assert.Equal("beta", loaded!.Name);
            Assert.Equal(7, loaded.Value);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void Load_InvalidJson_ShouldUseLegacyLoader()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-settings-" + Guid.NewGuid());
        Directory.CreateDirectory(root);

        try
        {
            string path = Path.Combine(root, "legacy.txt");
            File.WriteAllText(path, "name=beta;value=7");

            TestSettings? loaded = SettingsManager.Load(
                path,
                new TestSettings(),
                legacyLoader: LegacyParser);

            Assert.NotNull(loaded);
            Assert.Equal("beta", loaded!.Name);
            Assert.Equal(7, loaded.Value);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void Load_MissingFile_ShouldReturnDefaultValue()
    {
        string path = Path.Combine(Path.GetTempPath(), "osengine-settings-missing-" + Guid.NewGuid(), "none.toml");
        TestSettings fallback = new TestSettings { Name = "fallback", Value = 1 };

        TestSettings? loaded = SettingsManager.Load(path, fallback);

        Assert.NotNull(loaded);
        Assert.Equal("fallback", loaded!.Name);
        Assert.Equal(1, loaded.Value);
    }

    [Fact]
    public void Save_ShouldKeepAtomicBackup_ForTomlWrites()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-settings-" + Guid.NewGuid());
        Directory.CreateDirectory(root);

        try
        {
            string path = Path.Combine(root, "settings.toml");

            SettingsManager.Save(path, new TestSettings { Name = "first", Value = 1 });
            SettingsManager.Save(path, new TestSettings { Name = "second", Value = 2 });

            string current = File.ReadAllText(path);
            string backupPath = path + ".bak";

            Assert.True(File.Exists(backupPath));
            Assert.Contains("Name = \"second\"", current);
            Assert.Contains("Value = 2", current);

            string backup = File.ReadAllText(backupPath);
            Assert.Contains("Name = \"first\"", backup);
            Assert.Contains("Value = 1", backup);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private static TestSettings LegacyParser(string content)
    {
        string[] parts = content.Split(';');
        string name = parts[0].Split('=')[1];
        int value = int.Parse(parts[1].Split('=')[1]);

        return new TestSettings { Name = name, Value = value };
    }

    private sealed class TestSettings
    {
        public string Name { get; set; } = string.Empty;

        public int Value { get; set; }
    }
}
