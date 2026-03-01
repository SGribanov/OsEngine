#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Candles;
using OsEngine.Entity;
using OsEngine.Market.Connectors;
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
    public void Stage2Step2_2_TradeGridNonTradePeriods_ServiceAndRegime_WithNullSettings_ShouldStaySafe()
    {
        TradeGridNonTradePeriods periods =
            (TradeGridNonTradePeriods)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridNonTradePeriods));
        periods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        periods.NonTradePeriod2Regime = TradeGridRegime.OffAndCancelOrders;

        Exception? error = Record.Exception(() =>
        {
            periods.ShowDialogPeriod1();
            periods.ShowDialogPeriod2();
        });

        TradeGridRegime regime = periods.GetNonTradePeriodsRegime(DateTime.UtcNow);

        Assert.Null(error);
        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithNullGridOrTab_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true
        };

        TradeGridRegime nullTabRegime = stopBy.GetRegime(CreateBareGrid(), null!);
        TradeGridRegime nullGridRegime = stopBy.GetRegime(null!, (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple)));

        Assert.Equal(TradeGridRegime.On, nullTabRegime);
        Assert.Equal(TradeGridRegime.On, nullGridRegime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithNullLastCandle_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 1m
        };

        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.FirstPrice = 100m;

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGrid", StartProgram.IsTester, false);
        TimeFrameBuilder builder = new TimeFrameBuilder("CodexGrid", StartProgram.IsTester);
        CandleSeries series = new CandleSeries(builder, new Security(), StartProgram.IsTester)
        {
            CandlesAll = new List<Candle> { null! }
        };
        SetPrivateField(connector, "_mySeries", series);
        SetPrivateField(tab, "_connector", connector);

        TradeGridRegime regime = stopBy.GetRegime(grid, tab);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_Process_WithNullRuntimeContext_ShouldNotThrow()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit();
        TradeGrid uninitializedGrid =
            (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        Exception? error = Record.Exception(() =>
        {
            stopAndProfit.Process(null!);
            stopAndProfit.Process(uninitializedGrid);
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_SetTrailStop_WithNullLastCandle_ShouldNotThrow()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit();
        TradeGrid grid = CreateBareGrid();

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridTrailStop", StartProgram.IsTester, false);
        TimeFrameBuilder builder = new TimeFrameBuilder("CodexGridTrailStop", StartProgram.IsTester);
        CandleSeries series = new CandleSeries(builder, new Security(), StartProgram.IsTester)
        {
            CandlesAll = new List<Candle> { null! }
        };
        SetPrivateField(connector, "_mySeries", series);
        SetPrivateField(tab, "_connector", connector);
        grid.Tab = tab;

        Exception? error = Record.Exception(() =>
            InvokePrivateWithArgs(stopAndProfit, "SetTrailStop", grid, new List<Position>()));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_PrivateSetters_WithNullRuntimeContext_ShouldNotThrow()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit();

        Exception? error = Record.Exception(() =>
        {
            InvokePrivateWithArgs(stopAndProfit, "SetProfit", null!, 100m, null!);
            InvokePrivateWithArgs(stopAndProfit, "SetStop", null!, 100m, null!);
            InvokePrivateWithArgs(stopAndProfit, "SetTrailStop", null!, null!);
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_PrivateSetters_WithSparsePositionsList_ShouldNotThrow()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit();
        TradeGrid grid = CreateBareGrid();

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridSparsePositions", StartProgram.IsTester, false);
        TimeFrameBuilder builder = new TimeFrameBuilder("CodexGridSparsePositions", StartProgram.IsTester);
        CandleSeries series = new CandleSeries(builder, new Security(), StartProgram.IsTester)
        {
            CandlesAll = new List<Candle> { new Candle { Close = 100m } }
        };
        SetPrivateField(connector, "_mySeries", series);
        SetPrivateField(tab, "_connector", connector);
        SetPrivateField(tab, "_security", new Security
        {
            Name = connector.SecurityName,
            PriceStep = 1m
        });
        grid.Tab = tab;

        List<Position> sparsePositions = new List<Position> { null! };

        Exception? error = Record.Exception(() =>
        {
            InvokePrivateWithArgs(stopAndProfit, "SetProfit", grid, 100m, sparsePositions);
            InvokePrivateWithArgs(stopAndProfit, "SetStop", grid, 100m, sparsePositions);
            InvokePrivateWithArgs(stopAndProfit, "SetTrailStop", grid, sparsePositions);
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_RuntimeContextMissing_ShouldStaySafe()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = true,
            TrailingDownIsOn = true,
            TrailingUpStep = 1,
            TrailingDownStep = 1,
            TrailingUpLimit = 100,
            TrailingDownLimit = 1
        };

        bool moved = true;
        decimal max = -1;
        decimal min = -1;

        Exception? error = Record.Exception(() =>
        {
            moved = trailing.TryTrailingGrid();
            max = trailing.MaxGridPrice;
            min = trailing.MinGridPrice;
            trailing.ShiftGridUpOnValue(1);
            trailing.ShiftGridDownOnValue(1);
        });

        Assert.Null(error);
        Assert.False(moved);
        Assert.Equal(0, max);
        Assert.Equal(0, min);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_TryTrailingGrid_WithNullLastCandle_ShouldReturnFalse()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = true,
            TrailingUpStep = 1m,
            TrailingUpLimit = 1000m,
            TrailingDownIsOn = false
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridTrailingNullCandle", StartProgram.IsTester, false);
        TimeFrameBuilder builder = new TimeFrameBuilder("CodexGridTrailingNullCandle", StartProgram.IsTester);
        CandleSeries series = new CandleSeries(builder, new Security(), StartProgram.IsTester)
        {
            CandlesAll = new List<Candle> { null! }
        };
        SetPrivateField(connector, "_mySeries", series);
        SetPrivateField(tab, "_connector", connector);
        grid.Tab = tab;

        bool moved = true;
        Exception? error = Record.Exception(() => moved = trailing.TryTrailingGrid());

        Assert.Null(error);
        Assert.False(moved);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_AwaitOnStartConnector_WithNullServer_ShouldReturnFalse()
    {
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(CreateBareGrid());

        bool shouldAwait = reaction.AwaitOnStartConnector(null!);

        Assert.False(shouldAwait);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_FailEvents_WithNullPositionOrOrder_ShouldNotThrow()
    {
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(CreateBareGrid());
        Position sparsePosition = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(sparsePosition, "_openOrders", new List<Order> { null! });
        SetPrivateField(sparsePosition, "_closeOrders", new List<Order> { null! });

        Exception? error = Record.Exception(() =>
        {
            reaction.PositionOpeningFailEvent(null!);
            reaction.PositionClosingFailEvent(null!);
            reaction.PositionOpeningFailEvent(sparsePosition);
            reaction.PositionClosingFailEvent(sparsePosition);
        });

        Assert.Null(error);
        Assert.Equal(0, reaction.FailOpenOrdersCountFact);
        Assert.Equal(0, reaction.FailCancelOrdersCountFact);
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
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_WithMalformedMiddleFields_ShouldContinueTailParsing()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            LineCountStart = 9,
            LineStep = 0.5m,
            StartVolume = 1m,
            TradeAssetInPortfolio = "Prime"
        };

        Exception? error = Record.Exception(() => creator.LoadFromString(
            "Buy@101.5@badInt@Percent@badDecimal@1.2@Absolute@2@3@Contracts@5.5@2@USDT@100|1|Buy|101|-1|^"));

        Assert.Null(error);
        Assert.Equal(101.5m, creator.FirstPrice);
        Assert.Equal(9, creator.LineCountStart);
        Assert.Equal(0.5m, creator.LineStep);
        Assert.Equal(1.2m, creator.StepMultiplicator);
        Assert.Equal(5.5m, creator.StartVolume);
        Assert.Equal(2m, creator.MartingaleMultiplicator);
        Assert.Equal("USDT", creator.TradeAssetInPortfolio);
        Assert.Single(creator.Lines);
        Assert.Equal(100m, creator.Lines[0].PriceEnter);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_WithLowercaseEnumFields_ShouldParse()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            GridSide = Side.None,
            TypeStep = TradeGridValueType.Absolute,
            TypeProfit = TradeGridValueType.Absolute,
            TypeVolume = TradeGridVolumeType.Contracts
        };

        Exception? error = Record.Exception(() => creator.LoadFromString(
            "buy@101.5@3@percent@1@1@absolute@2@1@depositpercent@5@1@prime@"));

        Assert.Null(error);
        Assert.Equal(Side.Buy, creator.GridSide);
        Assert.Equal(TradeGridValueType.Percent, creator.TypeStep);
        Assert.Equal(TradeGridValueType.Absolute, creator.TypeProfit);
        Assert.Equal(TradeGridVolumeType.DepositPercent, creator.TypeVolume);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_WithInvalidNumericInvariants_ShouldKeepSafeValues()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            LineCountStart = 5,
            LineStep = 1m,
            StepMultiplicator = 1m,
            ProfitStep = 2m,
            ProfitMultiplicator = 1m,
            StartVolume = 3m,
            MartingaleMultiplicator = 1m
        };

        Exception? error = Record.Exception(() => creator.LoadFromString(
            "Buy@100@-5@Absolute@-1@0@Absolute@-2@0@Contracts@-3@0@Prime@"));

        Assert.Null(error);
        Assert.Equal(5, creator.LineCountStart);
        Assert.Equal(1m, creator.LineStep);
        Assert.Equal(1m, creator.StepMultiplicator);
        Assert.Equal(2m, creator.ProfitStep);
        Assert.Equal(1m, creator.ProfitMultiplicator);
        Assert.Equal(3m, creator.StartVolume);
        Assert.Equal(1m, creator.MartingaleMultiplicator);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_TradeAssetWithWhitespace_ShouldBeTrimmed()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            TradeAssetInPortfolio = "Prime"
        };

        Exception? error = Record.Exception(() => creator.LoadFromString(
            "Buy@100@3@Absolute@1@1@Absolute@1@1@Contracts@1@1@  USDT  @"));

        Assert.Null(error);
        Assert.Equal("USDT", creator.TradeAssetInPortfolio);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_WithMixedWhitespaceTokens_ShouldParse()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            GridSide = Side.None,
            FirstPrice = 0m,
            LineCountStart = 0,
            TypeStep = TradeGridValueType.Absolute,
            TypeVolume = TradeGridVolumeType.Contracts,
            TradeAssetInPortfolio = "Prime"
        };

        Exception? error = Record.Exception(() => creator.LoadFromString(
            "  buy  @ 101.5 @ 3 @ percent @ 1.25 @ 1 @ absolute @ 2 @ 1 @ depositpercent @ 5 @ 1 @  USDT  @"));

        Assert.Null(error);
        Assert.Equal(Side.Buy, creator.GridSide);
        Assert.Equal(101.5m, creator.FirstPrice);
        Assert.Equal(3, creator.LineCountStart);
        Assert.Equal(TradeGridValueType.Percent, creator.TypeStep);
        Assert.Equal(TradeGridVolumeType.DepositPercent, creator.TypeVolume);
        Assert.Equal("USDT", creator.TradeAssetInPortfolio);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_WithOutOfRangeEnumFields_ShouldKeepExistingValues()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            GridSide = Side.Sell,
            TypeStep = TradeGridValueType.Absolute,
            TypeProfit = TradeGridValueType.Percent,
            TypeVolume = TradeGridVolumeType.ContractCurrency
        };

        Exception? error = Record.Exception(() => creator.LoadFromString(
            "999@100@3@999@1@1@999@2@1@999@5@1@USDT@"));

        Assert.Null(error);
        Assert.Equal(Side.Sell, creator.GridSide);
        Assert.Equal(TradeGridValueType.Absolute, creator.TypeStep);
        Assert.Equal(TradeGridValueType.Percent, creator.TypeProfit);
        Assert.Equal(TradeGridVolumeType.ContractCurrency, creator.TypeVolume);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_WithNegativeFirstPrice_ShouldKeepExistingValue()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            FirstPrice = 123.45m
        };

        Exception? error = Record.Exception(() => creator.LoadFromString(
            "Buy@-100@3@Absolute@1@1@Absolute@2@1@Contracts@5@1@USDT@"));

        Assert.Null(error);
        Assert.Equal(123.45m, creator.FirstPrice);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_WithZeroLineCountStart_ShouldKeepExistingValue()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            LineCountStart = 7
        };

        Exception? error = Record.Exception(() => creator.LoadFromString(
            "Buy@100@0@Absolute@1@1@Absolute@2@1@Contracts@5@1@USDT@"));

        Assert.Null(error);
        Assert.Equal(7, creator.LineCountStart);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_WithZeroLineStep_ShouldKeepExistingValue()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            LineStep = 1.25m
        };

        Exception? error = Record.Exception(() => creator.LoadFromString(
            "Buy@100@3@Absolute@0@1@Absolute@2@1@Contracts@5@1@USDT@"));

        Assert.Null(error);
        Assert.Equal(1.25m, creator.LineStep);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_WithZeroStepMultiplicator_ShouldKeepExistingValue()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            StepMultiplicator = 1.75m
        };

        Exception? error = Record.Exception(() => creator.LoadFromString(
            "Buy@100@3@Absolute@1@0@Absolute@2@1@Contracts@5@1@USDT@"));

        Assert.Null(error);
        Assert.Equal(1.75m, creator.StepMultiplicator);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_WithZeroProfitMultiplicator_ShouldKeepExistingValue()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            ProfitMultiplicator = 2.25m
        };

        Exception? error = Record.Exception(() => creator.LoadFromString(
            "Buy@100@3@Absolute@1@1@Absolute@2@0@Contracts@5@1@USDT@"));

        Assert.Null(error);
        Assert.Equal(2.25m, creator.ProfitMultiplicator);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadLines_WithNullLinesCollection_ShouldNotThrow()
    {
        TradeGridCreator creator = (TradeGridCreator)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridCreator));
        creator.Lines = null!;

        Exception? error = Record.Exception(() => creator.LoadLines("100|1|Buy|101|-1|^"));

        Assert.Null(error);
        Assert.NotNull(creator.Lines);
        Assert.Single(creator.Lines);
        Assert.Equal(100m, creator.Lines[0].PriceEnter);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadLines_WithMixedInvalidPayload_ShouldKeepValidLines()
    {
        TradeGridCreator creator = new TradeGridCreator();
        creator.LogMessageEvent += (_, _) => { };

        Exception? error = Record.Exception(() =>
            creator.LoadLines("100|1|Buy|101|-1|^broken^200|2|Sell|199|-1|^"));

        Assert.Null(error);
        Assert.Equal(2, creator.Lines.Count);
        Assert.Equal(100m, creator.Lines[0].PriceEnter);
        Assert.Equal(200m, creator.Lines[1].PriceEnter);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadLines_WithLegacyLineWithoutPositionNum_ShouldLoadWithDefaultPosition()
    {
        TradeGridCreator creator = new TradeGridCreator();

        Exception? error = Record.Exception(() => creator.LoadLines("100|1|Buy|101|^"));

        Assert.Null(error);
        Assert.Single(creator.Lines);
        Assert.Equal(100m, creator.Lines[0].PriceEnter);
        Assert.Equal(1m, creator.Lines[0].Volume);
        Assert.Equal(Side.Buy, creator.Lines[0].Side);
        Assert.Equal(101m, creator.Lines[0].PriceExit);
        Assert.Equal(-1, creator.Lines[0].PositionNum);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadLines_WithWhitespaceAndCrLf_ShouldParseValidLines()
    {
        TradeGridCreator creator = new TradeGridCreator();

        Exception? error = Record.Exception(() =>
            creator.LoadLines(" 100|1|Buy|101|-1| \r\n^   ^\r\n 200|2|Sell|199|-1|  ^"));

        Assert.Null(error);
        Assert.Equal(2, creator.Lines.Count);
        Assert.Equal(100m, creator.Lines[0].PriceEnter);
        Assert.Equal(200m, creator.Lines[1].PriceEnter);
        Assert.Equal(Side.Sell, creator.Lines[1].Side);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadLines_WithSpacedPositionNum_ShouldParse()
    {
        TradeGridCreator creator = new TradeGridCreator();

        Exception? error = Record.Exception(() => creator.LoadLines("100|1|Buy|101|  42  |^"));

        Assert.Null(error);
        Assert.Single(creator.Lines);
        Assert.Equal(42, creator.Lines[0].PositionNum);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadLines_WithNegativeLineValues_ShouldSkipInvalidLine()
    {
        TradeGridCreator creator = new TradeGridCreator();

        Exception? error = Record.Exception(() =>
            creator.LoadLines("-100|1|Buy|101|-1|^200|2|Sell|199|-1|^"));

        Assert.Null(error);
        Assert.Single(creator.Lines);
        Assert.Equal(200m, creator.Lines[0].PriceEnter);
        Assert.Equal(2m, creator.Lines[0].Volume);
        Assert.Equal(199m, creator.Lines[0].PriceExit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadLines_WithInvalidNegativePositionNum_ShouldSkipInvalidLine()
    {
        TradeGridCreator creator = new TradeGridCreator();

        Exception? error = Record.Exception(() =>
            creator.LoadLines("100|1|Buy|101|-5|^200|2|Sell|199|-1|^"));

        Assert.Null(error);
        Assert.Single(creator.Lines);
        Assert.Equal(200m, creator.Lines[0].PriceEnter);
        Assert.Equal(-1, creator.Lines[0].PositionNum);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadLines_WithInvalidSide_ShouldSkipInvalidLine()
    {
        TradeGridCreator creator = new TradeGridCreator();

        Exception? error = Record.Exception(() =>
            creator.LoadLines("100|1|None|101|-1|^200|2|Sell|199|-1|^"));

        Assert.Null(error);
        Assert.Single(creator.Lines);
        Assert.Equal(Side.Sell, creator.Lines[0].Side);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_Mutators_WithNullLinesOrTab_ShouldNotThrow()
    {
        TradeGridCreator creator = (TradeGridCreator)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridCreator));
        creator.Lines = null!;

        string? saved = null;
        Exception? error = Record.Exception(() =>
        {
            creator.DeleteGrid();
            creator.CreateNewLine();
            saved = creator.GetSaveLinesString();
            creator.CreateNewGrid(null!, TradeGridPrimeType.MarketMaking);
        });

        Assert.Null(error);
        Assert.NotNull(creator.Lines);
        Assert.Single(creator.Lines);
        Assert.NotNull(saved);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_GetVolume_WithNullRuntimeContext_ShouldStaySafe()
    {
        TradeGridCreator creator = new TradeGridCreator();
        TradeGridLine line = new TradeGridLine
        {
            Volume = 7m,
            PriceEnter = 100m
        };

        creator.TypeVolume = TradeGridVolumeType.Contracts;
        decimal contractsVolume = creator.GetVolume(line, null!);

        creator.TypeVolume = TradeGridVolumeType.ContractCurrency;
        BotTabSimple nullSecurityTab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        decimal contractCurrencyVolume = creator.GetVolume(line, nullSecurityTab);
        TradeGridLine zeroPriceLine = new TradeGridLine
        {
            Volume = 7m,
            PriceEnter = 0m
        };
        BotTabSimple zeroPriceTab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        SetPrivateField(zeroPriceTab, "_security", new Security
        {
            Lot = 1m,
            DecimalsVolume = 4
        });
        decimal contractCurrencyZeroPriceVolume = creator.GetVolume(zeroPriceLine, zeroPriceTab);
        decimal nullLineVolume = creator.GetVolume(null!, nullSecurityTab);

        BotTabSimple negativeDecimalsContractTab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connectorNegativeDecimalsContract = new ConnectorCandles("CodexGridCreatorVolumeNegativeDecimalsContract", StartProgram.IsTester, false);
        SetPrivateField(negativeDecimalsContractTab, "_connector", connectorNegativeDecimalsContract);
        SetPrivateField(negativeDecimalsContractTab, "_security", new Security
        {
            Name = connectorNegativeDecimalsContract.SecurityName,
            Lot = 1m,
            DecimalsVolume = -1
        });
        decimal contractCurrencyNegativeDecimalsVolume = creator.GetVolume(line, negativeDecimalsContractTab);

        creator.TypeVolume = TradeGridVolumeType.DepositPercent;
        BotTabSimple tabWithZeroBestAsk = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridCreatorVolume", StartProgram.IsTester, false);
        SetPrivateField(tabWithZeroBestAsk, "_connector", connector);
        SetPrivateField(tabWithZeroBestAsk, "_security", new Security
        {
            Name = connector.SecurityName,
            Lot = 1m
        });
        SetPrivateField(tabWithZeroBestAsk, "_portfolio", new Portfolio
        {
            ValueCurrent = 1000m
        });
        decimal depositPercentVolume = creator.GetVolume(line, tabWithZeroBestAsk);

        BotTabSimple tabWithLotZeroAndSparsePortfolio = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connectorWithBestAsk = new ConnectorCandles("CodexGridCreatorVolumeLotZero", StartProgram.IsTester, false);
        SetPrivateField(connectorWithBestAsk, "_bestAsk", 100m);
        SetPrivateField(tabWithLotZeroAndSparsePortfolio, "_connector", connectorWithBestAsk);
        SetPrivateField(tabWithLotZeroAndSparsePortfolio, "_security", new Security
        {
            Name = connectorWithBestAsk.SecurityName,
            Lot = 0m,
            DecimalsVolume = 4
        });
        SetPrivateField(tabWithLotZeroAndSparsePortfolio, "_portfolio", new Portfolio
        {
            PositionOnBoard = new List<PositionOnBoard>
            {
                null!,
                new PositionOnBoard
                {
                    SecurityNameCode = "USDT",
                    ValueCurrent = 1000m
                }
            }
        });
        creator.TradeAssetInPortfolio = "USDT";
        decimal depositPercentWithLotZero = creator.GetVolume(line, tabWithLotZeroAndSparsePortfolio);

        BotTabSimple negativeDecimalsDepositTab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connectorNegativeDecimals = new ConnectorCandles("CodexGridCreatorVolumeNegativeDecimals", StartProgram.IsTester, false);
        SetPrivateField(connectorNegativeDecimals, "_bestAsk", 100m);
        SetPrivateField(negativeDecimalsDepositTab, "_connector", connectorNegativeDecimals);
        SetPrivateField(negativeDecimalsDepositTab, "_security", new Security
        {
            Name = connectorNegativeDecimals.SecurityName,
            Lot = 1m,
            DecimalsVolume = -1
        });
        SetPrivateField(negativeDecimalsDepositTab, "_portfolio", new Portfolio
        {
            ValueCurrent = 1000m
        });
        creator.TypeVolume = TradeGridVolumeType.DepositPercent;
        creator.TradeAssetInPortfolio = "Prime";
        decimal depositPercentNegativeDecimalsVolume = creator.GetVolume(new TradeGridLine
        {
            Volume = 10m,
            PriceEnter = 100m
        }, negativeDecimalsDepositTab);

        Assert.Equal(7m, contractsVolume);
        Assert.Equal(0m, contractCurrencyVolume);
        Assert.Equal(0m, contractCurrencyZeroPriceVolume);
        Assert.Equal(0.07m, contractCurrencyNegativeDecimalsVolume);
        Assert.Equal(0m, nullLineVolume);
        Assert.Equal(0m, depositPercentVolume);
        Assert.Equal(0.7m, depositPercentWithLotZero);
        Assert.Equal(1m, depositPercentNegativeDecimalsVolume);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_CreateNewGrid_WithZeroComputedPercentStep_ShouldNotCreateDegenerateLines()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            GridSide = Side.Buy,
            FirstPrice = 0m,
            LineCountStart = 3,
            TypeStep = TradeGridValueType.Percent,
            LineStep = 1m,
            StepMultiplicator = 1m,
            TypeProfit = TradeGridValueType.Absolute,
            ProfitStep = 1m,
            ProfitMultiplicator = 1m,
            TypeVolume = TradeGridVolumeType.Contracts,
            StartVolume = 1m,
            MartingaleMultiplicator = 1m
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridCreatorZeroComputedPercentStep", StartProgram.IsTester, false);
        SetPrivateField(tab, "_connector", connector);

        Exception? error = Record.Exception(() => creator.CreateNewGrid(tab, TradeGridPrimeType.MarketMaking));

        Assert.Null(error);
        Assert.NotNull(creator.Lines);
        Assert.Empty(creator.Lines);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_GetSaveLinesString_WithSparseLines_ShouldNotThrow()
    {
        TradeGridCreator creator = new TradeGridCreator();
        creator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine
            {
                PriceEnter = 100m,
                Volume = 1m,
                Side = Side.Buy,
                PriceExit = 101m,
                PositionNum = 7
            }
        };

        string? saved = null;
        Exception? error = Record.Exception(() => saved = creator.GetSaveLinesString());

        Assert.Null(error);
        Assert.NotNull(saved);
        Assert.Contains("100", saved);
        Assert.DoesNotContain("^^", saved);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_RemoveSelected_WithNullOrSparseInputs_ShouldNotThrow()
    {
        TradeGridCreator creator = (TradeGridCreator)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridCreator));
        creator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine
            {
                PriceEnter = 100m,
                Side = Side.Buy
            }
        };

        Exception? error = Record.Exception(() =>
        {
            creator.RemoveSelected(null!);
            creator.RemoveSelected(new List<int>());
            creator.RemoveSelected(new List<int> { -1 });
            creator.RemoveSelected(new List<int> { 0 });
        });

        Assert.Null(error);
        Assert.Single(creator.Lines);
        Assert.Equal(100m, creator.Lines[0].PriceEnter);
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
    public void Stage2Step2_2_TradeGridAutoStarter_RuntimeContextMissing_ShouldStaySafe()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter
        {
            AutoStartRegime = TradeGridAutoStartRegime.HigherOrEqual,
            AutoStartPrice = 100m,
            ShiftFirstPrice = 1m
        };

        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        bool haveEvent = true;
        decimal startPrice = -1m;

        Exception? error = Record.Exception(() =>
        {
            haveEvent = autoStarter.HaveEventToStart(grid);
            startPrice = autoStarter.GetNewGridPriceStart(grid);
            autoStarter.ShiftGridOnNewPrice(100m, grid);
        });

        Assert.Null(error);
        Assert.False(haveEvent);
        Assert.Equal(0m, startPrice);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_ShiftGridOnNewPrice_WithSparseLines_ShouldNotThrow()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter();
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine
            {
                Side = Side.Buy,
                PriceEnter = 100m,
                PriceExit = 110m
            }
        };

        Exception? error = Record.Exception(() => autoStarter.ShiftGridOnNewPrice(120m, grid));

        Assert.Null(error);
        Assert.Equal(120m, grid.GridCreator.Lines[1].PriceEnter);
        Assert.Equal(130m, grid.GridCreator.Lines[1].PriceExit);
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
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedPrimeFields_ShouldContinueTailParsing()
    {
        TradeGrid source = CreateBareGrid();
        source.Number = 5;
        source.MaxClosePositionsInJournal = 77;
        source.MaxOpenOrdersInMarket = 11;
        source.OpenOrdersMakerOnly = false;
        source.GridCreator.TradeAssetInPortfolio = "USDT";

        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[5] = "badInt";
        primeFields[6] = "badInt";
        primeFields[13] = "badDecimal";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.MaxClosePositionsInJournal = 13;
        loaded.MaxOpenOrdersInMarket = 17;
        loaded.MaxDistanceToOrdersPercent = 1.5m;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(13, loaded.MaxClosePositionsInJournal);
        Assert.Equal(17, loaded.MaxOpenOrdersInMarket);
        Assert.False(loaded.OpenOrdersMakerOnly);
        Assert.Equal(1.5m, loaded.MaxDistanceToOrdersPercent);
        Assert.Equal("USDT", loaded.GridCreator.TradeAssetInPortfolio);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedOptionalTail_ShouldApplyDefaults()
    {
        TradeGrid source = CreateBareGrid();
        source.DelayInReal = 100;
        source.CheckMicroVolumes = false;
        source.MaxDistanceToOrdersPercent = 2.25m;
        source.OpenOrdersMakerOnly = false;

        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[11] = "badDelay";
        primeFields[12] = "badBool";
        primeFields[13] = "badDecimal";
        primeFields[14] = "badBool";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.DelayInReal = 777;
        loaded.CheckMicroVolumes = false;
        loaded.MaxDistanceToOrdersPercent = 9m;
        loaded.OpenOrdersMakerOnly = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(500, loaded.DelayInReal);
        Assert.False(loaded.CheckMicroVolumes);
        Assert.Equal(9m, loaded.MaxDistanceToOrdersPercent);
        Assert.False(loaded.OpenOrdersMakerOnly);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithFlexibleBooleanTail_ShouldParse()
    {
        TradeGrid source = CreateBareGrid();
        source.AutoClearJournalIsOn = false;
        source.CheckMicroVolumes = true;
        source.OpenOrdersMakerOnly = true;

        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[4] = "1";
        primeFields[12] = "off";
        primeFields[14] = "0";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.AutoClearJournalIsOn = false;
        loaded.CheckMicroVolumes = true;
        loaded.OpenOrdersMakerOnly = true;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.True(loaded.AutoClearJournalIsOn);
        Assert.False(loaded.CheckMicroVolumes);
        Assert.False(loaded.OpenOrdersMakerOnly);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithLowercaseEnumFields_ShouldParse()
    {
        TradeGrid source = CreateBareGrid();
        source.Number = 12;

        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[1] = "openposition";
        primeFields[2] = "closeonly";
        primeFields[3] = "oncepersecond";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.GridType = TradeGridPrimeType.MarketMaking;
        loaded.Regime = TradeGridRegime.On;
        loaded.RegimeLogicEntry = TradeGridLogicEntryRegime.OnTrade;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(TradeGridPrimeType.OpenPosition, loaded.GridType);
        Assert.Equal(TradeGridRegime.CloseOnly, loaded.Regime);
        Assert.Equal(TradeGridLogicEntryRegime.OncePerSecond, loaded.RegimeLogicEntry);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedFirstTradeTime_ShouldKeepExistingValue()
    {
        TradeGrid source = CreateBareGrid();
        source.Number = 12;
        source.Regime = TradeGridRegime.On;

        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[10] = "bad-date-value";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        DateTime expected = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        SetPrivateField(loaded, "_firstTradeTime", expected);

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(expected, loaded.FirstTradeTime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithNegativeNumericLimits_ShouldKeepSafeValues()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[5] = "-10";
        primeFields[6] = "-2";
        primeFields[7] = "-3";
        primeFields[9] = "-4";
        primeFields[11] = "-500";
        primeFields[13] = "-1.2";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.MaxClosePositionsInJournal = 101;
        loaded.MaxOpenOrdersInMarket = 7;
        loaded.MaxCloseOrdersInMarket = 8;
        loaded.DelayInReal = 600;
        loaded.MaxDistanceToOrdersPercent = 2m;
        SetPrivateField(loaded, "_openPositionsBySession", 9);

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(101, loaded.MaxClosePositionsInJournal);
        Assert.Equal(7, loaded.MaxOpenOrdersInMarket);
        Assert.Equal(8, loaded.MaxCloseOrdersInMarket);
        Assert.Equal(9, loaded.OpenPositionsCount);
        Assert.Equal(500, loaded.DelayInReal);
        Assert.Equal(1.5m, loaded.MaxDistanceToOrdersPercent);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMixedWhitespaceTokens_ShouldParse()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[1] = " openposition ";
        primeFields[2] = " closeonly ";
        primeFields[3] = " oncepersecond ";
        primeFields[4] = " 1 ";
        primeFields[12] = " off ";
        primeFields[14] = " 0 ";
        sections[0] = "  " + string.Join("@", primeFields) + "  ";
        string payload = " \r\n " + string.Join("%", sections) + " \r\n ";

        TradeGrid loaded = CreateBareGrid();
        loaded.GridType = TradeGridPrimeType.MarketMaking;
        loaded.Regime = TradeGridRegime.On;
        loaded.RegimeLogicEntry = TradeGridLogicEntryRegime.OnTrade;
        loaded.AutoClearJournalIsOn = false;
        loaded.CheckMicroVolumes = true;
        loaded.OpenOrdersMakerOnly = true;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(TradeGridPrimeType.OpenPosition, loaded.GridType);
        Assert.Equal(TradeGridRegime.CloseOnly, loaded.Regime);
        Assert.Equal(TradeGridLogicEntryRegime.OncePerSecond, loaded.RegimeLogicEntry);
        Assert.True(loaded.AutoClearJournalIsOn);
        Assert.False(loaded.CheckMicroVolumes);
        Assert.False(loaded.OpenOrdersMakerOnly);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithWhitespaceOnlySections_ShouldSkipSubcomponentParsing()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.TradeAssetInPortfolio = "USDT";

        string payload =
            "1@OpenPosition@On@OnTrade@true@10@5@5@0@0@2026-03-01T00:00:00.0000000Z@500@true@1.5@true%" +
            "   %unused%   %   %   %   %   %   %";

        Exception? error = Record.Exception(() => grid.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal("USDT", grid.GridCreator.TradeAssetInPortfolio);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithNegativeNumberAndFirstPrice_ShouldKeepSafeValues()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[0] = "-7";
        primeFields[8] = "-100.5";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.Number = 9;
        SetPrivateField(loaded, "_firstTradePrice", 123.45m);

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(9, loaded.Number);
        Assert.Equal(123.45m, loaded.FirstPriceReal);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithOutOfRangeRegimeLogicEntry_ShouldKeepExistingValue()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[3] = "999";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.RegimeLogicEntry = TradeGridLogicEntryRegime.OnTrade;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(TradeGridLogicEntryRegime.OnTrade, loaded.RegimeLogicEntry);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithOutOfRangeGridType_ShouldKeepExistingValue()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[1] = "999";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.GridType = TradeGridPrimeType.MarketMaking;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(TradeGridPrimeType.MarketMaking, loaded.GridType);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithOutOfRangeRegime_ShouldKeepExistingValue()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[2] = "999";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.Regime = TradeGridRegime.On;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(TradeGridRegime.On, loaded.Regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithZeroDelay_ShouldApplyDefault()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[11] = "0";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.DelayInReal = 777;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(500, loaded.DelayInReal);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithInvalidMicroVolumesBool_ShouldKeepExistingValue()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[11] = "250";
        primeFields[12] = "badBool";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.CheckMicroVolumes = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(250, loaded.DelayInReal);
        Assert.False(loaded.CheckMicroVolumes);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMissingDistanceAndMakerOnlyTail_ShouldApplyDefaults()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        if (primeFields.Length > 14)
        {
            primeFields[13] = string.Empty;
            primeFields[14] = string.Empty;
        }

        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.MaxDistanceToOrdersPercent = 9m;
        loaded.OpenOrdersMakerOnly = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(1.5m, loaded.MaxDistanceToOrdersPercent);
        Assert.True(loaded.OpenOrdersMakerOnly);
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

    [Fact]
    public void Stage2Step2_2_TradeGrid_HaveOrdersWithNoMarketOrders_ShouldRemoveStaleNoneOpenOrder()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();
        grid.LogMessageEvent += (_, _) => { };

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(position, "_openOrders", new List<Order>
        {
            new Order
            {
                State = OrderStateType.None,
                NumberMarket = null
            }
        });
        SetPrivateField(position, "_closeOrders", new List<Order>());

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { Position = position, PositionNum = 1 }
        };

        SetPrivateField(grid, "_lastNoneOrderTime", DateTime.Now.AddMinutes(-6));

        bool hasNoMarketOrders = grid.HaveOrdersWithNoMarketOrders();

        Assert.True(hasNoMarketOrders);
        Assert.Empty(position.OpenOrders);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_HaveOrdersWithNoMarketOrders_ShouldRemoveStaleNoneCloseOrder()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();
        grid.LogMessageEvent += (_, _) => { };

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(position, "_openOrders", new List<Order>());
        SetPrivateField(position, "_closeOrders", new List<Order>
        {
            new Order
            {
                State = OrderStateType.None,
                NumberMarket = null
            }
        });

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { Position = position, PositionNum = 1 }
        };

        SetPrivateField(grid, "_lastNoneOrderTime", DateTime.Now.AddMinutes(-6));

        bool hasNoMarketOrders = grid.HaveOrdersWithNoMarketOrders();

        Assert.True(hasNoMarketOrders);
        Assert.Empty(position.CloseOrders);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_QueryMethods_WithSparseLines_ShouldNotThrow()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.GridSide = Side.Buy;

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridSparseLines", StartProgram.IsTester, false);
        SetPrivateField(tab, "_connector", connector);
        SetPrivateField(tab, "_security", new Security
        {
            Name = connector.SecurityName,
            PriceStep = 1m
        });
        grid.Tab = tab;

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(position, "_openOrders", new List<Order>
        {
            new Order { State = OrderStateType.Active, VolumeExecute = 1m }
        });
        SetPrivateField(position, "_closeOrders", new List<Order>
        {
            new Order { State = OrderStateType.Active, VolumeExecute = 0m }
        });

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine
            {
                Side = Side.Buy,
                PriceEnter = 100m,
                PriceExit = 101m,
                Volume = 1m,
                Position = position,
                PositionNum = 1
            }
        };

        List<TradeGridLine>? openPositions = null;
        List<Position>? positions = null;
        List<TradeGridLine>? openNeed = null;
        List<TradeGridLine>? openFact = null;
        List<TradeGridLine>? closeFact = null;

        Exception? error = Record.Exception(() =>
        {
            openPositions = grid.GetLinesWithOpenPosition();
            positions = grid.GetPositionByGrid();
            openNeed = grid.GetLinesWithOpenOrdersNeed(100m);
            openFact = grid.GetLinesWithOpenOrdersFact();
            closeFact = grid.GetLinesWithClosingOrdersFact();
        });

        Assert.Null(error);
        Assert.NotNull(openPositions);
        Assert.NotNull(positions);
        Assert.NotNull(openNeed);
        Assert.NotNull(openFact);
        Assert.NotNull(closeFact);
        Assert.Single(openPositions);
        Assert.Single(positions);
        Assert.Single(openNeed);
        Assert.Single(openFact);
        Assert.Single(closeFact);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_MiddleEntryPrice_WithZeroTradeVolume_ShouldReturnZero()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        Order openOrder = new Order();
        SetPrivateField(openOrder, "_trades", new List<MyTrade>
        {
            new MyTrade
            {
                Volume = 0m,
                Price = 100m
            }
        });
        SetPrivateField(position, "_openOrders", new List<Order> { openOrder });
        SetPrivateField(position, "_closeOrders", new List<Order>());

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine
            {
                Position = position,
                PositionNum = 1
            }
        };

        decimal middleEntryPrice = -1m;
        Exception? error = Record.Exception(() => middleEntryPrice = grid.MiddleEntryPrice);

        Assert.Null(error);
        Assert.Equal(0m, middleEntryPrice);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_OrderHelpers_WithNullLastCandle_ShouldStaySafe()
    {
        TradeGrid grid = CreateBareGrid();

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridOrderHelpers", StartProgram.IsTester, false);
        TimeFrameBuilder builder = new TimeFrameBuilder("CodexGridOrderHelpers", StartProgram.IsTester);
        CandleSeries series = new CandleSeries(builder, new Security(), StartProgram.IsTester)
        {
            CandlesAll = new List<Candle> { null! }
        };
        SetPrivateField(connector, "_mySeries", series);
        SetPrivateField(tab, "_connector", connector);
        SetPrivateField(tab, "_security", new Security
        {
            Name = connector.SecurityName,
            PriceStep = 1m,
            Lot = 1m
        });
        grid.Tab = tab;

        List<Order>? openOrdersHole = null;
        Exception? error = Record.Exception(() =>
        {
            InvokePrivateNoArg(grid, "TrySetOpenOrders");
            InvokePrivateNoArg(grid, "TryRemoveWrongOrders");

            MethodInfo method = typeof(TradeGrid).GetMethod("GetOpenOrdersGridHole", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method GetOpenOrdersGridHole not found.");
            openOrdersHole = method.Invoke(grid, null) as List<Order>;
        });

        Assert.Null(error);
        Assert.Null(openOrdersHole);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_OrderHelpers_WithEmptyCandles_ShouldStaySafe()
    {
        TradeGrid grid = CreateBareGrid();

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridOrderHelpersEmptyCandles", StartProgram.IsTester, false);
        TimeFrameBuilder builder = new TimeFrameBuilder("CodexGridOrderHelpersEmptyCandles", StartProgram.IsTester);
        CandleSeries series = new CandleSeries(builder, new Security(), StartProgram.IsTester)
        {
            CandlesAll = new List<Candle>()
        };
        SetPrivateField(connector, "_mySeries", series);
        SetPrivateField(tab, "_connector", connector);
        SetPrivateField(tab, "_security", new Security
        {
            Name = connector.SecurityName,
            PriceStep = 1m,
            Lot = 1m
        });
        grid.Tab = tab;

        List<Order>? openOrdersHole = null;
        Exception? error = Record.Exception(() =>
        {
            InvokePrivateNoArg(grid, "TrySetOpenOrders");
            InvokePrivateNoArg(grid, "TryRemoveWrongOrders");

            MethodInfo method = typeof(TradeGrid).GetMethod("GetOpenOrdersGridHole", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method GetOpenOrdersGridHole not found.");
            openOrdersHole = method.Invoke(grid, null) as List<Order>;
        });

        Assert.Null(error);
        Assert.Null(openOrdersHole);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_SparseLines_JournalAndOrderStatePaths_ShouldStaySafe()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();

        Position journalPosition = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        journalPosition.Number = 42;
        SetPrivateField(journalPosition, "_openOrders", new List<Order>());
        SetPrivateField(journalPosition, "_closeOrders", new List<Order>());

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine
            {
                Volume = 2m,
                PositionNum = 42,
                Position = null
            }
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        OsEngine.Journal.Journal journal =
            (OsEngine.Journal.Journal)RuntimeHelpers.GetUninitializedObject(typeof(OsEngine.Journal.Journal));
        PositionController controller = (PositionController)RuntimeHelpers.GetUninitializedObject(typeof(PositionController));
        SetPrivateField(controller, "_deals", new List<Position> { journalPosition });
        SetPrivateField(journal, "_positionController", controller);
        tab._journal = journal;
        grid.Tab = tab;

        decimal allVolume = -1m;
        bool hasNoMarketOrders = true;
        bool hasRecentCancel = true;

        Exception? error = Record.Exception(() =>
        {
            InvokePrivateNoArg(grid, "TryDeleteDonePositions");
            InvokePrivateNoArg(grid, "TryFindPositionsInJournalAfterReconnect");
            InvokePrivateWithArgs(grid, "TryDeletePositionsFromJournal", journalPosition);
            allVolume = grid.AllVolumeInLines;
            hasNoMarketOrders = grid.HaveOrdersWithNoMarketOrders();
            hasRecentCancel = grid.HaveOrdersTryToCancelLastSecond();
        });

        Assert.Null(error);
        Assert.Equal(2m, allVolume);
        Assert.False(hasNoMarketOrders);
        Assert.False(hasRecentCancel);
        Assert.NotNull(grid.GridCreator.Lines[1].Position);
        Assert.Equal(42, grid.GridCreator.Lines[1].PositionNum);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_EventAndLifecycle_WithSparseLines_ShouldStaySafe()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();
        grid.Regime = TradeGridRegime.On;

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        position.Number = 7;
        position.State = PositionStateType.OpeningFail;
        SetPrivateField(position, "_openOrders", new List<Order>());
        SetPrivateField(position, "_closeOrders", new List<Order>());

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine
            {
                Position = position,
                PositionNum = position.Number
            }
        };

        Exception? error = Record.Exception(() =>
        {
            InvokePrivateWithArgs(grid, "Tab_PositionOpeningSuccesEvent", position);
            InvokePrivateWithArgs(grid, "Tab_PositionOpeningFailEvent", position);
            InvokePrivateWithArgs(grid, "Tab_PositionClosingFailEvent", position);
            InvokePrivateNoArg(grid, "TryDeleteOpeningFailPositions");
            InvokePrivateNoArg(grid, "Connector_TestStartEvent");
        });

        Assert.Null(error);
        Assert.Null(grid.GridCreator.Lines[1].Position);
        Assert.Equal(0, grid.GridCreator.Lines[1].PositionNum);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_RemoveSelected_WithSparseLines_ShouldNotThrow()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine { PriceEnter = 100m, Side = Side.Buy }
        };

        Exception? error = Record.Exception(() =>
        {
            grid.RemoveSelected(new List<int> { -1 });
            grid.RemoveSelected(new List<int> { 0 });
            grid.RemoveSelected(new List<int> { 0 });
        });

        Assert.Null(error);
        Assert.Empty(grid.GridCreator.Lines);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_ForcedCloseAndVolume_WithSparseLines_ShouldStaySafe()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();
        grid.Tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        grid.CheckMicroVolumes = false;
        grid.Regime = TradeGridRegime.CloseForced;

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        position.State = PositionStateType.Done;
        SetPrivateField(position, "_openOrders", new List<Order>());
        SetPrivateField(position, "_closeOrders", new List<Order>
        {
            new Order { State = OrderStateType.Done, VolumeExecute = 1m }
        });

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine
            {
                Position = position,
                PositionNum = 1
            }
        };

        decimal openVolumeByLines = 0m;
        bool haveCloseOrders = true;

        Exception? error = Record.Exception(() =>
        {
            openVolumeByLines = grid.OpenVolumeByLines;
            haveCloseOrders = grid.HaveCloseOrders;
            InvokePrivateNoArg(grid, "TryForcedCloseGrid");
        });

        Assert.Null(error);
        Assert.Equal(-1m, openVolumeByLines);
        Assert.False(haveCloseOrders);
        Assert.Equal(TradeGridRegime.Off, grid.Regime);
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
