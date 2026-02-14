/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using OsEngine.Language;
using OsEngine.Logging;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Calculates optimization phases (InSample/OutOfSample) from time range and iteration settings.
    /// Вычисляет фазы оптимизации (InSample/OutOfSample) из временного диапазона и настроек итераций.
    /// </summary>
    public class PhaseCalculator
    {
        public List<OptimizerFaze> CalculatePhases(
            DateTime timeStart,
            DateTime timeEnd,
            int iterationCount,
            decimal percentOnFiltration,
            bool lastInSample)
        {
            int fazeCount = iterationCount;

            if (fazeCount < 1)
            {
                fazeCount = 1;
            }

            if (timeEnd == DateTime.MinValue || timeStart == DateTime.MinValue)
            {
                SendLogMessage(OsLocalization.Optimizer.Message12, LogMessageType.System);
                return null;
            }

            int dayAll = Convert.ToInt32((timeEnd - timeStart).TotalDays);

            if (dayAll < 2)
            {
                SendLogMessage(OsLocalization.Optimizer.Message12, LogMessageType.System);
                return null;
            }

            int daysOnInSample = (int)GetInSampleRecurs(dayAll, fazeCount, lastInSample, dayAll, percentOnFiltration);

            int daysOnForward = Convert.ToInt32(daysOnInSample * (percentOnFiltration / 100));

            List<OptimizerFaze> fazes = new List<OptimizerFaze>();

            DateTime time = timeStart;

            for (int i = 0; i < fazeCount; i++)
            {
                OptimizerFaze newFaze = new OptimizerFaze();
                newFaze.TypeFaze = OptimizerFazeType.InSample;
                newFaze.TimeStart = time;
                newFaze.TimeEnd = time.AddDays(daysOnInSample);
                time = time.AddDays(daysOnForward);
                newFaze.Days = daysOnInSample;
                fazes.Add(newFaze);

                if (lastInSample && i + 1 == fazeCount)
                {
                    newFaze.Days = daysOnInSample;
                    break;
                }

                OptimizerFaze newFazeOut = new OptimizerFaze();
                newFazeOut.TypeFaze = OptimizerFazeType.OutOfSample;
                newFazeOut.TimeStart = newFaze.TimeStart.AddDays(daysOnInSample);
                newFazeOut.TimeEnd = newFazeOut.TimeStart.AddDays(daysOnForward);
                newFazeOut.TimeStart = newFazeOut.TimeStart.AddDays(1);
                newFazeOut.Days = daysOnForward;
                fazes.Add(newFazeOut);
            }

            for (int i = 0; i < fazes.Count; i++)
            {
                if (fazes[i].Days <= 0)
                {
                    SendLogMessage(OsLocalization.Optimizer.Label50, LogMessageType.Error);
                    return new List<OptimizerFaze>();
                }
            }

            return fazes;
        }

        private decimal GetInSampleRecurs(decimal curLengthInSample, int fazeCount, bool lastInSample, int allDays, decimal percentOnFiltration)
        {
            decimal outOfSampleLength = curLengthInSample * (percentOnFiltration / 100);

            int count = fazeCount;

            if (lastInSample)
            {
                count--;
            }

            int allLength = Convert.ToInt32(curLengthInSample + outOfSampleLength * count);

            if (allLength > allDays)
            {
                if (Convert.ToDecimal(allLength) / allDays > 1.2m)
                {
                    if (curLengthInSample > 1000)
                    {
                        curLengthInSample -= 10;
                    }
                    else
                    {
                        curLengthInSample -= 5;
                    }
                }

                curLengthInSample--;
                return GetInSampleRecurs(curLengthInSample, fazeCount, lastInSample, allDays, percentOnFiltration);
            }
            else
            {
                return curLengthInSample;
            }
        }

        private void SendLogMessage(string message, LogMessageType type)
        {
            LogMessageEvent?.Invoke(message, type);
        }

        public event Action<string, LogMessageType> LogMessageEvent;
    }
}
