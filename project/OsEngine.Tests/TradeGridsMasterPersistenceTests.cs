#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.OsTrader.Grids;
using Xunit;

namespace OsEngine.Tests;

public class TradeGridsMasterPersistenceTests
{
    [Fact]
    public void SaveGrids_ShouldPersistToml()
    {
        const string botName = "CodexTradeGridsJson";
        using TradeGridsMasterFileScope scope = new TradeGridsMasterFileScope(botName);

        TradeGridsMaster source = scope.CreateMasterWithoutConstructor();
        source.TradeGrids.Add(scope.CreateTradeGridStub(1));

        scope.InvokePrivateSaveGrids(source);

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("GridSaveStrings =", content);
    }

    [Fact]
    public void ParseLegacyGridsSettings_ShouldSupportLineBasedFormat()
    {
        const string botName = "CodexTradeGridsLegacy";
        using TradeGridsMasterFileScope scope = new TradeGridsMasterFileScope(botName);

        string legacy = string.Join("\n", "1@A%", "2@B%", "3@C%") + "\n";

        object settings = scope.ParseLegacy(legacy);
        Assert.NotNull(settings);

        List<string> gridStrings = scope.ExtractGridStrings(settings);
        Assert.Equal(3, gridStrings.Count);
        Assert.Equal("1@A%", gridStrings[0]);
        Assert.Equal("2@B%", gridStrings[1]);
        Assert.Equal("3@C%", gridStrings[2]);
    }

    [Fact]
    public void TryExtractGridNumber_ShouldParseValidPrefix()
    {
        const string botName = "CodexTradeGridsNumberParseValid";
        using TradeGridsMasterFileScope scope = new TradeGridsMasterFileScope(botName);

        bool parsed = scope.TryExtractGridNumber("12@GridPayload", out int number);

        Assert.True(parsed);
        Assert.Equal(12, number);
    }

    [Fact]
    public void TryExtractGridNumber_ShouldReturnFalse_OnMalformedPrefix()
    {
        const string botName = "CodexTradeGridsNumberParseMalformed";
        using TradeGridsMasterFileScope scope = new TradeGridsMasterFileScope(botName);

        bool parsed = scope.TryExtractGridNumber("@GridPayload", out int number);

        Assert.False(parsed);
        Assert.Equal(0, number);
    }

    private sealed class TradeGridsMasterFileScope : IDisposable
    {
        private readonly string _botName;
        private readonly MethodInfo _saveGridsMethod;
        private readonly MethodInfo _parseLegacyMethod;
        private readonly MethodInfo _tryExtractGridNumberMethod;
        private readonly FieldInfo _masterStartProgramField;
        private readonly FieldInfo _masterNameBotField;
        private readonly FieldInfo _gridFirstTradePriceField;
        private readonly FieldInfo _gridOpenPositionsBySessionField;
        private readonly FieldInfo _gridFirstTradeTimeField;
        private readonly StructuredSettingsFileScope _settingsScope;

        public TradeGridsMasterFileScope(string botName)
        {
            _botName = botName;
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", _botName + "GridsSettings.toml"));

            _saveGridsMethod = typeof(TradeGridsMaster).GetMethod("SaveGrids", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SaveGrids not found.");
            _parseLegacyMethod = typeof(TradeGridsMaster).GetMethod(
                "ParseLegacyGridsSettings",
                BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ParseLegacyGridsSettings not found.");
            _tryExtractGridNumberMethod = typeof(TradeGridsMaster).GetMethod(
                "TryExtractGridNumber",
                BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method TryExtractGridNumber not found.");
            _masterStartProgramField = typeof(TradeGridsMaster).GetField("_startProgram", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _startProgram not found.");
            _masterNameBotField = typeof(TradeGridsMaster).GetField("_nameBot", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _nameBot not found.");
            _gridFirstTradePriceField = typeof(TradeGrid).GetField("_firstTradePrice", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _firstTradePrice not found.");
            _gridOpenPositionsBySessionField = typeof(TradeGrid).GetField("_openPositionsBySession", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _openPositionsBySession not found.");
            _gridFirstTradeTimeField = typeof(TradeGrid).GetField("_firstTradeTime", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _firstTradeTime not found.");

        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public TradeGridsMaster CreateMasterWithoutConstructor()
        {
            TradeGridsMaster master = (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
            master.TradeGrids = new List<TradeGrid>();
            _masterStartProgramField.SetValue(master, StartProgram.IsOsTrader);
            _masterNameBotField.SetValue(master, _botName);
            return master;
        }

        public TradeGrid CreateTradeGridStub(int number)
        {
            TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

            grid.Number = number;
            grid.GridType = TradeGridPrimeType.MarketMaking;
            grid.Regime = TradeGridRegime.Off;
            grid.RegimeLogicEntry = TradeGridLogicEntryRegime.OnTrade;
            grid.AutoClearJournalIsOn = false;
            grid.MaxClosePositionsInJournal = 100;
            grid.MaxOpenOrdersInMarket = 5;
            grid.MaxCloseOrdersInMarket = 5;
            grid.DelayInReal = 500;
            grid.CheckMicroVolumes = true;
            grid.MaxDistanceToOrdersPercent = 0;
            grid.OpenOrdersMakerOnly = true;

            _gridFirstTradePriceField.SetValue(grid, 0m);
            _gridOpenPositionsBySessionField.SetValue(grid, 0);
            _gridFirstTradeTimeField.SetValue(grid, DateTime.MinValue);

            grid.NonTradePeriods = new TradeGridNonTradePeriods("CodexGrid" + number);
            grid.StopBy = new TradeGridStopBy();
            grid.StopAndProfit = new TradeGridStopAndProfit();
            grid.AutoStarter = new TradeGridAutoStarter();
            grid.GridCreator = new TradeGridCreator();
            grid.ErrorsReaction = new TradeGridErrorsReaction(grid);
            grid.TrailingUp = new TrailingUp(grid);

            return grid;
        }

        public void InvokePrivateSaveGrids(TradeGridsMaster master)
        {
            _saveGridsMethod.Invoke(master, null);
        }

        public object ParseLegacy(string content)
        {
            return _parseLegacyMethod.Invoke(null, new object[] { content })!;
        }

        public List<string> ExtractGridStrings(object settings)
        {
            PropertyInfo gridSaveStringsProperty = settings.GetType().GetProperty("GridSaveStrings", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Property GridSaveStrings not found.");
            IEnumerable<string> values = (IEnumerable<string>)gridSaveStringsProperty.GetValue(settings)!;
            return values.ToList();
        }

        public bool TryExtractGridNumber(string payload, out int number)
        {
            object[] args = new object[] { payload, 0 };
            bool parsed = (bool)_tryExtractGridNumberMethod.Invoke(null, args)!;
            number = (int)args[1];
            return parsed;
        }

        public void Dispose()
        {
            _settingsScope.Dispose();
        }
    }
}
