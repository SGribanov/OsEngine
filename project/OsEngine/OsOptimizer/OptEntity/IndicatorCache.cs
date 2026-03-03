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
    public enum IndicatorCacheIsolationMode
    {
        CloneOnReadAndWrite = 0,
        TrustedReferences = 1
    }

    public readonly struct IndicatorCacheKey : IEquatable<IndicatorCacheKey>
    {
        private readonly int _hashCode;

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
            string securityNameSafe = securityName ?? string.Empty;
            string calculationNameSafe = calculationName ?? string.Empty;
            string parametersHashSafe = parametersHash ?? string.Empty;
            string sourceIdSafe = sourceId ?? string.Empty;

            SecurityName = securityNameSafe;
            TimeframeTicks = timeframeTicks;
            FirstTimeTicks = firstTimeTicks;
            LastTimeTicks = lastTimeTicks;
            CandleCount = candleCount;
            CalculationName = calculationNameSafe;
            ParametersHash = parametersHashSafe;
            SourceId = sourceIdSafe;
            OutputSeriesCount = outputSeriesCount;
            IncludeIndicatorsCount = includeIndicatorsCount;
            DataFingerprint = dataFingerprint;

            _hashCode = ComputeHashCode(
                securityNameSafe,
                timeframeTicks,
                firstTimeTicks,
                lastTimeTicks,
                candleCount,
                calculationNameSafe,
                parametersHashSafe,
                sourceIdSafe,
                outputSeriesCount,
                includeIndicatorsCount,
                dataFingerprint);
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
            if (_hashCode != 0)
            {
                return _hashCode;
            }

            return ComputeHashCode(
                SecurityName,
                TimeframeTicks,
                FirstTimeTicks,
                LastTimeTicks,
                CandleCount,
                CalculationName,
                ParametersHash,
                SourceId,
                OutputSeriesCount,
                IncludeIndicatorsCount,
                DataFingerprint);
        }

        private static int ComputeHashCode(
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
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + timeframeTicks.GetHashCode();
                hash = hash * 31 + firstTimeTicks.GetHashCode();
                hash = hash * 31 + lastTimeTicks.GetHashCode();
                hash = hash * 31 + candleCount;
                hash = hash * 31 + outputSeriesCount;
                hash = hash * 31 + includeIndicatorsCount;
                hash = hash * 31 + dataFingerprint;
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(securityName);
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(calculationName);
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(parametersHash);
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(sourceId);
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
        private readonly bool _cloneOnRead;
        private readonly bool _cloneOnWrite;
        private long _hits;
        private long _misses;
        private long _writes;
        private long _evictions;

        public IndicatorCache(
            int maxEntries = 512,
            IndicatorCacheIsolationMode isolationMode = IndicatorCacheIsolationMode.CloneOnReadAndWrite)
        {
            _maxEntries = maxEntries > 0 ? maxEntries : 1;
            _cloneOnRead = isolationMode == IndicatorCacheIsolationMode.CloneOnReadAndWrite;
            _cloneOnWrite = isolationMode == IndicatorCacheIsolationMode.CloneOnReadAndWrite;
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

            if (_cloneOnRead == false)
            {
                values = cachedValues;
                Interlocked.Increment(ref _hits);
                return true;
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

            List<decimal>[] storedValues = values;

            if (_cloneOnWrite)
            {
                List<decimal>[]? clone = CloneSeries(values);
                if (clone == null)
                {
                    return;
                }

                storedValues = clone;
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

                _cache[key] = storedValues;
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
