#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Entity;
using OsEngine.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class AindicatorPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_ForParametersAndSeries()
    {
        const string name = "CodexAindicatorJson";
        using AindicatorFileScope scope = new AindicatorFileScope(name);

        TestIndicator indicator = new TestIndicator
        {
            Name = name,
            StartProgram = StartProgram.IsOsTrader
        };
        indicator.CreateParameterInt("Length", 14);
        indicator.CreateSeries("Main", Color.Red, IndicatorChartPaintType.Line, true);

        indicator.Save();

        string parametersContent = File.ReadAllText(scope.ParametersPath);
        Assert.StartsWith("{", parametersContent.TrimStart());

        string valuesContent = File.ReadAllText(scope.ValuesPath);
        Assert.StartsWith("{", valuesContent.TrimStart());
    }

    [Fact]
    public void ParseLegacyLinesSettings_ShouldSupportLineBasedFormat()
    {
        const string name = "CodexAindicatorLegacy";
        using AindicatorFileScope scope = new AindicatorFileScope(name);

        object settings = scope.ParseLegacy("A#1\nB#2\n");
        Assert.NotNull(settings);

        List<string> lines = scope.ExtractLines(settings);
        Assert.Equal(2, lines.Count);
        Assert.Equal("A#1", lines[0]);
        Assert.Equal("B#2", lines[1]);
    }

    private sealed class TestIndicator : Aindicator
    {
        public override void OnStateChange(IndicatorState state)
        {
        }

        public override void OnProcess(List<Candle> source, int index)
        {
        }
    }

    private sealed class AindicatorFileScope : IDisposable
    {
        private readonly string _name;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _parametersFileExisted;
        private readonly bool _valuesFileExisted;
        private readonly string _parametersBackupPath;
        private readonly string _valuesBackupPath;
        private readonly MethodInfo _parseLegacyMethod;

        public AindicatorFileScope(string name)
        {
            _name = name;
            _engineDirPath = Path.GetFullPath("Engine");
            ParametersPath = Path.Combine(_engineDirPath, _name + "Parametrs.txt");
            ValuesPath = Path.Combine(_engineDirPath, _name + "Values.txt");
            _parametersBackupPath = ParametersPath + ".codex.bak";
            _valuesBackupPath = ValuesPath + ".codex.bak";

            _parseLegacyMethod = typeof(Aindicator).GetMethod("ParseLegacyLinesSettings", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ParseLegacyLinesSettings not found.");

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _parametersFileExisted = File.Exists(ParametersPath);
            if (_parametersFileExisted)
            {
                File.Copy(ParametersPath, _parametersBackupPath, overwrite: true);
            }
            else if (File.Exists(_parametersBackupPath))
            {
                File.Delete(_parametersBackupPath);
            }

            _valuesFileExisted = File.Exists(ValuesPath);
            if (_valuesFileExisted)
            {
                File.Copy(ValuesPath, _valuesBackupPath, overwrite: true);
            }
            else if (File.Exists(_valuesBackupPath))
            {
                File.Delete(_valuesBackupPath);
            }
        }

        public string ParametersPath { get; }

        public string ValuesPath { get; }

        public object ParseLegacy(string content)
        {
            return _parseLegacyMethod.Invoke(null, new object[] { content })!;
        }

        public List<string> ExtractLines(object settings)
        {
            PropertyInfo linesProperty = settings.GetType().GetProperty("Lines", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Property Lines not found.");
            IEnumerable<string> values = (IEnumerable<string>)linesProperty.GetValue(settings)!;
            return values.ToList();
        }

        public void Dispose()
        {
            RestoreFile(ParametersPath, _parametersBackupPath, _parametersFileExisted);
            RestoreFile(ValuesPath, _valuesBackupPath, _valuesFileExisted);

            if (!_engineDirExisted
                && Directory.Exists(_engineDirPath)
                && !Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
            {
                Directory.Delete(_engineDirPath);
            }
        }

        private static void RestoreFile(string path, string backupPath, bool fileExisted)
        {
            if (fileExisted)
            {
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, path, overwrite: true);
                    File.Delete(backupPath);
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
        }
    }
}
