/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;
using System;
using System.Linq;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Brute-force in-sample optimization strategy over parameter grid.
    /// </summary>
    public class BruteForceStrategy : IOptimizationStrategy
    {
        private readonly ParameterIterator _parameterIterator;
        private readonly IBotEvaluator _botEvaluator;
        private readonly int _maxParallel;

        public BruteForceStrategy(ParameterIterator parameterIterator, IBotEvaluator botEvaluator = null, int maxParallel = 1)
        {
            _parameterIterator = parameterIterator;
            _botEvaluator = botEvaluator;
            _maxParallel = maxParallel < 1 ? 1 : maxParallel;
        }

        public int EstimateBotCount(List<IIStrategyParameter> allParameters, List<bool> parametersToOptimization)
        {
            return _parameterIterator.CountCombinations(allParameters, parametersToOptimization);
        }

        public async Task<List<OptimizerReport>> OptimizeInSampleAsync(
            List<IIStrategyParameter> allParameters,
            List<bool> parametersToOptimization,
            CancellationToken cancellationToken = default)
        {
            if (_botEvaluator == null)
            {
                throw new InvalidOperationException("Bot evaluator is not configured for BruteForceStrategy.");
            }

            List<OptimizerReport> result = new List<OptimizerReport>();
            List<Task<OptimizerReport>> running = new List<Task<OptimizerReport>>();

            List<IIStrategyParameter> optimizedParametersStart = new List<IIStrategyParameter>();

            for (int i = 0; i < allParameters.Count; i++)
            {
                if (parametersToOptimization[i])
                {
                    optimizedParametersStart.Add(allParameters[i]);
                }
            }

            foreach (List<IIStrategyParameter> optimizedParameters in _parameterIterator.EnumerateCombinations(optimizedParametersStart))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                List<IIStrategyParameter> comboSnapshot = CloneCombinationSnapshot(optimizedParameters);
                Task<OptimizerReport> task = _botEvaluator.EvaluateAsync(allParameters, comboSnapshot, cancellationToken);
                running.Add(task);

                if (running.Count >= _maxParallel)
                {
                    Task<OptimizerReport> completed = await Task.WhenAny(running).ConfigureAwait(false);
                    running.Remove(completed);
                    OptimizerReport report = await completed.ConfigureAwait(false);
                    if (report != null)
                    {
                        result.Add(report);
                    }
                }
            }

            while (running.Count > 0)
            {
                Task<OptimizerReport> completed = await Task.WhenAny(running).ConfigureAwait(false);
                running.Remove(completed);
                OptimizerReport report = await completed.ConfigureAwait(false);
                if (report != null)
                {
                    result.Add(report);
                }
            }

            return result;
        }

        private List<IIStrategyParameter> CloneCombinationSnapshot(List<IIStrategyParameter> optimizedParameters)
        {
            List<IIStrategyParameter> cloned = _parameterIterator.CopyParameters(optimizedParameters);

            for (int i = 0; i < optimizedParameters.Count && i < cloned.Count; i++)
            {
                IIStrategyParameter src = optimizedParameters[i];
                IIStrategyParameter dst = cloned[i];

                if (src.Type == StrategyParameterType.Int)
                {
                    ((StrategyParameterInt)dst).ValueInt = ((StrategyParameterInt)src).ValueInt;
                }
                else if (src.Type == StrategyParameterType.Decimal)
                {
                    ((StrategyParameterDecimal)dst).ValueDecimal = ((StrategyParameterDecimal)src).ValueDecimal;
                }
                else if (src.Type == StrategyParameterType.DecimalCheckBox)
                {
                    ((StrategyParameterDecimalCheckBox)dst).ValueDecimal = ((StrategyParameterDecimalCheckBox)src).ValueDecimal;
                    ((StrategyParameterDecimalCheckBox)dst).CheckState = ((StrategyParameterDecimalCheckBox)src).CheckState;
                }
            }

            return cloned;
        }
    }
}
