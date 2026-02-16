using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class CandleConverterPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using CandleConverterFileScope scope = new CandleConverterFileScope();

        CandleConverter source = scope.CreateWithoutConstructor();
        source.TimeFrame = TimeFrame.Min30;
        scope.SetSourceFile(source, "C:\\data\\source.txt");
        scope.SetExitFile(source, "C:\\data\\result.txt");
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        CandleConverter loaded = scope.CreateWithoutConstructor();
        loaded.Load();

        Assert.Equal(TimeFrame.Min30, loaded.TimeFrame);
        Assert.Equal("C:\\data\\source.txt", scope.GetSourceFile(loaded));
        Assert.Equal("C:\\data\\result.txt", scope.GetExitFile(loaded));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using CandleConverterFileScope scope = new CandleConverterFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "Min5",
            "legacy_source.csv",
            "legacy_exit.csv"
        });

        CandleConverter loaded = scope.CreateWithoutConstructor();
        loaded.Load();

        Assert.Equal(TimeFrame.Min5, loaded.TimeFrame);
        Assert.Equal("legacy_source.csv", scope.GetSourceFile(loaded));
        Assert.Equal("legacy_exit.csv", scope.GetExitFile(loaded));
    }

    private sealed class CandleConverterFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _sourceFileField;
        private readonly FieldInfo _exitFileField;

        public CandleConverterFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "CandleConverter.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _sourceFileField = typeof(CandleConverter).GetField("_sourceFile", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _sourceFile not found.");
            _exitFileField = typeof(CandleConverter).GetField("_exitFile", BindingFlags.NonPublic | BindingFlags.Instance)
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

        public CandleConverter CreateWithoutConstructor()
        {
            return (CandleConverter)RuntimeHelpers.GetUninitializedObject(typeof(CandleConverter));
        }

        public void SetSourceFile(CandleConverter converter, string value)
        {
            _sourceFileField.SetValue(converter, value);
        }

        public void SetExitFile(CandleConverter converter, string value)
        {
            _exitFileField.SetValue(converter, value);
        }

        public string GetSourceFile(CandleConverter converter)
        {
            return (string)_sourceFileField.GetValue(converter)!;
        }

        public string GetExitFile(CandleConverter converter)
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
