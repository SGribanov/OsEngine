#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using OsEngine.Entity;
using System;
using System.Threading;

namespace OsEngine.Market.Servers.Plaza.Entity
{
    /// <summary>
    /// this class creates a loop to delay the transfer position at the start of the server, because sometimes the portfolio position comes before the portfolio itself
    /// этот класс создаёт петлю для задержки передачи позиции на старте сервера, 
    /// т.к. иногда позиция по портфелю приходит раньше самого портфеля
    /// </summary>
    public class PositionOnBoardSander
    {
        public PositionOnBoard PositionOnBoard;

        public void Go()
        {
            Thread.Sleep(5000);
            if (TimeSendPortfolio != null)
            {
                TimeSendPortfolio(PositionOnBoard);
            }
        }

        public event Action<PositionOnBoard> TimeSendPortfolio;
    }
}

