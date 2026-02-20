#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using OsEngine.Entity;
using System.Collections.Generic;

namespace OsEngine.Indicators.Samples
{
    [Indicator("Sample3IndicatorDataSeries")]
    public class Sample3IndicatorDataSeries : Aindicator
    {
        public IndicatorDataSeries Series1;

        public override void OnStateChange(IndicatorState state)
        {
            if(state == IndicatorState.Configure)
            {

                Series1 = CreateSeries("Series 1", System.Drawing.Color.AliceBlue, IndicatorChartPaintType.Line, true);
            
            }
        }

        public override void OnProcess(List<Candle> source, int index)
        {

            Series1.Values[index] = source[index].Center;

        }
    }
}

