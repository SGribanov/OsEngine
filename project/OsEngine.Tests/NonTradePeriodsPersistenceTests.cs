using System;
using System.IO;
using System.Linq;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class NonTradePeriodsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        string name = "codex_nontrade_json_" + Guid.NewGuid().ToString("N");

        using NonTradePeriodsFileScope scope = new NonTradePeriodsFileScope(name);

        NonTradePeriods source = new NonTradePeriods(name);
        source.TradeInMonday = false;
        source.TradeInSunday = false;
        source.NonTradePeriodGeneral.NonTradePeriod1OnOff = true;
        source.NonTradePeriodGeneral.NonTradePeriod1Start.Hour = 1;
        source.NonTradePeriodGeneral.NonTradePeriod1Start.Minute = 2;
        source.NonTradePeriodGeneral.NonTradePeriod1End.Hour = 3;
        source.NonTradePeriodGeneral.NonTradePeriod1End.Minute = 4;
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        NonTradePeriods loaded = new NonTradePeriods(name);
        Assert.False(loaded.TradeInMonday);
        Assert.False(loaded.TradeInSunday);
        Assert.True(loaded.NonTradePeriodGeneral.NonTradePeriod1OnOff);
        Assert.Equal(1, loaded.NonTradePeriodGeneral.NonTradePeriod1Start.Hour);
        Assert.Equal(2, loaded.NonTradePeriodGeneral.NonTradePeriod1Start.Minute);
        Assert.Equal(3, loaded.NonTradePeriodGeneral.NonTradePeriod1End.Hour);
        Assert.Equal(4, loaded.NonTradePeriodGeneral.NonTradePeriod1End.Minute);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        string name = "codex_nontrade_legacy_" + Guid.NewGuid().ToString("N");

        using NonTradePeriodsFileScope scope = new NonTradePeriodsFileScope(name);

        NonTradePeriods legacySource = new NonTradePeriods(name);
        legacySource.TradeInTuesday = false;
        legacySource.NonTradePeriodFriday.NonTradePeriod2OnOff = true;
        legacySource.NonTradePeriodFriday.NonTradePeriod2Start.Hour = 11;
        legacySource.NonTradePeriodFriday.NonTradePeriod2Start.Minute = 15;

        File.WriteAllLines(scope.SettingsPath, legacySource.GetFullSaveArray());

        NonTradePeriods loaded = new NonTradePeriods(name);
        Assert.False(loaded.TradeInTuesday);
        Assert.True(loaded.NonTradePeriodFriday.NonTradePeriod2OnOff);
        Assert.Equal(11, loaded.NonTradePeriodFriday.NonTradePeriod2Start.Hour);
        Assert.Equal(15, loaded.NonTradePeriodFriday.NonTradePeriod2Start.Minute);
    }

    private sealed class NonTradePeriodsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;

        public NonTradePeriodsFileScope(string name)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, name + "nonTradePeriod.txt");
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
