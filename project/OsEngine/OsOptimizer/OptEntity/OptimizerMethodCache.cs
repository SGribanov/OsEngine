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
    public readonly struct OptimizerMethodCacheKey : IEquatable<OptimizerMethodCacheKey>
    {
        private readonly int _hashCode;
        private readonly int _sourceIdToken;
        private readonly bool _sourceIdIsToken;

        public OptimizerMethodCacheKey(
            string securityName,
            long timeframeTicks,
            long firstTimeTicks,
            long lastTimeTicks,
            int candleCount,
            string calculationName,
            string parametersHash,
            string sourceId,
            int dataFingerprint,
            string resultTypeName)
            : this(
                securityName,
                timeframeTicks,
                firstTimeTicks,
                lastTimeTicks,
                candleCount,
                calculationName,
                parametersHash,
                sourceId,
                sourceIdToken: 0,
                sourceIdIsToken: false,
                dataFingerprint,
                resultTypeName)
        {
        }

        public OptimizerMethodCacheKey(
            string securityName,
            long timeframeTicks,
            long firstTimeTicks,
            long lastTimeTicks,
            int candleCount,
            string calculationName,
            string parametersHash,
            int sourceId,
            int dataFingerprint,
            string resultTypeName)
            : this(
                securityName,
                timeframeTicks,
                firstTimeTicks,
                lastTimeTicks,
                candleCount,
                calculationName,
                parametersHash,
                sourceIdText: string.Empty,
                sourceIdToken: sourceId,
                sourceIdIsToken: true,
                dataFingerprint,
                resultTypeName)
        {
        }

        private OptimizerMethodCacheKey(
            string securityName,
            long timeframeTicks,
            long firstTimeTicks,
            long lastTimeTicks,
            int candleCount,
            string calculationName,
            string parametersHash,
            string sourceIdText,
            int sourceIdToken,
            bool sourceIdIsToken,
            int dataFingerprint,
            string resultTypeName)
        {
            string securityNameSafe = securityName ?? string.Empty;
            string calculationNameSafe = calculationName ?? string.Empty;
            string parametersHashSafe = parametersHash ?? string.Empty;
            string sourceIdSafe = sourceIdText ?? string.Empty;
            string resultTypeNameSafe = resultTypeName ?? string.Empty;

            SecurityName = securityNameSafe;
            TimeframeTicks = timeframeTicks;
            FirstTimeTicks = firstTimeTicks;
            LastTimeTicks = lastTimeTicks;
            CandleCount = candleCount;
            CalculationName = calculationNameSafe;
            ParametersHash = parametersHashSafe;
            SourceId = sourceIdSafe;
            _sourceIdToken = sourceIdToken;
            _sourceIdIsToken = sourceIdIsToken;
            DataFingerprint = dataFingerprint;
            ResultTypeName = resultTypeNameSafe;

            _hashCode = NormalizeHashCode(ComputeHashCode(
                securityNameSafe,
                timeframeTicks,
                firstTimeTicks,
                lastTimeTicks,
                candleCount,
                calculationNameSafe,
                parametersHashSafe,
                sourceIdSafe,
                sourceIdToken,
                sourceIdIsToken,
                dataFingerprint,
                resultTypeNameSafe));
        }

        public string SecurityName { get; }

        public long TimeframeTicks { get; }

        public long FirstTimeTicks { get; }

        public long LastTimeTicks { get; }

        public int CandleCount { get; }

        public string CalculationName { get; }

        public string ParametersHash { get; }

        public string SourceId { get; }

        public int DataFingerprint { get; }

        public string ResultTypeName { get; }

        public bool Equals(OptimizerMethodCacheKey other)
        {
            return TimeframeTicks == other.TimeframeTicks
                && FirstTimeTicks == other.FirstTimeTicks
                && LastTimeTicks == other.LastTimeTicks
                && CandleCount == other.CandleCount
                && DataFingerprint == other.DataFingerprint
                && StringComparer.Ordinal.Equals(SecurityName, other.SecurityName)
                && StringComparer.Ordinal.Equals(CalculationName, other.CalculationName)
                && StringComparer.Ordinal.Equals(ParametersHash, other.ParametersHash)
                && _sourceIdIsToken == other._sourceIdIsToken
                && (_sourceIdIsToken
                    ? _sourceIdToken == other._sourceIdToken
                    : StringComparer.Ordinal.Equals(SourceId, other.SourceId))
                && StringComparer.Ordinal.Equals(ResultTypeName, other.ResultTypeName);
        }

        public override bool Equals(object? obj)
        {
            return obj is OptimizerMethodCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _hashCode;
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
            int sourceIdToken,
            bool sourceIdIsToken,
            int dataFingerprint,
            string resultTypeName)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + timeframeTicks.GetHashCode();
                hash = hash * 31 + firstTimeTicks.GetHashCode();
                hash = hash * 31 + lastTimeTicks.GetHashCode();
                hash = hash * 31 + candleCount;
                hash = hash * 31 + dataFingerprint;
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(securityName ?? string.Empty);
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(calculationName ?? string.Empty);
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(parametersHash ?? string.Empty);
                hash = hash * 31 + (sourceIdIsToken ? 1 : 0);
                hash = sourceIdIsToken
                    ? (hash * 31 + sourceIdToken)
                    : (hash * 31 + StringComparer.Ordinal.GetHashCode(sourceId ?? string.Empty));
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(resultTypeName ?? string.Empty);
                return hash;
            }
        }

        private static int NormalizeHashCode(int hash)
        {
            return hash == 0 ? 1 : hash;
        }

        public static bool operator ==(OptimizerMethodCacheKey left, OptimizerMethodCacheKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OptimizerMethodCacheKey left, OptimizerMethodCacheKey right)
        {
            return !(left == right);
        }
    }

    public readonly struct OptimizerMethodCacheStatistics
    {
        public OptimizerMethodCacheStatistics(long hits, long misses, long writes, long evictions, int entriesCount)
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
    /// Shared cache for deterministic internal robot calculation methods in optimizer mode.
    /// </summary>
    public class OptimizerMethodCache
    {
        private readonly ConcurrentDictionary<OptimizerMethodCacheKey, object> _cache =
            new ConcurrentDictionary<OptimizerMethodCacheKey, object>();

        private readonly Lock _sync = new();
        private readonly int _maxEntries;
        private long _hits;
        private long _misses;
        private long _writes;
        private long _evictions;

        public OptimizerMethodCache(int maxEntries = 1024)
        {
            _maxEntries = maxEntries > 0 ? maxEntries : 1;
        }

        public bool TryGet<T>(in OptimizerMethodCacheKey key, out T value)
        {
            value = default!;

            if (!_cache.TryGetValue(key, out object? cached))
            {
                Interlocked.Increment(ref _misses);
                return false;
            }

            if (cached is T typed)
            {
                value = typed;
                Interlocked.Increment(ref _hits);
                return true;
            }

            Interlocked.Increment(ref _misses);
            return false;
        }

        public void Set<T>(in OptimizerMethodCacheKey key, T value)
        {
            if (ReferenceEquals(value, null))
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

                _cache[key] = value;
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

        public OptimizerMethodCacheStatistics GetStatisticsSnapshot()
        {
            return new OptimizerMethodCacheStatistics(
                hits: Interlocked.Read(ref _hits),
                misses: Interlocked.Read(ref _misses),
                writes: Interlocked.Read(ref _writes),
                evictions: Interlocked.Read(ref _evictions),
                entriesCount: _cache.Count);
        }
    }
}
