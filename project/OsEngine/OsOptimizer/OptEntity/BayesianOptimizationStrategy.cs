/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;
using OsEngine.OsOptimizer;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Phase-5 Bayesian strategy skeleton.
    /// Current implementation uses staged candidate selection over parameter grid:
    /// initial sampling + iterative batches around best scored combinations.
    /// </summary>
    public class BayesianOptimizationStrategy : IOptimizationStrategy
    {
        private readonly ParameterIterator _parameterIterator;
        private readonly IBotEvaluator _botEvaluator;
        private readonly int _maxParallel;
        private readonly BruteForceStrategy _fallbackBackend;
        private readonly BayesianCandidateSelector _candidateSelector;
        private readonly BayesianAcquisitionPolicy _acquisitionPolicy;

        public BayesianOptimizationStrategy(
            ParameterIterator parameterIterator,
            IBotEvaluator botEvaluator,
            int maxParallel,
            SortBotsType objectiveMetric,
            int initialSamples,
            int maxIterations,
            int batchSize)
        {
            _parameterIterator = parameterIterator;
            _botEvaluator = botEvaluator;
            _maxParallel = maxParallel < 1 ? 1 : maxParallel;
            ObjectiveMetric = objectiveMetric;
            InitialSamples = initialSamples < 1 ? 1 : initialSamples;
            MaxIterations = maxIterations < 1 ? 1 : maxIterations;
            BatchSize = batchSize < 1 ? 1 : batchSize;
            _fallbackBackend = new BruteForceStrategy(parameterIterator, botEvaluator, _maxParallel);
            _candidateSelector = new BayesianCandidateSelector(BatchSize);
            _acquisitionPolicy = new BayesianAcquisitionPolicy();
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
            return OptimizeStagedAsync(allParameters, parametersToOptimization, cancellationToken);
        }

        private async Task<List<OptimizerReport>> OptimizeStagedAsync(
            List<IIStrategyParameter> allParameters,
            List<bool> parametersToOptimization,
            CancellationToken cancellationToken)
        {
            if (_botEvaluator == null)
            {
                throw new InvalidOperationException("Bot evaluator is not configured for BayesianOptimizationStrategy.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return new List<OptimizerReport>();
            }

            List<List<IIStrategyParameter>> candidates = BuildCandidatePool(allParameters, parametersToOptimization, cancellationToken);
            if (candidates.Count == 0 || cancellationToken.IsCancellationRequested)
            {
                return new List<OptimizerReport>();
            }

            // Protect runtime memory on huge grids in this phase-5 stage.
            if (candidates.Count > 250000)
            {
                return await _fallbackBackend
                    .OptimizeInSampleAsync(allParameters, parametersToOptimization, cancellationToken)
                    .ConfigureAwait(false);
            }

            HashSet<int> evaluated = new HashSet<int>();
            List<CandidateEvaluation> scored = new List<CandidateEvaluation>();
            List<OptimizerReport> reports = new List<OptimizerReport>();

            List<int> initialBatch = _candidateSelector.SelectInitialBatch(candidates.Count, evaluated, InitialSamples);
            await EvaluateBatchAsync(initialBatch, allParameters, candidates, cancellationToken, evaluated, scored, reports).ConfigureAwait(false);

            int iterationsLeft = MaxIterations;

            while (!cancellationToken.IsCancellationRequested && iterationsLeft > 0 && evaluated.Count < candidates.Count)
            {
                int targetBatchSize = Math.Min(BatchSize, iterationsLeft);
                List<BayesianCandidateSelector.CandidateScore> scoredForSelector = scored
                    .Select(s => new BayesianCandidateSelector.CandidateScore { Index = s.Index, Score = s.Score })
                    .ToList();

                List<int> nextBatch = _acquisitionPolicy.SelectNextBatch(
                    candidates.Count,
                    evaluated,
                    scoredForSelector,
                    targetBatchSize,
                    _candidateSelector);

                if (nextBatch.Count == 0)
                {
                    break;
                }

                await EvaluateBatchAsync(nextBatch, allParameters, candidates, cancellationToken, evaluated, scored, reports).ConfigureAwait(false);
                iterationsLeft -= nextBatch.Count;
            }

            return reports;
        }

        private List<List<IIStrategyParameter>> BuildCandidatePool(
            List<IIStrategyParameter> allParameters,
            List<bool> parametersToOptimization,
            CancellationToken cancellationToken)
        {
            List<IIStrategyParameter> optimizedStart = new List<IIStrategyParameter>();

            for (int i = 0; i < allParameters.Count; i++)
            {
                if (parametersToOptimization[i])
                {
                    optimizedStart.Add(allParameters[i]);
                }
            }

            List<List<IIStrategyParameter>> candidates = new List<List<IIStrategyParameter>>();

            foreach (List<IIStrategyParameter> optimized in _parameterIterator.EnumerateCombinations(optimizedStart))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                candidates.Add(CloneCombinationSnapshot(optimized));
            }

            return candidates;
        }

        private async Task EvaluateBatchAsync(
            List<int> batchIndices,
            List<IIStrategyParameter> allParameters,
            List<List<IIStrategyParameter>> candidates,
            CancellationToken cancellationToken,
            HashSet<int> evaluated,
            List<CandidateEvaluation> scored,
            List<OptimizerReport> reports)
        {
            List<Task<CandidateEvaluation>> running = new List<Task<CandidateEvaluation>>();

            for (int i = 0; i < batchIndices.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                int idx = batchIndices[i];

                if (idx < 0 || idx >= candidates.Count || evaluated.Contains(idx))
                {
                    continue;
                }

                Task<CandidateEvaluation> task = EvaluateCandidateAsync(idx, allParameters, candidates[idx], cancellationToken);
                running.Add(task);
                evaluated.Add(idx);

                if (running.Count >= _maxParallel)
                {
                    CandidateEvaluation completed = await AwaitOneAsync(running).ConfigureAwait(false);
                    if (completed.Report != null)
                    {
                        scored.Add(completed);
                        reports.Add(completed.Report);
                    }
                }
            }

            while (running.Count > 0)
            {
                CandidateEvaluation completed = await AwaitOneAsync(running).ConfigureAwait(false);
                if (completed.Report != null)
                {
                    scored.Add(completed);
                    reports.Add(completed.Report);
                }
            }
        }

        private async Task<CandidateEvaluation> AwaitOneAsync(List<Task<CandidateEvaluation>> running)
        {
            Task<CandidateEvaluation> completedTask = await Task.WhenAny(running).ConfigureAwait(false);
            running.Remove(completedTask);
            return await completedTask.ConfigureAwait(false);
        }

        private async Task<CandidateEvaluation> EvaluateCandidateAsync(
            int index,
            List<IIStrategyParameter> allParameters,
            List<IIStrategyParameter> optimizedParameters,
            CancellationToken cancellationToken)
        {
            OptimizerReport report = await _botEvaluator
                .EvaluateAsync(allParameters, optimizedParameters, cancellationToken)
                .ConfigureAwait(false);

            return new CandidateEvaluation
            {
                Index = index,
                Report = report,
                Score = GetObjectiveScore(report)
            };
        }

        private decimal GetObjectiveScore(OptimizerReport report)
        {
            if (report == null)
            {
                return decimal.MinValue;
            }

            switch (ObjectiveMetric)
            {
                case SortBotsType.TotalProfit:
                    return report.TotalProfit;
                case SortBotsType.PositionCount:
                    return report.PositionsCount;
                case SortBotsType.MaxDrawDawn:
                    return report.MaxDrawDawn;
                case SortBotsType.AverageProfit:
                    return report.AverageProfit;
                case SortBotsType.AverageProfitPercent:
                    return report.AverageProfitPercentOneContract;
                case SortBotsType.ProfitFactor:
                    return report.ProfitFactor;
                case SortBotsType.PayOffRatio:
                    return report.PayOffRatio;
                case SortBotsType.Recovery:
                    return report.Recovery;
                case SortBotsType.SharpRatio:
                    return report.SharpRatio;
                default:
                    return report.TotalProfit;
            }
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

        private class CandidateEvaluation
        {
            public int Index;
            public OptimizerReport Report;
            public decimal Score;
        }
    }
}
