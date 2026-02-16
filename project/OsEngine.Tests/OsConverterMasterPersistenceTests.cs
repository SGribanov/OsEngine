using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.OsConverter;
using Xunit;

namespace OsEngine.Tests;

public class OsConverterMasterPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using OsConverterFileScope scope = new OsConverterFileScope();

        OsConverterMaster source = scope.CreateWithoutConstructor();
        source.TimeFrame = TimeFrame.Min20;
        scope.SetSourceFile(source, "C:\\data\\legacy_source.csv");
        scope.SetExitFile(source, "C:\\data\\result.txt");
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        OsConverterMaster loaded = scope.CreateWithoutConstructor();
        loaded.Load();

        Assert.Equal(TimeFrame.Min20, loaded.TimeFrame);
        Assert.Equal("C:\\data\\legacy_source.csv", scope.GetSourceFile(loaded));
        Assert.Equal("C:\\data\\result.txt", scope.GetExitFile(loaded));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using OsConverterFileScope scope = new OsConverterFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "Min5",
            "legacy_source.csv",
            "legacy_exit.csv"
        });

        OsConverterMaster loaded = scope.CreateWithoutConstructor();
        loaded.Load();

        Assert.Equal(TimeFrame.Min5, loaded.TimeFrame);
        Assert.Equal("legacy_source.csv", scope.GetSourceFile(loaded));
        Assert.Equal("legacy_exit.csv", scope.GetExitFile(loaded));
    }

    private sealed class OsConverterFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _sourceFileField;
        private readonly FieldInfo _exitFileField;

        public OsConverterFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "Converter.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _sourceFileField = typeof(OsConverterMaster).GetField("_sourceFile", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _sourceFile not found.");
            _exitFileField = typeof(OsConverterMaster).GetField("_exitFile", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _exitFile not found.");

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

        public OsConverterMaster CreateWithoutConstructor()
        {
            OsConverterMaster converter = (OsConverterMaster)RuntimeHelpers.GetUninitializedObject(typeof(OsConverterMaster));
            converter.TimeFrame = TimeFrame.Sec1;
            return converter;
        }

        public void SetSourceFile(OsConverterMaster converter, string value)
        {
            _sourceFileField.SetValue(converter, value);
        }

        public void SetExitFile(OsConverterMaster converter, string value)
        {
            _exitFileField.SetValue(converter, value);
        }

        public string GetSourceFile(OsConverterMaster converter)
        {
            return (string)_sourceFileField.GetValue(converter)!;
        }

        public string GetExitFile(OsConverterMaster converter)
        {
            return (string)_exitFileField.GetValue(converter)!;
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
