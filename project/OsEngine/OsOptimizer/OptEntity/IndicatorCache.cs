/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Shared indicator-result cache for optimizer runs.
    /// </summary>
    public class IndicatorCache
    {
        private readonly ConcurrentDictionary<string, List<decimal>[]> _cache =
            new ConcurrentDictionary<string, List<decimal>[]>(StringComparer.Ordinal);

        private readonly object _sync = new object();
        private readonly int _maxEntries;

        public IndicatorCache(int maxEntries = 512)
        {
            _maxEntries = maxEntries > 0 ? maxEntries : 1;
        }

        public bool TryGet(string key, out List<decimal>[] values)
        {
            values = null;

            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            if (!_cache.TryGetValue(key, out List<decimal>[] cachedValues)
                || cachedValues == null)
            {
                return false;
            }

            List<decimal>[] clone = CloneSeries(cachedValues);
            if (clone == null)
            {
                return false;
            }

            values = clone;
            return true;
        }

        public void Set(string key, List<decimal>[] values)
        {
            if (string.IsNullOrWhiteSpace(key) || values == null)
            {
                return;
            }

            List<decimal>[] clone = CloneSeries(values);
            if (clone == null)
            {
                return;
            }

            lock (_sync)
            {
                if (_cache.Count >= _maxEntries)
                {
                    _cache.Clear();
                }

                _cache[key] = clone;
            }
        }

        public void Clear()
        {
            _cache.Clear();
        }

        private static List<decimal>[] CloneSeries(List<decimal>[] source)
        {
            if (source == null)
            {
                return null;
            }

            List<decimal>[] clone = new List<decimal>[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                List<decimal> sourceSeries = source[i];
                clone[i] = sourceSeries == null
                    ? new List<decimal>()
                    : new List<decimal>(sourceSeries);
            }

            return clone;
        }
    }
}
