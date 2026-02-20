#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab.Internal;
using Xunit;

namespace OsEngine.Tests;

public class BotManualControlPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexManualControlJson";
        using BotManualControlFileScope scope = new BotManualControlFileScope(name);

        BotManualControl source = scope.CreateWithoutConstructor();
        source.StopIsOn = true;
        source.StopDistance = 11;
        source.StopSlippage = 2;
        source.ProfitIsOn = true;
        source.ProfitDistance = 22;
        source.ProfitSlippage = 3;
        source.SecondToOpen = TimeSpan.FromSeconds(15);
        source.SecondToClose = TimeSpan.FromSeconds(25);
        source.DoubleExitIsOn = false;
        source.SecondToOpenIsOn = true;
        source.SecondToCloseIsOn = true;
        source.SetbackToOpenIsOn = true;
        source.SetbackToOpenPosition = 5;
        source.SetbackToCloseIsOn = true;
        source.SetbackToClosePosition = 6;
        source.DoubleExitSlippage = 7;
        source.TypeDoubleExitOrder = OrderPriceType.Market;
        source.ValuesType = ManualControlValuesType.Percent;
        source.OrderTypeTime = OrderTypeTime.GTC;
        source.LimitsMakerOnly = true;
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        BotManualControl loaded = scope.CreateWithoutConstructor();
        bool isLoaded = scope.InvokePrivateLoad(loaded);
        Assert.True(isLoaded);

        Assert.True(loaded.StopIsOn);
        Assert.Equal(11, loaded.StopDistance);
        Assert.Equal(2, loaded.StopSlippage);
        Assert.True(loaded.ProfitIsOn);
        Assert.Equal(22, loaded.ProfitDistance);
        Assert.Equal(3, loaded.ProfitSlippage);
        Assert.Equal(TimeSpan.FromSeconds(15), loaded.SecondToOpen);
        Assert.Equal(TimeSpan.FromSeconds(25), loaded.SecondToClose);
        Assert.False(loaded.DoubleExitIsOn);
        Assert.True(loaded.SecondToOpenIsOn);
        Assert.True(loaded.SecondToCloseIsOn);
        Assert.True(loaded.SetbackToOpenIsOn);
        Assert.Equal(5, loaded.SetbackToOpenPosition);
        Assert.True(loaded.SetbackToCloseIsOn);
        Assert.Equal(6, loaded.SetbackToClosePosition);
        Assert.Equal(7, loaded.DoubleExitSlippage);
        Assert.Equal(OrderPriceType.Market, loaded.TypeDoubleExitOrder);
        Assert.Equal(ManualControlValuesType.Percent, loaded.ValuesType);
        Assert.Equal(OrderTypeTime.GTC, loaded.OrderTypeTime);
        Assert.True(loaded.LimitsMakerOnly);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexManualControlLegacy";
        using BotManualControlFileScope scope = new BotManualControlFileScope(name);

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "True",
            "10",
            "2",
            "False",
            "20",
            "3",
            "00:00:11",
            "00:00:22",
            "True",
            "True",
            "True",
            "True",
            "5",
            "False",
            "6",
            "7",
            "Market",
            "Absolute",
            "Day",
            "True"
        });

        BotManualControl loaded = scope.CreateWithoutConstructor();
        bool isLoaded = scope.InvokePrivateLoad(loaded);
        Assert.True(isLoaded);

        Assert.True(loaded.StopIsOn);
        Assert.Equal(10, loaded.StopDistance);
        Assert.Equal(2, loaded.StopSlippage);
        Assert.False(loaded.ProfitIsOn);
        Assert.Equal(20, loaded.ProfitDistance);
        Assert.Equal(3, loaded.ProfitSlippage);
        Assert.Equal(TimeSpan.FromSeconds(11), loaded.SecondToOpen);
        Assert.Equal(TimeSpan.FromSeconds(22), loaded.SecondToClose);
        Assert.True(loaded.DoubleExitIsOn);
        Assert.True(loaded.SecondToOpenIsOn);
        Assert.True(loaded.SecondToCloseIsOn);
        Assert.True(loaded.SetbackToOpenIsOn);
        Assert.Equal(5, loaded.SetbackToOpenPosition);
        Assert.False(loaded.SetbackToCloseIsOn);
        Assert.Equal(6, loaded.SetbackToClosePosition);
        Assert.Equal(7, loaded.DoubleExitSlippage);
        Assert.Equal(OrderPriceType.Market, loaded.TypeDoubleExitOrder);
        Assert.Equal(ManualControlValuesType.Absolute, loaded.ValuesType);
        Assert.Equal(OrderTypeTime.Day, loaded.OrderTypeTime);
        Assert.True(loaded.LimitsMakerOnly);
    }

    private sealed class BotManualControlFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _nameField;
        private readonly MethodInfo _loadMethod;

        public BotManualControlFileScope(string name)
        {
            Name = name;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, name + "StrategSettings.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _nameField = typeof(BotManualControl).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _name not found.");
            _loadMethod = typeof(BotManualControl).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");

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

        public string Name { get; }

        public string SettingsPath { get; }

        public BotManualControl CreateWithoutConstructor()
        {
            BotManualControl instance =
                (BotManualControl)RuntimeHelpers.GetUninitializedObject(typeof(BotManualControl));
            _nameField.SetValue(instance, Name);
            instance._startProgram = StartProgram.IsOsTrader;
            return instance;
        }

        public bool InvokePrivateLoad(BotManualControl control)
        {
            return (bool)_loadMethod.Invoke(control, null)!;
        }

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
