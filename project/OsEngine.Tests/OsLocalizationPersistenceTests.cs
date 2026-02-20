#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Language;
using Xunit;

namespace OsEngine.Tests;

[Collection("OsLocalizationPersistence")]
public class OsLocalizationPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using OsLocalizationFileScope scope = new OsLocalizationFileScope();

        scope.SetState(OsLocalization.OsLocalType.Eng, "h:mm:ss tt", "M/d/yyyy");
        OsLocalization.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        scope.SetState(OsLocalization.OsLocalType.None, null, null);
        scope.InvokePrivateLoad();

        Assert.Equal(OsLocalization.OsLocalType.Eng, scope.GetLocalization());
        Assert.Equal("h:mm:ss tt", scope.GetLongTimePattern());
        Assert.Equal("M/d/yyyy", scope.GetShortDatePattern());
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using OsLocalizationFileScope scope = new OsLocalizationFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "Ru",
            "H:mm:ss",
            "dd.MM.yyyy"
        });

        scope.SetState(OsLocalization.OsLocalType.None, null, null);
        scope.InvokePrivateLoad();

        Assert.Equal(OsLocalization.OsLocalType.Ru, scope.GetLocalization());
        Assert.Equal("H:mm:ss", scope.GetLongTimePattern());
        Assert.Equal("dd.MM.yyyy", scope.GetShortDatePattern());
    }

    private sealed class OsLocalizationFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _localizationField;
        private readonly FieldInfo _longTimePatternField;
        private readonly FieldInfo _shortDatePatternField;
        private readonly MethodInfo _loadMethod;
        private readonly OsLocalization.OsLocalType _originalLocalization;
        private readonly string? _originalLongTimePattern;
        private readonly string? _originalShortDatePattern;

        public OsLocalizationFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "local.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _localizationField = typeof(OsLocalization).GetField("_curLocalization", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field _curLocalization not found.");
            _longTimePatternField = typeof(OsLocalization).GetField("_longTimePattern", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field _longTimePattern not found.");
            _shortDatePatternField = typeof(OsLocalization).GetField("_shortDatePattern", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field _shortDatePattern not found.");
            _loadMethod = typeof(OsLocalization).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method Load not found.");

            _originalLocalization = (OsLocalization.OsLocalType)_localizationField.GetValue(null)!;
            _originalLongTimePattern = _longTimePatternField.GetValue(null) as string;
            _originalShortDatePattern = _shortDatePatternField.GetValue(null) as string;

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

        public string SettingsPath { get; }

        public void SetState(OsLocalization.OsLocalType localization, string? longTimePattern, string? shortDatePattern)
        {
            _localizationField.SetValue(null, localization);
            _longTimePatternField.SetValue(null, longTimePattern);
            _shortDatePatternField.SetValue(null, shortDatePattern);
        }

        public OsLocalization.OsLocalType GetLocalization()
        {
            return (OsLocalization.OsLocalType)_localizationField.GetValue(null)!;
        }

        public string? GetLongTimePattern()
        {
            return (string)_longTimePatternField.GetValue(null)!;
        }

        public string? GetShortDatePattern()
        {
            return (string)_shortDatePatternField.GetValue(null)!;
        }

        public void InvokePrivateLoad()
        {
            _loadMethod.Invoke(null, null);
        }

        public void Dispose()
        {
            SetState(_originalLocalization, _originalLongTimePattern, _originalShortDatePattern);

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

[CollectionDefinition("OsLocalizationPersistence", DisableParallelization = true)]
public class OsLocalizationPersistenceCollectionDefinition
{
}
