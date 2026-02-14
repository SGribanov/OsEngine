/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;
using OsEngine.OsOptimizer;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Phase-5 Bayesian strategy skeleton.
    /// Current implementation preserves behavior by delegating execution to brute-force backend.
    /// </summary>
    public class BayesianOptimizationStrategy : IOptimizationStrategy
    {
        private readonly BruteForceStrategy _fallbackBackend;

        public BayesianOptimizationStrategy(
            ParameterIterator parameterIterator,
            IBotEvaluator botEvaluator,
            int maxParallel,
            SortBotsType objectiveMetric,
            int initialSamples,
            int maxIterations,
            int batchSize)
        {
            ObjectiveMetric = objectiveMetric;
            InitialSamples = initialSamples < 1 ? 1 : initialSamples;
            MaxIterations = maxIterations < 1 ? 1 : maxIterations;
            BatchSize = batchSize < 1 ? 1 : batchSize;
            _fallbackBackend = new BruteForceStrategy(parameterIterator, botEvaluator, maxParallel);
        }

        public SortBotsType ObjectiveMetric { get; }

        public int InitialSamples { get; }

        public int MaxIterations { get; }

        public int BatchSize { get; }

        public int EstimateBotCount(List<IIStrategyParameter> allParameters, List<bool> parametersToOptimization)
        {
            return _fallbackBackend.EstimateBotCount(allParameters, parametersToOptimization);
        }

        public Task<List<OptimizerReport>> OptimizeInSampleAsync(
            List<IIStrategyParameter> allParameters,
            List<bool> parametersToOptimization,
            CancellationToken cancellationToken = default)
        {
            return _fallbackBackend.OptimizeInSampleAsync(allParameters, parametersToOptimization, cancellationToken);
        }
    }
}
