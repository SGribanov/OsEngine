using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class IchimokuPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexIchimokuJson";
        using IchimokuFileScope scope = new IchimokuFileScope(name);

        Ichimoku source = new Ichimoku(name, canDelete: true)
        {
            ColorEtalonLine = Color.SteelBlue,
            ColorLineRounded = Color.Tomato,
            ColorLineLate = Color.Goldenrod,
            ColorLineFirst = Color.SeaGreen,
            ColorLineSecond = Color.SlateBlue,
            PaintOn = false
        };
        source.LengthFirst = 11;
        source.LengthSecond = 33;
        source.LengthFird = 55;
        source.LengthSdvig = 24;
        source.LengthChinkou = 30;
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        Ichimoku loaded = new Ichimoku(name, canDelete: true);

        Assert.Equal(11, loaded.LengthFirst);
        Assert.Equal(33, loaded.LengthSecond);
        Assert.Equal(55, loaded.LengthFird);
        Assert.Equal(24, loaded.LengthSdvig);
        Assert.Equal(30, loaded.LengthChinkou);
        Assert.Equal(Color.SteelBlue.ToArgb(), loaded.ColorEtalonLine.ToArgb());
        Assert.Equal(Color.Tomato.ToArgb(), loaded.ColorLineRounded.ToArgb());
        Assert.Equal(Color.Goldenrod.ToArgb(), loaded.ColorLineLate.ToArgb());
        Assert.Equal(Color.SeaGreen.ToArgb(), loaded.ColorLineFirst.ToArgb());
        Assert.Equal(Color.SlateBlue.ToArgb(), loaded.ColorLineSecond.ToArgb());
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_WithOptionalLengthChinkouMissing()
    {
        const string name = "CodexIchimokuLegacy";
        using IchimokuFileScope scope = new IchimokuFileScope(name);

        string legacy = string.Join(
            "\n",
            "9",
            "26",
            "52",
            Color.BlueViolet.ToArgb().ToString(),
            Color.OrangeRed.ToArgb().ToString(),
            Color.DarkRed.ToArgb().ToString(),
            Color.LimeGreen.ToArgb().ToString(),
            Color.DodgerBlue.ToArgb().ToString(),
            "True",
            "26") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        Ichimoku loaded = new Ichimoku(name, canDelete: true);

        Assert.Equal(9, loaded.LengthFirst);
        Assert.Equal(26, loaded.LengthSecond);
        Assert.Equal(52, loaded.LengthFird);
        Assert.Equal(26, loaded.LengthSdvig);
        Assert.Equal(26, loaded.LengthChinkou);
        Assert.Equal(Color.BlueViolet.ToArgb(), loaded.ColorEtalonLine.ToArgb());
        Assert.Equal(Color.OrangeRed.ToArgb(), loaded.ColorLineRounded.ToArgb());
        Assert.Equal(Color.DarkRed.ToArgb(), loaded.ColorLineLate.ToArgb());
        Assert.Equal(Color.LimeGreen.ToArgb(), loaded.ColorLineFirst.ToArgb());
        Assert.Equal(Color.DodgerBlue.ToArgb(), loaded.ColorLineSecond.ToArgb());
        Assert.True(loaded.PaintOn);
    }

    private sealed class IchimokuFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public IchimokuFileScope(string name)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, name + ".txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _settingsFileExisted = File.Exists(SettingsPath);
            if (_settingsFileExisted)
            {
                File.Copy(SettingsPath, _settingsBackupPath, overwrite: true);
            }
            else if (File.Exists(_settingsBackupPath))
            {
                File.Delete(_settingsBackupPath);
            }
        }

        public string SettingsPath { get; }

        public void Dispose()
        {
            if (_settingsFileExisted)
            {
                if (File.Exists(_settingsBackupPath))
                {
                    File.Copy(_settingsBackupPath, SettingsPath, overwrite: true);
                    File.Delete(_settingsBackupPath);
                }
            }
            else
            {
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }

                if (File.Exists(_settingsBackupPath))
                {
                    File.Delete(_settingsBackupPath);
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
