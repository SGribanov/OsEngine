/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Selects next candidate indices using a lightweight index-distance surrogate
    /// and UCB-like acquisition score.
    /// </summary>
    public class BayesianAcquisitionPolicy
    {
        private readonly decimal _kappa;

        public BayesianAcquisitionPolicy(decimal kappa = 0.25m)
        {
            _kappa = kappa < 0 ? 0 : kappa;
        }

        public List<int> SelectNextBatch(
            int totalCount,
            HashSet<int> evaluated,
            List<BayesianCandidateSelector.CandidateScore> scored,
            int batchSize,
            BayesianCandidateSelector fallbackSelector)
        {
            if (batchSize <= 0 || totalCount <= 0)
            {
                return new List<int>();
            }

            if (scored == null || scored.Count == 0)
            {
                return fallbackSelector.SelectInitialBatch(totalCount, evaluated, batchSize);
            }

            int maxDistance = Math.Max(1, totalCount - 1);
            List<CandidateAcquisition> ranked = new List<CandidateAcquisition>();

            for (int i = 0; i < totalCount; i++)
            {
                if (evaluated.Contains(i))
                {
                    continue;
                }

                BayesianCandidateSelector.CandidateScore nearest = null;
                int minDistance = int.MaxValue;

                for (int j = 0; j < scored.Count; j++)
                {
                    int dist = Math.Abs(i - scored[j].Index);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        nearest = scored[j];
                    }
                }

                decimal mean = nearest == null ? 0m : nearest.Score;
                decimal uncertainty = (decimal)minDistance / maxDistance;
                decimal acquisition = mean + (_kappa * uncertainty);

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

        private class CandidateAcquisition
        {
            public int Index;
            public decimal Mean;
            public decimal Uncertainty;
            public decimal Acquisition;
        }
    }
}
