#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

public class BotTabPolygonNamesPersistenceTests
{
    [Fact]
    public void SaveSequencesNames_ShouldPersistToml()
    {
        const string tabName = "CodexPolygonNamesJson";
        using BotTabPolygonNamesFileScope scope = new BotTabPolygonNamesFileScope(tabName);

        BotTabPolygon source = scope.CreateWithoutConstructor();
        source.Sequences = new List<PolygonToTrade>
        {
            scope.CreateSequenceWithoutConstructor("POLYGON_A"),
            scope.CreateSequenceWithoutConstructor("POLYGON_B")
        };

        source.SaveSequencesNames();

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("SequenceNames =", content);
    }

    [Fact]
    public void ParseLegacyPolygonNamesToLoadSettings_ShouldSupportLineBasedFormat()
    {
        const string tabName = "CodexPolygonNamesLegacy";
        using BotTabPolygonNamesFileScope scope = new BotTabPolygonNamesFileScope(tabName);

        string legacy = string.Join("\n", "POLYGON_X", "POLYGON_Y", "POLYGON_Z") + "\n";

        object settings = scope.ParseLegacy(legacy);
        Assert.NotNull(settings);

        List<string> names = scope.ExtractNames(settings);
        Assert.Equal(3, names.Count);
        Assert.Equal("POLYGON_X", names[0]);
        Assert.Equal("POLYGON_Y", names[1]);
        Assert.Equal("POLYGON_Z", names[2]);
    }

    private sealed class BotTabPolygonNamesFileScope : IDisposable
    {
        private readonly string _tabName;
        private readonly MethodInfo _parseLegacyMethod;
        private readonly StructuredSettingsFileScope _settingsScope;

        public BotTabPolygonNamesFileScope(string tabName)
        {
            _tabName = tabName;
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", _tabName + "PolygonsNamesToLoad.toml"));

            _parseLegacyMethod = typeof(BotTabPolygon).GetMethod(
                "ParseLegacyPolygonNamesToLoadSettings",
                BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ParseLegacyPolygonNamesToLoadSettings not found.");

        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public BotTabPolygon CreateWithoutConstructor()
        {
            BotTabPolygon tab = (BotTabPolygon)RuntimeHelpers.GetUninitializedObject(typeof(BotTabPolygon));
            tab.TabName = _tabName;
            tab.Sequences = new List<PolygonToTrade>();
            return tab;
        }

        public PolygonToTrade CreateSequenceWithoutConstructor(string name)
        {
            PolygonToTrade sequence = (PolygonToTrade)RuntimeHelpers.GetUninitializedObject(typeof(PolygonToTrade));
            sequence.Name = name;
            return sequence;
        }

        public object ParseLegacy(string content)
        {
            return _parseLegacyMethod.Invoke(null, new object[] { content })!;
        }

        public List<string> ExtractNames(object settings)
        {
            PropertyInfo namesProperty = settings.GetType().GetProperty("SequenceNames", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Property SequenceNames not found.");
            IEnumerable<string> values = (IEnumerable<string>)namesProperty.GetValue(settings)!;
            return values.ToList();
        }

        public void Dispose()
        {
            _settingsScope.Dispose();
        }
    }
}
