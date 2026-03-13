#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class HorizontalVolumePersistenceTests
{
    [Fact]
    public void Save_ShouldPersistToml_AndLoadRoundTrip()
    {
        string name = "codex_hv_json_" + Guid.NewGuid().ToString("N");

        using HorizontalVolumeFileScope scope = new HorizontalVolumeFileScope(name);

        HorizontalVolume source = new HorizontalVolume(name);
        source.StepLine = 0.25m;

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("LineStep = 0.25", content);

        HorizontalVolume loaded = new HorizontalVolume(name);
        Assert.Equal(0.25m, loaded.StepLine);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        string name = "codex_hv_legacy_" + Guid.NewGuid().ToString("N");

        using HorizontalVolumeFileScope scope = new HorizontalVolumeFileScope(name);

        File.WriteAllText(scope.LegacyTxtPath, "0.75");

        HorizontalVolume loaded = new HorizontalVolume(name);
        Assert.Equal(0.75m, loaded.StepLine);

        loaded.Save();
        Assert.True(File.Exists(scope.CanonicalPath));
        Assert.Contains("LineStep = 0.75", File.ReadAllText(scope.CanonicalPath));
    }

    private sealed class HorizontalVolumeFileScope : IDisposable
    {
        private readonly StructuredSettingsFileScope _settingsScope;

        public HorizontalVolumeFileScope(string name)
        {
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", name + "HorizontalVolumeSet.toml"));
        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

        public void Dispose()
        {
            _settingsScope.Dispose();
        }
    }
}
