using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Journal.Internal;
using Xunit;

namespace OsEngine.Tests;

public class PositionControllerStopLimitsPersistenceTests
{
    [Fact]
    public void TrySaveStopLimits_ShouldPersistJson_AndLoadRoundTrip()
    {
        using PositionControllerStopLimitsFileScope scope = new PositionControllerStopLimitsFileScope("CodexPositionController");

        PositionController controller = scope.CreateWithoutConstructor();
        scope.Setup(controller);

        PositionOpenerToStopLimit stop = CreateSampleStop();

        scope.SetStopLimitsForSave(controller, new List<PositionOpenerToStopLimit> { stop });
        scope.InvokePrivateTrySaveStopLimits(controller);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        List<PositionOpenerToStopLimit> loaded = controller.LoadStopLimits();
        Assert.NotNull(loaded);
        Assert.Single(loaded);
        Assert.Equal("SBER", loaded[0].Security);
        Assert.Equal("TabA", loaded[0].TabName);
        Assert.Equal(15, loaded[0].Number);
        Assert.Equal(Side.Buy, loaded[0].Side);
    }

    [Fact]
    public void LoadStopLimits_ShouldSupportLegacyLineBasedFormat()
    {
        using PositionControllerStopLimitsFileScope scope = new PositionControllerStopLimitsFileScope("CodexPositionControllerLegacy");

        PositionOpenerToStopLimit stop = CreateSampleStop();
        File.WriteAllText(scope.SettingsPath, stop.GetSaveString());

        PositionController controller = scope.CreateWithoutConstructor();
        scope.Setup(controller);

        List<PositionOpenerToStopLimit> loaded = controller.LoadStopLimits();
        Assert.NotNull(loaded);
        Assert.Single(loaded);
        Assert.Equal("SBER", loaded[0].Security);
        Assert.Equal("TabA", loaded[0].TabName);
        Assert.Equal(15, loaded[0].Number);
        Assert.Equal(Side.Buy, loaded[0].Side);
    }

    private static PositionOpenerToStopLimit CreateSampleStop()
    {
        return new PositionOpenerToStopLimit
        {
            Security = "SBER",
            TabName = "TabA",
            Number = 15,
            LifeTimeType = PositionOpenerToStopLifeTimeType.NoLifeTime,
            PriceOrder = 100m,
            PriceRedLine = 99m,
            ActivateType = StopActivateType.HigherOrEqual,
            Volume = 2m,
            Side = Side.Buy,
            OrderPriceType = OrderPriceType.Limit,
            ExpiresBars = 5,
            OrderCreateBarNumber = 10,
            LastCandleTime = new DateTime(2025, 1, 1, 10, 0, 0),
            SignalType = "SignalA",
            TimeCreate = new DateTime(2025, 1, 1, 10, 1, 0),
            PositionNumber = 123
        };
    }

    private sealed class PositionControllerStopLimitsFileScope : IDisposable
    {
        private readonly string _name;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _trySaveStopLimitsMethod;
        private readonly FieldInfo _nameField;
        private readonly FieldInfo _startProgramField;
        private readonly FieldInfo _actualStopLimitsField;
        private readonly FieldInfo _needToSaveStopLimitField;

        public PositionControllerStopLimitsFileScope(string name)
        {
            _name = name;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, $"{_name}DealControllerStopLimits.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _trySaveStopLimitsMethod = typeof(PositionController).GetMethod("TrySaveStopLimits", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method TrySaveStopLimits not found.");
            _nameField = typeof(PositionController).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _name not found.");
            _startProgramField = typeof(PositionController).GetField("_startProgram", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _startProgram not found.");
            _actualStopLimitsField = typeof(PositionController).GetField("_actualStopLimits", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _actualStopLimits not found.");
            _needToSaveStopLimitField = typeof(PositionController).GetField("_needToSaveStopLimit", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _needToSaveStopLimit not found.");

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

        public PositionController CreateWithoutConstructor()
        {
            return (PositionController)RuntimeHelpers.GetUninitializedObject(typeof(PositionController));
        }

        public void Setup(PositionController controller)
        {
            _nameField.SetValue(controller, _name);
            _startProgramField.SetValue(controller, StartProgram.IsOsTrader);
        }

        public void SetStopLimitsForSave(PositionController controller, List<PositionOpenerToStopLimit> limits)
        {
            _actualStopLimitsField.SetValue(controller, limits);
            _needToSaveStopLimitField.SetValue(controller, true);
        }

        public void InvokePrivateTrySaveStopLimits(PositionController controller)
        {
            _trySaveStopLimitsMethod.Invoke(controller, null);
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
