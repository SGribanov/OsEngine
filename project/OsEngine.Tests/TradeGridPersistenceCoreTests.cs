#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.OsTrader.Grids;
using Xunit;

namespace OsEngine.Tests;

public class TradeGridPersistenceCoreTests
{
    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_LoadFromString_WithReservedTail_ShouldParseRegimes()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");

        string payload = "CloseOnly@OffAndCancelOrders@@@@@";
        periods.LoadFromString(payload);

        Assert.Equal(TradeGridRegime.CloseOnly, periods.NonTradePeriod1Regime);
        Assert.Equal(TradeGridRegime.OffAndCancelOrders, periods.NonTradePeriod2Regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_LoadFromString_LegacyShortTail_ShouldKeepDefaultTimeSection()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter();

        string payload = "HigherOrEqual@123.45@On_ShiftOnNewPrice@1.5";
        autoStarter.LoadFromString(payload);

        Assert.Equal(TradeGridAutoStartRegime.HigherOrEqual, autoStarter.AutoStartRegime);
        Assert.Equal(123.45m, autoStarter.AutoStartPrice);
        Assert.Equal(GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice, autoStarter.RebuildGridRegime);
        Assert.Equal(1.5m, autoStarter.ShiftFirstPrice);
        Assert.False(autoStarter.StartGridByTimeOfDayIsOn);
        Assert.Equal(14, autoStarter.StartGridByTimeOfDayHour);
        Assert.Equal(15, autoStarter.StartGridByTimeOfDayMinute);
        Assert.Equal(0, autoStarter.StartGridByTimeOfDaySecond);
        Assert.True(autoStarter.SingleActivationMode);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_LegacyShortTail_ShouldFallbackOptionalDefaults()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid);

        string payload = "True@@11@@13@False";
        reaction.LoadFromString(payload);

        Assert.True(reaction.FailOpenOrdersReactionIsOn);
        Assert.Equal(11, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(13, reaction.FailCancelOrdersCountToReaction);
        Assert.False(reaction.FailCancelOrdersReactionIsOn);
        Assert.True(reaction.WaitOnStartConnectorIsOn);
        Assert.Equal(30, reaction.WaitSecondsOnStartConnector);
        Assert.True(reaction.ReduceOrdersCountInMarketOnNoFundsError);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_LegacyWithoutMoveFlags_ShouldKeepDefaultMoveFlags()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpCanMoveExitOrder = false,
            TrailingDownCanMoveExitOrder = false,
        };

        string payload = "True@1.25@10@False@0.5@5";
        trailing.LoadFromString(payload);

        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
        Assert.False(trailing.TrailingUpCanMoveExitOrder);
        Assert.False(trailing.TrailingDownCanMoveExitOrder);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_LegacyPrimeShortTail_ShouldApplyFallbackDefaults()
    {
        TradeGrid source = CreateBareGrid();
        source.Number = 3;
        source.GridType = TradeGridPrimeType.OpenPosition;
        source.Regime = TradeGridRegime.CloseOnly;
        source.RegimeLogicEntry = TradeGridLogicEntryRegime.OncePerSecond;
        source.AutoClearJournalIsOn = true;
        source.MaxClosePositionsInJournal = 77;
        source.MaxOpenOrdersInMarket = 9;
        source.MaxCloseOrdersInMarket = 8;

        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');

        // Keep only fields [0..10] to emulate legacy payload without newer optional prime tail values.
        sections[0] = string.Join("@", primeFields.Take(11));
        string legacy = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.LoadFromString(legacy);

        Assert.Equal(3, loaded.Number);
        Assert.Equal(TradeGridPrimeType.OpenPosition, loaded.GridType);
        Assert.Equal(TradeGridRegime.CloseOnly, loaded.Regime);
        Assert.Equal(TradeGridLogicEntryRegime.OncePerSecond, loaded.RegimeLogicEntry);
        Assert.True(loaded.AutoClearJournalIsOn);
        Assert.Equal(77, loaded.MaxClosePositionsInJournal);
        Assert.Equal(9, loaded.MaxOpenOrdersInMarket);
        Assert.Equal(8, loaded.MaxCloseOrdersInMarket);
        Assert.Equal(500, loaded.DelayInReal);
        Assert.True(loaded.CheckMicroVolumes);
        Assert.Equal(1.5m, loaded.MaxDistanceToOrdersPercent);
        Assert.True(loaded.OpenOrdersMakerOnly);
    }

    private static TradeGrid CreateBareGrid()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.NonTradePeriods = new TradeGridNonTradePeriods("CodexGrid");
        grid.StopBy = new TradeGridStopBy();
        grid.StopAndProfit = new TradeGridStopAndProfit();
        grid.AutoStarter = new TradeGridAutoStarter();
        grid.GridCreator = new TradeGridCreator();
        grid.ErrorsReaction = new TradeGridErrorsReaction(grid);
        grid.TrailingUp = new TrailingUp(grid);
        return grid;
    }
}

