/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;
using System;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Brute-force in-sample optimization strategy over parameter grid.
    /// </summary>
    public class BruteForceStrategy : IOptimizationStrategy
    {
        private readonly ParameterIterator _parameterIterator;
        private readonly IBotEvaluator _botEvaluator;

        public BruteForceStrategy(ParameterIterator parameterIterator, IBotEvaluator botEvaluator = null)
        {
            _parameterIterator = parameterIterator;
            _botEvaluator = botEvaluator;
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

                OptimizerReport report =
                    await _botEvaluator.EvaluateAsync(allParameters, optimizedParameters, cancellationToken).ConfigureAwait(false);

                if (report != null)
                {
                    result.Add(report);
                }
            }

            return result;
        }
    }
}
