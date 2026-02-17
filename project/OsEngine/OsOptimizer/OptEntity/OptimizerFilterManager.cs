/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using OsEngine.Logging;

#nullable enable

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Applies filter criteria to optimization reports.
    /// Применяет фильтры к результатам оптимизации.
    /// </summary>
    public class OptimizerFilterManager
    {
        private readonly OptimizerSettings _settings;

        public OptimizerFilterManager(OptimizerSettings settings)
        {
            _settings = settings;
        }

        public bool IsAcceptedByFilter(OptimizerReport? report)
        {
            if (report == null)
            {
                return false;
            }

            if (_settings.FilterMiddleProfitIsOn && report.AverageProfitPercentOneContract < _settings.FilterMiddleProfitValue)
            {
                return false;
            }

            if (_settings.FilterProfitIsOn && report.TotalProfit < _settings.FilterProfitValue)
            {
                return false;
            }

            if (_settings.FilterMaxDrawDownIsOn && report.MaxDrawDawn < _settings.FilterMaxDrawDownValue)
            {
                return false;
            }

            if (_settings.FilterProfitFactorIsOn && report.ProfitFactor < _settings.FilterProfitFactorValue)
            {
                return false;
            }

            if (_settings.FilterDealsCountIsOn && report.PositionsCount < _settings.FilterDealsCountValue)
            {
                return false;
            }

            return true;
        }

        public void ApplyEndOfFazeFiltration(OptimizerFazeReport? bots)
        {
            try
            {
                if (bots == null)
                {
                    return;
                }

                if (bots.Reports == null || bots.Reports.Count == 0)
                {
                    return;
                }

                OptimizerFazeReport botsFiltered = new OptimizerFazeReport();

                int startCount = bots.Reports.Count;

                for (int i = 0; i < bots.Reports.Count; i++)
                {
                    if (IsAcceptedByFilter(bots.Reports[i]))
                    {
                        botsFiltered.Reports.Add(bots.Reports[i]);
                    }
                }

                if (botsFiltered.Reports.Count != 0 && startCount != botsFiltered.Reports.Count)
                {
                    SendLogMessage(Language.OsLocalization.Optimizer.Message9 + (startCount - botsFiltered.Reports.Count), LogMessageType.System);
                }

                bots.Reports = botsFiltered.Reports;
            }
            catch (Exception ex)
            {
                SendLogMessage(ex.ToString(), LogMessageType.Error);
            }
        }

        private void SendLogMessage(string message, LogMessageType type)
        {
            LogMessageEvent?.Invoke(message, type);
        }

        public event Action<string, LogMessageType>? LogMessageEvent;
    }
}
