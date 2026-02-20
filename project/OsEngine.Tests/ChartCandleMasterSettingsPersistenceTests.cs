#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Charts.CandleChart;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class ChartCandleMasterSettingsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_WhenIndicatorsAreMissing()
    {
        const string name = "CodexChartMasterJson";
        using ChartCandleMasterFileScope scope = new ChartCandleMasterFileScope(name);

        ChartCandleMaster source = scope.CreateWithoutConstructor();
        scope.SetCanSave(source, true);
        scope.SetStartProgram(source, StartProgram.IsOsTrader);

        scope.InvokePrivateSave(source);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());
    }

    [Fact]
    public void ParseLegacySettings_ShouldSupportLineBasedFormat()
    {
        const string name = "CodexChartMasterLegacy";
        using ChartCandleMasterFileScope scope = new ChartCandleMasterFileScope(name);

        object settings = scope.ParseLegacy("Rsi@RsiName@Prime@True\nTrades\n");
        Assert.NotNull(settings);

        List<string> lines = scope.ExtractLines(settings);
        Assert.Equal(2, lines.Count);
        Assert.Equal("Rsi@RsiName@Prime@True", lines[0]);
        Assert.Equal("Trades", lines[1]);
    }

    private sealed class ChartCandleMasterFileScope : IDisposable
    {
        private readonly string _name;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _saveMethod;
        private readonly MethodInfo _parseLegacyMethod;
        private readonly FieldInfo _canSaveField;
        private readonly FieldInfo _startProgramField;
        private readonly FieldInfo _nameField;

        public ChartCandleMasterFileScope(string name)
        {
            _name = name;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, _name + ".txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _saveMethod = typeof(ChartCandleMaster).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Save not found.");
            _parseLegacyMethod = typeof(ChartCandleMaster).GetMethod("ParseLegacySettings", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ParseLegacySettings not found.");
            _canSaveField = typeof(ChartCandleMaster).GetField("_canSave", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _canSave not found.");
            _startProgramField = typeof(ChartCandleMaster).GetField("_startProgram", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _startProgram not found.");
            _nameField = typeof(ChartCandleMaster).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _name not found.");

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

        public ChartCandleMaster CreateWithoutConstructor()
        {
            ChartCandleMaster chartMaster = (ChartCandleMaster)RuntimeHelpers.GetUninitializedObject(typeof(ChartCandleMaster));
            _nameField.SetValue(chartMaster, _name);
            return chartMaster;
        }

        public void SetCanSave(ChartCandleMaster chartMaster, bool value)
        {
            _canSaveField.SetValue(chartMaster, value);
        }

        public void SetStartProgram(ChartCandleMaster chartMaster, StartProgram startProgram)
        {
            _startProgramField.SetValue(chartMaster, startProgram);
        }

        public void InvokePrivateSave(ChartCandleMaster chartMaster)
        {
            _saveMethod.Invoke(chartMaster, null);
        }

        public object ParseLegacy(string content)
        {
            return _parseLegacyMethod.Invoke(null, new object[] { content })!;
        }

        public List<string> ExtractLines(object settings)
        {
            PropertyInfo linesProperty = settings.GetType().GetProperty("Lines", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Property Lines not found.");
            IEnumerable<string> values = (IEnumerable<string>)linesProperty.GetValue(settings)!;
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
}
