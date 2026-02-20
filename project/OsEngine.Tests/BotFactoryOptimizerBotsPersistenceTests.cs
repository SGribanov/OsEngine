#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Robots;
using Xunit;

namespace OsEngine.Tests;

[Collection("BotFactoryOptimizerBotsPersistence")]
public class BotFactoryOptimizerBotsPersistenceTests
{
    [Fact]
    public void SaveOptimizerBotsNamesToFile_ShouldPersistJson_AndLoadRoundTrip()
    {
        using BotFactoryOptimizerBotsFileScope scope = new BotFactoryOptimizerBotsFileScope();

        List<string> expected = new List<string> { "BotOne", "BotTwo", "BotThree" };
        scope.SetOptimizerBots(expected);
        scope.InvokeSaveOptimizerBotsNamesToFile();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        scope.SetOptimizerBots(new List<string>());
        scope.InvokeLoadOptimizerBotsNamesFromFile();

        List<string> loaded = scope.GetOptimizerBots();
        Assert.Equal(expected, loaded);
    }

    [Fact]
    public void LoadOptimizerBotsNamesFromFile_ShouldSupportLegacyLineBasedFormat()
    {
        using BotFactoryOptimizerBotsFileScope scope = new BotFactoryOptimizerBotsFileScope();

        File.WriteAllText(scope.SettingsPath, "LegacyOne\nLegacyTwo\n");
        scope.SetOptimizerBots(new List<string> { "StaleValue" });

        scope.InvokeLoadOptimizerBotsNamesFromFile();

        List<string> loaded = scope.GetOptimizerBots();
        Assert.Equal(new List<string> { "LegacyOne", "LegacyTwo" }, loaded);
    }

    private sealed class BotFactoryOptimizerBotsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _loadMethod;
        private readonly MethodInfo _saveMethod;
        private readonly FieldInfo _optimizerBotsField;
        private readonly bool _needToReloadBackup;
        private readonly List<string> _optimizerBotsBackup;

        public BotFactoryOptimizerBotsFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "OptimizerBots.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _loadMethod = typeof(BotFactory).GetMethod("LoadOptimizerBotsNamesFromFile", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method LoadOptimizerBotsNamesFromFile not found.");
            _saveMethod = typeof(BotFactory).GetMethod("SaveOptimizerBotsNamesToFile", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method SaveOptimizerBotsNamesToFile not found.");
            _optimizerBotsField = typeof(BotFactory).GetField("_optimizerBotsWithParam", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field _optimizerBotsWithParam not found.");

            _needToReloadBackup = BotFactory.NeedToReloadOptimizerBots;
            _optimizerBotsBackup = GetOptimizerBots();

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

        public void SetOptimizerBots(List<string> names)
        {
            _optimizerBotsField.SetValue(null, new List<string>(names));
        }

        public List<string> GetOptimizerBots()
        {
            List<string>? value = _optimizerBotsField.GetValue(null) as List<string>;
            return value == null ? new List<string>() : new List<string>(value);
        }

        public void InvokeLoadOptimizerBotsNamesFromFile()
        {
            _loadMethod.Invoke(null, null);
        }

        public void InvokeSaveOptimizerBotsNamesToFile()
        {
            _saveMethod.Invoke(null, null);
        }

        public void Dispose()
        {
            BotFactory.NeedToReloadOptimizerBots = _needToReloadBackup;
            _optimizerBotsField.SetValue(null, new List<string>(_optimizerBotsBackup));

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

[CollectionDefinition("BotFactoryOptimizerBotsPersistence", DisableParallelization = true)]
public class BotFactoryOptimizerBotsPersistenceCollectionDefinition
{
}
