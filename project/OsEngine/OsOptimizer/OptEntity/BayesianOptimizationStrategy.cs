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
        private readonly BayesianCandidateSelector _candidateSelector;
        private readonly BayesianAcquisitionPolicy _acquisitionPolicy;

        public BayesianOptimizationStrategy(
            ParameterIterator parameterIterator,
            IBotEvaluator botEvaluator,
            int maxParallel,
            SortBotsType objectiveMetric,
            ObjectiveDirectionType objectiveDirection,
            int initialSamples,
            int maxIterations,
            int batchSize,
            BayesianAcquisitionModeType acquisitionMode,
            decimal acquisitionKappa,
            bool useExploitationTailPass,
            int tailSharePercent)
        {
            _parameterIterator = parameterIterator;
            _botEvaluator = botEvaluator;
            _maxParallel = maxParallel < 1 ? 1 : maxParallel;
            ObjectiveMetric = objectiveMetric;
            ObjectiveDirection = objectiveDirection;
            InitialSamples = initialSamples < 1 ? 1 : initialSamples;
            MaxIterations = maxIterations < 1 ? 1 : maxIterations;
            BatchSize = batchSize < 1 ? 1 : batchSize;
            AcquisitionMode = acquisitionMode;
            AcquisitionKappa = acquisitionKappa < 0 ? 0 : acquisitionKappa;
            UseExploitationTailPass = useExploitationTailPass;
            TailSharePercent = tailSharePercent < 1 ? 1 : (tailSharePercent > 50 ? 50 : tailSharePercent);
            _candidateSelector = new BayesianCandidateSelector(BatchSize);
            _acquisitionPolicy = new BayesianAcquisitionPolicy();
        }

        public SortBotsType ObjectiveMetric { get; }

        public int InitialSamples { get; }

        public int MaxIterations { get; }

        public int BatchSize { get; }

        public ObjectiveDirectionType ObjectiveDirection { get; }

        public BayesianAcquisitionModeType AcquisitionMode { get; }

        public decimal AcquisitionKappa { get; }

        public bool UseExploitationTailPass { get; }

        public int TailSharePercent { get; }

        public int LastTailBudgetPlanned { get; private set; }

        public int EstimateBotCount(List<IIStrategyParameter> allParameters, List<bool> parametersToOptimization)
        {
            ValidateInputs(allParameters, parametersToOptimization);
            if (!parametersToOptimization.Any(x => x))
            {
                return 1;
            }

            int totalCombinations = _parameterIterator.CountCombinations(allParameters, parametersToOptimization);
            int plannedBudget = InitialSamples + MaxIterations;
            if (plannedBudget < 1)
            {
                plannedBudget = 1;
            }

            return Math.Min(totalCombinations, plannedBudget);
        }

        public Task<List<OptimizerReport>> OptimizeInSampleAsync(
            List<IIStrategyParameter> allParameters,
            List<bool> parametersToOptimization,
            CancellationToken cancellationToken = default)
        {
            ValidateInputs(allParameters, parametersToOptimization);
            return OptimizeStagedAsync(allParameters, parametersToOptimization, cancellationToken);
        }

        private async Task<List<OptimizerReport>> OptimizeStagedAsync(
            List<IIStrategyParameter> allParameters,
            List<bool> parametersToOptimization,
            CancellationToken cancellationToken)
        {
            LastTailBudgetPlanned = 0;

            if (_botEvaluator == null)
            {
                throw new InvalidOperationException("Bot evaluator is not configured for BayesianOptimizationStrategy.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return new List<OptimizerReport>();
            }

            int plannedBudget = Math.Max(1, InitialSamples + MaxIterations);
            int maxCandidatePoolSize = Math.Min(50000, Math.Max(5000, plannedBudget * 10));

            List<List<IIStrategyParameter>> candidates =
                BuildCandidatePool(allParameters, parametersToOptimization, cancellationToken, maxCandidatePoolSize);
            if (candidates.Count == 0 || cancellationToken.IsCancellationRequested)
            {
                return new List<OptimizerReport>();
            }

            HashSet<int> evaluated = new HashSet<int>();
            List<CandidateEvaluation> scored = new List<CandidateEvaluation>();
            List<OptimizerReport> reports = new List<OptimizerReport>();

            List<int> initialBatch = _candidateSelector.SelectInitialBatch(candidates.Count, evaluated, InitialSamples);
            await EvaluateBatchAsync(initialBatch, allParameters, candidates, cancellationToken, evaluated, scored, reports).ConfigureAwait(false);

            int tailBudget = GetExploitationTailBudget();
            LastTailBudgetPlanned = tailBudget;
            int iterationsLeft = MaxIterations - tailBudget;

            while (!cancellationToken.IsCancellationRequested && iterationsLeft > 0 && evaluated.Count < candidates.Count)
            {
                int targetBatchSize = Math.Min(BatchSize, iterationsLeft);
                List<BayesianCandidateSelector.CandidateScore> scoredForSelector = BuildNormalizedScores(scored);
                decimal effectiveKappa = GetMetricAdjustedKappa();

                List<int> nextBatch = _acquisitionPolicy.SelectNextBatch(
                    candidates.Count,
                    evaluated,
                    scoredForSelector,
                    targetBatchSize,
                    _candidateSelector,
                    candidates,
                    AcquisitionMode,
                    effectiveKappa);

                if (nextBatch.Count == 0)
                {
                    break;
                }

                await EvaluateBatchAsync(nextBatch, allParameters, candidates, cancellationToken, evaluated, scored, reports).ConfigureAwait(false);
                iterationsLeft -= nextBatch.Count;
            }

            if (!cancellationToken.IsCancellationRequested
                && tailBudget > 0
                && evaluated.Count < candidates.Count
                && scored.Count > 0)
            {
                List<BayesianCandidateSelector.CandidateScore> scoredForTail = BuildNormalizedScores(scored);
                List<int> tailBatch = _acquisitionPolicy.SelectNextBatch(
                    candidates.Count,
                    evaluated,
                    scoredForTail,
                    Math.Min(tailBudget, candidates.Count - evaluated.Count),
                    _candidateSelector,
                    candidates,
                    BayesianAcquisitionModeType.Greedy,
                    0m);

                if (tailBatch.Count > 0)
                {
                    await EvaluateBatchAsync(tailBatch, allParameters, candidates, cancellationToken, evaluated, scored, reports).ConfigureAwait(false);
                }
            }

            return reports;
        }

        private List<BayesianCandidateSelector.CandidateScore> BuildNormalizedScores(List<CandidateEvaluation> scored)
        {
            List<BayesianCandidateSelector.CandidateScore> result = new List<BayesianCandidateSelector.CandidateScore>();
            if (scored == null || scored.Count == 0)
            {
                return result;
            }

            decimal min = scored.Min(s => s.Score);
            decimal max = scored.Max(s => s.Score);
            decimal range = max - min;

            for (int i = 0; i < scored.Count; i++)
            {
                decimal normalized = range <= 0 ? 0m : (scored[i].Score - min) / range;
                result.Add(new BayesianCandidateSelector.CandidateScore
                {
                    Index = scored[i].Index,
                    Score = normalized
                });
            }

            return result;
        }

        private decimal GetMetricAdjustedKappa()
        {
            decimal scale;
            switch (ObjectiveMetric)
            {
                case SortBotsType.PositionCount:
                    scale = 0.5m;
                    break;
                case SortBotsType.MaxDrawDawn:
                    scale = 1.3m;
                    break;
                case SortBotsType.SharpRatio:
                    scale = 1.2m;
                    break;
                case SortBotsType.ProfitFactor:
                case SortBotsType.PayOffRatio:
                    scale = 0.8m;
                    break;
                default:
                    scale = 1m;
                    break;
            }

            decimal adjusted = AcquisitionKappa * scale;
            return adjusted < 0 ? 0 : adjusted;
        }

        private int GetExploitationTailBudget()
        {
            if (!UseExploitationTailPass)
            {
                return 0;
            }

            if (AcquisitionMode == BayesianAcquisitionModeType.Greedy)
            {
                return 0;
            }

            if (MaxIterations < 4)
            {
                return 0;
            }

            int sharePercent = TailSharePercent;
            if (AcquisitionMode == BayesianAcquisitionModeType.ExpectedImprovement)
            {
                sharePercent = Math.Max(1, TailSharePercent - 5);
            }

            int byShare = Math.Max(1, (int)Math.Round(MaxIterations * (sharePercent / 100m), MidpointRounding.AwayFromZero));
            int byBatch = Math.Max(1, BatchSize);
            return Math.Min(MaxIterations - 1, Math.Min(byShare, byBatch));
        }

        private List<List<IIStrategyParameter>> BuildCandidatePool(
            List<IIStrategyParameter> allParameters,
            List<bool> parametersToOptimization,
            CancellationToken cancellationToken,
            int maxPoolSize)
        {
            List<IIStrategyParameter> optimizedStart = new List<IIStrategyParameter>();

            for (int i = 0; i < allParameters.Count; i++)
            {
                if (parametersToOptimization[i])
                {
                    optimizedStart.Add(allParameters[i]);
                }
            }

            if (optimizedStart.Count == 0)
            {
                return new List<List<IIStrategyParameter>>
                {
                    new List<IIStrategyParameter>()
                };
            }

            List<List<IIStrategyParameter>> candidates = new List<List<IIStrategyParameter>>();

            foreach (List<IIStrategyParameter> optimized in _parameterIterator.EnumerateCombinations(optimizedStart))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                candidates.Add(CloneCombinationSnapshot(optimized));

                if (candidates.Count >= maxPoolSize)
                {
                    break;
                }
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
                    return Orient(report.TotalProfit);
                case SortBotsType.PositionCount:
                    return Orient(report.PositionsCount);
                case SortBotsType.MaxDrawDawn:
                    return Orient(report.MaxDrawDawn);
                case SortBotsType.AverageProfit:
                    return Orient(report.AverageProfit);
                case SortBotsType.AverageProfitPercent:
                    return Orient(report.AverageProfitPercentOneContract);
                case SortBotsType.ProfitFactor:
                    return Orient(report.ProfitFactor);
                case SortBotsType.PayOffRatio:
                    return Orient(report.PayOffRatio);
                case SortBotsType.Recovery:
                    return Orient(report.Recovery);
                case SortBotsType.SharpRatio:
                    return Orient(report.SharpRatio);
                default:
                    return Orient(report.TotalProfit);
            }
        }

        private decimal Orient(decimal value)
        {
            return ObjectiveDirection == ObjectiveDirectionType.Minimize ? -value : value;
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

        private static void ValidateInputs(List<IIStrategyParameter> allParameters, List<bool> parametersToOptimization)
        {
            if (allParameters == null)
            {
                throw new ArgumentNullException(nameof(allParameters));
            }

            if (parametersToOptimization == null)
            {
                throw new ArgumentNullException(nameof(parametersToOptimization));
            }

            if (allParameters.Count != parametersToOptimization.Count)
            {
                throw new ArgumentException("allParameters and parametersToOptimization must have equal length.");
            }
        }
    }
}
