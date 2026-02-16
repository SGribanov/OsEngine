using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.OsTrader.Panels;
using Xunit;

namespace OsEngine.Tests;

public class BotPanelChartUILayoutPersistenceTests
{
    [Fact]
    public void SaveLeftPanelPosition_ShouldPersistJson()
    {
        const string panelName = "CodexBotPanelUiLayoutJson";
        using BotPanelChartUiFileScope scope = new BotPanelChartUiFileScope(panelName);

        BotPanelChartUi ui = scope.CreateWithoutConstructor();
        scope.SetSettingsPanelIsHide(ui, true);
        scope.SetInformPanelIsHide(ui, false);
        scope.SetLowPanelIsBig(ui, true);

        scope.InvokePrivateSaveLeftPanelPosition(ui);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());
    }

    [Fact]
    public void ParseLegacyLayoutSettings_ShouldSupportLineBasedFormat()
    {
        const string panelName = "CodexBotPanelUiLayoutLegacy";
        using BotPanelChartUiFileScope scope = new BotPanelChartUiFileScope(panelName);

        object settings = scope.ParseLegacy("True\nFalse\nTrue\n");
        Assert.NotNull(settings);

        Assert.True(scope.GetDtoBool(settings, "SettingsPanelIsHide"));
        Assert.False(scope.GetDtoBool(settings, "InformPanelIsHide"));
        Assert.True(scope.GetDtoBool(settings, "LowPanelIsBig"));
    }

    private sealed class BotPanelChartUiFileScope : IDisposable
    {
        private readonly string _panelName;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _saveLeftPanelPositionMethod;
        private readonly MethodInfo _parseLegacyMethod;
        private readonly FieldInfo _panelNameField;
        private readonly FieldInfo _settingsPanelIsHideField;
        private readonly FieldInfo _informPanelIsHideField;
        private readonly FieldInfo _lowPanelIsBigField;

        public BotPanelChartUiFileScope(string panelName)
        {
            _panelName = panelName;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "LayoutRobotUi" + _panelName + ".txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _saveLeftPanelPositionMethod = typeof(BotPanelChartUi).GetMethod("SaveLeftPanelPosition", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SaveLeftPanelPosition not found.");
            _parseLegacyMethod = typeof(BotPanelChartUi).GetMethod("ParseLegacyLayoutSettings", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ParseLegacyLayoutSettings not found.");
            _panelNameField = typeof(BotPanelChartUi).GetField("_panelName", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _panelName not found.");
            _settingsPanelIsHideField = typeof(BotPanelChartUi).GetField("_settingsPanelIsHide", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _settingsPanelIsHide not found.");
            _informPanelIsHideField = typeof(BotPanelChartUi).GetField("_informPanelIsHide", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _informPanelIsHide not found.");
            _lowPanelIsBigField = typeof(BotPanelChartUi).GetField("_lowPanelIsBig", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _lowPanelIsBig not found.");

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

        public BotPanelChartUi CreateWithoutConstructor()
        {
            BotPanelChartUi ui = (BotPanelChartUi)RuntimeHelpers.GetUninitializedObject(typeof(BotPanelChartUi));
            _panelNameField.SetValue(ui, _panelName);
            return ui;
        }

        public void SetSettingsPanelIsHide(BotPanelChartUi ui, bool value)
        {
            _settingsPanelIsHideField.SetValue(ui, value);
        }

        public void SetInformPanelIsHide(BotPanelChartUi ui, bool value)
        {
            _informPanelIsHideField.SetValue(ui, value);
        }

        public void SetLowPanelIsBig(BotPanelChartUi ui, bool value)
        {
            _lowPanelIsBigField.SetValue(ui, value);
        }

        public void InvokePrivateSaveLeftPanelPosition(BotPanelChartUi ui)
        {
            _saveLeftPanelPositionMethod.Invoke(ui, null);
        }

        public object ParseLegacy(string content)
        {
            return _parseLegacyMethod.Invoke(null, new object[] { content })!;
        }

        public bool GetDtoBool(object settings, string propertyName)
        {
            PropertyInfo property = settings.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException($"Property {propertyName} not found.");
            return (bool)property.GetValue(settings)!;
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
