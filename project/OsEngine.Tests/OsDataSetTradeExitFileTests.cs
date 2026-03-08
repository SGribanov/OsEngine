#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using OsEngine.Entity;
using OsEngine.OsData;
using Xunit;

namespace OsEngine.Tests;

public class OsDataSetTradeExitFileTests
{
    [Fact]
    public void TryBuildTradeExitFileLines_ShouldKeepTradeOrder_AndSaveStringFormat()
    {
        List<List<Trade>> tradeBlocks = new List<List<Trade>>
        {
            new List<Trade>
            {
                CreateTrade(new DateTime(2026, 3, 8, 9, 15, 30), "1", "first"),
                CreateTrade(new DateTime(2026, 3, 8, 9, 15, 31), "2", "second")
            },
            new List<Trade>
            {
                CreateTrade(new DateTime(2026, 3, 8, 9, 15, 32), "3", "third")
            }
        };

        bool result = SecurityTfLoader.TryBuildTradeExitFileLines(tradeBlocks, out List<string> lines);

        Assert.True(result);
        Assert.Equal(3, lines.Count);
        Assert.Equal(tradeBlocks[0][0].GetSaveString(), lines[0]);
        Assert.Equal(tradeBlocks[0][1].GetSaveString(), lines[1]);
        Assert.Equal(tradeBlocks[1][0].GetSaveString(), lines[2]);
    }

    [Fact]
    public void TryBuildTradeExitFileLines_ShouldRejectOutOfOrderPies_AndReturnNoLines()
    {
        List<List<Trade>> tradeBlocks = new List<List<Trade>>
        {
            new List<Trade>
            {
                CreateTrade(new DateTime(2026, 3, 8, 9, 15, 31), "1", "first")
            },
            new List<Trade>
            {
                CreateTrade(new DateTime(2026, 3, 8, 9, 15, 30), "2", "second")
            }
        };

        bool result = SecurityTfLoader.TryBuildTradeExitFileLines(tradeBlocks, out List<string> lines);

        Assert.False(result);
        Assert.Empty(lines);
    }

    private static Trade CreateTrade(DateTime time, string id, string securityNameCode)
    {
        return new Trade
        {
            Time = time,
            Id = id,
            SecurityNameCode = securityNameCode,
            Price = 123.45m,
            Volume = 7,
            Side = Side.Buy
        };
    }
}
