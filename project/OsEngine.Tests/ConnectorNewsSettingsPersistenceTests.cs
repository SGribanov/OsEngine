using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Market.Connectors;
using OsEngine.Market;
using Xunit;

namespace OsEngine.Tests;

public class ConnectorNewsSettingsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using ConnectorNewsFileScope scope = new ConnectorNewsFileScope("CodexNewsSettings");

        ConnectorNews source = scope.CreateWithoutConstructor();
        scope.SetupForSave(source);
        scope.SetServerType(source, ServerType.QuikLua);
        scope.SetEventsIsOn(source, false);
        scope.SetCountNewsToSave(source, 55);
        scope.SetServerFullName(source, "QuikLua_Main");

        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        ConnectorNews target = scope.CreateWithoutConstructor();
        scope.SetupForLoad(target);
        scope.InvokePrivateLoad(target);

        Assert.Equal(ServerType.QuikLua, scope.GetServerType(target));
        Assert.False(scope.GetEventsIsOn(target));
        Assert.Equal(55, scope.GetCountNewsToSave(target));
        Assert.Equal("QuikLua_Main", scope.GetServerFullName(target));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using ConnectorNewsFileScope scope = new ConnectorNewsFileScope("CodexNewsLegacy");

        string legacyContent = string.Join(
            Environment.NewLine,
            "MetaTrader5",
            "True",
            "77",
            "MetaTrader5_Main");
        File.WriteAllText(scope.SettingsPath, legacyContent);

        ConnectorNews target = scope.CreateWithoutConstructor();
        scope.SetupForLoad(target);
        scope.InvokePrivateLoad(target);

        Assert.Equal(ServerType.MetaTrader5, scope.GetServerType(target));
        Assert.True(scope.GetEventsIsOn(target));
        Assert.Equal(77, scope.GetCountNewsToSave(target));
        Assert.Equal("MetaTrader5_Main", scope.GetServerFullName(target));
    }

    private sealed class ConnectorNewsFileScope : IDisposable
    {
        private readonly string _name;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _loadMethod;
        private readonly FieldInfo _nameField;
        private readonly FieldInfo _canSaveField;
        private readonly FieldInfo _serverTypeField;
        private readonly FieldInfo _eventsIsOnField;
        private readonly FieldInfo _countNewsToSaveField;
        private readonly FieldInfo _serverFullNameField;

        public ConnectorNewsFileScope(string name)
        {
            _name = name;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, $"{_name}ConnectorNews.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _loadMethod = typeof(ConnectorNews).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");
            _nameField = typeof(ConnectorNews).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _name not found.");
            _canSaveField = typeof(ConnectorNews).GetField("_canSave", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _canSave not found.");
            _serverTypeField = typeof(ConnectorNews).GetField("_serverType", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _serverType not found.");
            _eventsIsOnField = typeof(ConnectorNews).GetField("_eventsIsOn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _eventsIsOn not found.");
            _countNewsToSaveField = typeof(ConnectorNews).GetField("_countNewsToSave", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _countNewsToSave not found.");
            _serverFullNameField = typeof(ConnectorNews).GetField("_serverFullName", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _serverFullName not found.");

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

        public ConnectorNews CreateWithoutConstructor()
        {
            return (ConnectorNews)RuntimeHelpers.GetUninitializedObject(typeof(ConnectorNews));
        }

        public void SetupForSave(ConnectorNews connector)
        {
            _nameField.SetValue(connector, _name);
            _canSaveField.SetValue(connector, true);
            connector.StartProgram = StartProgram.IsOsTrader;
        }

        public void SetupForLoad(ConnectorNews connector)
        {
            _nameField.SetValue(connector, _name);
            _eventsIsOnField.SetValue(connector, true);
            _countNewsToSaveField.SetValue(connector, 100);
        }

        public void InvokePrivateLoad(ConnectorNews connector)
        {
            _loadMethod.Invoke(connector, null);
        }

        public void SetServerType(ConnectorNews connector, ServerType value)
        {
            _serverTypeField.SetValue(connector, value);
        }

        public ServerType GetServerType(ConnectorNews connector)
        {
            return (ServerType)_serverTypeField.GetValue(connector)!;
        }

        public void SetEventsIsOn(ConnectorNews connector, bool value)
        {
            _eventsIsOnField.SetValue(connector, value);
        }

        public bool GetEventsIsOn(ConnectorNews connector)
        {
            return (bool)_eventsIsOnField.GetValue(connector)!;
        }

        public void SetCountNewsToSave(ConnectorNews connector, int value)
        {
            _countNewsToSaveField.SetValue(connector, value);
        }

        public int GetCountNewsToSave(ConnectorNews connector)
        {
            return (int)_countNewsToSaveField.GetValue(connector)!;
        }

        public void SetServerFullName(ConnectorNews connector, string value)
        {
            _serverFullNameField.SetValue(connector, value);
        }

        public string GetServerFullName(ConnectorNews connector)
        {
            return (string)_serverFullNameField.GetValue(connector)!;
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
