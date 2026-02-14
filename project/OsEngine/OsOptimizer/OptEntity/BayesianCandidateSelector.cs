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
    /// Selects candidate indices for staged Bayesian optimization loop.
    /// </summary>
    public class BayesianCandidateSelector
    {
        private readonly int _defaultBatchSize;

        public BayesianCandidateSelector(int defaultBatchSize)
        {
            _defaultBatchSize = defaultBatchSize < 1 ? 1 : defaultBatchSize;
        }

        public List<int> SelectInitialBatch(int totalCount, HashSet<int> evaluated, int take)
        {
            List<int> result = new List<int>();
            if (totalCount <= 0 || take <= 0)
            {
                return result;
            }
            HashSet<int> evaluatedSafe = evaluated ?? new HashSet<int>();

            int target = Math.Min(totalCount, take);
            if (target == totalCount)
            {
                for (int i = 0; i < totalCount; i++)
                {
                    if (!evaluatedSafe.Contains(i))
                    {
                        result.Add(i);
                    }
                }
                return result;
            }

            decimal step = (decimal)(totalCount - 1) / Math.Max(1, target - 1);
            for (int i = 0; i < target; i++)
            {
                int idx = (int)Math.Round(step * i, MidpointRounding.AwayFromZero);
                if (idx < 0) idx = 0;
                if (idx >= totalCount) idx = totalCount - 1;

                if (!evaluatedSafe.Contains(idx) && !result.Contains(idx))
                {
                    result.Add(idx);
                }
            }

            for (int i = 0; i < totalCount && result.Count < target; i++)
            {
                if (!evaluatedSafe.Contains(i) && !result.Contains(i))
                {
                    result.Add(i);
                }
            }

            return result;
        }

        public List<int> SelectNextBatch(
            int totalCount,
            HashSet<int> evaluated,
            List<CandidateScore> scored,
            int batchSize)
        {
            List<int> result = new List<int>();
            if (batchSize <= 0 || totalCount <= 0)
            {
                return result;
            }
            HashSet<int> evaluatedSafe = evaluated ?? new HashSet<int>();

            List<CandidateScore> scoredSafe = scored ?? new List<CandidateScore>();

            List<CandidateScore> top = scoredSafe
                .Where(s => s != null)
                .OrderByDescending(s => s.Score)
                .Take(Math.Max(1, Math.Min(10, _defaultBatchSize * 2)))
                .ToList();

            int radius = 1;
            while (result.Count < batchSize && radius <= 8 && top.Count > 0)
            {
                for (int i = 0; i < top.Count && result.Count < batchSize; i++)
                {
                    int left = top[i].Index - radius;
                    int right = top[i].Index + radius;

                    TryAddIndex(left, totalCount, evaluatedSafe, result);
                    if (result.Count < batchSize)
                    {
                        TryAddIndex(right, totalCount, evaluatedSafe, result);
                    }
                }

                radius++;
            }

            for (int i = 0; i < totalCount && result.Count < batchSize; i++)
            {
                if (!evaluatedSafe.Contains(i) && !result.Contains(i))
                {
                    result.Add(i);
                }
            }

            return result;
        }

        private void TryAddIndex(int idx, int totalCount, HashSet<int> evaluated, List<int> result)
        {
            if (idx < 0 || idx >= totalCount)
            {
                return;
            }

            if (evaluated.Contains(idx) || result.Contains(idx))
            {
                return;
            }

            result.Add(idx);
        }

        public class CandidateScore
        {
            public int Index;
            public decimal Score;
        }
    }
}
