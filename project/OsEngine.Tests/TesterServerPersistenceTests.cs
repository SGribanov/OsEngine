#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Market.Servers.Tester;
using Xunit;

namespace OsEngine.Tests;

public class TesterServerPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using TesterServerFileScope scope = new TesterServerFileScope();

        TesterServer source = scope.CreateWithoutConstructor();
        scope.SetPrivateField(source, "_activeSet", "C:\\sets\\alpha");
        scope.SetPrivateField(source, "_slippageToSimpleOrder", 3);
        scope.SetPrivateField(source, "_startPortfolio", 12345.67m);
        scope.SetPrivateField(source, "_typeTesterData", TesterDataType.TickOnlyReadyCandle);
        scope.SetPrivateField(source, "_sourceDataType", TesterSourceDataType.Folder);
        scope.SetPrivateField(source, "_pathToFolder", "C:\\data");
        scope.SetPrivateField(source, "_slippageToStopOrder", 4);
        scope.SetPrivateField(source, "_orderExecutionType", OrderExecutionType.FiftyFifty);
        scope.SetPrivateField(source, "_profitMarketIsOn", false);
        scope.SetPrivateField(source, "_guiIsOpenFullSettings", true);
        scope.SetPrivateField(source, "_removeTradesFromMemory", true);
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        TesterServer loaded = scope.CreateWithoutConstructor();
        scope.SetPrivateField(loaded, "_startPortfolio", 0m);
        scope.InvokePrivateLoad(loaded);

        Assert.Equal("C:\\sets\\alpha", scope.GetPrivateField<string>(loaded, "_activeSet"));
        Assert.Equal(3, scope.GetPrivateField<int>(loaded, "_slippageToSimpleOrder"));
        Assert.Equal(12345.67m, scope.GetPrivateField<decimal>(loaded, "_startPortfolio"));
        Assert.Equal(TesterDataType.TickOnlyReadyCandle, scope.GetPrivateField<TesterDataType>(loaded, "_typeTesterData"));
        Assert.Equal(TesterSourceDataType.Folder, scope.GetPrivateField<TesterSourceDataType>(loaded, "_sourceDataType"));
        Assert.Equal("C:\\data", scope.GetPrivateField<string>(loaded, "_pathToFolder"));
        Assert.Equal(4, scope.GetPrivateField<int>(loaded, "_slippageToStopOrder"));
        Assert.Equal(OrderExecutionType.FiftyFifty, scope.GetPrivateField<OrderExecutionType>(loaded, "_orderExecutionType"));
        Assert.False(scope.GetPrivateField<bool>(loaded, "_profitMarketIsOn"));
        Assert.True(scope.GetPrivateField<bool>(loaded, "_guiIsOpenFullSettings"));
        Assert.True(scope.GetPrivateField<bool>(loaded, "_removeTradesFromMemory"));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using TesterServerFileScope scope = new TesterServerFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "C:\\sets\\legacy",
            "5",
            "9999.5",
            "Candle",
            "Set",
            "C:\\legacy_data",
            "7",
            "Touch",
            "True",
            "False",
            "True"
        });

        TesterServer loaded = scope.CreateWithoutConstructor();
        scope.SetPrivateField(loaded, "_startPortfolio", 0m);
        scope.InvokePrivateLoad(loaded);

        Assert.Equal("C:\\sets\\legacy", scope.GetPrivateField<string>(loaded, "_activeSet"));
        Assert.Equal(5, scope.GetPrivateField<int>(loaded, "_slippageToSimpleOrder"));
        Assert.Equal(9999.5m, scope.GetPrivateField<decimal>(loaded, "_startPortfolio"));
        Assert.Equal(TesterDataType.Candle, scope.GetPrivateField<TesterDataType>(loaded, "_typeTesterData"));
        Assert.Equal(TesterSourceDataType.Set, scope.GetPrivateField<TesterSourceDataType>(loaded, "_sourceDataType"));
        Assert.Equal("C:\\legacy_data", scope.GetPrivateField<string>(loaded, "_pathToFolder"));
        Assert.Equal(7, scope.GetPrivateField<int>(loaded, "_slippageToStopOrder"));
        Assert.Equal(OrderExecutionType.Touch, scope.GetPrivateField<OrderExecutionType>(loaded, "_orderExecutionType"));
        Assert.True(scope.GetPrivateField<bool>(loaded, "_profitMarketIsOn"));
        Assert.False(scope.GetPrivateField<bool>(loaded, "_guiIsOpenFullSettings"));
        Assert.True(scope.GetPrivateField<bool>(loaded, "_removeTradesFromMemory"));
    }

    private sealed class TesterServerFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _loadMethod;

        public TesterServerFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "TestServer.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _loadMethod = typeof(TesterServer).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");

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

        public TesterServer CreateWithoutConstructor()
        {
            return (TesterServer)RuntimeHelpers.GetUninitializedObject(typeof(TesterServer));
        }

        public void InvokePrivateLoad(TesterServer server)
        {
            _loadMethod.Invoke(server, null);
        }

        public void SetPrivateField(TesterServer server, string fieldName, object value)
        {
            FieldInfo field = typeof(TesterServer).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);
            field.SetValue(server, value);
        }

        public T GetPrivateField<T>(TesterServer server, string fieldName)
        {
            FieldInfo field = typeof(TesterServer).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);
            return (T)field.GetValue(server)!;
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
