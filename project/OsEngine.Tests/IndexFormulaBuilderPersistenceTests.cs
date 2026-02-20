#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

public class IndexFormulaBuilderPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string botName = "CodexIndexFormulaJson";
        using IndexFormulaBuilderFileScope scope = new IndexFormulaBuilderFileScope(botName);

        IndexFormulaBuilder source = scope.CreateWithoutConstructor();
        scope.SetField(source, "_regime", IndexAutoFormulaBuilderRegime.OncePerWeek);
        scope.SetField(source, "_dayOfWeekToRebuildIndex", DayOfWeek.Friday);
        scope.SetField(source, "_hourInDayToRebuildIndex", 13);
        scope.SetField(source, "_indexSecCount", 8);
        scope.SetField(source, "_daysLookBackInBuilding", 50);
        scope.SetField(source, "_indexMultType", IndexMultType.VolumeWeighted);
        scope.SetField(source, "_indexSortType", SecuritySortType.MaxVolatilityWeighted);
        scope.SetField(source, "_lastTimeUpdateIndex", "2026-02-16 11:30:00");
        scope.SetField(source, "_writeLogMessageOnRebuild", false);
        scope.InvokePrivateSave(source);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        IndexFormulaBuilder loaded = scope.CreateWithoutConstructor();
        scope.InvokePrivateLoad(loaded);

        Assert.Equal(IndexAutoFormulaBuilderRegime.OncePerWeek, scope.GetField<IndexAutoFormulaBuilderRegime>(loaded, "_regime"));
        Assert.Equal(DayOfWeek.Friday, scope.GetField<DayOfWeek>(loaded, "_dayOfWeekToRebuildIndex"));
        Assert.Equal(13, scope.GetField<int>(loaded, "_hourInDayToRebuildIndex"));
        Assert.Equal(8, scope.GetField<int>(loaded, "_indexSecCount"));
        Assert.Equal(50, scope.GetField<int>(loaded, "_daysLookBackInBuilding"));
        Assert.Equal(IndexMultType.VolumeWeighted, scope.GetField<IndexMultType>(loaded, "_indexMultType"));
        Assert.Equal(SecuritySortType.MaxVolatilityWeighted, scope.GetField<SecuritySortType>(loaded, "_indexSortType"));
        Assert.Equal("2026-02-16 11:30:00", scope.GetField<string>(loaded, "_lastTimeUpdateIndex"));
        Assert.False(scope.GetField<bool>(loaded, "_writeLogMessageOnRebuild"));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string botName = "CodexIndexFormulaLegacy";
        using IndexFormulaBuilderFileScope scope = new IndexFormulaBuilderFileScope(botName);

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "OncePerDay",
            "Wednesday",
            "9",
            "4",
            "30",
            "EqualWeighted",
            "MinVolatilityWeighted",
            "2026-02-15 08:00:00",
            "True"
        });

        IndexFormulaBuilder loaded = scope.CreateWithoutConstructor();
        scope.InvokePrivateLoad(loaded);

        Assert.Equal(IndexAutoFormulaBuilderRegime.OncePerDay, scope.GetField<IndexAutoFormulaBuilderRegime>(loaded, "_regime"));
        Assert.Equal(DayOfWeek.Wednesday, scope.GetField<DayOfWeek>(loaded, "_dayOfWeekToRebuildIndex"));
        Assert.Equal(9, scope.GetField<int>(loaded, "_hourInDayToRebuildIndex"));
        Assert.Equal(4, scope.GetField<int>(loaded, "_indexSecCount"));
        Assert.Equal(30, scope.GetField<int>(loaded, "_daysLookBackInBuilding"));
        Assert.Equal(IndexMultType.EqualWeighted, scope.GetField<IndexMultType>(loaded, "_indexMultType"));
        Assert.Equal(SecuritySortType.MinVolatilityWeighted, scope.GetField<SecuritySortType>(loaded, "_indexSortType"));
        Assert.Equal("2026-02-15 08:00:00", scope.GetField<string>(loaded, "_lastTimeUpdateIndex"));
        Assert.True(scope.GetField<bool>(loaded, "_writeLogMessageOnRebuild"));
    }

    private sealed class IndexFormulaBuilderFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _botUniqNameField;
        private readonly FieldInfo _startProgramField;
        private readonly MethodInfo _saveMethod;
        private readonly MethodInfo _loadMethod;

        public IndexFormulaBuilderFileScope(string botUniqName)
        {
            BotUniqName = botUniqName;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, botUniqName + "IndexAutoFormulaSettings.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _botUniqNameField = typeof(IndexFormulaBuilder).GetField("_botUniqName", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _botUniqName not found.");
            _startProgramField = typeof(IndexFormulaBuilder).GetField("_startProgram", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _startProgram not found.");
            _saveMethod = typeof(IndexFormulaBuilder).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Save not found.");
            _loadMethod = typeof(IndexFormulaBuilder).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
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

        public string BotUniqName { get; }

        public string SettingsPath { get; }

        public IndexFormulaBuilder CreateWithoutConstructor()
        {
            IndexFormulaBuilder instance =
                (IndexFormulaBuilder)RuntimeHelpers.GetUninitializedObject(typeof(IndexFormulaBuilder));
            _botUniqNameField.SetValue(instance, BotUniqName);
            _startProgramField.SetValue(instance, StartProgram.IsOsTrader);
            return instance;
        }

        public void InvokePrivateSave(IndexFormulaBuilder builder)
        {
            _saveMethod.Invoke(builder, null);
        }

        public void InvokePrivateLoad(IndexFormulaBuilder builder)
        {
            _loadMethod.Invoke(builder, null);
        }

        public void SetField(IndexFormulaBuilder builder, string fieldName, object? value)
        {
            FieldInfo field = typeof(IndexFormulaBuilder).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);
            field.SetValue(builder, value);
        }

        public T GetField<T>(IndexFormulaBuilder builder, string fieldName)
        {
            FieldInfo field = typeof(IndexFormulaBuilder).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);
            return (T)field.GetValue(builder)!;
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
