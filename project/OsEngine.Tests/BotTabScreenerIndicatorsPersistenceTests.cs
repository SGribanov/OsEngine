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

public class BotTabScreenerIndicatorsPersistenceTests
{
    [Fact]
    public void SaveIndicators_ShouldPersistToml_AndLoadRoundTrip()
    {
        using BotTabScreenerIndicatorsFileScope scope = new BotTabScreenerIndicatorsFileScope("CodexScreenerIndicators");

        BotTabScreener source = scope.CreateWithoutConstructor();
        scope.Setup(source);
        source._indicators.Add(new IndicatorOnTabs
        {
            Type = "Sma",
            NameArea = "Prime",
            Num = 1,
            CanDelete = false,
            Parameters = new List<string> { "10", "20" }
        });

        scope.InvokePrivateSaveIndicators(source);

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("Indicators =", content);

        BotTabScreener target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        scope.InvokePrivateLoadIndicators(target);

        Assert.Single(target._indicators);
        IndicatorOnTabs loaded = target._indicators[0];
        Assert.Equal("Sma", loaded.Type);
        Assert.Equal("Prime", loaded.NameArea);
        Assert.Equal(1, loaded.Num);
        Assert.False(loaded.CanDelete);
        Assert.Equal(new List<string> { "10", "20" }, loaded.Parameters);
    }

    [Fact]
    public void LoadIndicators_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        using BotTabScreenerIndicatorsFileScope scope = new BotTabScreenerIndicatorsFileScope("CodexScreenerLegacy");

        IndicatorOnTabs legacy = new IndicatorOnTabs
        {
            Type = "Rsi",
            NameArea = "Prime",
            Num = 2,
            CanDelete = true,
            Parameters = new List<string> { "14" }
        };
        File.WriteAllText(scope.LegacyTxtPath, legacy.GetSaveStr());

        BotTabScreener target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        scope.InvokePrivateLoadIndicators(target);

        Assert.Single(target._indicators);
        IndicatorOnTabs loaded = target._indicators[0];
        Assert.Equal("Rsi", loaded.Type);
        Assert.Equal(2, loaded.Num);
        Assert.True(loaded.CanDelete);
        Assert.Equal(new List<string> { "14" }, loaded.Parameters);

        scope.InvokePrivateSaveIndicators(target);
        Assert.True(File.Exists(scope.CanonicalPath));
    }

    private sealed class BotTabScreenerIndicatorsFileScope : IDisposable
    {
        private readonly string _tabName;
        private readonly MethodInfo _saveIndicatorsMethod;
        private readonly MethodInfo _loadIndicatorsMethod;
        private readonly StructuredSettingsFileScope _settingsScope;

        public BotTabScreenerIndicatorsFileScope(string tabName)
        {
            _tabName = tabName;
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", $"{_tabName}ScreenerIndicators.toml"));

            _saveIndicatorsMethod = typeof(BotTabScreener).GetMethod("SaveIndicators", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SaveIndicators not found.");
            _loadIndicatorsMethod = typeof(BotTabScreener).GetMethod("LoadIndicators", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadIndicators not found.");

        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

        public BotTabScreener CreateWithoutConstructor()
        {
            return (BotTabScreener)RuntimeHelpers.GetUninitializedObject(typeof(BotTabScreener));
        }

        public void Setup(BotTabScreener tab)
        {
            tab.TabName = _tabName;
            tab._indicators = new List<IndicatorOnTabs>();
        }

        public void InvokePrivateSaveIndicators(BotTabScreener tab)
        {
            _saveIndicatorsMethod.Invoke(tab, null);
        }

        public void InvokePrivateLoadIndicators(BotTabScreener tab)
        {
            _loadIndicatorsMethod.Invoke(tab, null);
        }

        public void Dispose()
        {
            _settingsScope.Dispose();
        }
    }
}
