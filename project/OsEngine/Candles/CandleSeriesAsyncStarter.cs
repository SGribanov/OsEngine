#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Market.Servers.Entity;
using System;
using System.Threading.Tasks;
using OsEngine.Entity;

namespace OsEngine.Candles
{
    public class CandleSeriesAsyncStarter
    {
        public CandleSeriesAsyncStarter(int rateGateLimitMls)
        {
            if (rateGateLimitMls < 0)
            {
                rateGateLimitMls = 0;
            }

            if (rateGateLimitMls > 0)
            {
                _rateGate = new RateGate(1, TimeSpan.FromMilliseconds(rateGateLimitMls));
            }
        }

        private RateGate _rateGate;

        public void StartAsync(CandleSeries series)
        {
            if (_rateGate != null)
            {
                _rateGate.WaitToProceed();
            }

            Task.Run(() => StartSeriesEvent(series));
        }

        public event Action<CandleSeries> StartSeriesEvent;




    }
}

