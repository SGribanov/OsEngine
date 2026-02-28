#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Journal.Internal;
using OsEngine.Logging;
using OsEngine.OsTrader.Grids;
using OsEngine.OsTrader.Panels.Tab;
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
    public void Stage2Step2_2_TradeGridNonTradePeriods_LoadFromString_NullPayload_ShouldKeepDefaults()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");
        periods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        periods.NonTradePeriod2Regime = TradeGridRegime.OffAndCancelOrders;

        periods.LoadFromString(null);

        Assert.Equal(TradeGridRegime.CloseOnly, periods.NonTradePeriod1Regime);
        Assert.Equal(TradeGridRegime.OffAndCancelOrders, periods.NonTradePeriod2Regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_LoadFromString_ShortPayload_ShouldNotThrowAndKeepSecondDefault()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");
        periods.NonTradePeriod2Regime = TradeGridRegime.OffAndCancelOrders;

        Exception? error = Record.Exception(() => periods.LoadFromString("CloseOnly"));

        Assert.Null(error);
        Assert.Equal(TradeGridRegime.CloseOnly, periods.NonTradePeriod1Regime);
        Assert.Equal(TradeGridRegime.OffAndCancelOrders, periods.NonTradePeriod2Regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_Delete_WithNullSettings_ShouldNotThrow()
    {
        TradeGridNonTradePeriods periods =
            (TradeGridNonTradePeriods)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridNonTradePeriods));

        Exception? error = Record.Exception(periods.Delete);

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_NullPayload_ShouldKeepDefaults()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            GridSide = Side.Sell,
            FirstPrice = 123.45m,
            LineCountStart = 9,
            TradeAssetInPortfolio = "USDT",
            StartVolume = 3.21m
        };

        creator.LoadFromString(null);

        Assert.Equal(Side.Sell, creator.GridSide);
        Assert.Equal(123.45m, creator.FirstPrice);
        Assert.Equal(9, creator.LineCountStart);
        Assert.Equal("USDT", creator.TradeAssetInPortfolio);
        Assert.Equal(3.21m, creator.StartVolume);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_ShortPayload_ShouldNotThrowAndKeepTailDefaults()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            TradeAssetInPortfolio = "Prime",
            TypeVolume = TradeGridVolumeType.DepositPercent
        };

        Exception? error = Record.Exception(() => creator.LoadFromString("Buy@101.5@3"));

        Assert.Null(error);
        Assert.Equal(Side.Buy, creator.GridSide);
        Assert.Equal(101.5m, creator.FirstPrice);
        Assert.Equal(3, creator.LineCountStart);
        Assert.Equal(TradeGridVolumeType.DepositPercent, creator.TypeVolume);
        Assert.Equal("Prime", creator.TradeAssetInPortfolio);
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
    public void Stage2Step2_2_TrailingUp_LoadFromString_VeryShortLegacyPayload_ShouldNotThrowAndKeepDefaults()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingDownIsOn = true,
            TrailingDownStep = 2m,
            TrailingDownLimit = 20m
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(0m, trailing.TrailingUpLimit);
        Assert.True(trailing.TrailingDownIsOn);
        Assert.Equal(2m, trailing.TrailingDownStep);
        Assert.Equal(20m, trailing.TrailingDownLimit);
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

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_NullPayload_ShouldKeepConfiguredDefaults()
    {
        TradeGrid grid = CreateBareGrid();
        grid.Number = 99;
        grid.Regime = TradeGridRegime.CloseOnly;
        grid.MaxOpenOrdersInMarket = 7;

        grid.LoadFromString(null);

        Assert.Equal(99, grid.Number);
        Assert.Equal(TradeGridRegime.CloseOnly, grid.Regime);
        Assert.Equal(7, grid.MaxOpenOrdersInMarket);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_ShortPayload_ShouldParsePrefixWithoutThrow()
    {
        TradeGrid grid = CreateBareGrid();
        grid.MaxOpenOrdersInMarket = 11;

        Exception? error = Record.Exception(() => grid.LoadFromString("5@OpenPosition@On"));

        Assert.Null(error);
        Assert.Equal(5, grid.Number);
        Assert.Equal(TradeGridPrimeType.OpenPosition, grid.GridType);
        Assert.Equal(TradeGridRegime.On, grid.Regime);
        Assert.Equal(11, grid.MaxOpenOrdersInMarket);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_EventDispatchWithoutSubscribers_ShouldNotThrow()
    {
        TradeGrid grid = CreateBareGrid();

        Exception? error = Record.Exception(() =>
        {
            grid.Save();
            grid.RePaintGrid();
            grid.FullRePaintGrid();
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_SendNewLogMessage_WithNullTab_ShouldPublishErrorWithUnknownContext()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        string? capturedMessage = null;
        LogMessageType capturedType = LogMessageType.System;

        grid.LogMessageEvent += (message, type) =>
        {
            capturedMessage = message;
            capturedType = type;
        };

        Exception? error = Record.Exception(() => grid.SendNewLogMessage("test", LogMessageType.Error));

        Assert.Null(error);
        Assert.Equal(LogMessageType.Error, capturedType);
        Assert.NotNull(capturedMessage);
        Assert.Contains("Bot: unknown", capturedMessage);
        Assert.Contains("Security name: unknown", capturedMessage);
        Assert.Contains("test", capturedMessage);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetPositionByGrid_WithNullGridCreator_ShouldReturnEmpty()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        List<Position> positions = grid.GetPositionByGrid();

        Assert.Empty(positions);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetLinesWithOpenOrdersNeed_WithNullDependencies_ShouldReturnEmpty()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        List<TradeGridLine> lines = grid.GetLinesWithOpenOrdersNeed(100m);

        Assert.Empty(lines);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetLinesWithOpenOrdersNeed_WithNullSecurity_ShouldReturnEmpty()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.Tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        grid.GridCreator = new TradeGridCreator();
        grid.GridCreator.Lines = new List<TradeGridLine> { new TradeGridLine() };

        List<TradeGridLine> lines = grid.GetLinesWithOpenOrdersNeed(100m);

        Assert.Empty(lines);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetOpenAndClosingFact_WithNullGridCreator_ShouldReturnEmpty()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        List<TradeGridLine> openFact = grid.GetLinesWithOpenOrdersFact();
        List<TradeGridLine> closingFact = grid.GetLinesWithClosingOrdersFact();

        Assert.Empty(openFact);
        Assert.Empty(closingFact);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_PrivateLifecycleMethods_WithNullGridCreator_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        Exception? error = Record.Exception(() =>
        {
            InvokePrivateNoArg(grid, "Connector_TestStartEvent");
            InvokePrivateNoArg(grid, "TryDeleteOpeningFailPositions");
            InvokePrivateNoArg(grid, "TryDeleteDonePositions");
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_QueryProperties_WithNullGridCreator_ShouldReturnSafeDefaults()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        Exception? error = Record.Exception(() =>
        {
            decimal allVolume = grid.AllVolumeInLines;
            bool hasNoMarketOrders = grid.HaveOrdersWithNoMarketOrders();
            bool hasRecentCancel = grid.HaveOrdersTryToCancelLastSecond();
            List<TradeGridLine> openPositions = grid.GetLinesWithOpenPosition();

            Assert.Equal(0m, allVolume);
            Assert.False(hasNoMarketOrders);
            Assert.False(hasRecentCancel);
            Assert.Empty(openPositions);
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_PublicGridManagement_WithNullGridCreator_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        Exception? error = Record.Exception(() =>
        {
            grid.CreateNewGridSafe();
            grid.CreateNewLine();
            grid.DeleteGrid();
            grid.RemoveSelected(new List<int> { 0, 1 });
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_PositionOpeningSuccessHandler_WithNullGridCreator_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));

        Exception? error = Record.Exception(() => InvokePrivateWithArgs(grid, "Tab_PositionOpeningSuccesEvent", position));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_SaveLoad_WithNullSubcomponents_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        Exception? error = Record.Exception(() =>
        {
            string save = grid.GetSaveString();
            Assert.NotNull(save);
            grid.LoadFromString("1@On@Off%ntp@legacy%td@stopby@creator@stopprofit@autostart@errors@trailing");
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_PrivateTradingHelpers_WithNullDependencies_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));

        Exception? error = Record.Exception(() =>
        {
            InvokePrivateNoArg(grid, "TryRemoveWrongOrders");
            InvokePrivateWithArgs(grid, "TrySetClosingOrders", 0m);
            InvokePrivateNoArg(grid, "CheckWrongCloseOrders");
            InvokePrivateNoArg(grid, "TryCancelOpeningOrders");
            InvokePrivateNoArg(grid, "TryCancelClosingOrders");
            InvokePrivateNoArg(grid, "TrySetOpenOrders");
            InvokePrivateNoArg(grid, "TryFreeJournal");
            InvokePrivateWithArgs(grid, "TryDeletePositionsFromJournal", position);
            InvokePrivateNoArg(grid, "TryFindPositionsInJournalAfterReconnect");
            InvokePrivateNoArg(grid, "TryForcedCloseGrid");
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_Process_WithNullDependencies_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        Exception? error = Record.Exception(() => InvokePrivateNoArg(grid, "Process"));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_Process_WithNullGridLines_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.Tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        grid.GridCreator = (TradeGridCreator)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridCreator));
        grid.ErrorsReaction = (TradeGridErrorsReaction)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridErrorsReaction));
        grid.AutoStarter = (TradeGridAutoStarter)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridAutoStarter));
        grid.NonTradePeriods = (TradeGridNonTradePeriods)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridNonTradePeriods));
        grid.StopBy = (TradeGridStopBy)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridStopBy));
        grid.TrailingUp = (TrailingUp)RuntimeHelpers.GetUninitializedObject(typeof(TrailingUp));

        Exception? error = Record.Exception(() => InvokePrivateNoArg(grid, "Process"));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_ProfitAndMarketMakingHelpers_WithNullDependencies_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        Exception? error = Record.Exception(() =>
        {
            InvokePrivateNoArg(grid, "TrySetStopAndProfit");
            InvokePrivateNoArg(grid, "TrySetLimitProfit");
            InvokePrivateNoArg(grid, "TryCancelWrongCloseProfitOrders");
            InvokePrivateWithArgs(grid, "TrySetClosingProfitOrders", 0m);
            InvokePrivateWithArgs(grid, "GridTypeMarketMakingLogic", TradeGridRegime.On);
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_FailEventsAndOpenPositionLogic_WithNullHandlers_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));

        grid.Regime = TradeGridRegime.On;
        grid.GridCreator = new TradeGridCreator();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { Position = position, PositionNum = position.Number }
        };

        Exception? error = Record.Exception(() =>
        {
            InvokePrivateWithArgs(grid, "Tab_PositionClosingFailEvent", position);
            InvokePrivateWithArgs(grid, "Tab_PositionOpeningFailEvent", position);
            InvokePrivateWithArgs(grid, "GridTypeOpenPositionLogic", TradeGridRegime.On);
        });

        Assert.Null(error);
        Assert.Equal(0m, grid.MaxGridPrice);
        Assert.Equal(0m, grid.MinGridPrice);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_Delete_WithUninitializedTabConnector_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.Tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));

        Exception? error = Record.Exception(() => grid.Delete());

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_Delete_CalledTwice_ShouldNotThrow()
    {
        TradeGrid grid = CreateBareGrid();

        Exception? error = Record.Exception(() =>
        {
            grid.Delete();
            grid.Delete();
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_Delete_WithFaultyUninitializedSubcomponents_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.NonTradePeriods = (TradeGridNonTradePeriods)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridNonTradePeriods));
        grid.ErrorsReaction = (TradeGridErrorsReaction)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridErrorsReaction));
        grid.TrailingUp = (TrailingUp)RuntimeHelpers.GetUninitializedObject(typeof(TrailingUp));

        Exception? error = Record.Exception(() => grid.Delete());

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_EventHandlers_WithNullPosition_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.Regime = TradeGridRegime.On;

        Exception? error = Record.Exception(() =>
        {
            InvokePrivateWithArgs(grid, "Tab_PositionOpeningSuccesEvent", (object?)null);
            InvokePrivateWithArgs(grid, "Tab_PositionOpeningFailEvent", (object?)null);
            InvokePrivateWithArgs(grid, "Tab_PositionClosingFailEvent", (object?)null);
            InvokePrivateWithArgs(grid, "Tab_PositionClosingSuccesEvent", (object?)null);
            InvokePrivateWithArgs(grid, "Tab_PositionStopActivateEvent", (object?)null);
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_EventHandlers_WithNullGridLines_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.Regime = TradeGridRegime.On;
        grid.GridCreator = (TradeGridCreator)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridCreator));
        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));

        Exception? error = Record.Exception(() =>
        {
            InvokePrivateWithArgs(grid, "Tab_PositionOpeningSuccesEvent", position);
            InvokePrivateWithArgs(grid, "Tab_PositionOpeningFailEvent", position);
            InvokePrivateWithArgs(grid, "Tab_PositionClosingFailEvent", position);
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_EntryPointsAndLog_WithNullPayload_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.RegimeLogicEntry = TradeGridLogicEntryRegime.OnTrade;

        string? capturedMessage = null;
        grid.LogMessageEvent += (message, _) => capturedMessage = message;

        Exception? error = Record.Exception(() =>
        {
            InvokePrivateWithArgs(grid, "Tab_NewTickEvent", (object?)null);
            InvokePrivateWithArgs(grid, "Tab_PositionStopActivateEvent", (object?)null);
            grid.SendNewLogMessage(null!, LogMessageType.Error);
        });

        Assert.Null(error);
        Assert.NotNull(capturedMessage);
        Assert.Contains("Grid error. Bot: unknown", capturedMessage);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_RemoveSelected_WithNullGridCreatorLines_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = (TradeGridCreator)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridCreator));

        Exception? error = Record.Exception(() => grid.RemoveSelected(new List<int> { 0, 1 }));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_TryDeletePositionsFromJournal_WithNullJournal_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();
        grid.GridCreator.Lines = new List<TradeGridLine>();
        grid.Tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));

        Exception? error = Record.Exception(() => InvokePrivateWithArgs(grid, "TryDeletePositionsFromJournal", position));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_TryDeletePositionsFromJournal_WithNullLines_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = (TradeGridCreator)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridCreator));
        grid.Tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));

        Exception? error = Record.Exception(() => InvokePrivateWithArgs(grid, "TryDeletePositionsFromJournal", position));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_TryFindPositionsInJournalAfterReconnect_WithNullJournalEntries_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { PositionNum = 42, Position = null }
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        OsEngine.Journal.Journal journal =
            (OsEngine.Journal.Journal)RuntimeHelpers.GetUninitializedObject(typeof(OsEngine.Journal.Journal));
        PositionController controller = (PositionController)RuntimeHelpers.GetUninitializedObject(typeof(PositionController));
        SetPrivateField(controller, "_deals", new List<Position> { null! });
        SetPrivateField(journal, "_positionController", controller);
        tab._journal = journal;
        grid.Tab = tab;

        Exception? error = Record.Exception(() => InvokePrivateNoArg(grid, "TryFindPositionsInJournalAfterReconnect"));

        Assert.Null(error);
        Assert.Equal(-1, grid.GridCreator.Lines[0].PositionNum);
        Assert.Null(grid.GridCreator.Lines[0].Position);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_OrderStateChecks_WithEmptyOrderCollections_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(position, "_openOrders", new List<Order>());
        SetPrivateField(position, "_closeOrders", new List<Order>());

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { Position = position, PositionNum = 1 }
        };

        Exception? error = Record.Exception(() =>
        {
            bool hasNoMarketOrders = grid.HaveOrdersWithNoMarketOrders();
            bool hasRecentCancel = grid.HaveOrdersTryToCancelLastSecond();

            Assert.False(hasNoMarketOrders);
            Assert.False(hasRecentCancel);
        });

        Assert.Null(error);
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

    private static void InvokePrivateNoArg(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Method {methodName} not found.");
        method.Invoke(target, null);
    }

    private static void InvokePrivateWithArgs(object target, string methodName, params object[] args)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Method {methodName} not found.");
        method.Invoke(target, args);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Field {fieldName} not found.");
        field.SetValue(target, value);
    }
}
