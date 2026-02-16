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

public class PositionControllerDealsPersistenceTests
{
    [Fact]
    public void SavePositions_ShouldPersistJson_AndLoadRoundTrip()
    {
        using PositionControllerDealsFileScope scope = new PositionControllerDealsFileScope("CodexPositionControllerDeals");

        PositionController source = scope.CreateWithoutConstructor();
        scope.SetupForLoad(source);

        Position position = CreateSamplePosition();
        scope.SetupForSave(source, new List<Position> { position }, CommissionType.Percent, 0.15m);
        scope.InvokePrivateSavePositions(source);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        PositionController target = scope.CreateWithoutConstructor();
        scope.SetupForLoad(target);
        scope.InvokePrivateLoad(target);

        Assert.Equal(CommissionType.Percent, scope.GetCommissionType(target));
        Assert.Equal(0.15m, scope.GetCommissionValue(target));

        List<Position> loaded = scope.GetDeals(target);
        Assert.NotNull(loaded);
        Assert.Single(loaded);
        Assert.Equal(position.Number, loaded[0].Number);
        Assert.Equal(position.SecurityName, loaded[0].SecurityName);
        Assert.Equal(position.NameBot, loaded[0].NameBot);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using PositionControllerDealsFileScope scope = new PositionControllerDealsFileScope("CodexPositionControllerDealsLegacy");

        Position position = CreateSamplePosition();
        string legacyContent = $"{CommissionType.OneLotFix}\n0.33\n{position.GetStringForSave()}\n";
        File.WriteAllText(scope.SettingsPath, legacyContent);

        PositionController target = scope.CreateWithoutConstructor();
        scope.SetupForLoad(target);
        scope.InvokePrivateLoad(target);

        Assert.Equal(CommissionType.OneLotFix, scope.GetCommissionType(target));
        Assert.Equal(0.33m, scope.GetCommissionValue(target));

        List<Position> loaded = scope.GetDeals(target);
        Assert.NotNull(loaded);
        Assert.Single(loaded);
        Assert.Equal(position.Number, loaded[0].Number);
        Assert.Equal(position.SecurityName, loaded[0].SecurityName);
    }

    private static Position CreateSamplePosition()
    {
        return new Position
        {
            Number = 321,
            NameBot = "BotA",
            SecurityName = "SBER",
            Direction = Side.Buy,
            State = PositionStateType.Open,
            StopOrderPrice = 99.75m,
            ProfitOrderPrice = 101.25m,
            PriceStep = 0.01m,
            PriceStepCost = 0.01m,
            ProfitOperationAbs = 1.0m,
            ProfitOperationPercent = 0.5m,
            PortfolioValueOnOpenPosition = 100000m,
            CommissionValue = 0.15m,
            CommissionType = CommissionType.Percent
        };
    }

    private sealed class PositionControllerDealsFileScope : IDisposable
    {
        private readonly string _name;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _savePositionsMethod;
        private readonly MethodInfo _loadMethod;
        private readonly FieldInfo _nameField;
        private readonly FieldInfo _startProgramField;
        private readonly FieldInfo _dealsField;
        private readonly FieldInfo _needToSaveField;
        private readonly FieldInfo _openPositionsField;
        private readonly FieldInfo _positionsToPaintField;
        private readonly FieldInfo _commissionTypeField;
        private readonly FieldInfo _commissionValueField;

        public PositionControllerDealsFileScope(string name)
        {
            _name = name;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, $"{_name}DealController.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _savePositionsMethod = typeof(PositionController).GetMethod("SavePositions", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SavePositions not found.");
            _loadMethod = typeof(PositionController).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");
            _nameField = typeof(PositionController).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _name not found.");
            _startProgramField = typeof(PositionController).GetField("_startProgram", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _startProgram not found.");
            _dealsField = typeof(PositionController).GetField("_deals", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _deals not found.");
            _needToSaveField = typeof(PositionController).GetField("_needToSave", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _needToSave not found.");
            _openPositionsField = typeof(PositionController).GetField("_openPositions", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _openPositions not found.");
            _positionsToPaintField = typeof(PositionController).GetField("_positionsToPaint", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _positionsToPaint not found.");
            _commissionTypeField = typeof(PositionController).GetField("_commissionType", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _commissionType not found.");
            _commissionValueField = typeof(PositionController).GetField("_commissionValue", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _commissionValue not found.");

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

        public void SetupForLoad(PositionController controller)
        {
            _nameField.SetValue(controller, _name);
            _startProgramField.SetValue(controller, StartProgram.IsOsTrader);
            _openPositionsField.SetValue(controller, new List<Position>());
            _positionsToPaintField.SetValue(controller, new List<Position>());
            _dealsField.SetValue(controller, new List<Position>());
        }

        public void SetupForSave(
            PositionController controller,
            List<Position> deals,
            CommissionType commissionType,
            decimal commissionValue)
        {
            _dealsField.SetValue(controller, deals);
            _commissionTypeField.SetValue(controller, commissionType);
            _commissionValueField.SetValue(controller, commissionValue);
            _needToSaveField.SetValue(controller, true);
        }

        public void InvokePrivateSavePositions(PositionController controller)
        {
            _savePositionsMethod.Invoke(controller, null);
        }

        public void InvokePrivateLoad(PositionController controller)
        {
            _loadMethod.Invoke(controller, null);
        }

        public List<Position> GetDeals(PositionController controller)
        {
            List<Position>? deals = _dealsField.GetValue(controller) as List<Position>;
            return deals == null ? new List<Position>() : deals;
        }

        public CommissionType GetCommissionType(PositionController controller)
        {
            object? value = _commissionTypeField.GetValue(controller);
            return value is CommissionType commissionType ? commissionType : CommissionType.None;
        }

        public decimal GetCommissionValue(PositionController controller)
        {
            object? value = _commissionValueField.GetValue(controller);
            return value is decimal commissionValue ? commissionValue : 0m;
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
