#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Alerts;
using OsEngine.Charts.CandleChart;
using OsEngine.Entity;
using OsEngine.Market.Connectors;
using Xunit;

namespace OsEngine.Tests;

public class AlertMasterPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson()
    {
        const string name = "CodexAlertMasterJson";
        using AlertMasterFileScope scope = new AlertMasterFileScope(name);

        AlertMaster master = scope.CreateAlertMaster();
        FakeAlert first = new FakeAlert { TypeAlert = AlertType.ChartAlert, IsOn = true };
        FakeAlert second = new FakeAlert { TypeAlert = AlertType.PriceAlert, IsOn = true };
        scope.SetAlerts(master, new List<IIAlert> { first, second });

        scope.InvokePrivateSave(master);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());
        Assert.Equal("ChartAlert$CodexAlertMasterJson0", first.Name);
        Assert.Equal("PriceAlert$CodexAlertMasterJson1", second.Name);
        Assert.True(first.SaveCalled);
        Assert.True(second.SaveCalled);
    }

    [Fact]
    public void ParseLegacyAlertKeeperSettings_ShouldSupportLineBasedFormat()
    {
        const string name = "CodexAlertMasterLegacy";
        using AlertMasterFileScope scope = new AlertMasterFileScope(name);

        string legacy = string.Join("\n", "ChartAlert$A0", "PriceAlert$A1") + "\n";

        object settings = scope.ParseLegacy(legacy);
        Assert.NotNull(settings);

        List<string> alertNames = scope.ExtractAlertNames(settings);
        Assert.Equal(2, alertNames.Count);
        Assert.Equal("ChartAlert$A0", alertNames[0]);
        Assert.Equal("PriceAlert$A1", alertNames[1]);
    }

    private sealed class AlertMasterFileScope : IDisposable
    {
        private readonly string _name;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _saveMethod;
        private readonly MethodInfo _parseLegacyMethod;
        private readonly FieldInfo _alertsField;

        public AlertMasterFileScope(string name)
        {
            _name = name;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, _name + "AlertKeeper.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _saveMethod = typeof(AlertMaster).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Save not found.");
            _parseLegacyMethod = typeof(AlertMaster).GetMethod("ParseLegacyAlertKeeperSettings", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ParseLegacyAlertKeeperSettings not found.");
            _alertsField = typeof(AlertMaster).GetField("_alertArray", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _alertArray not found.");

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

        public AlertMaster CreateAlertMaster()
        {
            ConnectorCandles connector = (ConnectorCandles)RuntimeHelpers.GetUninitializedObject(typeof(ConnectorCandles));
            connector.StartProgram = StartProgram.IsOsTrader;

            ChartCandleMaster chartMaster = (ChartCandleMaster)RuntimeHelpers.GetUninitializedObject(typeof(ChartCandleMaster));

            return new AlertMaster(_name, connector, chartMaster);
        }

        public void SetAlerts(AlertMaster master, List<IIAlert> alerts)
        {
            _alertsField.SetValue(master, alerts);
        }

        public void InvokePrivateSave(AlertMaster master)
        {
            _saveMethod.Invoke(master, null);
        }

        public object ParseLegacy(string content)
        {
            return _parseLegacyMethod.Invoke(null, new object[] { content })!;
        }

        public List<string> ExtractAlertNames(object settings)
        {
            PropertyInfo namesProperty = settings.GetType().GetProperty("AlertNames", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Property AlertNames not found.");
            IEnumerable<string> values = (IEnumerable<string>)namesProperty.GetValue(settings)!;
            return values.ToList();
        }

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

    private sealed class FakeAlert : IIAlert
    {
        public bool SaveCalled { get; private set; }

        public bool IsOn { get; set; }

        public string Name { get; set; } = string.Empty;

        public AlertType TypeAlert { get; set; }

        public void Save()
        {
            SaveCalled = true;
        }

        public void Load()
        {
        }

        public void Delete()
        {
        }

        public AlertSignal CheckSignal(List<Candle> candles, Security sec)
        {
            return null!;
        }
    }
}
