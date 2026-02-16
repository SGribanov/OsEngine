using System;
using System.IO;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class SettingsManagerTests
{
    [Fact]
    public void SaveAndLoad_ShouldRoundTripJson()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-settings-" + Guid.NewGuid());
        Directory.CreateDirectory(root);

        try
        {
            string path = Path.Combine(root, "settings.json");
            TestSettings source = new TestSettings { Name = "alpha", Value = 42 };

            SettingsManager.Save(path, source);
            TestSettings loaded = SettingsManager.Load(path, new TestSettings());

            Assert.Equal("alpha", loaded.Name);
            Assert.Equal(42, loaded.Value);
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
    public void Load_InvalidJson_ShouldUseLegacyLoader()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-settings-" + Guid.NewGuid());
        Directory.CreateDirectory(root);

        try
        {
            string path = Path.Combine(root, "legacy.txt");
            File.WriteAllText(path, "name=beta;value=7");

            TestSettings loaded = SettingsManager.Load(
                path,
                new TestSettings(),
                legacyLoader: LegacyParser);

            Assert.Equal("beta", loaded.Name);
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
        string path = Path.Combine(Path.GetTempPath(), "osengine-settings-missing-" + Guid.NewGuid(), "none.json");
        TestSettings fallback = new TestSettings { Name = "fallback", Value = 1 };

        TestSettings loaded = SettingsManager.Load(path, fallback);

        Assert.Equal("fallback", loaded.Name);
        Assert.Equal(1, loaded.Value);
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
