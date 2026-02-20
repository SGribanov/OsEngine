#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Connectors;
using Xunit;

namespace OsEngine.Tests;

public class ConnectorCandlesSettingsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using ConnectorCandlesFileScope scope = new ConnectorCandlesFileScope("CodexCandlesSettings");

        ConnectorCandles source = scope.CreateWithoutConstructor();
        scope.SetupForSave(source);
        source.PortfolioName = "PF_MAIN";
        source.EmulatorIsOn = true;
        scope.SetSecurityName(source, "SBER");
        source.ServerType = ServerType.QuikLua;
        scope.SetSecurityClass(source, "TQBR");
        scope.SetEventsIsOn(source, false);
        source.ServerFullName = "QuikLua_Main";

        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        ConnectorCandles target = scope.CreateWithoutConstructor();
        scope.SetupForLoad(target);
        scope.InvokePrivateLoad(target);

        Assert.Equal("PF_MAIN", target.PortfolioName);
        Assert.True(target.EmulatorIsOn);
        Assert.Equal("SBER", scope.GetSecurityName(target));
        Assert.Equal(ServerType.QuikLua, target.ServerType);
        Assert.Equal("TQBR", scope.GetSecurityClass(target));
        Assert.False(scope.GetEventsIsOn(target));
        Assert.Equal("QuikLua_Main", target.ServerFullName);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_WithOptionalFieldsMissing()
    {
        using ConnectorCandlesFileScope scope = new ConnectorCandlesFileScope("CodexCandlesLegacy");

        string legacyContent = string.Join(
            Environment.NewLine,
            "PF_LEGACY",
            "False",
            "GAZP",
            "MetaTrader5",
            "Forex");
        File.WriteAllText(scope.SettingsPath, legacyContent);

        ConnectorCandles target = scope.CreateWithoutConstructor();
        scope.SetupForLoad(target);
        scope.InvokePrivateLoad(target);

        Assert.Equal("PF_LEGACY", target.PortfolioName);
        Assert.False(target.EmulatorIsOn);
        Assert.Equal("GAZP", scope.GetSecurityName(target));
        Assert.Equal(ServerType.MetaTrader5, target.ServerType);
        Assert.Equal("Forex", scope.GetSecurityClass(target));
        Assert.True(scope.GetEventsIsOn(target));
        Assert.Equal(ServerType.MetaTrader5.ToString(), target.ServerFullName);
    }

    private sealed class ConnectorCandlesFileScope : IDisposable
    {
        private readonly string _name;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _loadMethod;
        private readonly FieldInfo _nameField;
        private readonly FieldInfo _canSaveField;
        private readonly FieldInfo _securityNameField;
        private readonly FieldInfo _securityClassField;
        private readonly FieldInfo _eventsIsOnField;

        public ConnectorCandlesFileScope(string name)
        {
            _name = name;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, $"{_name}ConnectorPrime.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _loadMethod = typeof(ConnectorCandles).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");
            _nameField = typeof(ConnectorCandles).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _name not found.");
            _canSaveField = typeof(ConnectorCandles).GetField("_canSave", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _canSave not found.");
            _securityNameField = typeof(ConnectorCandles).GetField("_securityName", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _securityName not found.");
            _securityClassField = typeof(ConnectorCandles).GetField("_securityClass", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _securityClass not found.");
            _eventsIsOnField = typeof(ConnectorCandles).GetField("_eventsIsOn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _eventsIsOn not found.");

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

        public ConnectorCandles CreateWithoutConstructor()
        {
            return (ConnectorCandles)RuntimeHelpers.GetUninitializedObject(typeof(ConnectorCandles));
        }

        public void SetupForSave(ConnectorCandles connector)
        {
            _nameField.SetValue(connector, _name);
            _canSaveField.SetValue(connector, true);
            connector.StartProgram = StartProgram.IsOsTrader;
        }

        public void SetupForLoad(ConnectorCandles connector)
        {
            _nameField.SetValue(connector, _name);
            _eventsIsOnField.SetValue(connector, true);
            connector.ServerFullName = null;
        }

        public void InvokePrivateLoad(ConnectorCandles connector)
        {
            _loadMethod.Invoke(connector, null);
        }

        public void SetSecurityName(ConnectorCandles connector, string value)
        {
            _securityNameField.SetValue(connector, value);
        }

        public string GetSecurityName(ConnectorCandles connector)
        {
            return (string)_securityNameField.GetValue(connector)!;
        }

        public void SetSecurityClass(ConnectorCandles connector, string value)
        {
            _securityClassField.SetValue(connector, value);
        }

        public string GetSecurityClass(ConnectorCandles connector)
        {
            return (string)_securityClassField.GetValue(connector)!;
        }

        public void SetEventsIsOn(ConnectorCandles connector, bool value)
        {
            _eventsIsOnField.SetValue(connector, value);
        }

        public bool GetEventsIsOn(ConnectorCandles connector)
        {
            return (bool)_eventsIsOnField.GetValue(connector)!;
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
