using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OsEngine.Entity;
using OsEngine.Entity.Utils;
using OsEngine.Market.Servers.QuikLua;
using Xunit;

namespace OsEngine.Tests;

public class QuikLuaSecuritiesCachePersistenceTests
{
    [Fact]
    public void SaveToCache_ShouldPersistJson_AndLoadRoundTrip()
    {
        using QuikLuaCacheFileScope scope = new QuikLuaCacheFileScope();

        QuikLuaServerRealization server = scope.CreateWithoutConstructor();

        List<Security> securities = new List<Security>
        {
            new Security
            {
                Name = "AAA",
                NameClass = "TQBR",
                SecurityType = SecurityType.Stock
            }
        };

        scope.InvokePrivateSaveToCache(server, securities);

        string content = File.ReadAllText(scope.CachePath);
        Assert.StartsWith("{", content.TrimStart());

        List<Security> loaded = scope.InvokePrivateLoadSecuritiesFromCache(server);
        Assert.Single(loaded);
        Assert.Equal("AAA", loaded[0].Name);
        Assert.Equal("TQBR", loaded[0].NameClass);
    }

    [Fact]
    public void LoadSecuritiesFromCache_ShouldSupportLegacyCompressedStringFormat()
    {
        using QuikLuaCacheFileScope scope = new QuikLuaCacheFileScope();

        List<Security> securities = new List<Security>
        {
            new Security
            {
                Name = "BBB",
                NameClass = "SPBFUT",
                SecurityType = SecurityType.Futures
            }
        };

        string serialized = JsonConvert.SerializeObject(securities);
        string compressed = CompressionUtils.Compress(serialized);
        File.WriteAllText(scope.CachePath, compressed);

        QuikLuaServerRealization server = scope.CreateWithoutConstructor();
        List<Security> loaded = scope.InvokePrivateLoadSecuritiesFromCache(server);

        Assert.Single(loaded);
        Assert.Equal("BBB", loaded[0].Name);
        Assert.Equal("SPBFUT", loaded[0].NameClass);
    }

    private sealed class QuikLuaCacheFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _cacheFileExisted;
        private readonly string _cacheBackupPath;
        private readonly MethodInfo _saveToCacheMethod;
        private readonly MethodInfo _loadFromCacheMethod;

        public QuikLuaCacheFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            CachePath = Path.Combine(_engineDirPath, "QuikLuaSecuritiesCache.txt");
            _cacheBackupPath = CachePath + ".codex.bak";

            _saveToCacheMethod = typeof(QuikLuaServerRealization).GetMethod("SaveToCache", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SaveToCache not found.");
            _loadFromCacheMethod = typeof(QuikLuaServerRealization).GetMethod("LoadSecuritiesFromCache", BindingFlags.NonPublic | BindingFlags.Instance)
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

        public QuikLuaServerRealization CreateWithoutConstructor()
        {
            return (QuikLuaServerRealization)RuntimeHelpers.GetUninitializedObject(typeof(QuikLuaServerRealization));
        }

        public void InvokePrivateSaveToCache(QuikLuaServerRealization server, List<Security> securities)
        {
            _saveToCacheMethod.Invoke(server, new object[] { securities });
        }

        public List<Security> InvokePrivateLoadSecuritiesFromCache(QuikLuaServerRealization server)
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
