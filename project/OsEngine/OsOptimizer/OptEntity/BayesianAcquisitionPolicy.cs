/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Linq;
using OsEngine.Entity;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Selects next candidate indices using a lightweight parameter-space surrogate
    /// and UCB-like acquisition score.
    /// </summary>
    public class BayesianAcquisitionPolicy
    {
        public BayesianAcquisitionPolicy(decimal kappa = 0.25m)
        {
            _ = kappa;
        }

        public List<int> SelectNextBatch(
            int totalCount,
            HashSet<int> evaluated,
            List<BayesianCandidateSelector.CandidateScore> scored,
            int batchSize,
            BayesianCandidateSelector fallbackSelector,
            List<List<IIStrategyParameter>> candidates,
            BayesianAcquisitionModeType mode,
            decimal kappa)
        {
            decimal effectiveKappa = kappa < 0 ? 0 : kappa;
            HashSet<int> evaluatedSafe = evaluated ?? new HashSet<int>();
            List<BayesianCandidateSelector.CandidateScore> scoredSafe =
                scored ?? new List<BayesianCandidateSelector.CandidateScore>();

            if (batchSize <= 0 || totalCount <= 0)
            {
                return new List<int>();
            }

            if (scoredSafe.Count == 0)
            {
                if (fallbackSelector == null)
                {
                    throw new ArgumentNullException(nameof(fallbackSelector));
                }

                return fallbackSelector.SelectInitialBatch(totalCount, evaluatedSafe, batchSize);
            }

            List<BayesianCandidateSelector.CandidateScore> validScored = scoredSafe
                .Where(s => s.Index >= 0 && s.Index < totalCount)
                .ToList();

            if (validScored.Count == 0)
            {
                if (fallbackSelector == null)
                {
                    throw new ArgumentNullException(nameof(fallbackSelector));
                }

                return fallbackSelector.SelectNextBatch(totalCount, evaluatedSafe, scoredSafe, batchSize);
            }

            if (candidates == null || candidates.Count != totalCount)
            {
                if (fallbackSelector == null)
                {
                    throw new ArgumentNullException(nameof(fallbackSelector));
                }

                return fallbackSelector.SelectNextBatch(totalCount, evaluatedSafe, scoredSafe, batchSize);
            }

            List<CandidateAcquisition> ranked = new List<CandidateAcquisition>();
            decimal bestMean = validScored.Max(s => s.Score);

            for (int i = 0; i < totalCount; i++)
            {
                if (evaluatedSafe.Contains(i))
                {
                    continue;
                }

                BayesianCandidateSelector.CandidateScore nearest = null;
                decimal minDistance = decimal.MaxValue;

                for (int j = 0; j < validScored.Count; j++)
                {
                    int scoredIndex = validScored[j].Index;

                    decimal dist = CalculateParameterDistance(candidates[i], candidates[scoredIndex]);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        nearest = validScored[j];
                    }
                }

                decimal mean = nearest == null ? 0m : nearest.Score;
                decimal uncertainty = minDistance == decimal.MaxValue ? 1m : Math.Min(1m, minDistance);
                decimal acquisition;

                if (mode == BayesianAcquisitionModeType.Greedy)
                {
                    acquisition = mean;
                }
                else if (mode == BayesianAcquisitionModeType.ExpectedImprovement)
                {
                    decimal optimisticMean = mean + (effectiveKappa * uncertainty);
                    acquisition = Math.Max(0m, optimisticMean - bestMean);
                }
                else
                {
                    acquisition = mean + (effectiveKappa * uncertainty);
                }

                ranked.Add(new CandidateAcquisition
                {
                    Index = i,
                    Mean = mean,
                    Uncertainty = uncertainty,
                    Acquisition = acquisition
                });
            }

            return ranked
                .OrderByDescending(x => x.Acquisition)
                .ThenByDescending(x => x.Uncertainty)
                .ThenBy(x => x.Index)
                .Take(batchSize)
                .Select(x => x.Index)
                .ToList();
        }

        private decimal CalculateParameterDistance(List<IIStrategyParameter> left, List<IIStrategyParameter> right)
        {
            if (left == null || right == null || left.Count == 0 || right.Count == 0)
            {
                return 1m;
            }

            int dim = Math.Min(left.Count, right.Count);
            if (dim <= 0)
            {
                return 1m;
            }

            decimal sum = 0m;

            for (int i = 0; i < dim; i++)
            {
                sum += GetParameterDelta(left[i], right[i]);
            }

            return sum / dim;
        }

        private decimal GetParameterDelta(IIStrategyParameter a, IIStrategyParameter b)
        {
            if (a == null || b == null)
            {
                return 1m;
            }

            if (a.Type != b.Type)
            {
                return 1m;
            }

            if (a.Type == StrategyParameterType.Int)
            {
                StrategyParameterInt x = (StrategyParameterInt)a;
                StrategyParameterInt y = (StrategyParameterInt)b;
                decimal range = Math.Abs(x.ValueIntStop - x.ValueIntStart);
                if (range <= 0) range = 1;
                return Math.Abs(x.ValueInt - y.ValueInt) / range;
            }

            if (a.Type == StrategyParameterType.Decimal)
            {
                StrategyParameterDecimal x = (StrategyParameterDecimal)a;
                StrategyParameterDecimal y = (StrategyParameterDecimal)b;
                decimal range = Math.Abs(x.ValueDecimalStop - x.ValueDecimalStart);
                if (range <= 0) range = 1m;
                return Math.Abs(x.ValueDecimal - y.ValueDecimal) / range;
            }

            if (a.Type == StrategyParameterType.DecimalCheckBox)
            {
                StrategyParameterDecimalCheckBox x = (StrategyParameterDecimalCheckBox)a;
                StrategyParameterDecimalCheckBox y = (StrategyParameterDecimalCheckBox)b;
                decimal range = Math.Abs(x.ValueDecimalStop - x.ValueDecimalStart);
                if (range <= 0) range = 1m;
                decimal valueDelta = Math.Abs(x.ValueDecimal - y.ValueDecimal) / range;
                decimal checkDelta = x.CheckState == y.CheckState ? 0m : 1m;
                return (valueDelta + checkDelta) / 2m;
            }

            if (a.Type == StrategyParameterType.Bool)
            {
                return ((StrategyParameterBool)a).ValueBool == ((StrategyParameterBool)b).ValueBool ? 0m : 1m;
            }

            if (a.Type == StrategyParameterType.CheckBox)
            {
                return ((StrategyParameterCheckBox)a).CheckState == ((StrategyParameterCheckBox)b).CheckState ? 0m : 1m;
            }

            if (a.Type == StrategyParameterType.String)
            {
                return ((StrategyParameterString)a).ValueString == ((StrategyParameterString)b).ValueString ? 0m : 1m;
            }

            if (a.Type == StrategyParameterType.TimeOfDay)
            {
                TimeOfDay x = ((StrategyParameterTimeOfDay)a).Value;
                TimeOfDay y = ((StrategyParameterTimeOfDay)b).Value;
                decimal xSec = x.Hour * 3600m + x.Minute * 60m + x.Second + (x.Millisecond / 1000m);
                decimal ySec = y.Hour * 3600m + y.Minute * 60m + y.Second + (y.Millisecond / 1000m);
                return Math.Abs(xSec - ySec) / 86400m;
            }

            return a.GetStringToSave() == b.GetStringToSave() ? 0m : 1m;
        }

        private class CandidateAcquisition
        {
            public int Index;
            public decimal Mean;
            public decimal Uncertainty;
            public decimal Acquisition;
        }
    }
}
