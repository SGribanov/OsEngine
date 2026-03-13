#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.PrimeSettings;
using Xunit;

namespace OsEngine.Tests;

public class PrimeSettingsMasterPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistToml_AndLoadRoundTrip()
    {
        using PrimeSettingsFileScope scope = new PrimeSettingsFileScope();

        PrimeSettingsMaster.ErrorLogMessageBoxIsActive = false;
        PrimeSettingsMaster.ErrorLogBeepIsActive = false;
        PrimeSettingsMaster.TransactionBeepIsActive = true;
        PrimeSettingsMaster.RebootTradeUiLight = true;
        PrimeSettingsMaster.ReportCriticalErrors = false;
        PrimeSettingsMaster.LabelInHeaderBotStation = "json_header";
        PrimeSettingsMaster.MemoryCleanerRegime = MemoryCleanerRegime.At30Minutes;
        PrimeSettingsMaster.Save();

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("LabelInHeaderBotStation = \"json_header\"", content);

        scope.ResetStateForLoad();

        Assert.False(PrimeSettingsMaster.ErrorLogMessageBoxIsActive);
        Assert.False(PrimeSettingsMaster.ErrorLogBeepIsActive);
        Assert.True(PrimeSettingsMaster.TransactionBeepIsActive);
        Assert.True(PrimeSettingsMaster.RebootTradeUiLight);
        Assert.False(PrimeSettingsMaster.ReportCriticalErrors);
        Assert.Equal("json_header", PrimeSettingsMaster.LabelInHeaderBotStation);
        Assert.Equal(MemoryCleanerRegime.At30Minutes, PrimeSettingsMaster.MemoryCleanerRegime);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        using PrimeSettingsFileScope scope = new PrimeSettingsFileScope();

        File.WriteAllLines(scope.LegacyTxtPath, new[]
        {
            "True",
            "False",
            "True",
            "True",
            "True",
            "False",
            "At5Minutes"
        });

        scope.ResetStateForLoad();

        Assert.True(PrimeSettingsMaster.TransactionBeepIsActive);
        Assert.False(PrimeSettingsMaster.ErrorLogBeepIsActive);
        Assert.True(PrimeSettingsMaster.ErrorLogMessageBoxIsActive);
        Assert.Equal(string.Empty, PrimeSettingsMaster.LabelInHeaderBotStation);
        Assert.True(PrimeSettingsMaster.RebootTradeUiLight);
        Assert.False(PrimeSettingsMaster.ReportCriticalErrors);
        Assert.Equal(MemoryCleanerRegime.At5Minutes, PrimeSettingsMaster.MemoryCleanerRegime);

        PrimeSettingsMaster.Save();
        Assert.True(File.Exists(scope.CanonicalPath));
        Assert.Contains("MemoryCleanerRegime = 1", File.ReadAllText(scope.CanonicalPath));
    }

    private sealed class PrimeSettingsFileScope : IDisposable
    {
        private readonly StructuredSettingsFileScope _settingsScope;
        private readonly Dictionary<string, object?> _originalFields;

        public PrimeSettingsFileScope()
        {
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", "PrimeSettings.toml"));
            _originalFields = CaptureFields();
        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

        public void ResetStateForLoad()
        {
            SetField("_isLoad", false);
            SetField("_transactionBeepIsActive", false);
            SetField("_errorLogBeepIsActive", false);
            SetField("_errorLogMessageBoxIsActive", true);
            SetField("_labelInHeaderBotStation", null);
            SetField("_rebootTradeUiLight", false);
            SetField("_reportCriticalErrors", true);
            SetField("_memoryCleanerRegime", MemoryCleanerRegime.Disable);
        }

        public void Dispose()
        {
            foreach ((string key, object? value) in _originalFields)
            {
                SetField(key, value);
            }

            _settingsScope.Dispose();
        }

        private static Dictionary<string, object?> CaptureFields()
        {
            Dictionary<string, object?> values = new Dictionary<string, object?>();

            foreach (string fieldName in GetTrackedFieldNames())
            {
                values[fieldName] = GetField(fieldName).GetValue(null);
            }

            return values;
        }

        private static void SetField(string name, object? value)
        {
            GetField(name).SetValue(null, value);
        }

        private static FieldInfo GetField(string name)
        {
            return typeof(PrimeSettingsMaster).GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field not found: " + name);
        }

        private static IEnumerable<string> GetTrackedFieldNames()
        {
            yield return "_errorLogMessageBoxIsActive";
            yield return "_errorLogBeepIsActive";
            yield return "_transactionBeepIsActive";
            yield return "_rebootTradeUiLight";
            yield return "_reportCriticalErrors";
            yield return "_labelInHeaderBotStation";
            yield return "_memoryCleanerRegime";
            yield return "_isLoad";
        }
    }
}
