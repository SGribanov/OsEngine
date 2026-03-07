#nullable enable

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        private readonly int _sourceIdToken;
        private readonly bool _sourceIdIsToken;

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
                outputSeriesCount,
                includeIndicatorsCount,
                dataFingerprint)
        {
        }

        public IndicatorCacheKey(
            string securityName,
            long timeframeTicks,
            long firstTimeTicks,
            long lastTimeTicks,
            int candleCount,
            string calculationName,
            string parametersHash,
            int sourceId,
            int outputSeriesCount,
            int includeIndicatorsCount,
            int dataFingerprint)
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
                outputSeriesCount,
                includeIndicatorsCount,
                dataFingerprint)
        {
        }

        public IndicatorCacheKey(
            OrdinalHashedString securityName,
            long timeframeTicks,
            long firstTimeTicks,
            long lastTimeTicks,
            int candleCount,
            OrdinalHashedString calculationName,
            OrdinalHashedString parametersHash,
            int sourceId,
            int outputSeriesCount,
            int includeIndicatorsCount,
            int dataFingerprint)
        {
            SecurityName = securityName.Value;
            TimeframeTicks = timeframeTicks;
            FirstTimeTicks = firstTimeTicks;
            LastTimeTicks = lastTimeTicks;
            CandleCount = candleCount;
            CalculationName = calculationName.Value;
            ParametersHash = parametersHash.Value;
            SourceId = string.Empty;
            _sourceIdToken = sourceId;
            _sourceIdIsToken = true;
            OutputSeriesCount = outputSeriesCount;
            IncludeIndicatorsCount = includeIndicatorsCount;
            DataFingerprint = dataFingerprint;

            _hashCode = NormalizeHashCode(ComputeHashCodeForTokenSource(
                securityName.HashCode,
                timeframeTicks,
                firstTimeTicks,
                lastTimeTicks,
                candleCount,
                calculationName.HashCode,
                parametersHash.HashCode,
                sourceId,
                outputSeriesCount,
                includeIndicatorsCount,
                dataFingerprint));
        }

        private IndicatorCacheKey(
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
            int outputSeriesCount,
            int includeIndicatorsCount,
            int dataFingerprint)
        {
            string securityNameSafe = securityName ?? string.Empty;
            string calculationNameSafe = calculationName ?? string.Empty;
            string parametersHashSafe = parametersHash ?? string.Empty;
            string sourceIdSafe = sourceIdText ?? string.Empty;

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
            OutputSeriesCount = outputSeriesCount;
            IncludeIndicatorsCount = includeIndicatorsCount;
            DataFingerprint = dataFingerprint;

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
                outputSeriesCount,
                includeIndicatorsCount,
                dataFingerprint));
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
                && _sourceIdIsToken == other._sourceIdIsToken
                && (_sourceIdIsToken
                    ? _sourceIdToken == other._sourceIdToken
                    : StringComparer.Ordinal.Equals(SourceId, other.SourceId));
        }

        public override bool Equals(object? obj)
        {
            return obj is IndicatorCacheKey other && Equals(other);
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
            int outputSeriesCount,
            int includeIndicatorsCount,
            int dataFingerprint)
        {
            unchecked
            {
                int hash = 17;
                hash = MixInt(hash, candleCount);
                hash = MixInt(hash, outputSeriesCount);
                hash = MixInt(hash, includeIndicatorsCount);
                hash = MixInt(hash, dataFingerprint);
                hash = MixLong(hash, timeframeTicks);
                hash = MixLong(hash, firstTimeTicks);
                hash = MixLong(hash, lastTimeTicks);
                hash = MixInt(hash, StringComparer.Ordinal.GetHashCode(securityName));
                hash = MixInt(hash, StringComparer.Ordinal.GetHashCode(calculationName));
                hash = MixInt(hash, StringComparer.Ordinal.GetHashCode(parametersHash));
                hash = MixInt(hash, sourceIdIsToken ? 1 : 0);
                hash = sourceIdIsToken
                    ? MixInt(hash, sourceIdToken)
                    : MixInt(hash, StringComparer.Ordinal.GetHashCode(sourceId));
                return hash;
            }
        }

        private static int ComputeHashCodeForTokenSource(
            int securityNameHashCode,
            long timeframeTicks,
            long firstTimeTicks,
            long lastTimeTicks,
            int candleCount,
            int calculationNameHashCode,
            int parametersHashCode,
            int sourceIdToken,
            int outputSeriesCount,
            int includeIndicatorsCount,
            int dataFingerprint)
        {
            unchecked
            {
                int hash = 17;
                hash = MixInt(hash, candleCount);
                hash = MixInt(hash, outputSeriesCount);
                hash = MixInt(hash, includeIndicatorsCount);
                hash = MixInt(hash, dataFingerprint);
                hash = MixLong(hash, timeframeTicks);
                hash = MixLong(hash, firstTimeTicks);
                hash = MixLong(hash, lastTimeTicks);
                hash = MixInt(hash, securityNameHashCode);
                hash = MixInt(hash, calculationNameHashCode);
                hash = MixInt(hash, parametersHashCode);
                hash = MixInt(hash, 1);
                hash = MixInt(hash, sourceIdToken);
                return hash;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int MixInt(int hash, int value)
        {
            return unchecked(hash * 31 + value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int MixLong(int hash, long value)
        {
            return unchecked(hash * 31 + (int)value + (int)(value >> 32));
        }

        private static int NormalizeHashCode(int hash)
        {
            return hash == 0 ? 1 : hash;
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
