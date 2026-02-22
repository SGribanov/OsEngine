#nullable enable

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace OsEngine.OsOptimizer.OptEntity
{
    public readonly struct IndicatorCacheKey : IEquatable<IndicatorCacheKey>
    {
        public IndicatorCacheKey(
            string securityName,
            long timeframeTicks,
            long firstTimeTicks,
            long lastTimeTicks,
            int candleCount,
            string calculationName,
            string parametersHash,
            string sourceId,
            int outputSeriesCount,
            int includeIndicatorsCount,
            int dataFingerprint)
        {
            SecurityName = securityName ?? string.Empty;
            TimeframeTicks = timeframeTicks;
            FirstTimeTicks = firstTimeTicks;
            LastTimeTicks = lastTimeTicks;
            CandleCount = candleCount;
            CalculationName = calculationName ?? string.Empty;
            ParametersHash = parametersHash ?? string.Empty;
            SourceId = sourceId ?? string.Empty;
            OutputSeriesCount = outputSeriesCount;
            IncludeIndicatorsCount = includeIndicatorsCount;
            DataFingerprint = dataFingerprint;
        }

        public string SecurityName { get; }

        public long TimeframeTicks { get; }

        public long FirstTimeTicks { get; }

        public long LastTimeTicks { get; }

        public int CandleCount { get; }

        public string CalculationName { get; }

        public string ParametersHash { get; }

        public string SourceId { get; }

        public int OutputSeriesCount { get; }

        public int IncludeIndicatorsCount { get; }

        public int DataFingerprint { get; }

        public bool Equals(IndicatorCacheKey other)
        {
            return TimeframeTicks == other.TimeframeTicks
                && FirstTimeTicks == other.FirstTimeTicks
                && LastTimeTicks == other.LastTimeTicks
                && CandleCount == other.CandleCount
                && OutputSeriesCount == other.OutputSeriesCount
                && IncludeIndicatorsCount == other.IncludeIndicatorsCount
                && DataFingerprint == other.DataFingerprint
                && StringComparer.Ordinal.Equals(SecurityName, other.SecurityName)
                && StringComparer.Ordinal.Equals(CalculationName, other.CalculationName)
                && StringComparer.Ordinal.Equals(ParametersHash, other.ParametersHash)
                && StringComparer.Ordinal.Equals(SourceId, other.SourceId);
        }

        public override bool Equals(object? obj)
        {
            return obj is IndicatorCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + TimeframeTicks.GetHashCode();
                hash = hash * 31 + FirstTimeTicks.GetHashCode();
                hash = hash * 31 + LastTimeTicks.GetHashCode();
                hash = hash * 31 + CandleCount;
                hash = hash * 31 + OutputSeriesCount;
                hash = hash * 31 + IncludeIndicatorsCount;
                hash = hash * 31 + DataFingerprint;
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SecurityName);
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(CalculationName);
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(ParametersHash);
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SourceId);
                return hash;
            }
        }

        public static bool operator ==(IndicatorCacheKey left, IndicatorCacheKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IndicatorCacheKey left, IndicatorCacheKey right)
        {
            return !(left == right);
        }
    }

    public readonly struct IndicatorCacheStatistics
    {
        public IndicatorCacheStatistics(long hits, long misses, long writes, long evictions, int entriesCount)
        {
            Hits = hits;
            Misses = misses;
            Writes = writes;
            Evictions = evictions;
            EntriesCount = entriesCount;
        }

        public long Hits { get; }

        public long Misses { get; }

        public long Writes { get; }

        public long Evictions { get; }

        public int EntriesCount { get; }

        public long RequestsCount => Hits + Misses;

        public double HitRate => RequestsCount == 0
            ? 0d
            : (double)Hits / RequestsCount;
    }

    /// <summary>
    /// Shared indicator-result cache for optimizer runs.
    /// </summary>
    public class IndicatorCache
    {
        private readonly ConcurrentDictionary<IndicatorCacheKey, List<decimal>[]> _cache =
            new ConcurrentDictionary<IndicatorCacheKey, List<decimal>[]>();

        private readonly Lock _sync = new();
        private readonly int _maxEntries;
        private long _hits;
        private long _misses;
        private long _writes;
        private long _evictions;

        public IndicatorCache(int maxEntries = 512)
        {
            _maxEntries = maxEntries > 0 ? maxEntries : 1;
        }

        public bool TryGet(in IndicatorCacheKey key, out List<decimal>[]? values)
        {
            values = null;

            if (!_cache.TryGetValue(key, out List<decimal>[]? cachedValues)
                || cachedValues == null)
            {
                Interlocked.Increment(ref _misses);
                return false;
            }

            List<decimal>[]? clone = CloneSeries(cachedValues);
            if (clone == null)
            {
                Interlocked.Increment(ref _misses);
                return false;
            }

            values = clone;
            Interlocked.Increment(ref _hits);
            return true;
        }

        public void Set(in IndicatorCacheKey key, List<decimal>[] values)
        {
            if (values == null)
            {
                return;
            }

            List<decimal>[]? clone = CloneSeries(values);
            if (clone == null)
            {
                return;
            }

            lock (_sync)
            {
                bool exists = _cache.ContainsKey(key);

                if (!exists && _cache.Count >= _maxEntries)
                {
                    int removed = _cache.Count;
                    _cache.Clear();
                    Interlocked.Add(ref _evictions, removed);
                }

                _cache[key] = clone;
            }

            Interlocked.Increment(ref _writes);
        }

        public void Clear()
        {
            _cache.Clear();
            Interlocked.Exchange(ref _hits, 0);
            Interlocked.Exchange(ref _misses, 0);
            Interlocked.Exchange(ref _writes, 0);
            Interlocked.Exchange(ref _evictions, 0);
        }

        public IndicatorCacheStatistics GetStatisticsSnapshot()
        {
            return new IndicatorCacheStatistics(
                hits: Interlocked.Read(ref _hits),
                misses: Interlocked.Read(ref _misses),
                writes: Interlocked.Read(ref _writes),
                evictions: Interlocked.Read(ref _evictions),
                entriesCount: _cache.Count);
        }

        private static List<decimal>[]? CloneSeries(List<decimal>[]? source)
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
