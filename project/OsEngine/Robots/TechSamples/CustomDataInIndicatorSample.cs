#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Entity;
using OsEngine.Indicators;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Attributes;
using System.Collections.Generic;
using OsEngine.Language;


/* Description
TechSample robot for OsEngine

An example of drawing a series of indicator data calculated in the robot.
 */

namespace OsEngine.Robots.TechSamples
{
    [Bot("CustomDataInIndicatorSample")] // We create an attribute so that we don't write anything to the BotFactory
    public class CustomDataInIndicatorSample : BotPanel
    {
        // Simple tab
        private BotTabSimple _tab;

        // Internal calculation setting
        private StrategyParameterInt _customAverageLength;

        // Indicator
        private Aindicator _indicatorEmpty;

        public CustomDataInIndicatorSample(string name, StartProgram startProgram) : base(name, startProgram)
        {
            // Create Simple tabs
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            // Subscribe to the candle finished event
            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;

            _customAverageLength = CreateParameter("Custom average length", 20, 2, 400, 1, "Base");

            // Create indicator EmptyIndicator
            _indicatorEmpty = IndicatorsFactory.CreateIndicatorByName("EmptyIndicator", name + "EmptyIndicator", false);
            _indicatorEmpty = (Aindicator)_tab.CreateCandleIndicator(_indicatorEmpty, "SecondArea");

            Description = OsLocalization.Description.DescriptionLabel102;
        }

        // Candle finished event
        private void _tab_CandleFinishedEvent(List<Candle> candles)
        {
            if (candles == null || candles.Count == 0)
            {
                return;
            }

            decimal dataPoint = GetCachedHalfCloseAverage(candles);

            _indicatorEmpty.DataSeries[0].Values[_indicatorEmpty.DataSeries[0].Values.Count-1] = dataPoint;
            _indicatorEmpty.RePaint();
        }

        private decimal GetCachedHalfCloseAverage(List<Candle> candles)
        {
            int actualLength = _customAverageLength.ValueInt;

            if (actualLength > candles.Count)
            {
                actualLength = candles.Count;
            }

            string parametersHash = BuildOptimizerMethodCacheParameterHash(actualLength);

            // During optimizer runs this calculation is memoized by candle window + method parameters.
            return GetOrCreateOptimizerMethodCacheValue(
                _tab,
                "CustomDataInIndicatorSample.HalfCloseAverage",
                parametersHash,
                candles,
                () =>
                {
                    decimal sum = 0;
                    int startIndex = candles.Count - actualLength;

                    for (int i = startIndex; i < candles.Count; i++)
                    {
                        sum += candles[i].Close / 2m;
                    }

                    return sum / actualLength;
                });
        }

        // The name of the robot in OsEngine
        public override string GetNameStrategyType()
        {
            return "CustomDataInIndicatorSample";
        }

        // Show settings GUI
        public override void ShowIndividualSettingsDialog()
        {

        }
    }
}
