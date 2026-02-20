#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class VwapPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexVwapJson";
        using VwapFileScope scope = new VwapFileScope(name);

        DateTime dateStart = new DateTime(2024, 03, 10, 0, 0, 0);
        DateTime timeStart = new DateTime(2024, 03, 10, 9, 0, 0);
        DateTime dateEnd = new DateTime(2024, 03, 20, 0, 0, 0);
        DateTime timeEnd = new DateTime(2024, 03, 20, 19, 0, 0);

        Vwap source = new Vwap(name, canDelete: true)
        {
            UseDate = true,
            DatePickerStart = dateStart,
            TimePickerStart = timeStart,
            ToEndTicks = false,
            DatePickerEnd = dateEnd,
            TimePickerEnd = timeEnd,
            DateDev2 = true,
            DateDev3 = false,
            DateDev4 = true,
            ColorDate = Color.BlueViolet,
            ColorDateDev = Color.SandyBrown,
            UseDay = true,
            DayDev2 = false,
            DayDev3 = true,
            DayDev4 = false,
            ColorDay = Color.CornflowerBlue,
            ColorDayDev = Color.BurlyWood,
            UseWeekly = true,
            WeekDev2 = true,
            WeekDev3 = true,
            WeekDev4 = false,
            ColorWeek = Color.DarkTurquoise,
            ColorWeekDev = Color.Khaki,
            PaintOn = false
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        Vwap loaded = new Vwap(name, canDelete: true);

        Assert.True(loaded.UseDate);
        AssertDateTimeEqual(dateStart, loaded.DatePickerStart);
        AssertDateTimeEqual(timeStart, loaded.TimePickerStart);
        Assert.False(loaded.ToEndTicks);
        AssertDateTimeEqual(dateEnd, loaded.DatePickerEnd);
        AssertDateTimeEqual(timeEnd, loaded.TimePickerEnd);
        Assert.True(loaded.DateDev2);
        Assert.False(loaded.DateDev3);
        Assert.True(loaded.DateDev4);
        Assert.Equal(Color.BlueViolet.ToArgb(), loaded.ColorDate.ToArgb());
        Assert.Equal(Color.SandyBrown.ToArgb(), loaded.ColorDateDev.ToArgb());
        Assert.True(loaded.UseDay);
        Assert.False(loaded.DayDev2);
        Assert.True(loaded.DayDev3);
        Assert.False(loaded.DayDev4);
        Assert.Equal(Color.CornflowerBlue.ToArgb(), loaded.ColorDay.ToArgb());
        Assert.Equal(Color.BurlyWood.ToArgb(), loaded.ColorDayDev.ToArgb());
        Assert.True(loaded.UseWeekly);
        Assert.True(loaded.WeekDev2);
        Assert.True(loaded.WeekDev3);
        Assert.False(loaded.WeekDev4);
        Assert.Equal(Color.DarkTurquoise.ToArgb(), loaded.ColorWeek.ToArgb());
        Assert.Equal(Color.Khaki.ToArgb(), loaded.ColorWeekDev.ToArgb());
        Assert.False(loaded.PaintOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexVwapLegacy";
        using VwapFileScope scope = new VwapFileScope(name);

        DateTime dateStart = new DateTime(2023, 01, 11, 0, 0, 0);
        DateTime timeStart = new DateTime(2023, 01, 11, 10, 0, 0);
        DateTime dateEnd = new DateTime(2023, 01, 21, 0, 0, 0);
        DateTime timeEnd = new DateTime(2023, 01, 21, 18, 0, 0);

        string legacy = string.Join(
            "\n",
            "True",
            dateStart.ToString("O", CultureInfo.InvariantCulture),
            timeStart.ToString("O", CultureInfo.InvariantCulture),
            "False",
            dateEnd.ToString("O", CultureInfo.InvariantCulture),
            timeEnd.ToString("O", CultureInfo.InvariantCulture),
            "True",
            "False",
            "True",
            Color.BlueViolet.ToArgb().ToString(CultureInfo.InvariantCulture),
            Color.AntiqueWhite.ToArgb().ToString(CultureInfo.InvariantCulture),
            "True",
            "False",
            "True",
            "False",
            Color.CornflowerBlue.ToArgb().ToString(CultureInfo.InvariantCulture),
            Color.BurlyWood.ToArgb().ToString(CultureInfo.InvariantCulture),
            "True",
            "True",
            "False",
            "True",
            Color.DarkCyan.ToArgb().ToString(CultureInfo.InvariantCulture),
            Color.Khaki.ToArgb().ToString(CultureInfo.InvariantCulture),
            "True") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        Vwap loaded = new Vwap(name, canDelete: true);

        Assert.True(loaded.UseDate);
        AssertDateTimeEqual(dateStart, loaded.DatePickerStart);
        AssertDateTimeEqual(timeStart, loaded.TimePickerStart);
        Assert.False(loaded.ToEndTicks);
        AssertDateTimeEqual(dateEnd, loaded.DatePickerEnd);
        AssertDateTimeEqual(timeEnd, loaded.TimePickerEnd);
        Assert.True(loaded.DateDev2);
        Assert.False(loaded.DateDev3);
        Assert.True(loaded.DateDev4);
        Assert.Equal(Color.BlueViolet.ToArgb(), loaded.ColorDate.ToArgb());
        Assert.Equal(Color.AntiqueWhite.ToArgb(), loaded.ColorDateDev.ToArgb());
        Assert.True(loaded.UseDay);
        Assert.False(loaded.DayDev2);
        Assert.True(loaded.DayDev3);
        Assert.False(loaded.DayDev4);
        Assert.Equal(Color.CornflowerBlue.ToArgb(), loaded.ColorDay.ToArgb());
        Assert.Equal(Color.BurlyWood.ToArgb(), loaded.ColorDayDev.ToArgb());
        Assert.True(loaded.UseWeekly);
        Assert.True(loaded.WeekDev2);
        Assert.False(loaded.WeekDev3);
        Assert.True(loaded.WeekDev4);
        Assert.Equal(Color.DarkCyan.ToArgb(), loaded.ColorWeek.ToArgb());
        Assert.Equal(Color.Khaki.ToArgb(), loaded.ColorWeekDev.ToArgb());
        Assert.True(loaded.PaintOn);
    }

    private static void AssertDateTimeEqual(DateTime expected, DateTime actual)
    {
        Assert.Equal(expected.Year, actual.Year);
        Assert.Equal(expected.Month, actual.Month);
        Assert.Equal(expected.Day, actual.Day);
        Assert.Equal(expected.Hour, actual.Hour);
        Assert.Equal(expected.Minute, actual.Minute);
        Assert.Equal(expected.Second, actual.Second);
    }

    private sealed class VwapFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public VwapFileScope(string name)
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
