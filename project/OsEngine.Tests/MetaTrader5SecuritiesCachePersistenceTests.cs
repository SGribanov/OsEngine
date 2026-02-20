#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OsEngine.Entity;
using OsEngine.Entity.Utils;
using OsEngine.Market.Servers.MetaTrader5;
using Xunit;

namespace OsEngine.Tests;

public class MetaTrader5SecuritiesCachePersistenceTests
{
    [Fact]
    public void SaveToCache_ShouldPersistJson_AndLoadRoundTrip()
    {
        using MetaTrader5CacheFileScope scope = new MetaTrader5CacheFileScope();

        MetaTrader5ServerRealization server = scope.CreateWithoutConstructor();

        List<Security> securities = new List<Security>
        {
            new Security
            {
                Name = "EURUSD",
                NameClass = "Forex",
                SecurityType = SecurityType.CurrencyPair
            }
        };

        scope.InvokePrivateSaveToCache(server, securities);

        string content = File.ReadAllText(scope.CachePath);
        Assert.StartsWith("{", content.TrimStart());

        List<Security> loaded = scope.InvokePrivateLoadSecuritiesFromCache(server);
        Assert.Single(loaded);
        Assert.Equal("EURUSD", loaded[0].Name);
        Assert.Equal("Forex", loaded[0].NameClass);
    }

    [Fact]
    public void LoadSecuritiesFromCache_ShouldSupportLegacyCompressedStringFormat()
    {
        using MetaTrader5CacheFileScope scope = new MetaTrader5CacheFileScope();

        List<Security> securities = new List<Security>
        {
            new Security
            {
                Name = "XAUUSD",
                NameClass = "Metals",
                SecurityType = SecurityType.Commodities
            }
        };

        string serialized = JsonConvert.SerializeObject(securities);
        string compressed = CompressionUtils.Compress(serialized);
        File.WriteAllText(scope.CachePath, compressed);

        MetaTrader5ServerRealization server = scope.CreateWithoutConstructor();
        List<Security> loaded = scope.InvokePrivateLoadSecuritiesFromCache(server);

        Assert.Single(loaded);
        Assert.Equal("XAUUSD", loaded[0].Name);
        Assert.Equal("Metals", loaded[0].NameClass);
    }

    private sealed class MetaTrader5CacheFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _cacheFileExisted;
        private readonly string _cacheBackupPath;
        private readonly MethodInfo _saveToCacheMethod;
        private readonly MethodInfo _loadFromCacheMethod;

        public MetaTrader5CacheFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            CachePath = Path.Combine(_engineDirPath, "MetaTrader5SecuritiesCache.txt");
            _cacheBackupPath = CachePath + ".codex.bak";

            _saveToCacheMethod = typeof(MetaTrader5ServerRealization).GetMethod("SaveToCache", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SaveToCache not found.");
            _loadFromCacheMethod = typeof(MetaTrader5ServerRealization).GetMethod("LoadSecuritiesFromCache", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadSecuritiesFromCache not found.");

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _cacheFileExisted = File.Exists(CachePath);
            if (_cacheFileExisted)
            {
                File.Copy(CachePath, _cacheBackupPath, overwrite: true);
            }
            else if (File.Exists(_cacheBackupPath))
            {
                File.Delete(_cacheBackupPath);
            }
        }

        public string CachePath { get; }

        public MetaTrader5ServerRealization CreateWithoutConstructor()
        {
            return (MetaTrader5ServerRealization)RuntimeHelpers.GetUninitializedObject(typeof(MetaTrader5ServerRealization));
        }

        public void InvokePrivateSaveToCache(MetaTrader5ServerRealization server, List<Security> securities)
        {
            _saveToCacheMethod.Invoke(server, new object[] { securities });
        }

        public List<Security> InvokePrivateLoadSecuritiesFromCache(MetaTrader5ServerRealization server)
        {
            return (List<Security>)_loadFromCacheMethod.Invoke(server, null)!;
        }

        public void Dispose()
        {
            if (_cacheFileExisted)
            {
                if (File.Exists(_cacheBackupPath))
                {
                    File.Copy(_cacheBackupPath, CachePath, overwrite: true);
                    File.Delete(_cacheBackupPath);
                }
            }
            else
            {
                if (File.Exists(CachePath))
                {
                    File.Delete(CachePath);
                }

                if (File.Exists(_cacheBackupPath))
                {
                    File.Delete(_cacheBackupPath);
                }
            }

            if (!_engineDirExisted
                && Directory.Exists(_engineDirPath)
                && !Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
            {
                Directory.Delete(_engineDirPath);
            }
        }
    }
}
