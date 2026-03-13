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
    public void Save_ShouldPersistToml_ForParametersAndSeries()
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

        string parametersContent = File.ReadAllText(scope.ParametersCanonicalPath);
        Assert.Contains("Lines =", parametersContent);

        string valuesContent = File.ReadAllText(scope.ValuesCanonicalPath);
        Assert.Contains("Lines =", valuesContent);
    }

    [Fact]
    public void Save_ShouldReadLegacyTxt_AndWriteToml_ForParametersAndSeries()
    {
        const string name = "CodexAindicatorLegacyRoundTrip";
        using AindicatorFileScope scope = new AindicatorFileScope(name);

        File.WriteAllText(scope.ParametersLegacyTxtPath, "Length#42");
        File.WriteAllText(scope.ValuesLegacyTxtPath, "Main&Color [Red]&False&True&0");

        TestIndicator indicator = new TestIndicator
        {
            Name = name,
            StartProgram = StartProgram.IsOsTrader
        };

        IndicatorParameterInt length = indicator.CreateParameterInt("Length", 14);
        IndicatorDataSeries series = indicator.CreateSeries("Main", Color.Red, IndicatorChartPaintType.Line, true);

        Assert.Equal(42, length.ValueInt);
        Assert.Equal("Main", series.Name);
        Assert.Equal(Color.Red.ToArgb(), series.Color.ToArgb());
        Assert.Equal(IndicatorChartPaintType.Line, series.ChartPaintType);
        Assert.True(series.IsPaint);

        indicator.Save();

        Assert.True(File.Exists(scope.ParametersCanonicalPath));
        Assert.True(File.Exists(scope.ValuesCanonicalPath));
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
        private readonly MethodInfo _parseLegacyMethod;
        private readonly StructuredSettingsFileScope _parametersScope;
        private readonly StructuredSettingsFileScope _valuesScope;
        private readonly StructuredSettingsFileScope _baseScope;

        public AindicatorFileScope(string name)
        {
            _parametersScope = new StructuredSettingsFileScope(Path.Combine("Engine", name + "Parametrs.toml"));
            _valuesScope = new StructuredSettingsFileScope(Path.Combine("Engine", name + "Values.toml"));
            _baseScope = new StructuredSettingsFileScope(Path.Combine("Engine", name + "Base.toml"));

            _parseLegacyMethod = typeof(Aindicator).GetMethod("ParseLegacyLinesSettings", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ParseLegacyLinesSettings not found.");
        }

        public string ParametersCanonicalPath => _parametersScope.CanonicalPath;

        public string ParametersLegacyTxtPath => _parametersScope.LegacyTxtPath;

        public string ValuesCanonicalPath => _valuesScope.CanonicalPath;

        public string ValuesLegacyTxtPath => _valuesScope.LegacyTxtPath;

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
            _parametersScope.Dispose();
            _valuesScope.Dispose();
            _baseScope.Dispose();
        }
    }
}
