#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Candles;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Market.Connectors;
using OsEngine.Journal.Internal;
using OsEngine.Logging;
using OsEngine.OsTrader.Grids;
using OsEngine.OsTrader.Panels.Tab;
using System.Windows.Forms;
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
    public void Stage2Step2_2_TradeGridNonTradePeriods_LoadFromString_WithInvalidEnumTokens_ShouldKeepExistingValues()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");
        periods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        periods.NonTradePeriod2Regime = TradeGridRegime.OffAndCancelOrders;

        Exception? error = Record.Exception(() => periods.LoadFromString("badEnum@alsoBad"));

        Assert.Null(error);
        Assert.Equal(TradeGridRegime.CloseOnly, periods.NonTradePeriod1Regime);
        Assert.Equal(TradeGridRegime.OffAndCancelOrders, periods.NonTradePeriod2Regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_LoadFromString_WithCaseInsensitiveEnums_ShouldParse()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");

        Exception? error = Record.Exception(() => periods.LoadFromString("closeonly@offandcancelorders"));

        Assert.Null(error);
        Assert.Equal(TradeGridRegime.CloseOnly, periods.NonTradePeriod1Regime);
        Assert.Equal(TradeGridRegime.OffAndCancelOrders, periods.NonTradePeriod2Regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_GetRegime_WithFirstPeriodBlocked_ShouldPreferFirstRegime()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");
        DateTime curTime = DateTime.Today;
        periods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        periods.NonTradePeriod2Regime = TradeGridRegime.OffAndCancelOrders;

        switch (curTime.DayOfWeek)
        {
            case DayOfWeek.Monday:
                periods.SettingsPeriod1.TradeInMonday = false;
                periods.SettingsPeriod2.TradeInMonday = false;
                break;
            case DayOfWeek.Tuesday:
                periods.SettingsPeriod1.TradeInTuesday = false;
                periods.SettingsPeriod2.TradeInTuesday = false;
                break;
            case DayOfWeek.Wednesday:
                periods.SettingsPeriod1.TradeInWednesday = false;
                periods.SettingsPeriod2.TradeInWednesday = false;
                break;
            case DayOfWeek.Thursday:
                periods.SettingsPeriod1.TradeInThursday = false;
                periods.SettingsPeriod2.TradeInThursday = false;
                break;
            case DayOfWeek.Friday:
                periods.SettingsPeriod1.TradeInFriday = false;
                periods.SettingsPeriod2.TradeInFriday = false;
                break;
            case DayOfWeek.Saturday:
                periods.SettingsPeriod1.TradeInSaturday = false;
                periods.SettingsPeriod2.TradeInSaturday = false;
                break;
            case DayOfWeek.Sunday:
                periods.SettingsPeriod1.TradeInSunday = false;
                periods.SettingsPeriod2.TradeInSunday = false;
                break;
        }

        TradeGridRegime regime = periods.GetNonTradePeriodsRegime(curTime);

        Assert.Equal(TradeGridRegime.CloseOnly, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_GetRegime_WithSecondPeriodBlocked_ShouldReturnSecondRegime()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");
        DateTime curTime = DateTime.Today;
        periods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        periods.NonTradePeriod2Regime = TradeGridRegime.OffAndCancelOrders;

        switch (curTime.DayOfWeek)
        {
            case DayOfWeek.Monday:
                periods.SettingsPeriod2.TradeInMonday = false;
                break;
            case DayOfWeek.Tuesday:
                periods.SettingsPeriod2.TradeInTuesday = false;
                break;
            case DayOfWeek.Wednesday:
                periods.SettingsPeriod2.TradeInWednesday = false;
                break;
            case DayOfWeek.Thursday:
                periods.SettingsPeriod2.TradeInThursday = false;
                break;
            case DayOfWeek.Friday:
                periods.SettingsPeriod2.TradeInFriday = false;
                break;
            case DayOfWeek.Saturday:
                periods.SettingsPeriod2.TradeInSaturday = false;
                break;
            case DayOfWeek.Sunday:
                periods.SettingsPeriod2.TradeInSunday = false;
                break;
        }

        TradeGridRegime regime = periods.GetNonTradePeriodsRegime(curTime);

        Assert.Equal(TradeGridRegime.OffAndCancelOrders, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_GetRegime_WithBothPeriodsOpen_ShouldReturnOn()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");

        TradeGridRegime regime = periods.GetNonTradePeriodsRegime(DateTime.Today);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_GetSaveString_ShouldKeepReservedTailShape()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");
        periods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        periods.NonTradePeriod2Regime = TradeGridRegime.OffAndCancelOrders;

        string save = periods.GetSaveString();

        Assert.Equal("CloseOnly@OffAndCancelOrders@@@@@", save);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_Delete_ShouldClearSettingsAndStayIdempotent()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");

        Exception? error = Record.Exception(() =>
        {
            periods.Delete();
            periods.Delete();
        });

        Assert.Null(error);
        Assert.Null(periods.SettingsPeriod1);
        Assert.Null(periods.SettingsPeriod2);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_SendNewLogMessage_WithSubscriber_ShouldForwardMessage()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");
        string? receivedMessage = null;
        LogMessageType? receivedType = null;

        periods.LogMessageEvent += (message, type) =>
        {
            receivedMessage = message;
            receivedType = type;
        };

        periods.SendNewLogMessage("CodexPeriodsMessage", LogMessageType.Signal);

        Assert.Equal("CodexPeriodsMessage", receivedMessage);
        Assert.Equal(LogMessageType.Signal, receivedType);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");

        Exception? error = Record.Exception(() =>
            periods.SendNewLogMessage("CodexPeriodsNoSubscriber", LogMessageType.System));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_LoadFromString_WithEmptyLikePayloads_ShouldKeepExistingValues()
    {
        TradeGridNonTradePeriods periods = new TradeGridNonTradePeriods("CodexGridPeriods");
        periods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        periods.NonTradePeriod2Regime = TradeGridRegime.OffAndCancelOrders;

        Exception? error = Record.Exception(() =>
        {
            periods.LoadFromString(null);
            periods.LoadFromString(string.Empty);
            periods.LoadFromString("  \r\n \t  ");
        });

        Assert.Null(error);
        Assert.Equal(TradeGridRegime.CloseOnly, periods.NonTradePeriod1Regime);
        Assert.Equal(TradeGridRegime.OffAndCancelOrders, periods.NonTradePeriod2Regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridNonTradePeriods_GetSaveString_LoadFromString_ShouldRoundTrip()
    {
        TradeGridNonTradePeriods source = new TradeGridNonTradePeriods("CodexGridPeriods");
        source.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        source.NonTradePeriod2Regime = TradeGridRegime.OffAndCancelOrders;

        TradeGridNonTradePeriods loaded = new TradeGridNonTradePeriods("CodexGridPeriodsLoaded");

        Exception? error = Record.Exception(() => loaded.LoadFromString(source.GetSaveString()));

        Assert.Null(error);
        Assert.Equal(TradeGridRegime.CloseOnly, loaded.NonTradePeriod1Regime);
        Assert.Equal(TradeGridRegime.OffAndCancelOrders, loaded.NonTradePeriod2Regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetSaveString_ShouldKeepReservedTailShape()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 2.5m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseOnly,
            StopGridByMoveDownIsOn = false,
            StopGridByMoveDownValuePercent = 1.5m,
            StopGridByMoveDownReaction = TradeGridRegime.Off,
            StopGridByPositionsCountIsOn = true,
            StopGridByPositionsCountValue = 7,
            StopGridByPositionsCountReaction = TradeGridRegime.CloseForced,
            StopGridByLifeTimeIsOn = true,
            StopGridByLifeTimeSecondsToLife = 600,
            StopGridByLifeTimeReaction = TradeGridRegime.OffAndCancelOrders,
            StopGridByTimeOfDayIsOn = true,
            StopGridByTimeOfDayHour = 9,
            StopGridByTimeOfDayMinute = 30,
            StopGridByTimeOfDaySecond = 45,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseOnly
        };

        string save = stopBy.GetSaveString();

        Assert.Equal("True@2.5@CloseOnly@False@1.5@Off@True@7@CloseForced@True@600@OffAndCancelOrders@True@9@30@45@CloseOnly@@@@@@", save);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_SendNewLogMessage_WithSubscriber_ShouldForwardMessage()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy();
        string? receivedMessage = null;
        LogMessageType? receivedType = null;

        stopBy.LogMessageEvent += (message, type) =>
        {
            receivedMessage = message;
            receivedType = type;
        };

        stopBy.SendNewLogMessage("CodexStopByMessage", LogMessageType.Signal);

        Assert.Equal("CodexStopByMessage", receivedMessage);
        Assert.Equal(LogMessageType.Signal, receivedType);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy();

        Exception? error = Record.Exception(() =>
            stopBy.SendNewLogMessage("CodexStopByNoSubscriber", LogMessageType.System));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithEmptyLikePayloads_ShouldKeepExistingValues()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 2.5m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseOnly
        };

        Exception? error = Record.Exception(() =>
        {
            stopBy.LoadFromString(null);
            stopBy.LoadFromString(string.Empty);
            stopBy.LoadFromString("  \r\n \t  ");
        });

        Assert.Null(error);
        Assert.True(stopBy.StopGridByMoveUpIsOn);
        Assert.Equal(2.5m, stopBy.StopGridByMoveUpValuePercent);
        Assert.Equal(TradeGridRegime.CloseOnly, stopBy.StopGridByMoveUpReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetSaveString_LoadFromString_ShouldRoundTrip()
    {
        TradeGridStopBy source = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 2.5m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseOnly,
            StopGridByMoveDownIsOn = true,
            StopGridByMoveDownValuePercent = 1.5m,
            StopGridByMoveDownReaction = TradeGridRegime.Off,
            StopGridByPositionsCountIsOn = true,
            StopGridByPositionsCountValue = 7,
            StopGridByPositionsCountReaction = TradeGridRegime.CloseForced,
            StopGridByLifeTimeIsOn = true,
            StopGridByLifeTimeSecondsToLife = 600,
            StopGridByLifeTimeReaction = TradeGridRegime.OffAndCancelOrders,
            StopGridByTimeOfDayIsOn = true,
            StopGridByTimeOfDayHour = 9,
            StopGridByTimeOfDayMinute = 30,
            StopGridByTimeOfDaySecond = 45,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseOnly
        };

        TradeGridStopBy loaded = new TradeGridStopBy();

        Exception? error = Record.Exception(() => loaded.LoadFromString(source.GetSaveString()));

        Assert.Null(error);
        Assert.True(loaded.StopGridByMoveUpIsOn);
        Assert.Equal(2.5m, loaded.StopGridByMoveUpValuePercent);
        Assert.Equal(TradeGridRegime.CloseOnly, loaded.StopGridByMoveUpReaction);
        Assert.True(loaded.StopGridByMoveDownIsOn);
        Assert.Equal(1.5m, loaded.StopGridByMoveDownValuePercent);
        Assert.Equal(TradeGridRegime.Off, loaded.StopGridByMoveDownReaction);
        Assert.True(loaded.StopGridByPositionsCountIsOn);
        Assert.Equal(7, loaded.StopGridByPositionsCountValue);
        Assert.Equal(TradeGridRegime.CloseForced, loaded.StopGridByPositionsCountReaction);
        Assert.True(loaded.StopGridByLifeTimeIsOn);
        Assert.Equal(600, loaded.StopGridByLifeTimeSecondsToLife);
        Assert.Equal(TradeGridRegime.OffAndCancelOrders, loaded.StopGridByLifeTimeReaction);
        Assert.True(loaded.StopGridByTimeOfDayIsOn);
        Assert.Equal(9, loaded.StopGridByTimeOfDayHour);
        Assert.Equal(30, loaded.StopGridByTimeOfDayMinute);
        Assert.Equal(45, loaded.StopGridByTimeOfDaySecond);
        Assert.Equal(TradeGridRegime.CloseOnly, loaded.StopGridByTimeOfDayReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_GetSaveString_ShouldKeepReservedTailShape()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            GridSide = Side.Sell,
            FirstPrice = 123.45m,
            LineCountStart = 3,
            TypeStep = TradeGridValueType.Absolute,
            LineStep = 1.2m,
            StepMultiplicator = 1.1m,
            TypeProfit = TradeGridValueType.Percent,
            ProfitStep = 0.8m,
            ProfitMultiplicator = 1.3m,
            TypeVolume = TradeGridVolumeType.Contracts,
            StartVolume = 2.5m,
            MartingaleMultiplicator = 1.4m,
            TradeAssetInPortfolio = "AssetX",
            Lines = new List<TradeGridLine>()
        };

        string save = creator.GetSaveString();

        Assert.Equal("Sell@123.45@3@Absolute@1.2@1.1@Percent@0.8@1.3@Contracts@2.5@1.4@AssetX@@@@@@", save);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_LoadFromString_WithEmptyLikePayloads_ShouldKeepExistingValues()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            GridSide = Side.Sell,
            TypeVolume = TradeGridVolumeType.Contracts,
            TradeAssetInPortfolio = "AssetX"
        };

        Exception? error = Record.Exception(() =>
        {
            creator.LoadFromString(null);
            creator.LoadFromString(string.Empty);
            creator.LoadFromString("  \r\n \t  ");
        });

        Assert.Null(error);
        Assert.Equal(Side.Sell, creator.GridSide);
        Assert.Equal(TradeGridVolumeType.Contracts, creator.TypeVolume);
        Assert.Equal("AssetX", creator.TradeAssetInPortfolio);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_GetSaveString_LoadFromString_ShouldRoundTrip()
    {
        TradeGridCreator source = new TradeGridCreator
        {
            GridSide = Side.Sell,
            FirstPrice = 123.45m,
            LineCountStart = 3,
            TypeStep = TradeGridValueType.Absolute,
            LineStep = 1.2m,
            StepMultiplicator = 1.1m,
            TypeProfit = TradeGridValueType.Percent,
            ProfitStep = 0.8m,
            ProfitMultiplicator = 1.3m,
            TypeVolume = TradeGridVolumeType.Contracts,
            StartVolume = 2.5m,
            MartingaleMultiplicator = 1.4m,
            TradeAssetInPortfolio = "AssetX"
        };

        TradeGridCreator loaded = new TradeGridCreator();

        Exception? error = Record.Exception(() => loaded.LoadFromString(source.GetSaveString()));

        Assert.Null(error);
        Assert.Equal(Side.Sell, loaded.GridSide);
        Assert.Equal(123.45m, loaded.FirstPrice);
        Assert.Equal(3, loaded.LineCountStart);
        Assert.Equal(TradeGridValueType.Absolute, loaded.TypeStep);
        Assert.Equal(1.2m, loaded.LineStep);
        Assert.Equal(1.1m, loaded.StepMultiplicator);
        Assert.Equal(TradeGridValueType.Percent, loaded.TypeProfit);
        Assert.Equal(0.8m, loaded.ProfitStep);
        Assert.Equal(1.3m, loaded.ProfitMultiplicator);
        Assert.Equal(TradeGridVolumeType.Contracts, loaded.TypeVolume);
        Assert.Equal(2.5m, loaded.StartVolume);
        Assert.Equal(1.4m, loaded.MartingaleMultiplicator);
        Assert.Equal("AssetX", loaded.TradeAssetInPortfolio);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridLine_GetSaveStr_ShouldUseInvariantCultureAndTrailingSeparator()
    {
        TradeGridLine line = new TradeGridLine
        {
            PriceEnter = 123.45m,
            Volume = 2.5m,
            Side = Side.Buy,
            PriceExit = 130.75m,
            PositionNum = 7
        };

        string save = line.GetSaveStr();

        Assert.Equal("123.45|2.5|Buy|130.75|7|", save);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridLine_SetFromStr_WithValidPayload_ShouldParseAllFields()
    {
        TradeGridLine line = new TradeGridLine();

        bool parsed = line.SetFromStr("123.45|2.5|sell|130.75|7|");

        Assert.True(parsed);
        Assert.Equal(123.45m, line.PriceEnter);
        Assert.Equal(2.5m, line.Volume);
        Assert.Equal(Side.Sell, line.Side);
        Assert.Equal(130.75m, line.PriceExit);
        Assert.Equal(7, line.PositionNum);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridLine_SetFromStr_WithNegativeValue_ShouldReturnFalseAndKeepDefaults()
    {
        TradeGridLine line = new TradeGridLine
        {
            PriceEnter = 1m,
            Volume = 1m,
            Side = Side.Buy,
            PriceExit = 2m,
            PositionNum = 5
        };

        bool parsed = line.SetFromStr("-1|2|Buy|3|7|");

        Assert.False(parsed);
        Assert.Equal(1m, line.PriceEnter);
        Assert.Equal(1m, line.Volume);
        Assert.Equal(Side.Buy, line.Side);
        Assert.Equal(2m, line.PriceExit);
        Assert.Equal(5, line.PositionNum);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridLine_SetFromStr_WithInvalidSide_ShouldReturnFalse()
    {
        TradeGridLine line = new TradeGridLine();

        bool parsed = line.SetFromStr("1|2|Hold|3|7|");

        Assert.False(parsed);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridLine_SetFromStr_WithWhitespacePayload_ShouldReturnFalse()
    {
        TradeGridLine line = new TradeGridLine();

        bool parsed = line.SetFromStr("   ");

        Assert.False(parsed);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridLine_SetFromStr_WithInvalidPositionNum_ShouldKeepDefaultPosition()
    {
        TradeGridLine line = new TradeGridLine
        {
            PositionNum = 9
        };

        bool parsed = line.SetFromStr("1|2|Buy|3|bad|");

        Assert.True(parsed);
        Assert.Equal(1m, line.PriceEnter);
        Assert.Equal(2m, line.Volume);
        Assert.Equal(Side.Buy, line.Side);
        Assert.Equal(3m, line.PriceExit);
        Assert.Equal(9, line.PositionNum);
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
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMalformedFields_ShouldKeepValuesAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = false,
            StopGridByMoveUpValuePercent = 2.5m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseForced,
            StopGridByMoveDownIsOn = false,
            StopGridByMoveDownValuePercent = 2.5m,
            StopGridByMoveDownReaction = TradeGridRegime.CloseForced,
            StopGridByPositionsCountIsOn = false,
            StopGridByPositionsCountValue = 200,
            StopGridByPositionsCountReaction = TradeGridRegime.CloseForced,
            StopGridByLifeTimeIsOn = false,
            StopGridByLifeTimeSecondsToLife = 600,
            StopGridByLifeTimeReaction = TradeGridRegime.CloseForced,
            StopGridByTimeOfDayIsOn = false,
            StopGridByTimeOfDayHour = 14,
            StopGridByTimeOfDayMinute = 15,
            StopGridByTimeOfDaySecond = 16,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "badBool@badDecimal@badEnum@on@3.5@Off@1@250@CloseForced@badBool@900@Off@on@20@30@40@CloseOnly"));

        Assert.Null(error);
        Assert.False(stopBy.StopGridByMoveUpIsOn);
        Assert.Equal(2.5m, stopBy.StopGridByMoveUpValuePercent);
        Assert.Equal(TradeGridRegime.CloseForced, stopBy.StopGridByMoveUpReaction);
        Assert.True(stopBy.StopGridByMoveDownIsOn);
        Assert.Equal(3.5m, stopBy.StopGridByMoveDownValuePercent);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByMoveDownReaction);
        Assert.True(stopBy.StopGridByPositionsCountIsOn);
        Assert.Equal(250, stopBy.StopGridByPositionsCountValue);
        Assert.Equal(TradeGridRegime.CloseForced, stopBy.StopGridByPositionsCountReaction);
        Assert.False(stopBy.StopGridByLifeTimeIsOn);
        Assert.Equal(900, stopBy.StopGridByLifeTimeSecondsToLife);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByLifeTimeReaction);
        Assert.True(stopBy.StopGridByTimeOfDayIsOn);
        Assert.Equal(20, stopBy.StopGridByTimeOfDayHour);
        Assert.Equal(30, stopBy.StopGridByTimeOfDayMinute);
        Assert.Equal(40, stopBy.StopGridByTimeOfDaySecond);
        Assert.Equal(TradeGridRegime.CloseOnly, stopBy.StopGridByTimeOfDayReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithNonPositiveValues_ShouldKeepExistingValues()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpValuePercent = 2.5m,
            StopGridByMoveDownValuePercent = 2.5m,
            StopGridByPositionsCountValue = 200,
            StopGridByLifeTimeSecondsToLife = 600
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@0@Off@on@-1@Off@on@0@CloseForced@on@-5@Off@off@14@15@16@CloseForced"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByMoveUpIsOn);
        Assert.Equal(2.5m, stopBy.StopGridByMoveUpValuePercent);
        Assert.True(stopBy.StopGridByMoveDownIsOn);
        Assert.Equal(2.5m, stopBy.StopGridByMoveDownValuePercent);
        Assert.True(stopBy.StopGridByPositionsCountIsOn);
        Assert.Equal(200, stopBy.StopGridByPositionsCountValue);
        Assert.True(stopBy.StopGridByLifeTimeIsOn);
        Assert.Equal(600, stopBy.StopGridByLifeTimeSecondsToLife);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithFlexibleBools_ShouldParse()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy();

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "yes@2.5@CloseForced@off@2.5@Off@1@200@CloseOnly@on@600@CloseForced@no@14@15@16@Off"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByMoveUpIsOn);
        Assert.False(stopBy.StopGridByMoveDownIsOn);
        Assert.True(stopBy.StopGridByPositionsCountIsOn);
        Assert.True(stopBy.StopGridByLifeTimeIsOn);
        Assert.False(stopBy.StopGridByTimeOfDayIsOn);
        Assert.Equal(TradeGridRegime.CloseOnly, stopBy.StopGridByPositionsCountReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithOutOfRangeTimeFields_ShouldKeepExistingValues()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDayHour = 14,
            StopGridByTimeOfDayMinute = 15,
            StopGridByTimeOfDaySecond = 16
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@CloseForced@off@2.5@Off@1@200@CloseOnly@on@600@CloseForced@on@24@60@61@Off"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByTimeOfDayIsOn);
        Assert.Equal(14, stopBy.StopGridByTimeOfDayHour);
        Assert.Equal(15, stopBy.StopGridByTimeOfDayMinute);
        Assert.Equal(16, stopBy.StopGridByTimeOfDaySecond);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByTimeOfDayReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingMoveDownBool_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveDownIsOn = false,
            StopGridByMoveDownValuePercent = 2.5m,
            StopGridByMoveDownReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@CloseForced@@3.5@Off@1@250@CloseOnly@on@900@Off@on@20@30@40@CloseForced"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByMoveUpIsOn);
        Assert.False(stopBy.StopGridByMoveDownIsOn);
        Assert.Equal(3.5m, stopBy.StopGridByMoveDownValuePercent);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByMoveDownReaction);
        Assert.Equal(250, stopBy.StopGridByPositionsCountValue);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidPositionsReaction_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByPositionsCountReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@Off@1@250@badEnum@on@900@Off@on@20@30@40@CloseOnly"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByPositionsCountIsOn);
        Assert.Equal(250, stopBy.StopGridByPositionsCountValue);
        Assert.Equal(TradeGridRegime.CloseForced, stopBy.StopGridByPositionsCountReaction);
        Assert.Equal(TradeGridRegime.CloseOnly, stopBy.StopGridByTimeOfDayReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingLifeTimeSeconds_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByLifeTimeSecondsToLife = 600,
            StopGridByLifeTimeReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@Off@1@250@CloseOnly@on@@Off@on@20@30@40@CloseForced"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByLifeTimeIsOn);
        Assert.Equal(600, stopBy.StopGridByLifeTimeSecondsToLife);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByLifeTimeReaction);
        Assert.True(stopBy.StopGridByTimeOfDayIsOn);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidTimeHourAndValidTail_ShouldKeepHourAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDayHour = 14,
            StopGridByTimeOfDayMinute = 15,
            StopGridByTimeOfDaySecond = 16,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@Off@1@250@CloseOnly@on@900@Off@on@99@30@40@Off"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByTimeOfDayIsOn);
        Assert.Equal(14, stopBy.StopGridByTimeOfDayHour);
        Assert.Equal(30, stopBy.StopGridByTimeOfDayMinute);
        Assert.Equal(40, stopBy.StopGridByTimeOfDaySecond);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByTimeOfDayReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingMoveUpBool_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = false,
            StopGridByMoveUpValuePercent = 2.5m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "@3.5@Off@on@2.5@Off@1@250@CloseOnly@on@900@Off@on@20@30@40@CloseForced"));

        Assert.Null(error);
        Assert.False(stopBy.StopGridByMoveUpIsOn);
        Assert.Equal(3.5m, stopBy.StopGridByMoveUpValuePercent);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByMoveUpReaction);
        Assert.True(stopBy.StopGridByMoveDownIsOn);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidMoveUpReaction_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@3.5@badEnum@on@2.5@Off@1@250@CloseOnly@on@900@Off@on@20@30@40@CloseForced"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByMoveUpIsOn);
        Assert.Equal(3.5m, stopBy.StopGridByMoveUpValuePercent);
        Assert.Equal(TradeGridRegime.CloseForced, stopBy.StopGridByMoveUpReaction);
        Assert.Equal(TradeGridRegime.CloseOnly, stopBy.StopGridByPositionsCountReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingTimeMinute_ShouldKeepValueAndContinueTailParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDayMinute = 15,
            StopGridByTimeOfDaySecond = 16,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@Off@1@250@CloseOnly@on@900@Off@on@20@@40@Off"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByTimeOfDayIsOn);
        Assert.Equal(20, stopBy.StopGridByTimeOfDayHour);
        Assert.Equal(15, stopBy.StopGridByTimeOfDayMinute);
        Assert.Equal(40, stopBy.StopGridByTimeOfDaySecond);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByTimeOfDayReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingTimeSecond_ShouldKeepValueAndParseReaction()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDaySecond = 16,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@Off@1@250@CloseOnly@on@900@Off@on@20@30@@Off"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByTimeOfDayIsOn);
        Assert.Equal(20, stopBy.StopGridByTimeOfDayHour);
        Assert.Equal(30, stopBy.StopGridByTimeOfDayMinute);
        Assert.Equal(16, stopBy.StopGridByTimeOfDaySecond);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByTimeOfDayReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidTimeReaction_ShouldKeepValue()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@Off@1@250@CloseOnly@on@900@Off@on@20@30@40@badEnum"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByTimeOfDayIsOn);
        Assert.Equal(20, stopBy.StopGridByTimeOfDayHour);
        Assert.Equal(30, stopBy.StopGridByTimeOfDayMinute);
        Assert.Equal(40, stopBy.StopGridByTimeOfDaySecond);
        Assert.Equal(TradeGridRegime.CloseForced, stopBy.StopGridByTimeOfDayReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingMoveUpValue_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpValuePercent = 2.5m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@@Off@on@3.5@CloseOnly@1@250@Off@on@900@CloseForced@on@20@30@40@Off"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByMoveUpIsOn);
        Assert.Equal(2.5m, stopBy.StopGridByMoveUpValuePercent);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByMoveUpReaction);
        Assert.True(stopBy.StopGridByMoveDownIsOn);
        Assert.Equal(3.5m, stopBy.StopGridByMoveDownValuePercent);
        Assert.Equal(TradeGridRegime.CloseOnly, stopBy.StopGridByMoveDownReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidMoveDownReaction_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveDownReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@badEnum@1@250@CloseOnly@on@900@Off@on@20@30@40@CloseForced"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByMoveDownIsOn);
        Assert.Equal(3.5m, stopBy.StopGridByMoveDownValuePercent);
        Assert.Equal(TradeGridRegime.CloseForced, stopBy.StopGridByMoveDownReaction);
        Assert.True(stopBy.StopGridByPositionsCountIsOn);
        Assert.Equal(250, stopBy.StopGridByPositionsCountValue);
        Assert.Equal(TradeGridRegime.CloseOnly, stopBy.StopGridByPositionsCountReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingPositionsCountValue_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByPositionsCountValue = 200,
            StopGridByPositionsCountReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@CloseOnly@1@@Off@on@900@CloseForced@on@20@30@40@Off"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByPositionsCountIsOn);
        Assert.Equal(200, stopBy.StopGridByPositionsCountValue);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByPositionsCountReaction);
        Assert.True(stopBy.StopGridByLifeTimeIsOn);
        Assert.Equal(900, stopBy.StopGridByLifeTimeSecondsToLife);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingTimeFlag_ShouldKeepValueAndContinueTailParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDayIsOn = false,
            StopGridByTimeOfDayHour = 14,
            StopGridByTimeOfDayMinute = 15,
            StopGridByTimeOfDaySecond = 16,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@CloseOnly@1@250@Off@on@900@CloseForced@@20@30@40@Off"));

        Assert.Null(error);
        Assert.False(stopBy.StopGridByTimeOfDayIsOn);
        Assert.Equal(20, stopBy.StopGridByTimeOfDayHour);
        Assert.Equal(30, stopBy.StopGridByTimeOfDayMinute);
        Assert.Equal(40, stopBy.StopGridByTimeOfDaySecond);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByTimeOfDayReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingMoveDownValue_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveDownValuePercent = 2.5m,
            StopGridByMoveDownReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@@CloseOnly@1@250@Off@on@900@CloseForced@on@20@30@40@Off"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByMoveDownIsOn);
        Assert.Equal(2.5m, stopBy.StopGridByMoveDownValuePercent);
        Assert.Equal(TradeGridRegime.CloseOnly, stopBy.StopGridByMoveDownReaction);
        Assert.True(stopBy.StopGridByPositionsCountIsOn);
        Assert.Equal(250, stopBy.StopGridByPositionsCountValue);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingPositionsReaction_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByPositionsCountReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@CloseOnly@1@250@@on@900@Off@on@20@30@40@CloseOnly"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByPositionsCountIsOn);
        Assert.Equal(250, stopBy.StopGridByPositionsCountValue);
        Assert.Equal(TradeGridRegime.CloseForced, stopBy.StopGridByPositionsCountReaction);
        Assert.True(stopBy.StopGridByLifeTimeIsOn);
        Assert.Equal(900, stopBy.StopGridByLifeTimeSecondsToLife);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByLifeTimeReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidLifeTimeReaction_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByLifeTimeReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@CloseOnly@1@250@Off@on@900@badEnum@on@20@30@40@CloseOnly"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByLifeTimeIsOn);
        Assert.Equal(900, stopBy.StopGridByLifeTimeSecondsToLife);
        Assert.Equal(TradeGridRegime.CloseForced, stopBy.StopGridByLifeTimeReaction);
        Assert.True(stopBy.StopGridByTimeOfDayIsOn);
        Assert.Equal(20, stopBy.StopGridByTimeOfDayHour);
        Assert.Equal(TradeGridRegime.CloseOnly, stopBy.StopGridByTimeOfDayReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingTimeHour_ShouldKeepValueAndContinueTailParsing()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDayHour = 14,
            StopGridByTimeOfDayMinute = 15,
            StopGridByTimeOfDaySecond = 16,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced
        };

        Exception? error = Record.Exception(() => stopBy.LoadFromString(
            "on@2.5@Off@on@3.5@CloseOnly@1@250@Off@on@900@CloseForced@on@@30@40@Off"));

        Assert.Null(error);
        Assert.True(stopBy.StopGridByTimeOfDayIsOn);
        Assert.Equal(14, stopBy.StopGridByTimeOfDayHour);
        Assert.Equal(30, stopBy.StopGridByTimeOfDayMinute);
        Assert.Equal(40, stopBy.StopGridByTimeOfDaySecond);
        Assert.Equal(TradeGridRegime.Off, stopBy.StopGridByTimeOfDayReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithPositionsCountLimitReached_ShouldReturnConfiguredReaction()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByPositionsCountIsOn = true,
            StopGridByPositionsCountValue = 3,
            StopGridByPositionsCountReaction = TradeGridRegime.CloseOnly
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_openPositionsBySession", 3);

        TradeGridRegime regime = stopBy.GetRegime(grid, (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple)));

        Assert.Equal(TradeGridRegime.CloseOnly, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveUpLimitReached_ShouldReturnConfiguredReaction()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 2m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseForced
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradePrice", 100m);
        AttachSingleCloseCandle(grid, 102m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.CloseForced, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveDownLimitReached_ShouldReturnConfiguredReaction()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveDownIsOn = true,
            StopGridByMoveDownValuePercent = 2m,
            StopGridByMoveDownReaction = TradeGridRegime.Off
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradePrice", 100m);
        AttachSingleCloseCandle(grid, 98m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.Off, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithLifeTimeExpired_ShouldReturnConfiguredReaction()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByLifeTimeIsOn = true,
            StopGridByLifeTimeSecondsToLife = 5,
            StopGridByLifeTimeReaction = TradeGridRegime.CloseOnly
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradeTime", DateTime.Now.AddSeconds(-10));
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.CloseOnly, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithTimeOfDayReached_ShouldReturnConfiguredReaction()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDayIsOn = true,
            StopGridByTimeOfDayHour = DateTime.Now.Hour,
            StopGridByTimeOfDayMinute = 0,
            StopGridByTimeOfDaySecond = 0,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced
        };

        TradeGrid grid = CreateBareGrid();
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.CloseForced, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithPositionsCountBelowLimit_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByPositionsCountIsOn = true,
            StopGridByPositionsCountValue = 4,
            StopGridByPositionsCountReaction = TradeGridRegime.CloseOnly
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_openPositionsBySession", 3);

        TradeGridRegime regime = stopBy.GetRegime(grid, (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple)));

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveUpBelowLimit_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 2m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseForced
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradePrice", 100m);
        AttachSingleCloseCandle(grid, 101.99m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveDownAboveLimit_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveDownIsOn = true,
            StopGridByMoveDownValuePercent = 2m,
            StopGridByMoveDownReaction = TradeGridRegime.Off
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradePrice", 100m);
        AttachSingleCloseCandle(grid, 98.01m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithLifeTimeNotExpired_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByLifeTimeIsOn = true,
            StopGridByLifeTimeSecondsToLife = 30,
            StopGridByLifeTimeReaction = TradeGridRegime.CloseOnly
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradeTime", DateTime.Now.AddSeconds(-5));
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithTimeOfDayNotReached_ShouldReturnOn()
    {
        DateTime now = DateTime.Now;

        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDayIsOn = true,
            StopGridByTimeOfDayHour = now.Hour,
            StopGridByTimeOfDayMinute = now.Minute,
            StopGridByTimeOfDaySecond = Math.Min(now.Second + 5, 59),
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced
        };

        TradeGrid grid = CreateBareGrid();
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithNoTriggersEnabled_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy();
        TradeGrid grid = CreateBareGrid();
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithEmptyCandles_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 2m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseForced
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradePrice", 100m);
        grid.Tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithZeroFirstPrice_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 2m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseForced
        };

        TradeGrid grid = CreateBareGrid();
        AttachSingleCloseCandle(grid, 102m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithMissingFirstTradeTime_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByLifeTimeIsOn = true,
            StopGridByLifeTimeSecondsToLife = 5,
            StopGridByLifeTimeReaction = TradeGridRegime.CloseOnly
        };

        TradeGrid grid = CreateBareGrid();
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithMissingServerTime_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByLifeTimeIsOn = true,
            StopGridByLifeTimeSecondsToLife = 10,
            StopGridByLifeTimeReaction = TradeGridRegime.CloseOnly
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradeTime", DateTime.Now.AddSeconds(-100));
        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        grid.Tab = tab;

        TradeGridRegime regime = stopBy.GetRegime(grid, tab);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithPositionsAndMoveTriggersReady_ShouldPreferPositionsReaction()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByPositionsCountIsOn = true,
            StopGridByPositionsCountValue = 3,
            StopGridByPositionsCountReaction = TradeGridRegime.CloseOnly,
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 2m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseForced
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_openPositionsBySession", 3);
        SetPrivateField(grid, "_firstTradePrice", 100m);
        AttachSingleCloseCandle(grid, 102m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.CloseOnly, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveAndLifeTimeTriggersReady_ShouldPreferMoveReaction()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 2m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseForced,
            StopGridByLifeTimeIsOn = true,
            StopGridByLifeTimeSecondsToLife = 5,
            StopGridByLifeTimeReaction = TradeGridRegime.CloseOnly
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradePrice", 100m);
        SetPrivateField(grid, "_firstTradeTime", DateTime.Now.AddSeconds(-100));
        AttachSingleCloseCandle(grid, 102m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.CloseForced, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithLifeTimeAndTimeTriggersReady_ShouldPreferLifeTimeReaction()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByLifeTimeIsOn = true,
            StopGridByLifeTimeSecondsToLife = 5,
            StopGridByLifeTimeReaction = TradeGridRegime.CloseOnly,
            StopGridByTimeOfDayIsOn = true,
            StopGridByTimeOfDayHour = DateTime.Now.Hour,
            StopGridByTimeOfDayMinute = 0,
            StopGridByTimeOfDaySecond = 0,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradeTime", DateTime.Now.AddSeconds(-100));
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.CloseOnly, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithOnlyLaterTriggerReady_ShouldReturnLaterReaction()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByPositionsCountIsOn = true,
            StopGridByPositionsCountValue = 10,
            StopGridByPositionsCountReaction = TradeGridRegime.CloseOnly,
            StopGridByLifeTimeIsOn = true,
            StopGridByLifeTimeSecondsToLife = 5,
            StopGridByLifeTimeReaction = TradeGridRegime.Off
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_openPositionsBySession", 3);
        SetPrivateField(grid, "_firstTradeTime", DateTime.Now.AddSeconds(-100));
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.Off, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveUpAndMoveDownBothReady_ShouldPreferMoveUpReaction()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 0m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseOnly,
            StopGridByMoveDownIsOn = true,
            StopGridByMoveDownValuePercent = 0m,
            StopGridByMoveDownReaction = TradeGridRegime.CloseForced
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradePrice", 100m);
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.CloseOnly, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithZeroLastPrice_ShouldReturnOn()
    {
        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByMoveUpIsOn = true,
            StopGridByMoveUpValuePercent = 2m,
            StopGridByMoveUpReaction = TradeGridRegime.CloseForced
        };

        TradeGrid grid = CreateBareGrid();
        SetPrivateField(grid, "_firstTradePrice", 100m);
        AttachSingleCloseCandle(grid, 0m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithTimeOfDayLaterMinute_ShouldReturnConfiguredReaction()
    {
        DateTime now = DateTime.Now;
        int targetHour = now.Hour;
        int targetMinute = now.Minute;
        int targetSecond = 0;

        if (now.Minute > 0)
        {
            targetMinute = now.Minute - 1;
        }
        else if (now.Second > 0)
        {
            targetSecond = now.Second - 1;
        }
        else
        {
            targetHour = now.Hour == 0 ? 0 : now.Hour - 1;
            targetMinute = 59;
        }

        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDayIsOn = true,
            StopGridByTimeOfDayHour = targetHour,
            StopGridByTimeOfDayMinute = targetMinute,
            StopGridByTimeOfDaySecond = targetSecond,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseOnly
        };

        TradeGrid grid = CreateBareGrid();
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.CloseOnly, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithTimeOfDayLaterHour_ShouldReturnConfiguredReaction()
    {
        DateTime now = DateTime.Now;

        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDayIsOn = true,
            StopGridByTimeOfDayHour = now.Hour == 0 ? 0 : now.Hour - 1,
            StopGridByTimeOfDayMinute = 59,
            StopGridByTimeOfDaySecond = 59,
            StopGridByTimeOfDayReaction = TradeGridRegime.Off
        };

        TradeGrid grid = CreateBareGrid();
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        Assert.Equal(TradeGridRegime.Off, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopBy_GetRegime_WithTimeOfDayFutureWithinSameDay_ShouldReturnOn()
    {
        DateTime now = DateTime.Now;

        int targetHour = now.Hour;
        int targetMinute = now.Minute;
        int targetSecond = now.Second;

        if (now.Second < 59)
        {
            targetSecond = now.Second + 1;
        }
        else if (now.Minute < 59)
        {
            targetMinute = now.Minute + 1;
            targetSecond = 0;
        }
        else if (now.Hour < 23)
        {
            targetHour = now.Hour + 1;
            targetMinute = 0;
            targetSecond = 0;
        }
        else
        {
            targetHour = now.Hour;
            targetMinute = now.Minute;
            targetSecond = now.Second;
        }

        TradeGridStopBy stopBy = new TradeGridStopBy
        {
            StopGridByTimeOfDayIsOn = true,
            StopGridByTimeOfDayHour = targetHour,
            StopGridByTimeOfDayMinute = targetMinute,
            StopGridByTimeOfDaySecond = targetSecond,
            StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced
        };

        TradeGrid grid = CreateBareGrid();
        AttachSingleCloseCandle(grid, 100m);

        TradeGridRegime regime = stopBy.GetRegime(grid, grid.Tab);

        if (now.Hour == 23 && now.Minute == 59 && now.Second == 59)
        {
            Assert.Equal(TradeGridRegime.CloseForced, regime);
            return;
        }

        Assert.Equal(TradeGridRegime.On, regime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_ParseLegacyGridsSettings_WithWhitespaceContent_ShouldReturnNull()
    {
        MethodInfo method = typeof(TradeGridsMaster).GetMethod("ParseLegacyGridsSettings", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method ParseLegacyGridsSettings not found.");

        object? result = method.Invoke(null, new object?[] { "   \r\n  " });

        Assert.Null(result);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_ParseLegacyGridsSettings_WithMultilineContent_ShouldCollectNonEmptyLines()
    {
        MethodInfo method = typeof(TradeGridsMaster).GetMethod("ParseLegacyGridsSettings", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method ParseLegacyGridsSettings not found.");

        object result = method.Invoke(null, new object?[] { "1@gridA\r\n\r\n2@gridB\n3@gridC\r\n" })
            ?? throw new InvalidOperationException("Legacy settings parse returned null.");
        PropertyInfo property = result.GetType().GetProperty("GridSaveStrings")
            ?? throw new InvalidOperationException("Property GridSaveStrings not found.");
        List<string> values = (List<string>)(property.GetValue(result)
            ?? throw new InvalidOperationException("GridSaveStrings value is null."));

        Assert.Equal(3, values.Count);
        Assert.Equal("1@gridA", values[0]);
        Assert.Equal("2@gridB", values[1]);
        Assert.Equal("3@gridC", values[2]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithSeparator_ShouldParseNumber()
    {
        MethodInfo method = typeof(TradeGridsMaster).GetMethod("TryExtractGridNumber", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method TryExtractGridNumber not found.");

        object[] args = { "15@payload", 0 };
        bool parsed = (bool)method.Invoke(null, args)!;

        Assert.True(parsed);
        Assert.Equal(15, (int)args[1]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithoutSeparator_ShouldParseNumber()
    {
        MethodInfo method = typeof(TradeGridsMaster).GetMethod("TryExtractGridNumber", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method TryExtractGridNumber not found.");

        object[] args = { "42", 0 };
        bool parsed = (bool)method.Invoke(null, args)!;

        Assert.True(parsed);
        Assert.Equal(42, (int)args[1]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithInvalidPrefix_ShouldReturnFalse()
    {
        MethodInfo method = typeof(TradeGridsMaster).GetMethod("TryExtractGridNumber", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method TryExtractGridNumber not found.");

        object[] args = { "abc@payload", 0 };
        bool parsed = (bool)method.Invoke(null, args)!;

        Assert.False(parsed);
        Assert.Equal(0, (int)args[1]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_ParseLegacyGridsSettings_WithSingleLine_ShouldReturnSingleEntry()
    {
        MethodInfo method = typeof(TradeGridsMaster).GetMethod("ParseLegacyGridsSettings", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method ParseLegacyGridsSettings not found.");

        object result = method.Invoke(null, new object?[] { "7@gridOnly" })
            ?? throw new InvalidOperationException("Legacy settings parse returned null.");
        PropertyInfo property = result.GetType().GetProperty("GridSaveStrings")
            ?? throw new InvalidOperationException("Property GridSaveStrings not found.");
        List<string> values = (List<string>)(property.GetValue(result)
            ?? throw new InvalidOperationException("GridSaveStrings value is null."));

        Assert.Single(values);
        Assert.Equal("7@gridOnly", values[0]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithNullInput_ShouldReturnFalse()
    {
        MethodInfo method = typeof(TradeGridsMaster).GetMethod("TryExtractGridNumber", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method TryExtractGridNumber not found.");

        object?[] args = { null, 0 };
        bool parsed = (bool)method.Invoke(null, args)!;

        Assert.False(parsed);
        Assert.Equal(0, (int)args[1]!);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithLeadingSeparator_ShouldReturnFalse()
    {
        MethodInfo method = typeof(TradeGridsMaster).GetMethod("TryExtractGridNumber", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method TryExtractGridNumber not found.");

        object[] args = { "@payload", 0 };
        bool parsed = (bool)method.Invoke(null, args)!;

        Assert.False(parsed);
        Assert.Equal(0, (int)args[1]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithWhitespaceNumberPart_ShouldReturnFalse()
    {
        MethodInfo method = typeof(TradeGridsMaster).GetMethod("TryExtractGridNumber", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method TryExtractGridNumber not found.");

        object[] args = { "   @payload", 0 };
        bool parsed = (bool)method.Invoke(null, args)!;

        Assert.False(parsed);
        Assert.Equal(0, (int)args[1]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_GetGridsSettingsPath_ShouldComposeExpectedPath()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        SetPrivateField(master, "_nameBot", "CodexBot");

        MethodInfo method = typeof(TradeGridsMaster).GetMethod("GetGridsSettingsPath", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetGridsSettingsPath not found.");

        string path = (string)(method.Invoke(master, null)
            ?? throw new InvalidOperationException("GetGridsSettingsPath returned null."));

        Assert.Equal(@"Engine\CodexBotGridsSettings.txt", path);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_SendNewLogMessage_WithSubscriber_ShouldForwardMessage()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        string? receivedMessage = null;
        LogMessageType? receivedType = null;

        master.LogMessageEvent += (message, type) =>
        {
            receivedMessage = message;
            receivedType = type;
        };

        master.SendNewLogMessage("CodexGridsMasterMessage", LogMessageType.Signal);

        Assert.Equal("CodexGridsMasterMessage", receivedMessage);
        Assert.Equal(LogMessageType.Signal, receivedType);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));

        Exception? error = Record.Exception(() =>
            master.SendNewLogMessage("CodexGridsMasterNoSubscriber", LogMessageType.System));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_Clear_WithEmptyCollectionInOptimizerMode_ShouldNotThrow()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        SetPrivateField(master, "_startProgram", StartProgram.IsOsOptimizer);
        master.TradeGrids = new List<TradeGrid>();

        Exception? error = Record.Exception(master.Clear);

        Assert.Null(error);
        Assert.Empty(master.TradeGrids);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_Delete_WithOptimizerMode_ShouldClearTabAndNotThrow()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        SetPrivateField(master, "_startProgram", StartProgram.IsOsOptimizer);
        SetPrivateField(master, "_tab", (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple)));

        Exception? error = Record.Exception(master.Delete);

        FieldInfo tabField = typeof(TradeGridsMaster).GetField("_tab", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Field _tab not found.");

        Assert.Null(error);
        Assert.Null(tabField.GetValue(master));
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_Delete_WithMissingSettingsFile_ShouldClearTabAndNotThrow()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        SetPrivateField(master, "_startProgram", StartProgram.IsTester);
        SetPrivateField(master, "_nameBot", "CodexMissingDelete");
        SetPrivateField(master, "_tab", (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple)));

        string path = Path.Combine("Engine", "CodexMissingDeleteGridsSettings.txt");
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        Exception? error = Record.Exception(master.Delete);

        FieldInfo tabField = typeof(TradeGridsMaster).GetField("_tab", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Field _tab not found.");

        Assert.Null(error);
        Assert.False(File.Exists(path));
        Assert.Null(tabField.GetValue(master));
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_Delete_WithExistingSettingsFile_ShouldRemoveFileAndStayIdempotent()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        SetPrivateField(master, "_startProgram", StartProgram.IsTester);
        SetPrivateField(master, "_nameBot", "CodexExistingDelete");
        SetPrivateField(master, "_tab", (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple)));

        Directory.CreateDirectory("Engine");
        string path = Path.Combine("Engine", "CodexExistingDeleteGridsSettings.txt");
        File.WriteAllText(path, "codex");

        Exception? error = Record.Exception(() =>
        {
            master.Delete();
            master.Delete();
        });

        FieldInfo tabField = typeof(TradeGridsMaster).GetField("_tab", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Field _tab not found.");

        Assert.Null(error);
        Assert.False(File.Exists(path));
        Assert.Null(tabField.GetValue(master));
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_DeleteAtNum_WithMissingNumberInOptimizerMode_ShouldNotThrow()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        SetPrivateField(master, "_startProgram", StartProgram.IsOsOptimizer);
        master.TradeGrids = new List<TradeGrid>();

        Exception? error = Record.Exception(() => master.DeleteAtNum(42, true));

        Assert.Null(error);
        Assert.Empty(master.TradeGrids);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_StopPaint_WithNullHost_ShouldNotThrow()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));

        Exception? error = Record.Exception(master.StopPaint);

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_LoadAndPaint_WithSafeEarlyReturns_ShouldNotThrow()
    {
        TradeGridsMaster testerMaster =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        SetPrivateField(testerMaster, "_startProgram", StartProgram.IsTester);

        MethodInfo loadMethod = typeof(TradeGridsMaster).GetMethod("LoadGrids", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method LoadGrids not found.");
        MethodInfo paintMethod = typeof(TradeGridsMaster).GetMethod("PaintGridView", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method PaintGridView not found.");

        Exception? error = Record.Exception(() =>
        {
            loadMethod.Invoke(testerMaster, null);
            paintMethod.Invoke(testerMaster, null);
        });

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_SaveGrids_WithOptimizerMode_ShouldNotThrow()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        SetPrivateField(master, "_startProgram", StartProgram.IsOsOptimizer);
        master.TradeGrids = new List<TradeGrid> { null! };

        MethodInfo method = typeof(TradeGridsMaster).GetMethod("SaveGrids", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method SaveGrids not found.");

        Exception? error = Record.Exception(() => method.Invoke(master, null));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_ShowDialog_WithMissingGrid_ShouldNotThrow()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        master.TradeGrids = new List<TradeGrid>();

        Exception? error = Record.Exception(() => master.ShowDialog(7));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_UiClosed_WithNullTradeGridEntry_ShouldCleanList()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));

        TradeGridUi staleUi = (TradeGridUi)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridUi));
        TradeGridUi senderUi = (TradeGridUi)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridUi));
        staleUi.TradeGrid = null!;
        senderUi.TradeGrid = CreateBareGrid();
        staleUi.Number = 1;
        senderUi.Number = 9;

        List<TradeGridUi> uiList = new List<TradeGridUi> { staleUi };
        SetPrivateField(master, "_tradeGridUis", uiList);

        MethodInfo method = typeof(TradeGridsMaster).GetMethod("Ui_Closed", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method Ui_Closed not found.");

        Exception? error = Record.Exception(() => method.Invoke(master, new object[] { senderUi, EventArgs.Empty }));

        Assert.Null(error);
        Assert.Empty(uiList);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_UiClosed_WithUnknownSender_ShouldKeepOtherEntries()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));

        TradeGrid trackedGrid = CreateBareGrid();
        trackedGrid.Number = 5;
        TradeGridUi trackedUi = (TradeGridUi)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridUi));
        trackedUi.TradeGrid = trackedGrid;
        trackedUi.Number = 5;

        TradeGrid senderGrid = CreateBareGrid();
        senderGrid.Number = 9;
        TradeGridUi senderUi = (TradeGridUi)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridUi));
        senderUi.TradeGrid = senderGrid;
        senderUi.Number = 9;

        List<TradeGridUi> uiList = new List<TradeGridUi> { trackedUi };
        SetPrivateField(master, "_tradeGridUis", uiList);

        MethodInfo method = typeof(TradeGridsMaster).GetMethod("Ui_Closed", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method Ui_Closed not found.");

        Exception? error = Record.Exception(() => method.Invoke(master, new object[] { senderUi, EventArgs.Empty }));

        Assert.Null(error);
        Assert.Single(uiList);
        Assert.Same(trackedUi, uiList[0]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_GetGridRow_ShouldBuildExpectedCells()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        TradeGrid grid = CreateBareGrid();
        grid.Number = 12;
        grid.Regime = TradeGridRegime.CloseOnly;

        MethodInfo method = typeof(TradeGridsMaster).GetMethod("GetGridRow", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetGridRow not found.");

        DataGridViewRow row = (DataGridViewRow)(method.Invoke(master, new object[] { grid })
            ?? throw new InvalidOperationException("GetGridRow returned null."));

        Assert.Equal(5, row.Cells.Count);
        Assert.Equal(12, row.Cells[0].Value);
        Assert.Equal(grid.GridType.ToString(), row.Cells[1].Value);
        Assert.Equal(TradeGridRegime.CloseOnly.ToString(), row.Cells[2].Value);
        Assert.Equal(OsLocalization.Trader.Label469, row.Cells[3].Value);
        Assert.Equal(OsLocalization.Trader.Label470, row.Cells[4].Value);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_GetLastRow_ShouldBuildAddButtonRow()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));

        MethodInfo method = typeof(TradeGridsMaster).GetMethod("GetLastRow", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetLastRow not found.");

        DataGridViewRow row = (DataGridViewRow)(method.Invoke(master, null)
            ?? throw new InvalidOperationException("GetLastRow returned null."));

        Assert.Equal(5, row.Cells.Count);
        Assert.Equal(OsLocalization.Trader.Label471, row.Cells[4].Value);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridsMaster_GridViewDataError_ShouldNotThrow()
    {
        TradeGridsMaster master =
            (TradeGridsMaster)RuntimeHelpers.GetUninitializedObject(typeof(TradeGridsMaster));
        master.LogMessageEvent += (_, _) => { };

        MethodInfo method = typeof(TradeGridsMaster).GetMethod("_gridViewInstances_DataError", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method _gridViewInstances_DataError not found.");

        DataGridView dataGridView = new DataGridView();
        var exception = new FormatException("Codex");
        DataGridViewDataErrorEventArgs eventArgs = new DataGridViewDataErrorEventArgs(
            exception, 0, 0, DataGridViewDataErrorContexts.Commit);

        Exception? error = Record.Exception(() => method.Invoke(master, new object[] { dataGridView, eventArgs }));

        Assert.Null(error);
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
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithMalformedFields_ShouldKeepValuesAndContinueParsing()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            ProfitRegime = OnOffRegime.Off,
            ProfitValueType = TradeGridValueType.Percent,
            ProfitValue = 1.5m,
            StopRegime = OnOffRegime.Off,
            StopValueType = TradeGridValueType.Percent,
            StopValue = 0.8m,
            TrailStopRegime = OnOffRegime.Off,
            TrailStopValueType = TradeGridValueType.Percent,
            TrailStopValue = 0.8m,
            StopTradingAfterProfit = true
        };

        Exception? error = Record.Exception(() => stopAndProfit.LoadFromString(
            "badEnum@badEnum@badDecimal@On@Absolute@2.2@On@Percent@1.1@0"));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.Off, stopAndProfit.ProfitRegime);
        Assert.Equal(TradeGridValueType.Percent, stopAndProfit.ProfitValueType);
        Assert.Equal(1.5m, stopAndProfit.ProfitValue);
        Assert.Equal(OnOffRegime.On, stopAndProfit.StopRegime);
        Assert.Equal(TradeGridValueType.Absolute, stopAndProfit.StopValueType);
        Assert.Equal(2.2m, stopAndProfit.StopValue);
        Assert.Equal(OnOffRegime.On, stopAndProfit.TrailStopRegime);
        Assert.Equal(TradeGridValueType.Percent, stopAndProfit.TrailStopValueType);
        Assert.Equal(1.1m, stopAndProfit.TrailStopValue);
        Assert.False(stopAndProfit.StopTradingAfterProfit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithNonPositiveValues_ShouldKeepExistingValues()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            ProfitValue = 1.5m,
            StopValue = 0.8m,
            TrailStopValue = 0.8m
        };

        Exception? error = Record.Exception(() => stopAndProfit.LoadFromString(
            "On@Percent@0@On@Percent@-1@On@Percent@0@True"));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.On, stopAndProfit.ProfitRegime);
        Assert.Equal(1.5m, stopAndProfit.ProfitValue);
        Assert.Equal(OnOffRegime.On, stopAndProfit.StopRegime);
        Assert.Equal(0.8m, stopAndProfit.StopValue);
        Assert.Equal(OnOffRegime.On, stopAndProfit.TrailStopRegime);
        Assert.Equal(0.8m, stopAndProfit.TrailStopValue);
        Assert.True(stopAndProfit.StopTradingAfterProfit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithFlexibleStopTradingBool_ShouldParse()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            StopTradingAfterProfit = true
        };

        Exception? error = Record.Exception(() => stopAndProfit.LoadFromString(
            "On@Percent@1.5@On@Percent@0.8@On@Percent@0.8@off"));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.On, stopAndProfit.ProfitRegime);
        Assert.Equal(OnOffRegime.On, stopAndProfit.StopRegime);
        Assert.Equal(OnOffRegime.On, stopAndProfit.TrailStopRegime);
        Assert.False(stopAndProfit.StopTradingAfterProfit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithMissingTrailStopRegime_ShouldKeepValueAndContinueTailParsing()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            TrailStopRegime = OnOffRegime.Off,
            TrailStopValueType = TradeGridValueType.Percent,
            TrailStopValue = 0.8m,
            StopTradingAfterProfit = true
        };

        Exception? error = Record.Exception(() => stopAndProfit.LoadFromString(
            "On@Percent@1.5@On@Percent@0.8@@Absolute@1.1@0"));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.On, stopAndProfit.ProfitRegime);
        Assert.Equal(OnOffRegime.On, stopAndProfit.StopRegime);
        Assert.Equal(OnOffRegime.Off, stopAndProfit.TrailStopRegime);
        Assert.Equal(TradeGridValueType.Absolute, stopAndProfit.TrailStopValueType);
        Assert.Equal(1.1m, stopAndProfit.TrailStopValue);
        Assert.False(stopAndProfit.StopTradingAfterProfit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithInvalidTrailStopType_ShouldKeepValueAndContinueTailParsing()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            TrailStopRegime = OnOffRegime.Off,
            TrailStopValueType = TradeGridValueType.Percent,
            TrailStopValue = 0.8m,
            StopTradingAfterProfit = true
        };

        Exception? error = Record.Exception(() => stopAndProfit.LoadFromString(
            "On@Percent@1.5@On@Percent@0.8@On@badEnum@1.1@0"));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.On, stopAndProfit.TrailStopRegime);
        Assert.Equal(TradeGridValueType.Percent, stopAndProfit.TrailStopValueType);
        Assert.Equal(1.1m, stopAndProfit.TrailStopValue);
        Assert.False(stopAndProfit.StopTradingAfterProfit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithMissingTrailStopValue_ShouldKeepValueAndParseStopTradingBool()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            TrailStopValue = 0.8m,
            StopTradingAfterProfit = true
        };

        Exception? error = Record.Exception(() => stopAndProfit.LoadFromString(
            "On@Percent@1.5@On@Percent@0.8@On@Absolute@@off"));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.On, stopAndProfit.TrailStopRegime);
        Assert.Equal(TradeGridValueType.Absolute, stopAndProfit.TrailStopValueType);
        Assert.Equal(0.8m, stopAndProfit.TrailStopValue);
        Assert.False(stopAndProfit.StopTradingAfterProfit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithInvalidStopTradingBool_ShouldKeepValue()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            StopTradingAfterProfit = true
        };

        Exception? error = Record.Exception(() => stopAndProfit.LoadFromString(
            "On@Percent@1.5@On@Percent@0.8@On@Absolute@1.1@badBool"));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.On, stopAndProfit.TrailStopRegime);
        Assert.Equal(TradeGridValueType.Absolute, stopAndProfit.TrailStopValueType);
        Assert.Equal(1.1m, stopAndProfit.TrailStopValue);
        Assert.True(stopAndProfit.StopTradingAfterProfit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_GetSaveString_ShouldKeepReservedTailShape()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            ProfitRegime = OnOffRegime.On,
            ProfitValueType = TradeGridValueType.Absolute,
            ProfitValue = 2.2m,
            StopRegime = OnOffRegime.On,
            StopValueType = TradeGridValueType.Percent,
            StopValue = 1.1m,
            TrailStopRegime = OnOffRegime.On,
            TrailStopValueType = TradeGridValueType.Absolute,
            TrailStopValue = 0.9m,
            StopTradingAfterProfit = false
        };

        string save = stopAndProfit.GetSaveString();

        Assert.Equal("On@Absolute@2.2@On@Percent@1.1@On@Absolute@0.9@False@@@@@@", save);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithEmptyLikePayloads_ShouldKeepExistingValues()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            ProfitRegime = OnOffRegime.On,
            ProfitValueType = TradeGridValueType.Absolute,
            ProfitValue = 2.2m
        };

        Exception? error = Record.Exception(() =>
        {
            stopAndProfit.LoadFromString(null);
            stopAndProfit.LoadFromString(string.Empty);
            stopAndProfit.LoadFromString("  \r\n \t  ");
        });

        Assert.Null(error);
        Assert.Equal(OnOffRegime.On, stopAndProfit.ProfitRegime);
        Assert.Equal(TradeGridValueType.Absolute, stopAndProfit.ProfitValueType);
        Assert.Equal(2.2m, stopAndProfit.ProfitValue);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_GetSaveString_LoadFromString_ShouldRoundTrip()
    {
        TradeGridStopAndProfit source = new TradeGridStopAndProfit
        {
            ProfitRegime = OnOffRegime.On,
            ProfitValueType = TradeGridValueType.Absolute,
            ProfitValue = 2.2m,
            StopRegime = OnOffRegime.On,
            StopValueType = TradeGridValueType.Percent,
            StopValue = 1.1m,
            TrailStopRegime = OnOffRegime.On,
            TrailStopValueType = TradeGridValueType.Absolute,
            TrailStopValue = 0.9m,
            StopTradingAfterProfit = false
        };

        TradeGridStopAndProfit loaded = new TradeGridStopAndProfit();

        Exception? error = Record.Exception(() => loaded.LoadFromString(source.GetSaveString()));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.On, loaded.ProfitRegime);
        Assert.Equal(TradeGridValueType.Absolute, loaded.ProfitValueType);
        Assert.Equal(2.2m, loaded.ProfitValue);
        Assert.Equal(OnOffRegime.On, loaded.StopRegime);
        Assert.Equal(TradeGridValueType.Percent, loaded.StopValueType);
        Assert.Equal(1.1m, loaded.StopValue);
        Assert.Equal(OnOffRegime.On, loaded.TrailStopRegime);
        Assert.Equal(TradeGridValueType.Absolute, loaded.TrailStopValueType);
        Assert.Equal(0.9m, loaded.TrailStopValue);
        Assert.False(loaded.StopTradingAfterProfit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_SendNewLogMessage_WithSubscriber_ShouldForwardMessage()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit();
        string? receivedMessage = null;
        LogMessageType? receivedType = null;

        stopAndProfit.LogMessageEvent += (message, type) =>
        {
            receivedMessage = message;
            receivedType = type;
        };

        stopAndProfit.SendNewLogMessage("CodexStopAndProfitMessage", LogMessageType.System);

        Assert.Equal("CodexStopAndProfitMessage", receivedMessage);
        Assert.Equal(LogMessageType.System, receivedType);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit();

        Exception? error = Record.Exception(() =>
            stopAndProfit.SendNewLogMessage("CodexStopAndProfitNoSubscriber", LogMessageType.System));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithMissingProfitRegime_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            ProfitRegime = OnOffRegime.Off,
            ProfitValueType = TradeGridValueType.Percent,
            ProfitValue = 1.5m
        };

        Exception? error = Record.Exception(() => stopAndProfit.LoadFromString(
            "@Absolute@2.2@On@Percent@0.8@On@Percent@0.8@0"));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.Off, stopAndProfit.ProfitRegime);
        Assert.Equal(TradeGridValueType.Absolute, stopAndProfit.ProfitValueType);
        Assert.Equal(2.2m, stopAndProfit.ProfitValue);
        Assert.Equal(OnOffRegime.On, stopAndProfit.StopRegime);
        Assert.False(stopAndProfit.StopTradingAfterProfit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithInvalidProfitType_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            ProfitValueType = TradeGridValueType.Percent,
            ProfitValue = 1.5m
        };

        Exception? error = Record.Exception(() => stopAndProfit.LoadFromString(
            "On@badEnum@2.2@On@Absolute@0.8@On@Percent@0.8@0"));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.On, stopAndProfit.ProfitRegime);
        Assert.Equal(TradeGridValueType.Percent, stopAndProfit.ProfitValueType);
        Assert.Equal(2.2m, stopAndProfit.ProfitValue);
        Assert.Equal(TradeGridValueType.Absolute, stopAndProfit.StopValueType);
        Assert.False(stopAndProfit.StopTradingAfterProfit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithMissingProfitValue_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            ProfitValue = 1.5m
        };

        Exception? error = Record.Exception(() => stopAndProfit.LoadFromString(
            "On@Absolute@@On@Percent@0.8@On@Percent@0.8@0"));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.On, stopAndProfit.ProfitRegime);
        Assert.Equal(TradeGridValueType.Absolute, stopAndProfit.ProfitValueType);
        Assert.Equal(1.5m, stopAndProfit.ProfitValue);
        Assert.Equal(OnOffRegime.On, stopAndProfit.StopRegime);
        Assert.False(stopAndProfit.StopTradingAfterProfit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithInvalidStopRegime_ShouldKeepValueAndContinueParsing()
    {
        TradeGridStopAndProfit stopAndProfit = new TradeGridStopAndProfit
        {
            StopRegime = OnOffRegime.Off,
            StopValueType = TradeGridValueType.Percent,
            StopValue = 0.8m
        };

        Exception? error = Record.Exception(() => stopAndProfit.LoadFromString(
            "On@Absolute@2.2@badEnum@Absolute@1.1@On@Percent@0.8@0"));

        Assert.Null(error);
        Assert.Equal(OnOffRegime.On, stopAndProfit.ProfitRegime);
        Assert.Equal(TradeGridValueType.Absolute, stopAndProfit.ProfitValueType);
        Assert.Equal(2.2m, stopAndProfit.ProfitValue);
        Assert.Equal(OnOffRegime.Off, stopAndProfit.StopRegime);
        Assert.Equal(TradeGridValueType.Absolute, stopAndProfit.StopValueType);
        Assert.Equal(1.1m, stopAndProfit.StopValue);
        Assert.False(stopAndProfit.StopTradingAfterProfit);
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
    public void Stage2Step2_2_TrailingUp_SendNewLogMessage_WithSubscriber_ShouldForwardMessage()
    {
        TrailingUp trailing = new TrailingUp(CreateBareGrid());
        string? receivedMessage = null;
        LogMessageType? receivedType = null;

        trailing.LogMessageEvent += (message, type) =>
        {
            receivedMessage = message;
            receivedType = type;
        };

        trailing.SendNewLogMessage("CodexTrailingMessage", LogMessageType.Signal);

        Assert.Equal("CodexTrailingMessage", receivedMessage);
        Assert.Equal(LogMessageType.Signal, receivedType);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow()
    {
        TrailingUp trailing = new TrailingUp(CreateBareGrid());

        Exception? error = Record.Exception(() =>
            trailing.SendNewLogMessage("CodexTrailingNoSubscriber", LogMessageType.System));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_GetSaveString_ShouldKeepReservedTailShape()
    {
        TrailingUp trailing = new TrailingUp(CreateBareGrid())
        {
            TrailingUpIsOn = true,
            TrailingUpStep = 1.5m,
            TrailingUpLimit = 110m,
            TrailingDownIsOn = false,
            TrailingDownStep = 2.5m,
            TrailingDownLimit = 90m,
            TrailingUpCanMoveExitOrder = true,
            TrailingDownCanMoveExitOrder = false
        };

        string save = trailing.GetSaveString();

        Assert.Equal("True@1.5@110@False@2.5@90@True@False@@@@@@", save);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithEmptyLikePayloads_ShouldKeepExistingValues()
    {
        TrailingUp trailing = new TrailingUp(CreateBareGrid())
        {
            TrailingUpIsOn = true,
            TrailingUpStep = 1.5m,
            TrailingUpLimit = 110m
        };

        Exception? error = Record.Exception(() =>
        {
            trailing.LoadFromString(null);
            trailing.LoadFromString(string.Empty);
            trailing.LoadFromString("  \r\n \t  ");
        });

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.5m, trailing.TrailingUpStep);
        Assert.Equal(110m, trailing.TrailingUpLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_GetSaveString_LoadFromString_ShouldRoundTrip()
    {
        TrailingUp source = new TrailingUp(CreateBareGrid())
        {
            TrailingUpIsOn = true,
            TrailingUpStep = 1.5m,
            TrailingUpLimit = 110m,
            TrailingDownIsOn = false,
            TrailingDownStep = 2.5m,
            TrailingDownLimit = 90m,
            TrailingUpCanMoveExitOrder = true,
            TrailingDownCanMoveExitOrder = false
        };

        TrailingUp loaded = new TrailingUp(CreateBareGrid());

        Exception? error = Record.Exception(() => loaded.LoadFromString(source.GetSaveString()));

        Assert.Null(error);
        Assert.True(loaded.TrailingUpIsOn);
        Assert.Equal(1.5m, loaded.TrailingUpStep);
        Assert.Equal(110m, loaded.TrailingUpLimit);
        Assert.False(loaded.TrailingDownIsOn);
        Assert.Equal(2.5m, loaded.TrailingDownStep);
        Assert.Equal(90m, loaded.TrailingDownLimit);
        Assert.True(loaded.TrailingUpCanMoveExitOrder);
        Assert.False(loaded.TrailingDownCanMoveExitOrder);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_Delete_ShouldClearGridAndStayIdempotent()
    {
        TrailingUp trailing = new TrailingUp(CreateBareGrid());

        Exception? error = Record.Exception(() =>
        {
            trailing.Delete();
            trailing.Delete();
        });

        object? grid = typeof(TrailingUp).GetField("_grid", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(trailing);

        Assert.Null(error);
        Assert.Null(grid);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_MaxMinGridPrice_WithSparseLines_ShouldForwardTrailingBounds()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine { PriceEnter = 105m },
            new TradeGridLine { PriceEnter = 99m },
            new TradeGridLine { PriceEnter = 101m }
        };

        decimal max = grid.MaxGridPrice;
        decimal min = grid.MinGridPrice;

        Assert.Equal(105m, max);
        Assert.Equal(99m, min);
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
    public void Stage2Step2_2_TrailingUp_TryTrailingGrid_WithNegativeUpStepRuntimeValue_ShouldNotShiftGrid()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { PriceEnter = 100m, PriceExit = 110m }
        };
        AttachSingleCloseCandle(grid, 102m);

        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = true,
            TrailingUpStep = -1m,
            TrailingUpLimit = 200m
        };

        bool moved = true;
        Exception? error = Record.Exception(() => moved = trailing.TryTrailingGrid());

        Assert.Null(error);
        Assert.False(moved);
        Assert.Equal(100m, grid.GridCreator.Lines[0].PriceEnter);
        Assert.Equal(110m, grid.GridCreator.Lines[0].PriceExit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_TryTrailingGrid_WithNegativeUpLimitRuntimeValue_ShouldNotShiftGrid()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { PriceEnter = 100m, PriceExit = 110m }
        };
        AttachSingleCloseCandle(grid, 102m);

        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = true,
            TrailingUpStep = 1m,
            TrailingUpLimit = -200m
        };

        bool moved = true;
        Exception? error = Record.Exception(() => moved = trailing.TryTrailingGrid());

        Assert.Null(error);
        Assert.False(moved);
        Assert.Equal(100m, grid.GridCreator.Lines[0].PriceEnter);
        Assert.Equal(110m, grid.GridCreator.Lines[0].PriceExit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_TryTrailingGrid_WithNegativeDownStepRuntimeValue_ShouldNotShiftGrid()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { PriceEnter = 100m, PriceExit = 110m }
        };
        AttachSingleCloseCandle(grid, 98m);

        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingDownIsOn = true,
            TrailingDownStep = -1m,
            TrailingDownLimit = 1m
        };

        bool moved = true;
        Exception? error = Record.Exception(() => moved = trailing.TryTrailingGrid());

        Assert.Null(error);
        Assert.False(moved);
        Assert.Equal(100m, grid.GridCreator.Lines[0].PriceEnter);
        Assert.Equal(110m, grid.GridCreator.Lines[0].PriceExit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_TryTrailingGrid_WithNegativeDownLimitRuntimeValue_ShouldNotShiftGrid()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { PriceEnter = 100m, PriceExit = 110m }
        };
        AttachSingleCloseCandle(grid, 98m);

        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingDownIsOn = true,
            TrailingDownStep = 1m,
            TrailingDownLimit = -1m
        };

        bool moved = true;
        Exception? error = Record.Exception(() => moved = trailing.TryTrailingGrid());

        Assert.Null(error);
        Assert.False(moved);
        Assert.Equal(100m, grid.GridCreator.Lines[0].PriceEnter);
        Assert.Equal(110m, grid.GridCreator.Lines[0].PriceExit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_TryTrailingGrid_WithZeroUpStepRuntimeValue_ShouldNotShiftGrid()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { PriceEnter = 100m, PriceExit = 110m }
        };
        AttachSingleCloseCandle(grid, 102m);

        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = true,
            TrailingUpStep = 0m,
            TrailingUpLimit = 200m
        };

        bool moved = true;
        Exception? error = Record.Exception(() => moved = trailing.TryTrailingGrid());

        Assert.Null(error);
        Assert.False(moved);
        Assert.Equal(100m, grid.GridCreator.Lines[0].PriceEnter);
        Assert.Equal(110m, grid.GridCreator.Lines[0].PriceExit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_TryTrailingGrid_WithZeroUpLimitRuntimeValue_ShouldNotShiftGrid()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { PriceEnter = 100m, PriceExit = 110m }
        };
        AttachSingleCloseCandle(grid, 102m);

        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = true,
            TrailingUpStep = 1m,
            TrailingUpLimit = 0m
        };

        bool moved = true;
        Exception? error = Record.Exception(() => moved = trailing.TryTrailingGrid());

        Assert.Null(error);
        Assert.False(moved);
        Assert.Equal(100m, grid.GridCreator.Lines[0].PriceEnter);
        Assert.Equal(110m, grid.GridCreator.Lines[0].PriceExit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_TryTrailingGrid_WithZeroDownStepRuntimeValue_ShouldNotShiftGrid()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { PriceEnter = 100m, PriceExit = 110m }
        };
        AttachSingleCloseCandle(grid, 98m);

        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingDownIsOn = true,
            TrailingDownStep = 0m,
            TrailingDownLimit = 1m
        };

        bool moved = true;
        Exception? error = Record.Exception(() => moved = trailing.TryTrailingGrid());

        Assert.Null(error);
        Assert.False(moved);
        Assert.Equal(100m, grid.GridCreator.Lines[0].PriceEnter);
        Assert.Equal(110m, grid.GridCreator.Lines[0].PriceExit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_TryTrailingGrid_WithZeroDownLimitRuntimeValue_ShouldNotShiftGrid()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { PriceEnter = 100m, PriceExit = 110m }
        };
        AttachSingleCloseCandle(grid, 98m);

        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingDownIsOn = true,
            TrailingDownStep = 1m,
            TrailingDownLimit = 0m
        };

        bool moved = true;
        Exception? error = Record.Exception(() => moved = trailing.TryTrailingGrid());

        Assert.Null(error);
        Assert.False(moved);
        Assert.Equal(100m, grid.GridCreator.Lines[0].PriceEnter);
        Assert.Equal(110m, grid.GridCreator.Lines[0].PriceExit);
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
    public void Stage2Step2_2_TradeGridCreator_LoadLines_WithNullOrWhitespacePayload_ShouldKeepExistingLines()
    {
        TradeGridCreator creator = new TradeGridCreator();
        creator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine
            {
                PriceEnter = 100m,
                Volume = 1m,
                Side = Side.Buy,
                PriceExit = 101m,
                PositionNum = 7
            }
        };

        Exception? error = Record.Exception(() =>
        {
            creator.LoadLines(null!);
            creator.LoadLines(string.Empty);
            creator.LoadLines("   ");
        });

        Assert.Null(error);
        Assert.Single(creator.Lines);
        Assert.Equal(100m, creator.Lines[0].PriceEnter);
        Assert.Equal(7, creator.Lines[0].PositionNum);
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
    public void Stage2Step2_2_TradeGridCreator_GetVolume_WithContractCurrency_ShouldReturnRoundedVolume()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            TypeVolume = TradeGridVolumeType.ContractCurrency
        };
        TradeGridLine line = new TradeGridLine
        {
            Volume = 100m,
            PriceEnter = 25m
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridCreatorContractCurrencyPositive", StartProgram.IsTester, false);
        SetPrivateField(tab, "_connector", connector);
        SetPrivateField(tab, "_security", new Security
        {
            Name = connector.SecurityName,
            Lot = 2m,
            DecimalsVolume = 4
        });

        decimal volume = creator.GetVolume(line, tab);

        Assert.Equal(2m, volume);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_GetVolume_WithDepositPercentPrime_ShouldReturnRoundedVolume()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            TypeVolume = TradeGridVolumeType.DepositPercent,
            TradeAssetInPortfolio = "Prime"
        };
        TradeGridLine line = new TradeGridLine
        {
            Volume = 10m,
            PriceEnter = 100m
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridCreatorDepositPercentPrime", StartProgram.IsTester, false);
        SetPrivateField(connector, "_bestAsk", 100m);
        SetPrivateField(tab, "_connector", connector);
        SetPrivateField(tab, "_security", new Security
        {
            Name = connector.SecurityName,
            Lot = 1m,
            DecimalsVolume = 4
        });
        SetPrivateField(tab, "_portfolio", new Portfolio
        {
            ValueCurrent = 1000m
        });

        decimal volume = creator.GetVolume(line, tab);

        Assert.Equal(1m, volume);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_GetVolume_WithDepositPercentCustomAsset_ShouldReturnRoundedVolume()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            TypeVolume = TradeGridVolumeType.DepositPercent,
            TradeAssetInPortfolio = "USDT"
        };
        TradeGridLine line = new TradeGridLine
        {
            Volume = 10m,
            PriceEnter = 100m
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridCreatorDepositPercentAsset", StartProgram.IsTester, false);
        SetPrivateField(connector, "_bestAsk", 100m);
        SetPrivateField(tab, "_connector", connector);
        SetPrivateField(tab, "_security", new Security
        {
            Name = connector.SecurityName,
            Lot = 1m,
            DecimalsVolume = 4
        });
        SetPrivateField(tab, "_portfolio", new Portfolio
        {
            PositionOnBoard = new List<PositionOnBoard>
            {
                new PositionOnBoard
                {
                    SecurityNameCode = "USDT",
                    ValueCurrent = 1000m
                }
            }
        });

        decimal volume = creator.GetVolume(line, tab);

        Assert.Equal(1m, volume);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_GetVolume_WithMissingCustomAsset_ShouldReturnZero()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            TypeVolume = TradeGridVolumeType.DepositPercent,
            TradeAssetInPortfolio = "USDT"
        };
        string? receivedMessage = null;
        LogMessageType? receivedType = null;

        creator.LogMessageEvent += (message, type) =>
        {
            receivedMessage = message;
            receivedType = type;
        };

        TradeGridLine line = new TradeGridLine
        {
            Volume = 10m,
            PriceEnter = 100m
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridCreatorDepositPercentMissingAsset", StartProgram.IsTester, false);
        SetPrivateField(connector, "_bestAsk", 100m);
        SetPrivateField(tab, "_connector", connector);
        SetPrivateField(tab, "_security", new Security
        {
            Name = connector.SecurityName,
            Lot = 1m,
            DecimalsVolume = 4
        });
        SetPrivateField(tab, "_portfolio", new Portfolio
        {
            PositionOnBoard = new List<PositionOnBoard>
            {
                new PositionOnBoard
                {
                    SecurityNameCode = "BTC",
                    ValueCurrent = 1000m
                }
            }
        });

        decimal volume = creator.GetVolume(line, tab);

        Assert.Equal(0m, volume);
        Assert.Equal("Can`t found portfolio in Deposit Percent volume mode USDT", receivedMessage);
        Assert.Equal(LogMessageType.System, receivedType);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_GetVolume_WithPriceStepCostModeInTester_ShouldUseStandardFormula()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            TypeVolume = TradeGridVolumeType.DepositPercent,
            TradeAssetInPortfolio = "Prime"
        };
        TradeGridLine line = new TradeGridLine
        {
            Volume = 10m,
            PriceEnter = 100m
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridCreatorStepCostMode", StartProgram.IsTester, false);
        SetPrivateField(connector, "_bestAsk", 100m);
        SetPrivateField(tab, "_connector", connector);
        SetPrivateField(tab, "_security", new Security
        {
            Name = connector.SecurityName,
            Lot = 1m,
            DecimalsVolume = 4,
            UsePriceStepCostToCalculateVolume = true,
            PriceStep = 10m,
            PriceStepCost = 5m
        });
        SetPrivateField(tab, "_portfolio", new Portfolio
        {
            ValueCurrent = 1000m
        });

        decimal volume = creator.GetVolume(line, tab);

        Assert.Equal(1m, volume);
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
    public void Stage2Step2_2_TradeGridCreator_CreateNewGrid_WithNegativeStepMultiplicatorRuntimeValue_ShouldStopAfterFirstLine()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            GridSide = Side.Buy,
            FirstPrice = 100m,
            LineCountStart = 3,
            TypeStep = TradeGridValueType.Absolute,
            LineStep = 1m,
            StepMultiplicator = -1m,
            TypeProfit = TradeGridValueType.Absolute,
            ProfitStep = 1m,
            ProfitMultiplicator = 1m,
            TypeVolume = TradeGridVolumeType.Contracts,
            StartVolume = 1m,
            MartingaleMultiplicator = 1m
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridCreatorNegativeStepMultiplicator", StartProgram.IsTester, false);
        SetPrivateField(tab, "_connector", connector);

        Exception? error = Record.Exception(() => creator.CreateNewGrid(tab, TradeGridPrimeType.MarketMaking));

        Assert.Null(error);
        Assert.NotNull(creator.Lines);
        Assert.Single(creator.Lines);
        Assert.Equal(100m, creator.Lines[0].PriceEnter);
        Assert.Equal(101m, creator.Lines[0].PriceExit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_CreateNewGrid_WithNegativeProfitMultiplicatorRuntimeValue_ShouldStopAfterFirstLine()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            GridSide = Side.Buy,
            FirstPrice = 100m,
            LineCountStart = 3,
            TypeStep = TradeGridValueType.Absolute,
            LineStep = 1m,
            StepMultiplicator = 1m,
            TypeProfit = TradeGridValueType.Absolute,
            ProfitStep = 1m,
            ProfitMultiplicator = -1m,
            TypeVolume = TradeGridVolumeType.Contracts,
            StartVolume = 1m,
            MartingaleMultiplicator = 1m
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridCreatorNegativeProfitMultiplicator", StartProgram.IsTester, false);
        SetPrivateField(tab, "_connector", connector);

        Exception? error = Record.Exception(() => creator.CreateNewGrid(tab, TradeGridPrimeType.MarketMaking));

        Assert.Null(error);
        Assert.NotNull(creator.Lines);
        Assert.Single(creator.Lines);
        Assert.Equal(100m, creator.Lines[0].PriceEnter);
        Assert.Equal(101m, creator.Lines[0].PriceExit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_CreateNewGrid_WithNegativeMartingaleRuntimeValue_ShouldStopAfterFirstLine()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            GridSide = Side.Buy,
            FirstPrice = 100m,
            LineCountStart = 3,
            TypeStep = TradeGridValueType.Absolute,
            LineStep = 1m,
            StepMultiplicator = 1m,
            TypeProfit = TradeGridValueType.Absolute,
            ProfitStep = 1m,
            ProfitMultiplicator = 1m,
            TypeVolume = TradeGridVolumeType.Contracts,
            StartVolume = 1m,
            MartingaleMultiplicator = -1m
        };

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridCreatorNegativeMartingaleMultiplicator", StartProgram.IsTester, false);
        SetPrivateField(tab, "_connector", connector);

        Exception? error = Record.Exception(() => creator.CreateNewGrid(tab, TradeGridPrimeType.MarketMaking));

        Assert.Null(error);
        Assert.NotNull(creator.Lines);
        Assert.Single(creator.Lines);
        Assert.Equal(100m, creator.Lines[0].PriceEnter);
        Assert.Equal(1m, creator.Lines[0].Volume);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_CreateNewGrid_WithNegativeFirstPriceRuntimeValue_ShouldNotCreateLines()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            GridSide = Side.Buy,
            FirstPrice = -100m,
            LineCountStart = 3,
            TypeStep = TradeGridValueType.Absolute,
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
        ConnectorCandles connector = new ConnectorCandles("CodexGridCreatorNegativeFirstPrice", StartProgram.IsTester, false);
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
    public void Stage2Step2_2_TradeGridCreator_GetSaveLinesString_WithEmptyLines_ShouldReturnEmptyString()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            Lines = new List<TradeGridLine>()
        };

        string saved = creator.GetSaveLinesString();

        Assert.Equal(string.Empty, saved);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_GetSaveLinesString_WithMultipleLines_ShouldConcatenateWithCaretSeparators()
    {
        TradeGridCreator creator = new TradeGridCreator
        {
            Lines = new List<TradeGridLine>
            {
                new TradeGridLine
                {
                    PriceEnter = 100m,
                    Volume = 1m,
                    Side = Side.Buy,
                    PriceExit = 101m,
                    PositionNum = 7
                },
                new TradeGridLine
                {
                    PriceEnter = 102.5m,
                    Volume = 2m,
                    Side = Side.Sell,
                    PriceExit = 99.5m,
                    PositionNum = 8
                }
            }
        };

        string saved = creator.GetSaveLinesString();

        Assert.Equal("100|1|Buy|101|7|^102.5|2|Sell|99.5|8|^", saved);
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
    public void Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithMalformedFields_ShouldKeepValuesAndContinueParsing()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter
        {
            AutoStartRegime = TradeGridAutoStartRegime.Off,
            AutoStartPrice = 77m,
            RebuildGridRegime = GridAutoStartShiftFirstPriceRegime.Off,
            ShiftFirstPrice = 3m,
            StartGridByTimeOfDayIsOn = false,
            StartGridByTimeOfDayHour = 14,
            StartGridByTimeOfDayMinute = 15,
            StartGridByTimeOfDaySecond = 16,
            SingleActivationMode = true
        };

        Exception? error = Record.Exception(() => autoStarter.LoadFromString("badEnum@badDecimal@On_ShiftOnNewPrice@2.5@badBool@20@30@40@0"));

        Assert.Null(error);
        Assert.Equal(TradeGridAutoStartRegime.Off, autoStarter.AutoStartRegime);
        Assert.Equal(77m, autoStarter.AutoStartPrice);
        Assert.Equal(GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice, autoStarter.RebuildGridRegime);
        Assert.Equal(2.5m, autoStarter.ShiftFirstPrice);
        Assert.False(autoStarter.StartGridByTimeOfDayIsOn);
        Assert.Equal(20, autoStarter.StartGridByTimeOfDayHour);
        Assert.Equal(30, autoStarter.StartGridByTimeOfDayMinute);
        Assert.Equal(40, autoStarter.StartGridByTimeOfDaySecond);
        Assert.False(autoStarter.SingleActivationMode);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithFlexibleTimeBools_ShouldParse()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter
        {
            StartGridByTimeOfDayIsOn = false,
            SingleActivationMode = true
        };

        Exception? error = Record.Exception(() => autoStarter.LoadFromString("HigherOrEqual@123.45@On_ShiftOnNewPrice@1.5@on@20@30@40@no"));

        Assert.Null(error);
        Assert.Equal(TradeGridAutoStartRegime.HigherOrEqual, autoStarter.AutoStartRegime);
        Assert.Equal(123.45m, autoStarter.AutoStartPrice);
        Assert.Equal(GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice, autoStarter.RebuildGridRegime);
        Assert.Equal(1.5m, autoStarter.ShiftFirstPrice);
        Assert.True(autoStarter.StartGridByTimeOfDayIsOn);
        Assert.Equal(20, autoStarter.StartGridByTimeOfDayHour);
        Assert.Equal(30, autoStarter.StartGridByTimeOfDayMinute);
        Assert.Equal(40, autoStarter.StartGridByTimeOfDaySecond);
        Assert.False(autoStarter.SingleActivationMode);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithOutOfRangeTimeFields_ShouldKeepExistingValues()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter
        {
            StartGridByTimeOfDayHour = 14,
            StartGridByTimeOfDayMinute = 15,
            StartGridByTimeOfDaySecond = 16
        };

        Exception? error = Record.Exception(() => autoStarter.LoadFromString("HigherOrEqual@123.45@On_ShiftOnNewPrice@1.5@True@24@60@61@1"));

        Assert.Null(error);
        Assert.Equal(14, autoStarter.StartGridByTimeOfDayHour);
        Assert.Equal(15, autoStarter.StartGridByTimeOfDayMinute);
        Assert.Equal(16, autoStarter.StartGridByTimeOfDaySecond);
        Assert.True(autoStarter.StartGridByTimeOfDayIsOn);
        Assert.True(autoStarter.SingleActivationMode);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithMissingTimeFlag_ShouldKeepValueAndContinueTailParsing()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter
        {
            StartGridByTimeOfDayIsOn = false,
            StartGridByTimeOfDayHour = 14,
            StartGridByTimeOfDayMinute = 15,
            StartGridByTimeOfDaySecond = 16,
            SingleActivationMode = true
        };

        Exception? error = Record.Exception(() => autoStarter.LoadFromString("HigherOrEqual@123.45@On_ShiftOnNewPrice@1.5@@20@30@40@0"));

        Assert.Null(error);
        Assert.False(autoStarter.StartGridByTimeOfDayIsOn);
        Assert.Equal(20, autoStarter.StartGridByTimeOfDayHour);
        Assert.Equal(30, autoStarter.StartGridByTimeOfDayMinute);
        Assert.Equal(40, autoStarter.StartGridByTimeOfDaySecond);
        Assert.False(autoStarter.SingleActivationMode);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithMissingHour_ShouldKeepValueAndContinueTailParsing()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter
        {
            StartGridByTimeOfDayIsOn = false,
            StartGridByTimeOfDayHour = 14,
            StartGridByTimeOfDayMinute = 15,
            StartGridByTimeOfDaySecond = 16,
            SingleActivationMode = true
        };

        Exception? error = Record.Exception(() => autoStarter.LoadFromString("HigherOrEqual@123.45@On_ShiftOnNewPrice@1.5@on@@30@40@0"));

        Assert.Null(error);
        Assert.True(autoStarter.StartGridByTimeOfDayIsOn);
        Assert.Equal(14, autoStarter.StartGridByTimeOfDayHour);
        Assert.Equal(30, autoStarter.StartGridByTimeOfDayMinute);
        Assert.Equal(40, autoStarter.StartGridByTimeOfDaySecond);
        Assert.False(autoStarter.SingleActivationMode);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithMissingMinute_ShouldKeepValueAndContinueTailParsing()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter
        {
            StartGridByTimeOfDayIsOn = false,
            StartGridByTimeOfDayHour = 14,
            StartGridByTimeOfDayMinute = 15,
            StartGridByTimeOfDaySecond = 16,
            SingleActivationMode = true
        };

        Exception? error = Record.Exception(() => autoStarter.LoadFromString("HigherOrEqual@123.45@On_ShiftOnNewPrice@1.5@on@20@@40@0"));

        Assert.Null(error);
        Assert.True(autoStarter.StartGridByTimeOfDayIsOn);
        Assert.Equal(20, autoStarter.StartGridByTimeOfDayHour);
        Assert.Equal(15, autoStarter.StartGridByTimeOfDayMinute);
        Assert.Equal(40, autoStarter.StartGridByTimeOfDaySecond);
        Assert.False(autoStarter.SingleActivationMode);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithMissingSecond_ShouldKeepValueAndParseSingleActivation()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter
        {
            StartGridByTimeOfDayIsOn = false,
            StartGridByTimeOfDayHour = 14,
            StartGridByTimeOfDayMinute = 15,
            StartGridByTimeOfDaySecond = 16,
            SingleActivationMode = true
        };

        Exception? error = Record.Exception(() => autoStarter.LoadFromString("HigherOrEqual@123.45@On_ShiftOnNewPrice@1.5@on@20@30@@0"));

        Assert.Null(error);
        Assert.True(autoStarter.StartGridByTimeOfDayIsOn);
        Assert.Equal(20, autoStarter.StartGridByTimeOfDayHour);
        Assert.Equal(30, autoStarter.StartGridByTimeOfDayMinute);
        Assert.Equal(16, autoStarter.StartGridByTimeOfDaySecond);
        Assert.False(autoStarter.SingleActivationMode);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithInvalidHourAndValidMinuteSecond_ShouldKeepHourAndContinueTailParsing()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter
        {
            StartGridByTimeOfDayIsOn = false,
            StartGridByTimeOfDayHour = 14,
            StartGridByTimeOfDayMinute = 15,
            StartGridByTimeOfDaySecond = 16,
            SingleActivationMode = true
        };

        Exception? error = Record.Exception(() => autoStarter.LoadFromString("HigherOrEqual@123.45@On_ShiftOnNewPrice@1.5@on@99@30@40@0"));

        Assert.Null(error);
        Assert.True(autoStarter.StartGridByTimeOfDayIsOn);
        Assert.Equal(14, autoStarter.StartGridByTimeOfDayHour);
        Assert.Equal(30, autoStarter.StartGridByTimeOfDayMinute);
        Assert.Equal(40, autoStarter.StartGridByTimeOfDaySecond);
        Assert.False(autoStarter.SingleActivationMode);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_GetSaveString_ShouldKeepReservedTailShape()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter
        {
            AutoStartRegime = TradeGridAutoStartRegime.HigherOrEqual,
            AutoStartPrice = 101.25m,
            RebuildGridRegime = GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice,
            ShiftFirstPrice = 1.5m,
            StartGridByTimeOfDayIsOn = true,
            StartGridByTimeOfDayHour = 9,
            StartGridByTimeOfDayMinute = 30,
            StartGridByTimeOfDaySecond = 45,
            SingleActivationMode = false
        };

        string save = autoStarter.GetSaveString();

        Assert.Equal("HigherOrEqual@101.25@On_ShiftOnNewPrice@1.5@True@9@30@45@False@@@@", save);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithEmptyLikePayloads_ShouldKeepExistingValues()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter
        {
            AutoStartRegime = TradeGridAutoStartRegime.HigherOrEqual,
            AutoStartPrice = 101.25m,
            RebuildGridRegime = GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice
        };

        Exception? error = Record.Exception(() =>
        {
            autoStarter.LoadFromString(null);
            autoStarter.LoadFromString(string.Empty);
            autoStarter.LoadFromString("  \r\n \t  ");
        });

        Assert.Null(error);
        Assert.Equal(TradeGridAutoStartRegime.HigherOrEqual, autoStarter.AutoStartRegime);
        Assert.Equal(101.25m, autoStarter.AutoStartPrice);
        Assert.Equal(GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice, autoStarter.RebuildGridRegime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_GetSaveString_LoadFromString_ShouldRoundTrip()
    {
        TradeGridAutoStarter source = new TradeGridAutoStarter
        {
            AutoStartRegime = TradeGridAutoStartRegime.HigherOrEqual,
            AutoStartPrice = 101.25m,
            RebuildGridRegime = GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice,
            ShiftFirstPrice = 1.5m,
            StartGridByTimeOfDayIsOn = true,
            StartGridByTimeOfDayHour = 9,
            StartGridByTimeOfDayMinute = 30,
            StartGridByTimeOfDaySecond = 45,
            SingleActivationMode = false
        };

        TradeGridAutoStarter loaded = new TradeGridAutoStarter();

        Exception? error = Record.Exception(() => loaded.LoadFromString(source.GetSaveString()));

        Assert.Null(error);
        Assert.Equal(TradeGridAutoStartRegime.HigherOrEqual, loaded.AutoStartRegime);
        Assert.Equal(101.25m, loaded.AutoStartPrice);
        Assert.Equal(GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice, loaded.RebuildGridRegime);
        Assert.Equal(1.5m, loaded.ShiftFirstPrice);
        Assert.True(loaded.StartGridByTimeOfDayIsOn);
        Assert.Equal(9, loaded.StartGridByTimeOfDayHour);
        Assert.Equal(30, loaded.StartGridByTimeOfDayMinute);
        Assert.Equal(45, loaded.StartGridByTimeOfDaySecond);
        Assert.False(loaded.SingleActivationMode);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_SendNewLogMessage_WithSubscriber_ShouldForwardMessage()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter();
        string? receivedMessage = null;
        LogMessageType? receivedType = null;

        autoStarter.LogMessageEvent += (message, type) =>
        {
            receivedMessage = message;
            receivedType = type;
        };

        autoStarter.SendNewLogMessage("CodexAutoStarterMessage", LogMessageType.Connect);

        Assert.Equal("CodexAutoStarterMessage", receivedMessage);
        Assert.Equal(LogMessageType.Connect, receivedType);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridAutoStarter_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow()
    {
        TradeGridAutoStarter autoStarter = new TradeGridAutoStarter();

        Exception? error = Record.Exception(() =>
            autoStarter.SendNewLogMessage("CodexAutoStarterNoSubscriber", LogMessageType.System));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_SendNewLogMessage_WithSubscriber_ShouldForwardMessage()
    {
        TradeGridCreator creator = new TradeGridCreator();
        string? receivedMessage = null;
        LogMessageType? receivedType = null;

        creator.LogMessageEvent += (message, type) =>
        {
            receivedMessage = message;
            receivedType = type;
        };

        creator.SendNewLogMessage("CodexCreatorMessage", LogMessageType.Signal);

        Assert.Equal("CodexCreatorMessage", receivedMessage);
        Assert.Equal(LogMessageType.Signal, receivedType);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridCreator_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow()
    {
        TradeGridCreator creator = new TradeGridCreator();

        Exception? error = Record.Exception(() =>
            creator.SendNewLogMessage("CodexCreatorNoSubscriber", LogMessageType.System));

        Assert.Null(error);
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
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithInvalidBoolToken_ShouldKeepValueAndContinueParsing()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            FailOpenOrdersReactionIsOn = false,
            FailOpenOrdersCountToReaction = 10,
            FailCancelOrdersCountToReaction = 11,
            FailCancelOrdersReactionIsOn = true,
            WaitOnStartConnectorIsOn = false,
            WaitSecondsOnStartConnector = 31,
            ReduceOrdersCountInMarketOnNoFundsError = false
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("badBool@@15@@17@False@True@35@1"));

        Assert.Null(error);
        Assert.False(reaction.FailOpenOrdersReactionIsOn);
        Assert.Equal(15, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(17, reaction.FailCancelOrdersCountToReaction);
        Assert.False(reaction.FailCancelOrdersReactionIsOn);
        Assert.True(reaction.WaitOnStartConnectorIsOn);
        Assert.Equal(35, reaction.WaitSecondsOnStartConnector);
        Assert.True(reaction.ReduceOrdersCountInMarketOnNoFundsError);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithInvalidCountToken_ShouldKeepValueAndContinueParsing()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            FailOpenOrdersCountToReaction = 10,
            FailCancelOrdersCountToReaction = 11
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("True@@badCount@@17@False@True@35@False"));

        Assert.Null(error);
        Assert.True(reaction.FailOpenOrdersReactionIsOn);
        Assert.Equal(10, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(17, reaction.FailCancelOrdersCountToReaction);
        Assert.False(reaction.FailCancelOrdersReactionIsOn);
        Assert.True(reaction.WaitOnStartConnectorIsOn);
        Assert.Equal(35, reaction.WaitSecondsOnStartConnector);
        Assert.False(reaction.ReduceOrdersCountInMarketOnNoFundsError);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithInvalidWaitBoolAndValidTail_ShouldKeepBoolAndContinueTailParsing()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            WaitOnStartConnectorIsOn = false,
            WaitSecondsOnStartConnector = 31,
            ReduceOrdersCountInMarketOnNoFundsError = false
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("True@@15@@17@False@badBool@45@1"));

        Assert.Null(error);
        Assert.False(reaction.WaitOnStartConnectorIsOn);
        Assert.Equal(45, reaction.WaitSecondsOnStartConnector);
        Assert.True(reaction.ReduceOrdersCountInMarketOnNoFundsError);
        Assert.Equal(15, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(17, reaction.FailCancelOrdersCountToReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithInvalidWaitSecondsAndValidReduceFlag_ShouldKeepSecondsAndParseReduceFlag()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            WaitOnStartConnectorIsOn = false,
            WaitSecondsOnStartConnector = 31,
            ReduceOrdersCountInMarketOnNoFundsError = true
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("True@@15@@17@False@on@badInt@0"));

        Assert.Null(error);
        Assert.True(reaction.WaitOnStartConnectorIsOn);
        Assert.Equal(31, reaction.WaitSecondsOnStartConnector);
        Assert.False(reaction.ReduceOrdersCountInMarketOnNoFundsError);
        Assert.Equal(15, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(17, reaction.FailCancelOrdersCountToReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithZeroCounts_ShouldKeepExistingValues()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            FailOpenOrdersCountToReaction = 10,
            FailCancelOrdersCountToReaction = 11
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("True@@0@@0@False@True@35@False"));

        Assert.Null(error);
        Assert.Equal(10, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(11, reaction.FailCancelOrdersCountToReaction);
        Assert.False(reaction.FailCancelOrdersReactionIsOn);
        Assert.True(reaction.WaitOnStartConnectorIsOn);
        Assert.Equal(35, reaction.WaitSecondsOnStartConnector);
        Assert.False(reaction.ReduceOrdersCountInMarketOnNoFundsError);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithNegativeWaitSeconds_ShouldKeepExistingValue()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            WaitSecondsOnStartConnector = 31,
            ReduceOrdersCountInMarketOnNoFundsError = true
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("True@@15@@17@False@True@-5@0"));

        Assert.Null(error);
        Assert.Equal(31, reaction.WaitSecondsOnStartConnector);
        Assert.False(reaction.ReduceOrdersCountInMarketOnNoFundsError);
        Assert.Equal(15, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(17, reaction.FailCancelOrdersCountToReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithFlexiblePrimaryBools_ShouldParse()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            FailOpenOrdersReactionIsOn = false,
            FailCancelOrdersReactionIsOn = true
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("yes@@15@@17@off"));

        Assert.Null(error);
        Assert.True(reaction.FailOpenOrdersReactionIsOn);
        Assert.Equal(15, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(17, reaction.FailCancelOrdersCountToReaction);
        Assert.False(reaction.FailCancelOrdersReactionIsOn);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithFlexibleOptionalBools_ShouldParse()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            WaitOnStartConnectorIsOn = false,
            ReduceOrdersCountInMarketOnNoFundsError = false
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("True@@15@@17@False@on@35@yes"));

        Assert.Null(error);
        Assert.True(reaction.WaitOnStartConnectorIsOn);
        Assert.Equal(35, reaction.WaitSecondsOnStartConnector);
        Assert.True(reaction.ReduceOrdersCountInMarketOnNoFundsError);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithMissingWaitBool_ShouldKeepValueAndContinueTailParsing()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            WaitOnStartConnectorIsOn = false,
            WaitSecondsOnStartConnector = 31,
            ReduceOrdersCountInMarketOnNoFundsError = false
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("True@@15@@17@False@@45@1"));

        Assert.Null(error);
        Assert.False(reaction.WaitOnStartConnectorIsOn);
        Assert.Equal(45, reaction.WaitSecondsOnStartConnector);
        Assert.True(reaction.ReduceOrdersCountInMarketOnNoFundsError);
        Assert.Equal(15, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(17, reaction.FailCancelOrdersCountToReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithMissingWaitSeconds_ShouldKeepValueAndParseReduceFlag()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            WaitOnStartConnectorIsOn = false,
            WaitSecondsOnStartConnector = 31,
            ReduceOrdersCountInMarketOnNoFundsError = true
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("True@@15@@17@False@on@@0"));

        Assert.Null(error);
        Assert.True(reaction.WaitOnStartConnectorIsOn);
        Assert.Equal(31, reaction.WaitSecondsOnStartConnector);
        Assert.False(reaction.ReduceOrdersCountInMarketOnNoFundsError);
        Assert.Equal(15, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(17, reaction.FailCancelOrdersCountToReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithMissingReduceFlag_ShouldKeepValueAndPreserveParsedTailPrefix()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            WaitOnStartConnectorIsOn = false,
            WaitSecondsOnStartConnector = 31,
            ReduceOrdersCountInMarketOnNoFundsError = false
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("True@@15@@17@False@on@45"));

        Assert.Null(error);
        Assert.True(reaction.WaitOnStartConnectorIsOn);
        Assert.Equal(45, reaction.WaitSecondsOnStartConnector);
        Assert.False(reaction.ReduceOrdersCountInMarketOnNoFundsError);
        Assert.Equal(15, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(17, reaction.FailCancelOrdersCountToReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_SendNewLogMessage_WithSubscriber_ShouldForwardMessage()
    {
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(CreateBareGrid());
        string? receivedMessage = null;
        LogMessageType? receivedType = null;

        reaction.LogMessageEvent += (message, type) =>
        {
            receivedMessage = message;
            receivedType = type;
        };

        reaction.SendNewLogMessage("CodexErrorsReactionMessage", LogMessageType.Signal);

        Assert.Equal("CodexErrorsReactionMessage", receivedMessage);
        Assert.Equal(LogMessageType.Signal, receivedType);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow()
    {
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(CreateBareGrid());

        Exception? error = Record.Exception(() =>
            reaction.SendNewLogMessage("CodexErrorsReactionNoSubscriber", LogMessageType.System));

        Assert.Null(error);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithInvalidWaitBoolAndMissingWaitSeconds_ShouldKeepValuesAndParseReduceFlag()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(grid)
        {
            WaitOnStartConnectorIsOn = false,
            WaitSecondsOnStartConnector = 31,
            ReduceOrdersCountInMarketOnNoFundsError = true
        };

        Exception? error = Record.Exception(() => reaction.LoadFromString("True@@15@@17@False@badBool@@0"));

        Assert.Null(error);
        Assert.False(reaction.WaitOnStartConnectorIsOn);
        Assert.Equal(31, reaction.WaitSecondsOnStartConnector);
        Assert.False(reaction.ReduceOrdersCountInMarketOnNoFundsError);
        Assert.Equal(15, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(17, reaction.FailCancelOrdersCountToReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_GetSaveString_ShouldKeepReservedTailShape()
    {
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(CreateBareGrid())
        {
            FailOpenOrdersReactionIsOn = false,
            FailOpenOrdersCountToReaction = 3,
            FailCancelOrdersCountToReaction = 4,
            FailCancelOrdersReactionIsOn = true,
            WaitOnStartConnectorIsOn = false,
            WaitSecondsOnStartConnector = 12,
            ReduceOrdersCountInMarketOnNoFundsError = false
        };

        string save = reaction.GetSaveString();

        Assert.Equal("False@@3@@4@True@False@12@False@@@@@@", save);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithEmptyLikePayloads_ShouldKeepExistingValues()
    {
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(CreateBareGrid())
        {
            FailOpenOrdersReactionIsOn = false,
            FailOpenOrdersCountToReaction = 3,
            FailCancelOrdersCountToReaction = 4
        };

        Exception? error = Record.Exception(() =>
        {
            reaction.LoadFromString(null);
            reaction.LoadFromString(string.Empty);
            reaction.LoadFromString("  \r\n \t  ");
        });

        Assert.Null(error);
        Assert.False(reaction.FailOpenOrdersReactionIsOn);
        Assert.Equal(3, reaction.FailOpenOrdersCountToReaction);
        Assert.Equal(4, reaction.FailCancelOrdersCountToReaction);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_GetSaveString_LoadFromString_ShouldRoundTrip()
    {
        TradeGridErrorsReaction source = new TradeGridErrorsReaction(CreateBareGrid())
        {
            FailOpenOrdersReactionIsOn = false,
            FailOpenOrdersCountToReaction = 3,
            FailCancelOrdersCountToReaction = 4,
            FailCancelOrdersReactionIsOn = true,
            WaitOnStartConnectorIsOn = false,
            WaitSecondsOnStartConnector = 12,
            ReduceOrdersCountInMarketOnNoFundsError = false
        };

        TradeGridErrorsReaction loaded = new TradeGridErrorsReaction(CreateBareGrid());

        Exception? error = Record.Exception(() => loaded.LoadFromString(source.GetSaveString()));

        Assert.Null(error);
        Assert.False(loaded.FailOpenOrdersReactionIsOn);
        Assert.Equal(3, loaded.FailOpenOrdersCountToReaction);
        Assert.Equal(4, loaded.FailCancelOrdersCountToReaction);
        Assert.True(loaded.FailCancelOrdersReactionIsOn);
        Assert.False(loaded.WaitOnStartConnectorIsOn);
        Assert.Equal(12, loaded.WaitSecondsOnStartConnector);
        Assert.False(loaded.ReduceOrdersCountInMarketOnNoFundsError);
    }

    [Fact]
    public void Stage2Step2_2_TradeGridErrorsReaction_Delete_ShouldClearGridAndStayIdempotent()
    {
        TradeGridErrorsReaction reaction = new TradeGridErrorsReaction(CreateBareGrid());

        Exception? error = Record.Exception(() =>
        {
            reaction.Delete();
            reaction.Delete();
        });

        object? grid = typeof(TradeGridErrorsReaction).GetField("_myGrid", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(reaction);

        Assert.Null(error);
        Assert.Null(grid);
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
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidBoolToken_ShouldKeepBoolAndContinueParsing()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = false,
            TrailingUpStep = 0.5m,
            TrailingUpLimit = 1m,
            TrailingDownIsOn = true,
            TrailingDownStep = 2m,
            TrailingDownLimit = 20m,
            TrailingUpCanMoveExitOrder = false,
            TrailingDownCanMoveExitOrder = true
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("badBool@1.25@10@False@0.5@5@1@0"));

        Assert.Null(error);
        Assert.False(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
        Assert.True(trailing.TrailingUpCanMoveExitOrder);
        Assert.False(trailing.TrailingDownCanMoveExitOrder);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidDecimalToken_ShouldKeepDecimalAndContinueParsing()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = false,
            TrailingUpStep = 0.5m,
            TrailingUpLimit = 1m,
            TrailingDownIsOn = true,
            TrailingDownStep = 2m,
            TrailingDownLimit = 20m
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@badDecimal@10@False@0.5@5"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(0.5m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithNegativeTrailingUpStep_ShouldKeepExistingValue()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpStep = 0.5m
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@-1.25@10@False@0.5@5"));

        Assert.Null(error);
        Assert.Equal(0.5m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithNegativeTrailingDownStep_ShouldKeepExistingValue()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingDownStep = 2m
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@10@False@-0.5@5"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(2m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithNegativeTrailingUpLimit_ShouldKeepExistingValue()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpLimit = 11m
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@-10@False@0.5@5"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(11m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithNegativeTrailingDownLimit_ShouldKeepExistingValue()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingDownLimit = 7m
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@10@False@0.5@-5"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(7m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithZeroTrailingUpStep_ShouldKeepExistingValue()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpStep = 0.5m
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@0@10@False@0.5@5"));

        Assert.Null(error);
        Assert.Equal(0.5m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithZeroTrailingUpLimit_ShouldKeepExistingValue()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpLimit = 11m
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@0@False@0.5@5"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(11m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithZeroTrailingDownStep_ShouldKeepExistingValue()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingDownStep = 2m
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@10@False@0@5"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(2m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithZeroTrailingDownLimit_ShouldKeepExistingValue()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingDownLimit = 7m
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@10@False@0.5@0"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(7m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidUpMoveFlag_ShouldKeepValueAndParseDownMoveFlag()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpCanMoveExitOrder = false,
            TrailingDownCanMoveExitOrder = false
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@10@False@0.5@5@badBool@1"));

        Assert.Null(error);
        Assert.False(trailing.TrailingUpCanMoveExitOrder);
        Assert.True(trailing.TrailingDownCanMoveExitOrder);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidDownMoveFlag_ShouldParseUpMoveFlagAndKeepValue()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpCanMoveExitOrder = false,
            TrailingDownCanMoveExitOrder = false
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@10@False@0.5@5@1@badBool"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpCanMoveExitOrder);
        Assert.False(trailing.TrailingDownCanMoveExitOrder);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithMissingUpMoveFlag_ShouldKeepValueAndParseDownMoveFlag()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpCanMoveExitOrder = false,
            TrailingDownCanMoveExitOrder = true
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@10@False@0.5@5@@0"));

        Assert.Null(error);
        Assert.False(trailing.TrailingUpCanMoveExitOrder);
        Assert.False(trailing.TrailingDownCanMoveExitOrder);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidBothMoveFlags_ShouldKeepExistingValues()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpCanMoveExitOrder = true,
            TrailingDownCanMoveExitOrder = false
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@10@False@0.5@5@badBool@alsoBad"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpCanMoveExitOrder);
        Assert.False(trailing.TrailingDownCanMoveExitOrder);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.False(trailing.TrailingDownIsOn);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithFlexiblePrimaryBools_ShouldParse()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = false,
            TrailingDownIsOn = true
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("yes@1.25@10@off@0.5@5"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithFlexibleMoveFlagBools_ShouldParse()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpCanMoveExitOrder = false,
            TrailingDownCanMoveExitOrder = true
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@10@False@0.5@5@on@no"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpCanMoveExitOrder);
        Assert.False(trailing.TrailingDownCanMoveExitOrder);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithNumericPrimaryBools_ShouldParse()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = false,
            TrailingDownIsOn = true
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("1@1.25@10@0@0.5@5"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithNumericMoveFlagBools_ShouldParse()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpCanMoveExitOrder = false,
            TrailingDownCanMoveExitOrder = false
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@10@False@0.5@5@1@0"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpCanMoveExitOrder);
        Assert.False(trailing.TrailingDownCanMoveExitOrder);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithMissingUpBool_ShouldKeepValueAndContinueParsing()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = false,
            TrailingDownIsOn = true
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("@1.25@10@False@0.5@5"));

        Assert.Null(error);
        Assert.False(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithMissingDownBool_ShouldKeepValueAndContinueParsing()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = false,
            TrailingDownIsOn = true
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("True@1.25@10@@0.5@5"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.True(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidUpBoolAndFlexibleDownBool_ShouldKeepValueAndContinueParsing()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = false,
            TrailingDownIsOn = true
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("badBool@1.25@10@off@0.5@5"));

        Assert.Null(error);
        Assert.False(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.False(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
    }

    [Fact]
    public void Stage2Step2_2_TrailingUp_LoadFromString_WithFlexibleUpBoolAndInvalidDownBool_ShouldKeepValueAndContinueParsing()
    {
        TradeGrid grid = CreateBareGrid();
        TrailingUp trailing = new TrailingUp(grid)
        {
            TrailingUpIsOn = false,
            TrailingDownIsOn = true
        };

        Exception? error = Record.Exception(() => trailing.LoadFromString("on@1.25@10@badBool@0.5@5"));

        Assert.Null(error);
        Assert.True(trailing.TrailingUpIsOn);
        Assert.Equal(1.25m, trailing.TrailingUpStep);
        Assert.Equal(10m, trailing.TrailingUpLimit);
        Assert.True(trailing.TrailingDownIsOn);
        Assert.Equal(0.5m, trailing.TrailingDownStep);
        Assert.Equal(5m, trailing.TrailingDownLimit);
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
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithInvalidAutoClearBool_ShouldKeepExistingValue()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[4] = "badBool";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.AutoClearJournalIsOn = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.False(loaded.AutoClearJournalIsOn);
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
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithNullPayload_ShouldKeepExistingValues()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridType = TradeGridPrimeType.OpenPosition;
        grid.Regime = TradeGridRegime.CloseOnly;
        grid.GridCreator.TradeAssetInPortfolio = "USDT";

        Exception? error = Record.Exception(() => grid.LoadFromString(null));

        Assert.Null(error);
        Assert.Equal(TradeGridPrimeType.OpenPosition, grid.GridType);
        Assert.Equal(TradeGridRegime.CloseOnly, grid.Regime);
        Assert.Equal("USDT", grid.GridCreator.TradeAssetInPortfolio);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithEmptyPayload_ShouldKeepExistingValues()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridType = TradeGridPrimeType.OpenPosition;
        grid.Regime = TradeGridRegime.CloseOnly;
        grid.GridCreator.TradeAssetInPortfolio = "USDT";

        Exception? error = Record.Exception(() => grid.LoadFromString(string.Empty));

        Assert.Null(error);
        Assert.Equal(TradeGridPrimeType.OpenPosition, grid.GridType);
        Assert.Equal(TradeGridRegime.CloseOnly, grid.Regime);
        Assert.Equal("USDT", grid.GridCreator.TradeAssetInPortfolio);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithWhitespaceOnlyPayload_ShouldKeepExistingValues()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridType = TradeGridPrimeType.OpenPosition;
        grid.Regime = TradeGridRegime.CloseOnly;
        grid.GridCreator.TradeAssetInPortfolio = "USDT";

        Exception? error = Record.Exception(() => grid.LoadFromString("  \r\n \t  "));

        Assert.Null(error);
        Assert.Equal(TradeGridPrimeType.OpenPosition, grid.GridType);
        Assert.Equal(TradeGridRegime.CloseOnly, grid.Regime);
        Assert.Equal("USDT", grid.GridCreator.TradeAssetInPortfolio);
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
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMissingMicroVolumesTail_ShouldKeepParsedDelayAndApplyDefaultMicroFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[11] = "250";
        primeFields[12] = string.Empty;
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.DelayInReal = 777;
        loaded.CheckMicroVolumes = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(250, loaded.DelayInReal);
        Assert.True(loaded.CheckMicroVolumes);
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
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMissingMakerOnlyTail_ShouldKeepParsedDistanceAndApplyDefaultMakerFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[13] = "2.75";
        primeFields[14] = string.Empty;
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.MaxDistanceToOrdersPercent = 9m;
        loaded.OpenOrdersMakerOnly = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(2.75m, loaded.MaxDistanceToOrdersPercent);
        Assert.True(loaded.OpenOrdersMakerOnly);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMissingDistanceTail_ShouldApplyDefaultDistanceAndKeepParsedMakerFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[13] = string.Empty;
        primeFields[14] = "false";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.MaxDistanceToOrdersPercent = 9m;
        loaded.OpenOrdersMakerOnly = true;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(1.5m, loaded.MaxDistanceToOrdersPercent);
        Assert.False(loaded.OpenOrdersMakerOnly);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDistanceAndMissingMakerTail_ShouldKeepDistanceAndApplyDefaultMakerFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[13] = "badDecimal";
        primeFields[14] = string.Empty;
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.MaxDistanceToOrdersPercent = 9m;
        loaded.OpenOrdersMakerOnly = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(9m, loaded.MaxDistanceToOrdersPercent);
        Assert.True(loaded.OpenOrdersMakerOnly);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDelayAndMissingMicroTail_ShouldApplyDefaultDelayAndDefaultMicroFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[11] = "badDelay";
        primeFields[12] = string.Empty;
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.DelayInReal = 777;
        loaded.CheckMicroVolumes = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(500, loaded.DelayInReal);
        Assert.True(loaded.CheckMicroVolumes);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDelayAndValidMicroTail_ShouldApplyDefaultDelayAndParseMicroFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[11] = "badDelay";
        primeFields[12] = "false";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.DelayInReal = 777;
        loaded.CheckMicroVolumes = true;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(500, loaded.DelayInReal);
        Assert.False(loaded.CheckMicroVolumes);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMissingDelayAndValidMicroTail_ShouldApplyDefaultDelayAndParseMicroFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[11] = string.Empty;
        primeFields[12] = "false";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.DelayInReal = 777;
        loaded.CheckMicroVolumes = true;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(500, loaded.DelayInReal);
        Assert.False(loaded.CheckMicroVolumes);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDelayAndInvalidMicroTail_ShouldApplyDefaultDelayAndKeepMicroFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[11] = "badDelay";
        primeFields[12] = "badBool";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.DelayInReal = 777;
        loaded.CheckMicroVolumes = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(500, loaded.DelayInReal);
        Assert.False(loaded.CheckMicroVolumes);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMissingDelayAndInvalidMicroTail_ShouldApplyDefaultDelayAndKeepMicroFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[11] = string.Empty;
        primeFields[12] = "badBool";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.DelayInReal = 777;
        loaded.CheckMicroVolumes = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(500, loaded.DelayInReal);
        Assert.False(loaded.CheckMicroVolumes);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDistanceAndValidMakerTail_ShouldKeepDistanceAndParseMakerFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[13] = "badDecimal";
        primeFields[14] = "false";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.MaxDistanceToOrdersPercent = 9m;
        loaded.OpenOrdersMakerOnly = true;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(9m, loaded.MaxDistanceToOrdersPercent);
        Assert.False(loaded.OpenOrdersMakerOnly);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMissingDistanceAndInvalidMakerTail_ShouldApplyDefaultDistanceAndKeepMakerFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[13] = string.Empty;
        primeFields[14] = "badBool";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.MaxDistanceToOrdersPercent = 9m;
        loaded.OpenOrdersMakerOnly = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(1.5m, loaded.MaxDistanceToOrdersPercent);
        Assert.False(loaded.OpenOrdersMakerOnly);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDistanceAndInvalidMakerTail_ShouldKeepDistanceAndKeepMakerFlag()
    {
        TradeGrid source = CreateBareGrid();
        string save = source.GetSaveString();
        string[] sections = save.Split('%');
        string[] primeFields = sections[0].Split('@');
        primeFields[13] = "badDecimal";
        primeFields[14] = "badBool";
        sections[0] = string.Join("@", primeFields);
        string payload = string.Join("%", sections);

        TradeGrid loaded = CreateBareGrid();
        loaded.MaxDistanceToOrdersPercent = 9m;
        loaded.OpenOrdersMakerOnly = false;

        Exception? error = Record.Exception(() => loaded.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(9m, loaded.MaxDistanceToOrdersPercent);
        Assert.False(loaded.OpenOrdersMakerOnly);
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
    public void Stage2Step2_2_TradeGrid_EventDispatch_WithSubscribers_ShouldInvokeEachHandlerOnce()
    {
        TradeGrid grid = CreateBareGrid();
        int saveCalls = 0;
        int repaintCalls = 0;
        int fullRepaintCalls = 0;

        grid.NeedToSaveEvent += () => saveCalls++;
        grid.RePaintSettingsEvent += () => repaintCalls++;
        grid.FullRePaintGridEvent += () => fullRepaintCalls++;

        grid.Save();
        grid.RePaintGrid();
        grid.FullRePaintGrid();

        Assert.Equal(1, saveCalls);
        Assert.Equal(1, repaintCalls);
        Assert.Equal(1, fullRepaintCalls);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_EventDispatch_WithoutSubscribers_ShouldNotThrow()
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
    public void Stage2Step2_2_TradeGrid_SendNewLogMessage_WithSubscriber_ShouldForwardMessage()
    {
        TradeGrid grid = CreateBareGrid();
        string? receivedMessage = null;
        LogMessageType? receivedType = null;

        grid.LogMessageEvent += (message, type) =>
        {
            receivedMessage = message;
            receivedType = type;
        };

        grid.SendNewLogMessage("CodexTradeGridMessage", LogMessageType.Error);

        Assert.NotNull(receivedMessage);
        Assert.Contains("Grid error. Bot: unknown", receivedMessage);
        Assert.Contains("Security name: unknown", receivedMessage);
        Assert.Contains("CodexTradeGridMessage", receivedMessage);
        Assert.Equal(LogMessageType.Error, receivedType);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow()
    {
        TradeGrid grid = CreateBareGrid();

        Exception? error = Record.Exception(() =>
            grid.SendNewLogMessage("CodexTradeGridNoSubscriber", LogMessageType.System));

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
    public void Stage2Step2_2_TradeGrid_HaveOpenPositionsByGrid_WithOpenVolumeLine_ShouldReturnTrue()
    {
        TradeGrid grid = CreateBareGrid();
        Position position = new Position();
        position.AddNewOpenOrder(new Order
        {
            State = OrderStateType.Done,
            Volume = 1,
            VolumeExecute = 1
        });
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine
            {
                Position = position
            }
        };

        bool haveOpenPositions = grid.HaveOpenPositionsByGrid;

        Assert.True(haveOpenPositions);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_HaveOrdersInMarketInGrid_WithActiveOpenOrder_ShouldReturnTrue()
    {
        TradeGrid grid = CreateBareGrid();
        Position position = new Position();
        position.AddNewOpenOrder(new Order
        {
            State = OrderStateType.Active,
            Volume = 1,
            VolumeExecute = 0
        });
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine
            {
                Position = position
            }
        };

        bool haveOrdersInMarket = grid.HaveOrdersInMarketInGrid;

        Assert.True(haveOrdersInMarket);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_HaveOrdersInMarketInGrid_WithActiveCloseOrder_ShouldReturnTrue()
    {
        TradeGrid grid = CreateBareGrid();
        Position position = new Position();
        position.AddNewOpenOrder(new Order
        {
            State = OrderStateType.Done,
            Volume = 1,
            VolumeExecute = 1
        });
        position.AddNewCloseOrder(new Order
        {
            State = OrderStateType.Active,
            Volume = 1,
            VolumeExecute = 0
        });
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine
            {
                Position = position
            }
        };

        bool haveOrdersInMarket = grid.HaveOrdersInMarketInGrid;

        Assert.True(haveOrdersInMarket);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_HaveCloseOrders_WithActiveCloseOrder_ShouldReturnTrue()
    {
        TradeGrid grid = CreateBareGrid();
        Position position = new Position();
        position.AddNewOpenOrder(new Order
        {
            State = OrderStateType.Done,
            Volume = 1,
            VolumeExecute = 1
        });
        position.AddNewCloseOrder(new Order
        {
            State = OrderStateType.Active,
            Volume = 1,
            VolumeExecute = 0
        });
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine
            {
                Position = position
            }
        };

        bool haveCloseOrders = grid.HaveCloseOrders;

        Assert.True(haveCloseOrders);
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
    public void Stage2Step2_2_TradeGrid_GetSaveString_ShouldComposePrimeAndSectionPayloads()
    {
        TradeGrid grid = CreateBareGrid();
        grid.Number = 42;
        grid.GridType = TradeGridPrimeType.MarketMaking;
        grid.Regime = TradeGridRegime.CloseOnly;
        grid.RegimeLogicEntry = TradeGridLogicEntryRegime.OncePerSecond;
        grid.AutoClearJournalIsOn = true;
        grid.MaxClosePositionsInJournal = 11;
        grid.MaxOpenOrdersInMarket = 3;
        grid.MaxCloseOrdersInMarket = 2;
        grid.DelayInReal = 700;
        grid.CheckMicroVolumes = false;
        grid.MaxDistanceToOrdersPercent = 1.5m;
        grid.OpenOrdersMakerOnly = false;

        SetPrivateField(grid, "_firstTradePrice", 123.45m);
        SetPrivateField(grid, "_openPositionsBySession", 4);
        DateTime firstTradeTime = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        SetPrivateField(grid, "_firstTradeTime", firstTradeTime);

        string expected =
            "42@MarketMaking@CloseOnly@OncePerSecond@True@11@3@2@123.45@4@" + firstTradeTime.ToString("O", CultureInfo.InvariantCulture) + "@700@False@1.5@False@@@" +
            "%" + grid.NonTradePeriods.GetSaveString() +
            "%" +
            "%" + grid.StopBy.GetSaveString() +
            "%" + grid.GridCreator.GetSaveString() +
            "%" + grid.StopAndProfit.GetSaveString() +
            "%" + grid.AutoStarter.GetSaveString() +
            "%" + grid.ErrorsReaction.GetSaveString() +
            "%" + grid.TrailingUp.GetSaveString() +
            "%";

        string save = grid.GetSaveString();

        Assert.Equal(expected, save);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetSaveString_WithNullChildSections_ShouldKeepEmptySectionShape()
    {
        TradeGrid grid = CreateBareGrid();
        grid.Number = 7;
        grid.GridType = TradeGridPrimeType.OpenPosition;
        grid.Regime = TradeGridRegime.On;
        grid.RegimeLogicEntry = TradeGridLogicEntryRegime.OnTrade;
        grid.NonTradePeriods = null;
        grid.StopBy = null;
        grid.GridCreator = null;
        grid.StopAndProfit = null;
        grid.AutoStarter = null;
        grid.ErrorsReaction = null;
        grid.TrailingUp = null;

        string save = string.Empty;
        Exception? error = Record.Exception(() => save = grid.GetSaveString());

        Assert.Null(error);
        Assert.Equal("7@OpenPosition@On@OnTrade@False@0@0@0@0@0@" +
            DateTime.MinValue.ToString("O", CultureInfo.InvariantCulture) +
            "@0@False@0@False@@@%%%%%%%%%", save);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithEmptyChildSections_ShouldKeepExistingChildValues()
    {
        TradeGrid grid = CreateBareGrid();
        grid.NonTradePeriods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        grid.StopBy.StopGridByMoveUpIsOn = true;
        grid.StopBy.StopGridByMoveUpValuePercent = 2.5m;
        grid.GridCreator.TradeAssetInPortfolio = "USDT";
        grid.StopAndProfit.ProfitValue = 2.2m;
        grid.AutoStarter.AutoStartPrice = 101.25m;
        grid.ErrorsReaction.FailOpenOrdersCountToReaction = 3;
        grid.TrailingUp.TrailingUpStep = 1.5m;

        string payload =
            "7@OpenPosition@On@OnTrade@False@0@0@0@0@0@" +
            DateTime.MinValue.ToString("O", CultureInfo.InvariantCulture) +
            "@0@False@0@False@@@%%%%%%%%%";

        Exception? error = Record.Exception(() => grid.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(7, grid.Number);
        Assert.Equal(TradeGridPrimeType.OpenPosition, grid.GridType);
        Assert.Equal(TradeGridRegime.On, grid.Regime);
        Assert.Equal(TradeGridLogicEntryRegime.OnTrade, grid.RegimeLogicEntry);

        Assert.Equal(TradeGridRegime.CloseOnly, grid.NonTradePeriods.NonTradePeriod1Regime);
        Assert.True(grid.StopBy.StopGridByMoveUpIsOn);
        Assert.Equal(2.5m, grid.StopBy.StopGridByMoveUpValuePercent);
        Assert.Equal("USDT", grid.GridCreator.TradeAssetInPortfolio);
        Assert.Equal(2.2m, grid.StopAndProfit.ProfitValue);
        Assert.Equal(101.25m, grid.AutoStarter.AutoStartPrice);
        Assert.Equal(3, grid.ErrorsReaction.FailOpenOrdersCountToReaction);
        Assert.Equal(1.5m, grid.TrailingUp.TrailingUpStep);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithNullChildReferencesAndPresentPayloads_ShouldApplyPrimeAndNotThrow()
    {
        TradeGrid grid = CreateBareGrid();
        grid.NonTradePeriods = null;
        grid.StopBy = null;
        grid.GridCreator = null;
        grid.StopAndProfit = null;
        grid.AutoStarter = null;
        grid.ErrorsReaction = null;
        grid.TrailingUp = null;

        string payload =
            "42@MarketMaking@CloseOnly@OncePerSecond@True@11@3@2@123.45@4@" +
            new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture) +
            "@700@False@1.5@False@@@" +
            "%CloseOnly@OffAndCancelOrders@@@@@" +
            "%" +
            "%True@2.5@CloseOnly@False@1.5@Off@True@7@CloseForced@True@600@OffAndCancelOrders@True@9@30@45@CloseOnly@@@@@@" +
            "%Sell@123.45@3@Absolute@1.2@1.1@Percent@0.8@1.3@Contracts@2.5@1.4@AssetX@@@@@@" +
            "%On@Absolute@2.2@On@Percent@1.1@On@Absolute@0.9@False@@@@@@" +
            "%HigherOrEqual@101.25@On_ShiftOnNewPrice@1.5@True@9@30@45@False@@@@" +
            "%False@@3@@4@True@False@12@False@@@@@@" +
            "%True@1.5@110@False@2.5@90@True@False@@@@@@%";

        Exception? error = Record.Exception(() => grid.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(42, grid.Number);
        Assert.Equal(TradeGridPrimeType.MarketMaking, grid.GridType);
        Assert.Equal(TradeGridRegime.CloseOnly, grid.Regime);
        Assert.Equal(TradeGridLogicEntryRegime.OncePerSecond, grid.RegimeLogicEntry);
        Assert.True(grid.AutoClearJournalIsOn);
        Assert.Equal(11, grid.MaxClosePositionsInJournal);
        Assert.Equal(3, grid.MaxOpenOrdersInMarket);
        Assert.Equal(2, grid.MaxCloseOrdersInMarket);
        Assert.Equal(123.45m, grid.FirstPriceReal);
        Assert.Equal(4, grid.OpenPositionsCount);
        Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), grid.FirstTradeTime);
        Assert.Equal(700, grid.DelayInReal);
        Assert.False(grid.CheckMicroVolumes);
        Assert.Equal(1.5m, grid.MaxDistanceToOrdersPercent);
        Assert.False(grid.OpenOrdersMakerOnly);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithPrimeOnlyPayload_ShouldApplyPrimeAndKeepExistingChildValues()
    {
        TradeGrid grid = CreateBareGrid();
        grid.NonTradePeriods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        grid.StopBy.StopGridByMoveUpIsOn = true;
        grid.GridCreator.TradeAssetInPortfolio = "USDT";
        grid.StopAndProfit.ProfitValue = 2.2m;
        grid.AutoStarter.AutoStartPrice = 101.25m;
        grid.ErrorsReaction.FailOpenOrdersCountToReaction = 3;
        grid.TrailingUp.TrailingUpStep = 1.5m;

        string payload =
            "42@MarketMaking@CloseOnly@OncePerSecond@True@11@3@2@123.45@4@" +
            new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture) +
            "@700@False@1.5@False@@@";

        Exception? error = Record.Exception(() => grid.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(42, grid.Number);
        Assert.Equal(TradeGridPrimeType.MarketMaking, grid.GridType);
        Assert.Equal(TradeGridRegime.CloseOnly, grid.Regime);
        Assert.Equal(TradeGridLogicEntryRegime.OncePerSecond, grid.RegimeLogicEntry);
        Assert.True(grid.AutoClearJournalIsOn);
        Assert.Equal(11, grid.MaxClosePositionsInJournal);
        Assert.Equal(3, grid.MaxOpenOrdersInMarket);
        Assert.Equal(2, grid.MaxCloseOrdersInMarket);
        Assert.Equal(123.45m, grid.FirstPriceReal);
        Assert.Equal(4, grid.OpenPositionsCount);
        Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), grid.FirstTradeTime);
        Assert.Equal(700, grid.DelayInReal);
        Assert.False(grid.CheckMicroVolumes);
        Assert.Equal(1.5m, grid.MaxDistanceToOrdersPercent);
        Assert.False(grid.OpenOrdersMakerOnly);

        Assert.Equal(TradeGridRegime.CloseOnly, grid.NonTradePeriods.NonTradePeriod1Regime);
        Assert.True(grid.StopBy.StopGridByMoveUpIsOn);
        Assert.Equal("USDT", grid.GridCreator.TradeAssetInPortfolio);
        Assert.Equal(2.2m, grid.StopAndProfit.ProfitValue);
        Assert.Equal(101.25m, grid.AutoStarter.AutoStartPrice);
        Assert.Equal(3, grid.ErrorsReaction.FailOpenOrdersCountToReaction);
        Assert.Equal(1.5m, grid.TrailingUp.TrailingUpStep);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithOnlyStopBySection_ShouldApplyPrimeAndTargetChildOnly()
    {
        TradeGrid grid = CreateBareGrid();
        grid.NonTradePeriods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        grid.StopBy.StopGridByMoveUpIsOn = false;
        grid.StopBy.StopGridByMoveUpValuePercent = 1.1m;
        grid.GridCreator.TradeAssetInPortfolio = "USDT";
        grid.StopAndProfit.ProfitValue = 2.2m;
        grid.AutoStarter.AutoStartPrice = 101.25m;
        grid.ErrorsReaction.FailOpenOrdersCountToReaction = 3;
        grid.TrailingUp.TrailingUpStep = 1.5m;

        string payload =
            "42@MarketMaking@CloseOnly@OncePerSecond@True@11@3@2@123.45@4@" +
            new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture) +
            "@700@False@1.5@False@@@" +
            "%%%" +
            "True@2.5@CloseOnly@False@1.5@Off@True@7@CloseForced@True@600@OffAndCancelOrders@True@9@30@45@CloseOnly@@@@@@" +
            "%%%%%";

        Exception? error = Record.Exception(() => grid.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(42, grid.Number);
        Assert.Equal(TradeGridPrimeType.MarketMaking, grid.GridType);
        Assert.Equal(TradeGridRegime.CloseOnly, grid.Regime);

        Assert.Equal(TradeGridRegime.CloseOnly, grid.NonTradePeriods.NonTradePeriod1Regime);

        Assert.True(grid.StopBy.StopGridByMoveUpIsOn);
        Assert.Equal(2.5m, grid.StopBy.StopGridByMoveUpValuePercent);
        Assert.Equal(TradeGridRegime.CloseOnly, grid.StopBy.StopGridByMoveUpReaction);

        Assert.Equal("USDT", grid.GridCreator.TradeAssetInPortfolio);
        Assert.Equal(2.2m, grid.StopAndProfit.ProfitValue);
        Assert.Equal(101.25m, grid.AutoStarter.AutoStartPrice);
        Assert.Equal(3, grid.ErrorsReaction.FailOpenOrdersCountToReaction);
        Assert.Equal(1.5m, grid.TrailingUp.TrailingUpStep);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_LoadFromString_WithOnlyGridCreatorSection_ShouldApplyPrimeAndTargetChildOnly()
    {
        TradeGrid grid = CreateBareGrid();
        grid.NonTradePeriods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        grid.StopBy.StopGridByMoveUpIsOn = true;
        grid.StopBy.StopGridByMoveUpValuePercent = 2.5m;
        grid.GridCreator.TradeAssetInPortfolio = "USDT";
        grid.StopAndProfit.ProfitValue = 2.2m;
        grid.AutoStarter.AutoStartPrice = 101.25m;
        grid.ErrorsReaction.FailOpenOrdersCountToReaction = 3;
        grid.TrailingUp.TrailingUpStep = 1.5m;

        string payload =
            "42@MarketMaking@CloseOnly@OncePerSecond@True@11@3@2@123.45@4@" +
            new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture) +
            "@700@False@1.5@False@@@" +
            "%%%%" +
            "Sell@123.45@3@Absolute@1.2@1.1@Percent@0.8@1.3@Contracts@2.5@1.4@AssetX@@@@@@" +
            "%%%%";

        Exception? error = Record.Exception(() => grid.LoadFromString(payload));

        Assert.Null(error);
        Assert.Equal(42, grid.Number);
        Assert.Equal(TradeGridPrimeType.MarketMaking, grid.GridType);
        Assert.Equal(TradeGridRegime.CloseOnly, grid.Regime);

        Assert.Equal(TradeGridRegime.CloseOnly, grid.NonTradePeriods.NonTradePeriod1Regime);
        Assert.True(grid.StopBy.StopGridByMoveUpIsOn);
        Assert.Equal(2.5m, grid.StopBy.StopGridByMoveUpValuePercent);

        Assert.Equal(Side.Sell, grid.GridCreator.GridSide);
        Assert.Equal(123.45m, grid.GridCreator.FirstPrice);
        Assert.Equal(3, grid.GridCreator.LineCountStart);
        Assert.Equal(TradeGridValueType.Absolute, grid.GridCreator.TypeStep);
        Assert.Equal(1.2m, grid.GridCreator.LineStep);
        Assert.Equal(1.1m, grid.GridCreator.StepMultiplicator);
        Assert.Equal(TradeGridValueType.Percent, grid.GridCreator.TypeProfit);
        Assert.Equal(0.8m, grid.GridCreator.ProfitStep);
        Assert.Equal(1.3m, grid.GridCreator.ProfitMultiplicator);
        Assert.Equal(TradeGridVolumeType.Contracts, grid.GridCreator.TypeVolume);
        Assert.Equal(2.5m, grid.GridCreator.StartVolume);
        Assert.Equal(1.4m, grid.GridCreator.MartingaleMultiplicator);
        Assert.Equal("AssetX", grid.GridCreator.TradeAssetInPortfolio);

        Assert.Equal(2.2m, grid.StopAndProfit.ProfitValue);
        Assert.Equal(101.25m, grid.AutoStarter.AutoStartPrice);
        Assert.Equal(3, grid.ErrorsReaction.FailOpenOrdersCountToReaction);
        Assert.Equal(1.5m, grid.TrailingUp.TrailingUpStep);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetSaveString_LoadFromString_ShouldRoundTrip()
    {
        TradeGrid source = CreateBareGrid();
        source.Number = 42;
        source.GridType = TradeGridPrimeType.MarketMaking;
        source.Regime = TradeGridRegime.CloseOnly;
        source.RegimeLogicEntry = TradeGridLogicEntryRegime.OncePerSecond;
        source.AutoClearJournalIsOn = true;
        source.MaxClosePositionsInJournal = 11;
        source.MaxOpenOrdersInMarket = 3;
        source.MaxCloseOrdersInMarket = 2;
        source.DelayInReal = 700;
        source.CheckMicroVolumes = false;
        source.MaxDistanceToOrdersPercent = 1.5m;
        source.OpenOrdersMakerOnly = false;

        SetPrivateField(source, "_firstTradePrice", 123.45m);
        SetPrivateField(source, "_openPositionsBySession", 4);
        DateTime firstTradeTime = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        SetPrivateField(source, "_firstTradeTime", firstTradeTime);

        source.NonTradePeriods.NonTradePeriod1Regime = TradeGridRegime.CloseOnly;
        source.NonTradePeriods.NonTradePeriod2Regime = TradeGridRegime.OffAndCancelOrders;

        source.StopBy.StopGridByMoveUpIsOn = true;
        source.StopBy.StopGridByMoveUpValuePercent = 2.5m;
        source.StopBy.StopGridByMoveUpReaction = TradeGridRegime.CloseOnly;

        source.GridCreator.GridSide = Side.Sell;
        source.GridCreator.FirstPrice = 123.45m;
        source.GridCreator.LineCountStart = 3;
        source.GridCreator.TypeStep = TradeGridValueType.Absolute;
        source.GridCreator.LineStep = 1.2m;
        source.GridCreator.StepMultiplicator = 1.1m;
        source.GridCreator.TypeProfit = TradeGridValueType.Percent;
        source.GridCreator.ProfitStep = 0.8m;
        source.GridCreator.ProfitMultiplicator = 1.3m;
        source.GridCreator.TypeVolume = TradeGridVolumeType.Contracts;
        source.GridCreator.StartVolume = 2.5m;
        source.GridCreator.MartingaleMultiplicator = 1.4m;
        source.GridCreator.TradeAssetInPortfolio = "AssetX";

        source.StopAndProfit.ProfitRegime = OnOffRegime.On;
        source.StopAndProfit.ProfitValueType = TradeGridValueType.Absolute;
        source.StopAndProfit.ProfitValue = 2.2m;

        source.AutoStarter.AutoStartRegime = TradeGridAutoStartRegime.HigherOrEqual;
        source.AutoStarter.AutoStartPrice = 101.25m;
        source.AutoStarter.RebuildGridRegime = GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice;

        source.ErrorsReaction.FailOpenOrdersReactionIsOn = false;
        source.ErrorsReaction.FailOpenOrdersCountToReaction = 3;
        source.ErrorsReaction.FailCancelOrdersCountToReaction = 4;

        source.TrailingUp.TrailingUpIsOn = true;
        source.TrailingUp.TrailingUpStep = 1.5m;
        source.TrailingUp.TrailingUpLimit = 110m;

        TradeGrid loaded = CreateBareGrid();

        Exception? error = Record.Exception(() => loaded.LoadFromString(source.GetSaveString()));

        Assert.Null(error);
        Assert.Equal(42, loaded.Number);
        Assert.Equal(TradeGridPrimeType.MarketMaking, loaded.GridType);
        Assert.Equal(TradeGridRegime.CloseOnly, loaded.Regime);
        Assert.Equal(TradeGridLogicEntryRegime.OncePerSecond, loaded.RegimeLogicEntry);
        Assert.True(loaded.AutoClearJournalIsOn);
        Assert.Equal(11, loaded.MaxClosePositionsInJournal);
        Assert.Equal(3, loaded.MaxOpenOrdersInMarket);
        Assert.Equal(2, loaded.MaxCloseOrdersInMarket);
        Assert.Equal(123.45m, loaded.FirstPriceReal);
        Assert.Equal(4, loaded.OpenPositionsCount);
        Assert.Equal(firstTradeTime, loaded.FirstTradeTime);
        Assert.Equal(700, loaded.DelayInReal);
        Assert.False(loaded.CheckMicroVolumes);
        Assert.Equal(1.5m, loaded.MaxDistanceToOrdersPercent);
        Assert.False(loaded.OpenOrdersMakerOnly);

        Assert.Equal(TradeGridRegime.CloseOnly, loaded.NonTradePeriods.NonTradePeriod1Regime);
        Assert.Equal(TradeGridRegime.OffAndCancelOrders, loaded.NonTradePeriods.NonTradePeriod2Regime);

        Assert.True(loaded.StopBy.StopGridByMoveUpIsOn);
        Assert.Equal(2.5m, loaded.StopBy.StopGridByMoveUpValuePercent);
        Assert.Equal(TradeGridRegime.CloseOnly, loaded.StopBy.StopGridByMoveUpReaction);

        Assert.Equal(Side.Sell, loaded.GridCreator.GridSide);
        Assert.Equal(123.45m, loaded.GridCreator.FirstPrice);
        Assert.Equal(3, loaded.GridCreator.LineCountStart);
        Assert.Equal(TradeGridValueType.Absolute, loaded.GridCreator.TypeStep);
        Assert.Equal(1.2m, loaded.GridCreator.LineStep);
        Assert.Equal(1.1m, loaded.GridCreator.StepMultiplicator);
        Assert.Equal(TradeGridValueType.Percent, loaded.GridCreator.TypeProfit);
        Assert.Equal(0.8m, loaded.GridCreator.ProfitStep);
        Assert.Equal(1.3m, loaded.GridCreator.ProfitMultiplicator);
        Assert.Equal(TradeGridVolumeType.Contracts, loaded.GridCreator.TypeVolume);
        Assert.Equal(2.5m, loaded.GridCreator.StartVolume);
        Assert.Equal(1.4m, loaded.GridCreator.MartingaleMultiplicator);
        Assert.Equal("AssetX", loaded.GridCreator.TradeAssetInPortfolio);

        Assert.Equal(OnOffRegime.On, loaded.StopAndProfit.ProfitRegime);
        Assert.Equal(TradeGridValueType.Absolute, loaded.StopAndProfit.ProfitValueType);
        Assert.Equal(2.2m, loaded.StopAndProfit.ProfitValue);

        Assert.Equal(TradeGridAutoStartRegime.HigherOrEqual, loaded.AutoStarter.AutoStartRegime);
        Assert.Equal(101.25m, loaded.AutoStarter.AutoStartPrice);
        Assert.Equal(GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice, loaded.AutoStarter.RebuildGridRegime);

        Assert.False(loaded.ErrorsReaction.FailOpenOrdersReactionIsOn);
        Assert.Equal(3, loaded.ErrorsReaction.FailOpenOrdersCountToReaction);
        Assert.Equal(4, loaded.ErrorsReaction.FailCancelOrdersCountToReaction);

        Assert.True(loaded.TrailingUp.TrailingUpIsOn);
        Assert.Equal(1.5m, loaded.TrailingUp.TrailingUpStep);
        Assert.Equal(110m, loaded.TrailingUp.TrailingUpLimit);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_Delete_WithInitializedComponents_ShouldClearReferencesAndStayIdempotent()
    {
        TradeGrid grid = CreateBareGrid();
        TradeGridErrorsReaction errorsReaction = grid.ErrorsReaction;
        TrailingUp trailingUp = grid.TrailingUp;
        grid.Tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));

        Exception? error = Record.Exception(() =>
        {
            grid.Delete();
            grid.Delete();
        });

        object? isDeleted = typeof(TradeGrid).GetField("_isDeleted", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(grid);
        object? errorsReactionGrid = typeof(TradeGridErrorsReaction).GetField("_myGrid", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(errorsReaction);
        object? trailingGrid = typeof(TrailingUp).GetField("_grid", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(trailingUp);

        Assert.Null(error);
        Assert.True((bool)(isDeleted ?? false));
        Assert.Null(grid.Tab);
        Assert.Null(grid.NonTradePeriods);
        Assert.Null(grid.StopBy);
        Assert.Null(grid.StopAndProfit);
        Assert.Null(grid.AutoStarter);
        Assert.Null(grid.GridCreator);
        Assert.Null(grid.ErrorsReaction);
        Assert.Null(grid.TrailingUp);
        Assert.Null(errorsReactionGrid);
        Assert.Null(trailingGrid);
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
    public void Stage2Step2_2_TradeGrid_HaveOrdersWithNoMarketOrders_WithFreshNoneOrder_ShouldPrimeTimestampAndReturnTrue()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();

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

        bool hasNoMarketOrders = grid.HaveOrdersWithNoMarketOrders();
        DateTime lastNoneOrderTime = (DateTime)(typeof(TradeGrid).GetField("_lastNoneOrderTime", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(grid)
            ?? throw new InvalidOperationException("_lastNoneOrderTime not found."));

        Assert.True(hasNoMarketOrders);
        Assert.NotEqual(DateTime.MinValue, lastNoneOrderTime);
        Assert.Single(position.OpenOrders);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_HaveOrdersWithNoMarketOrders_WithoutNoneOrders_ShouldResetTimestampAndReturnFalse()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(position, "_openOrders", new List<Order>
        {
            new Order
            {
                State = OrderStateType.Active,
                NumberMarket = "mk-1"
            }
        });
        SetPrivateField(position, "_closeOrders", new List<Order>());

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { Position = position, PositionNum = 1 }
        };

        SetPrivateField(grid, "_lastNoneOrderTime", DateTime.Now.AddMinutes(-1));

        bool hasNoMarketOrders = grid.HaveOrdersWithNoMarketOrders();
        DateTime lastNoneOrderTime = (DateTime)(typeof(TradeGrid).GetField("_lastNoneOrderTime", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(grid)
            ?? throw new InvalidOperationException("_lastNoneOrderTime not found."));

        Assert.False(hasNoMarketOrders);
        Assert.Equal(DateTime.MinValue, lastNoneOrderTime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_HaveOrdersTryToCancelLastSecond_WithRecentOpenCancel_ShouldReturnTrue()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(position, "_openOrders", new List<Order>
        {
            new Order
            {
                State = OrderStateType.Active,
                IsSendToCancel = true,
                LastCancelTryLocalTime = DateTime.Now.AddSeconds(-1)
            }
        });
        SetPrivateField(position, "_closeOrders", new List<Order>());

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { Position = position, PositionNum = 1 }
        };

        bool hasRecentCancel = grid.HaveOrdersTryToCancelLastSecond();

        Assert.True(hasRecentCancel);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_HaveOrdersTryToCancelLastSecond_WithExpiredCloseCancel_ShouldReturnFalse()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(position, "_openOrders", new List<Order>());
        SetPrivateField(position, "_closeOrders", new List<Order>
        {
            new Order
            {
                State = OrderStateType.Active,
                IsSendToCancel = true,
                LastCancelTryLocalTime = DateTime.Now.AddSeconds(-4)
            }
        });

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine { Position = position, PositionNum = 1 }
        };

        bool hasRecentCancel = grid.HaveOrdersTryToCancelLastSecond();

        Assert.False(hasRecentCancel);
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
    public void Stage2Step2_2_TradeGrid_GetLinesWithOpenOrdersNeed_WithEligibleBuyLine_ShouldReturnLine()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.GridSide = Side.Buy;
        grid.MaxOpenOrdersInMarket = 1;
        grid.OpenOrdersMakerOnly = false;

        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridOpenNeed", StartProgram.IsTester, false);
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
            new Order { State = OrderStateType.Active, VolumeExecute = 0m }
        });
        SetPrivateField(position, "_closeOrders", new List<Order>());

        TradeGridLine line = new TradeGridLine
        {
            Side = Side.Buy,
            PriceEnter = 100m,
            Position = position,
            PositionNum = 1
        };
        grid.GridCreator.Lines = new List<TradeGridLine> { line };

        List<TradeGridLine> openNeed = grid.GetLinesWithOpenOrdersNeed(100m);

        Assert.Single(openNeed);
        Assert.Same(line, openNeed[0]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetLinesWithOpenPosition_WithSparseLines_ShouldReturnOnlyOpenVolumeLines()
    {
        TradeGrid grid = CreateBareGrid();

        Position openPosition = new Position();
        openPosition.AddNewOpenOrder(new Order
        {
            State = OrderStateType.Done,
            Volume = 1m,
            VolumeExecute = 1m
        });

        Position flatPosition = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(flatPosition, "_openOrders", new List<Order>());
        SetPrivateField(flatPosition, "_closeOrders", new List<Order>());

        TradeGridLine openLine = new TradeGridLine
        {
            Position = openPosition,
            PositionNum = 1
        };

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine
            {
                Position = flatPosition,
                PositionNum = 2
            },
            openLine
        };

        List<TradeGridLine> openPositions = grid.GetLinesWithOpenPosition();

        Assert.Single(openPositions);
        Assert.Same(openLine, openPositions[0]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetPositionByGrid_WithSparseLines_ShouldReturnOnlyExistingPositions()
    {
        TradeGrid grid = CreateBareGrid();
        Position firstPosition = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        Position secondPosition = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine
            {
                Position = null
            },
            new TradeGridLine
            {
                Position = firstPosition
            },
            new TradeGridLine
            {
                Position = secondPosition
            }
        };

        List<Position> positions = grid.GetPositionByGrid();

        Assert.Equal(2, positions.Count);
        Assert.Same(firstPosition, positions[0]);
        Assert.Same(secondPosition, positions[1]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetLinesWithClosingOrdersFact_WithActiveCloseLine_ShouldReturnLine()
    {
        TradeGrid grid = CreateBareGrid();

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(position, "_openOrders", new List<Order>
        {
            new Order { State = OrderStateType.Done, VolumeExecute = 1m }
        });
        SetPrivateField(position, "_closeOrders", new List<Order>
        {
            new Order { State = OrderStateType.Active, VolumeExecute = 0m }
        });

        TradeGridLine line = new TradeGridLine
        {
            Position = position,
            PositionNum = 1
        };
        grid.GridCreator.Lines = new List<TradeGridLine> { line };

        List<TradeGridLine> closeFact = grid.GetLinesWithClosingOrdersFact();

        Assert.Single(closeFact);
        Assert.Same(line, closeFact[0]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_OpenPositionsCount_ShouldReturnBackingField()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        SetPrivateField(grid, "_openPositionsBySession", 7);

        int openPositionsCount = grid.OpenPositionsCount;

        Assert.Equal(7, openPositionsCount);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_FirstPriceReal_ShouldReturnBackingField()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        SetPrivateField(grid, "_firstTradePrice", 123.45m);

        decimal firstPriceReal = grid.FirstPriceReal;

        Assert.Equal(123.45m, firstPriceReal);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_FirstTradeTime_ShouldReturnBackingField()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        DateTime expected = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        SetPrivateField(grid, "_firstTradeTime", expected);

        DateTime firstTradeTime = grid.FirstTradeTime;

        Assert.Equal(expected, firstTradeTime);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_Regime_SetSameValue_ShouldNotRaiseRepaintEvents()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        SetPrivateField(grid, "_regime", TradeGridRegime.On);
        int fullRepaintCalls = 0;
        int settingsRepaintCalls = 0;
        grid.FullRePaintGridEvent += () => fullRepaintCalls++;
        grid.RePaintSettingsEvent += () => settingsRepaintCalls++;

        grid.Regime = TradeGridRegime.On;

        Assert.Equal(0, fullRepaintCalls);
        Assert.Equal(0, settingsRepaintCalls);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_Regime_SetNewValue_ShouldRaiseRepaintEventsOnce()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        SetPrivateField(grid, "_regime", TradeGridRegime.Off);
        int fullRepaintCalls = 0;
        int settingsRepaintCalls = 0;
        grid.FullRePaintGridEvent += () => fullRepaintCalls++;
        grid.RePaintSettingsEvent += () => settingsRepaintCalls++;

        grid.Regime = TradeGridRegime.On;

        Assert.Equal(TradeGridRegime.On, grid.Regime);
        Assert.Equal(1, fullRepaintCalls);
        Assert.Equal(1, settingsRepaintCalls);
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
    public void Stage2Step2_2_TradeGrid_MiddleEntryPrice_WithWeightedTrades_ShouldReturnAveragePrice()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();

        Order openOrder = new Order();
        SetPrivateField(openOrder, "_trades", new List<MyTrade>
        {
            new MyTrade
            {
                Volume = 1m,
                Price = 100m
            },
            new MyTrade
            {
                Volume = 2m,
                Price = 103m
            }
        });

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
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

        decimal middleEntryPrice = grid.MiddleEntryPrice;

        Assert.Equal((1m * 100m + 2m * 103m) / 3m, middleEntryPrice);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_MiddleEntryPrice_WithSparseOrders_ShouldIgnoreNullsAndEmptyTrades()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();

        Order openOrder = new Order();
        SetPrivateField(openOrder, "_trades", new List<MyTrade>
        {
            null!,
            new MyTrade
            {
                Volume = 2m,
                Price = 101m
            }
        });

        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(position, "_openOrders", new List<Order>
        {
            null!,
            new Order(),
            openOrder
        });
        SetPrivateField(position, "_closeOrders", new List<Order>());

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine
            {
                Position = position,
                PositionNum = 1
            }
        };

        decimal middleEntryPrice = grid.MiddleEntryPrice;

        Assert.Equal(101m, middleEntryPrice);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_OpenVolumeByLines_WithMultipleOpenPositions_ShouldSumOpenVolume()
    {
        TradeGrid grid = CreateBareGrid();

        Position firstPosition = new Position();
        firstPosition.AddNewOpenOrder(new Order
        {
            State = OrderStateType.Done,
            Volume = 1m,
            VolumeExecute = 1m
        });

        Position secondPosition = new Position();
        secondPosition.AddNewOpenOrder(new Order
        {
            State = OrderStateType.Done,
            Volume = 2m,
            VolumeExecute = 2m
        });

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine
            {
                Position = firstPosition
            },
            new TradeGridLine
            {
                Position = secondPosition
            }
        };

        decimal openVolumeByLines = grid.OpenVolumeByLines;

        Assert.Equal(3m, openVolumeByLines);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_AllVolumeInLines_WithSparseLines_ShouldSumLineVolumes()
    {
        TradeGrid grid = CreateBareGrid();
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            null!,
            new TradeGridLine
            {
                Volume = 1.25m
            },
            new TradeGridLine
            {
                Volume = 2.75m
            }
        };

        decimal allVolumeInLines = grid.AllVolumeInLines;

        Assert.Equal(4m, allVolumeInLines);
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
    public void Stage2Step2_2_TradeGrid_GetOrdersBadLinesMaxCount_WithNullOpenLines_ShouldReturnEmpty()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.MaxOpenOrdersInMarket = 1;

        MethodInfo method = typeof(TradeGrid).GetMethod("GetOrdersBadLinesMaxCount", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetOrdersBadLinesMaxCount not found.");

        List<Order> orders = (List<Order>)(method.Invoke(grid, null)
            ?? throw new InvalidOperationException("GetOrdersBadLinesMaxCount returned null."));

        Assert.Empty(orders);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetCloseOrdersGridHole_WithNullOpenPositions_ShouldReturnEmpty()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.MaxCloseOrdersInMarket = 1;

        MethodInfo method = typeof(TradeGrid).GetMethod("GetCloseOrdersGridHole", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetCloseOrdersGridHole not found.");

        List<Order> orders = (List<Order>)(method.Invoke(grid, null)
            ?? throw new InvalidOperationException("GetCloseOrdersGridHole returned null."));

        Assert.Empty(orders);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetOrdersBadLinesMaxCount_WithNegativeLimit_ShouldTreatAsZero()
    {
        TradeGrid grid = CreateBareGrid();
        Order openOrder = new Order
        {
            NumberUser = 101,
            State = OrderStateType.Active,
            Volume = 1,
            VolumeExecute = 1
        };
        Position position = new Position();
        position.AddNewOpenOrder(openOrder);
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine
            {
                Position = position
            }
        };
        grid.MaxOpenOrdersInMarket = -1;

        MethodInfo method = typeof(TradeGrid).GetMethod("GetOrdersBadLinesMaxCount", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetOrdersBadLinesMaxCount not found.");

        List<Order> orders = (List<Order>)(method.Invoke(grid, null)
            ?? throw new InvalidOperationException("GetOrdersBadLinesMaxCount returned null."));

        Assert.Single(orders);
        Assert.Same(openOrder, orders[0]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetCloseOrdersGridHole_WithNegativeLimit_ShouldTreatAsZero()
    {
        TradeGrid grid = CreateBareGrid();
        Order openOrder = new Order
        {
            NumberUser = 201,
            State = OrderStateType.Done,
            Volume = 1,
            VolumeExecute = 1
        };
        Order closeOrder = new Order
        {
            NumberUser = 202,
            State = OrderStateType.Active,
            Volume = 1,
            VolumeExecute = 0
        };
        Position position = new Position();
        position.AddNewOpenOrder(openOrder);
        position.AddNewCloseOrder(closeOrder);
        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            new TradeGridLine
            {
                Position = position
            }
        };
        grid.MaxCloseOrdersInMarket = -1;

        MethodInfo method = typeof(TradeGrid).GetMethod("GetCloseOrdersGridHole", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetCloseOrdersGridHole not found.");

        List<Order> orders = (List<Order>)(method.Invoke(grid, null)
            ?? throw new InvalidOperationException("GetCloseOrdersGridHole returned null."));

        Assert.Single(orders);
        Assert.Same(closeOrder, orders[0]);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetOrdersBadPriceToGrid_WithNullOrderLines_ShouldReturnEmpty()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        MethodInfo method = typeof(TradeGrid).GetMethod("GetOrdersBadPriceToGrid", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetOrdersBadPriceToGrid not found.");

        List<Order> orders = (List<Order>)(method.Invoke(grid, null)
            ?? throw new InvalidOperationException("GetOrdersBadPriceToGrid returned null."));

        Assert.Empty(orders);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetOpenOrdersGridHole_WithNullTab_ShouldReturnNull()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.GridCreator = new TradeGridCreator();

        MethodInfo method = typeof(TradeGrid).GetMethod("GetOpenOrdersGridHole", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetOpenOrdersGridHole not found.");

        object? result = method.Invoke(grid, null);

        Assert.Null(result);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_GetOpenOrdersGridHole_WithNullGridCreator_ShouldReturnNull()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.Tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));

        MethodInfo method = typeof(TradeGrid).GetMethod("GetOpenOrdersGridHole", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetOpenOrdersGridHole not found.");

        object? result = method.Invoke(grid, null);

        Assert.Null(result);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_TryRemoveWrongOrders_WithNullDependencies_ShouldReturnZero()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        MethodInfo method = typeof(TradeGrid).GetMethod("TryRemoveWrongOrders", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method TryRemoveWrongOrders not found.");

        int cancelledOrders = (int)(method.Invoke(grid, null)
            ?? throw new InvalidOperationException("TryRemoveWrongOrders returned null."));

        Assert.Equal(0, cancelledOrders);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_TryCancelOpeningOrders_WithNullTab_ShouldReturnZero()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        MethodInfo method = typeof(TradeGrid).GetMethod("TryCancelOpeningOrders", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method TryCancelOpeningOrders not found.");

        int cancelledOrders = (int)(method.Invoke(grid, null)
            ?? throw new InvalidOperationException("TryCancelOpeningOrders returned null."));

        Assert.Equal(0, cancelledOrders);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_TryCancelClosingOrders_WithNullTab_ShouldReturnZero()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));

        MethodInfo method = typeof(TradeGrid).GetMethod("TryCancelClosingOrders", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method TryCancelClosingOrders not found.");

        int cancelledOrders = (int)(method.Invoke(grid, null)
            ?? throw new InvalidOperationException("TryCancelClosingOrders returned null."));

        Assert.Equal(0, cancelledOrders);
    }

    [Fact]
    public void Stage2Step2_2_TradeGrid_TrySetClosingOrders_WithNullSecurity_ShouldStayNoOp()
    {
        TradeGrid grid = CreateBareGrid();
        grid.Tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));

        Exception? error = Record.Exception(() => InvokePrivateWithArgs(grid, "TrySetClosingOrders", 100m));

        Assert.Null(error);
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

    private static void AttachSingleCloseCandle(TradeGrid grid, decimal close)
    {
        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        ConnectorCandles connector = new ConnectorCandles("CodexGridTrailingRuntime", StartProgram.IsTester, false);
        TimeFrameBuilder builder = new TimeFrameBuilder("CodexGridTrailingRuntime", StartProgram.IsTester);
        CandleSeries series = new CandleSeries(builder, new Security(), StartProgram.IsTester)
        {
            CandlesAll = new List<Candle> { new Candle { Close = close } }
        };

        SetPrivateField(connector, "_mySeries", series);
        SetPrivateField(tab, "_connector", connector);
        grid.Tab = tab;
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
