#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.ColorKeeper;
using Xunit;

namespace OsEngine.Tests;

public class ChartMasterColorKeeperPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistToml_AndLoadRoundTrip()
    {
        const string name = "CodexColorKeeperJson";
        using ChartMasterColorKeeperFileScope scope = new ChartMasterColorKeeperFileScope(name);

        ChartMasterColorKeeper source = new ChartMasterColorKeeper(name);
        source.ColorUpBodyCandle = Color.Red;
        source.ColorUpBorderCandle = Color.Green;
        source.ColorDownBodyCandle = Color.Blue;
        source.ColorDownBorderCandle = Color.Yellow;
        source.ColorBackSecond = Color.Black;
        source.ColorBackChart = Color.White;
        source.ColorBackCursor = Color.Purple;
        source.ColorText = Color.Orange;
        source.PointType = PointType.TriAngle;
        source.Save();

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("PointType = 3", content);

        ChartMasterColorKeeper loaded = new ChartMasterColorKeeper(name);
        Assert.Equal(Color.Red.ToArgb(), loaded.ColorUpBodyCandle.ToArgb());
        Assert.Equal(Color.Green.ToArgb(), loaded.ColorUpBorderCandle.ToArgb());
        Assert.Equal(Color.Blue.ToArgb(), loaded.ColorDownBodyCandle.ToArgb());
        Assert.Equal(Color.Yellow.ToArgb(), loaded.ColorDownBorderCandle.ToArgb());
        Assert.Equal(Color.Black.ToArgb(), loaded.ColorBackSecond.ToArgb());
        Assert.Equal(Color.White.ToArgb(), loaded.ColorBackChart.ToArgb());
        Assert.Equal(Color.Purple.ToArgb(), loaded.ColorBackCursor.ToArgb());
        Assert.Equal(Color.Orange.ToArgb(), loaded.ColorText.ToArgb());
        Assert.Equal(PointType.TriAngle, loaded.PointType);
        Assert.Equal(ChartColorScheme.Black, loaded.ColorScheme);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        const string name = "CodexColorKeeperLegacy";
        using ChartMasterColorKeeperFileScope scope = new ChartMasterColorKeeperFileScope(name);

        File.WriteAllLines(scope.LegacyTxtPath, new[]
        {
            Color.AliceBlue.ToArgb().ToString(),
            Color.Beige.ToArgb().ToString(),
            Color.Coral.ToArgb().ToString(),
            Color.Crimson.ToArgb().ToString(),
            Color.DarkBlue.ToArgb().ToString(),
            Color.DarkGoldenrod.ToArgb().ToString(),
            Color.DarkGray.ToArgb().ToString(),
            Color.DarkGreen.ToArgb().ToString(),
            "Circle",
            "White"
        });

        ChartMasterColorKeeper loaded = new ChartMasterColorKeeper(name);
        Assert.Equal(Color.AliceBlue.ToArgb(), loaded.ColorUpBodyCandle.ToArgb());
        Assert.Equal(Color.Beige.ToArgb(), loaded.ColorUpBorderCandle.ToArgb());
        Assert.Equal(Color.Coral.ToArgb(), loaded.ColorDownBodyCandle.ToArgb());
        Assert.Equal(Color.Crimson.ToArgb(), loaded.ColorDownBorderCandle.ToArgb());
        Assert.Equal(Color.DarkBlue.ToArgb(), loaded.ColorBackSecond.ToArgb());
        Assert.Equal(Color.DarkGoldenrod.ToArgb(), loaded.ColorBackChart.ToArgb());
        Assert.Equal(Color.DarkGray.ToArgb(), loaded.ColorBackCursor.ToArgb());
        Assert.Equal(Color.DarkGreen.ToArgb(), loaded.ColorText.ToArgb());
        Assert.Equal(PointType.Circle, loaded.PointType);
        Assert.Equal(ChartColorScheme.White, loaded.ColorScheme);

        loaded.Save();
        Assert.True(File.Exists(scope.CanonicalPath));
        Assert.Contains("ColorScheme = 1", File.ReadAllText(scope.CanonicalPath));
    }

    private sealed class ChartMasterColorKeeperFileScope : IDisposable
    {
        private readonly StructuredSettingsFileScope _settingsScope;

        public ChartMasterColorKeeperFileScope(string name)
        {
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", "Color", name + "Color.toml"));
        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

        public void Dispose()
        {
            _settingsScope.Dispose();
        }
    }
}
