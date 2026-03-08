#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Market.Connectors;
using OsEngine.Market.Servers;
using OsEngine.Market.Servers.Tester;
using OsEngine.OsTrader.Grids;
using OsEngine.OsTrader.Panels.Tab;

namespace OsEngine.Tests;

public class TradeGridCloseSideCollectionTests
{
    [Fact]
    public void Stage2P1_TradeGrid_CollectTailOpenPositions_ShouldReturnLastOpenLinesInOriginalOrder()
    {
        TradeGrid grid = CreateBareGrid();
        BotTabSimple tab = CreateTabForCollection(StartProgram.IsTester);

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            CreateOpenLine(1, 1m),
            CreateOpenLine(2, 2m),
            CreateFlatLine(3),
            CreateOpenLine(4, 4m),
            CreateOpenLine(5, 5m)
        };

        List<TradeGridLine> tail = InvokeTailCollector(grid, tab, 2);

        Assert.Equal(new[] { 4, 5 }, tail.ConvertAll(line => line.PositionNum));
    }

    [Fact]
    public void Stage2P1_TradeGrid_CollectTailOpenPositions_ShouldPreserveWrongCloseOrderChecksOutsideTail()
    {
        TradeGrid grid = CreateBareGrid();
        BotTabSimple tab = CreateTabForCollection(StartProgram.IsOsTrader);

        Order wrongCloseOrder = CreateCloseOrder(101, 4m, 0m, OrderStateType.Active);
        TradeGridLine earlyWrongLine = CreateOpenLine(1, 5m, wrongCloseOrder);
        TradeGridLine middleLine = CreateOpenLine(2, 2m);
        TradeGridLine tailLineA = CreateOpenLine(3, 3m);
        TradeGridLine tailLineB = CreateOpenLine(4, 4m);

        grid.GridCreator.Lines = new List<TradeGridLine>
        {
            earlyWrongLine,
            middleLine,
            tailLineA,
            tailLineB
        };

        TesterServer testerServer = AttachTesterServerForCancellation(tab, wrongCloseOrder);

        List<TradeGridLine> tail = InvokeTailCollector(grid, tab, 2);

        Assert.Equal(new[] { 3, 4 }, tail.ConvertAll(line => line.PositionNum));
        Assert.Equal(OrderStateType.Cancel, wrongCloseOrder.State);
        Assert.Empty(GetPrivateField<List<Order>>(testerServer, "OrdersActive"));
    }

    private static TradeGrid CreateBareGrid()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.NonTradePeriods = new TradeGridNonTradePeriods("CodexGridCloseSide");
        grid.StopBy = new TradeGridStopBy();
        grid.StopAndProfit = new TradeGridStopAndProfit();
        grid.AutoStarter = new TradeGridAutoStarter();
        grid.GridCreator = new TradeGridCreator();
        grid.ErrorsReaction = new TradeGridErrorsReaction(grid);
        grid.TrailingUp = new TrailingUp(grid);
        return grid;
    }

    private static BotTabSimple CreateTabForCollection(StartProgram startProgram)
    {
        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        tab.StartProgram = startProgram;

        ConnectorCandles connector = (ConnectorCandles)RuntimeHelpers.GetUninitializedObject(typeof(ConnectorCandles));
        SetPrivateField(tab, "_connector", connector);

        return tab;
    }

    private static TesterServer AttachTesterServerForCancellation(BotTabSimple tab, Order activeOrder)
    {
        ConnectorCandles connector = GetPrivateField<ConnectorCandles>(tab, "_connector");
        TesterServer server = (TesterServer)RuntimeHelpers.GetUninitializedObject(typeof(TesterServer));

        SetPrivateField(server, "_serverConnectStatus", ServerConnectStatus.Connect);
        SetPrivateField(server, "_serverTime", DateTime.UtcNow);
        SetPrivateField(server, "OrdersActive", new List<Order> { activeOrder });

        SetPrivateField(connector, "_myServer", server);

        return server;
    }

    private static TradeGridLine CreateOpenLine(int positionNum, decimal openVolume, Order? closeOrder = null)
    {
        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(position, "_openOrders", new List<Order> { CreateOpenOrder(positionNum, openVolume) });
        SetPrivateField(position, "_closeOrders", closeOrder == null ? new List<Order>() : new List<Order> { closeOrder });

        return new TradeGridLine
        {
            PositionNum = positionNum,
            Position = position
        };
    }

    private static TradeGridLine CreateFlatLine(int positionNum)
    {
        Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
        SetPrivateField(position, "_openOrders", new List<Order>());
        SetPrivateField(position, "_closeOrders", new List<Order>());

        return new TradeGridLine
        {
            PositionNum = positionNum,
            Position = position
        };
    }

    private static Order CreateOpenOrder(int numberUser, decimal volumeExecute)
    {
        return new Order
        {
            NumberUser = numberUser,
            Volume = volumeExecute,
            VolumeExecute = volumeExecute,
            State = OrderStateType.Done,
            SecurityNameCode = "TEST",
            PortfolioNumber = "PF"
        };
    }

    private static Order CreateCloseOrder(int numberUser, decimal volume, decimal volumeExecute, OrderStateType state)
    {
        return new Order
        {
            NumberUser = numberUser,
            NumberMarket = "M" + numberUser,
            Volume = volume,
            VolumeExecute = volumeExecute,
            State = state,
            SecurityNameCode = "TEST",
            PortfolioNumber = "PF"
        };
    }

    private static List<TradeGridLine> InvokeTailCollector(TradeGrid grid, BotTabSimple tab, int keepLastOpenPositionsCount)
    {
        MethodInfo method = typeof(TradeGrid).GetMethod(
            "CollectTailOpenPositionsAndCheckWrongCloseOrders",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("CollectTailOpenPositionsAndCheckWrongCloseOrders not found.");

        return (List<TradeGridLine>)method.Invoke(grid, new object[] { tab, keepLastOpenPositionsCount })!;
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Field " + fieldName + " not found.");

        return (T)field.GetValue(target)!;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Field " + fieldName + " not found.");

        field.SetValue(target, value);
    }
}
