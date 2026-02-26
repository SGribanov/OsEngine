#nullable enable

using System.Linq;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class PortfolioCoreTests
{
    [Fact]
    public void SetNewPosition_ShouldIgnore_WhenSecurityNameIsEmpty()
    {
        Portfolio portfolio = new Portfolio { Number = "P1" };

        portfolio.SetNewPosition(new PositionOnBoard
        {
            SecurityNameCode = "",
            ValueCurrent = 1m
        });

        Assert.Null(portfolio.PositionOnBoard);
    }

    [Fact]
    public void SetNewPosition_ShouldSetPortfolioName_WhenMissing()
    {
        Portfolio portfolio = new Portfolio { Number = "P_MAIN" };
        PositionOnBoard position = new PositionOnBoard
        {
            SecurityNameCode = "AAPL",
            PortfolioName = "",
            ValueCurrent = 5m
        };

        portfolio.SetNewPosition(position);

        Assert.NotNull(portfolio.PositionOnBoard);
        Assert.Single(portfolio.PositionOnBoard!);
        Assert.Equal("P_MAIN", portfolio.PositionOnBoard![0].PortfolioName);
    }

    [Fact]
    public void SetNewPosition_ShouldUpdateExistingPosition()
    {
        Portfolio portfolio = new Portfolio { Number = "P1" };

        portfolio.SetNewPosition(new PositionOnBoard
        {
            SecurityNameCode = "BTCUSDT",
            ValueCurrent = 1m,
            ValueBlocked = 2m,
            UnrealizedPnl = 3m
        });

        portfolio.SetNewPosition(new PositionOnBoard
        {
            SecurityNameCode = "BTCUSDT",
            ValueCurrent = 10m,
            ValueBlocked = 20m,
            UnrealizedPnl = 30m
        });

        Assert.NotNull(portfolio.PositionOnBoard);
        Assert.Single(portfolio.PositionOnBoard!);
        Assert.Equal(10m, portfolio.PositionOnBoard![0].ValueCurrent);
        Assert.Equal(20m, portfolio.PositionOnBoard![0].ValueBlocked);
        Assert.Equal(30m, portfolio.PositionOnBoard![0].UnrealizedPnl);
    }

    [Fact]
    public void SetNewPosition_ShouldKeepCashTickersFirst_AndSortOthersByName()
    {
        Portfolio portfolio = new Portfolio { Number = "P1" };

        portfolio.SetNewPosition(new PositionOnBoard { SecurityNameCode = "ZETA", ValueCurrent = 1m });
        portfolio.SetNewPosition(new PositionOnBoard { SecurityNameCode = "ALFA", ValueCurrent = 1m });
        portfolio.SetNewPosition(new PositionOnBoard { SecurityNameCode = "USD", ValueCurrent = 1m });
        portfolio.SetNewPosition(new PositionOnBoard { SecurityNameCode = "BETA", ValueCurrent = 1m });

        Assert.NotNull(portfolio.PositionOnBoard);
        string[] names = portfolio.PositionOnBoard!.Select(x => x.SecurityNameCode).ToArray();

        Assert.Equal("USD", names[0]);
        Assert.Equal("ALFA", names[1]);
        Assert.Equal("BETA", names[2]);
        Assert.Equal("ZETA", names[3]);
    }

    [Fact]
    public void ClearPositionOnBoard_ShouldResetCollection()
    {
        Portfolio portfolio = new Portfolio { Number = "P1" };
        portfolio.SetNewPosition(new PositionOnBoard { SecurityNameCode = "A", ValueCurrent = 1m });

        portfolio.ClearPositionOnBoard();

        Assert.NotNull(portfolio.PositionOnBoard);
        Assert.Empty(portfolio.PositionOnBoard!);
    }
}
