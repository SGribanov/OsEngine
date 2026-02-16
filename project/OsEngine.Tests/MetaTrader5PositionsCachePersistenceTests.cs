using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OsEngine.Entity.Utils;
using OsEngine.Market.Servers.MetaTrader5;
using Xunit;

namespace OsEngine.Tests;

public class MetaTrader5PositionsCachePersistenceTests
{
    [Fact]
    public void SavePositionsInFile_ShouldPersistJson_AndLoadRoundTrip()
    {
        using MetaTrader5PositionsFileScope scope = new MetaTrader5PositionsFileScope();

        MetaTrader5ServerRealization server = scope.CreateWithoutConstructor();
        scope.SetOpenPositions(server, new Dictionary<int, ulong> { [42] = 100500UL });

        scope.InvokePrivateSavePositionsInFile(server);

        string content = File.ReadAllText(scope.CachePath);
        Assert.StartsWith("{", content.TrimStart());

        scope.SetOpenPositions(server, new Dictionary<int, ulong>());
        scope.InvokePrivateLoadPositionsFromFile(server);

        Dictionary<int, ulong> loaded = scope.GetOpenPositions(server);
        Assert.Single(loaded);
        Assert.Equal(100500UL, loaded[42]);
    }

    [Fact]
    public void LoadPositionsFromFile_ShouldSupportLegacyCompressedStringFormat()
    {
        using MetaTrader5PositionsFileScope scope = new MetaTrader5PositionsFileScope();

        Dictionary<int, ulong> positions = new Dictionary<int, ulong> { [7] = 9000UL };
        string serialized = JsonConvert.SerializeObject(positions);
        string compressed = CompressionUtils.Compress(serialized);
        File.WriteAllText(scope.CachePath, compressed);

        MetaTrader5ServerRealization server = scope.CreateWithoutConstructor();
        scope.SetOpenPositions(server, new Dictionary<int, ulong>());
        scope.InvokePrivateLoadPositionsFromFile(server);

        Dictionary<int, ulong> loaded = scope.GetOpenPositions(server);
        Assert.Single(loaded);
        Assert.Equal(9000UL, loaded[7]);
    }

    private sealed class MetaTrader5PositionsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _cacheFileExisted;
        private readonly string _cacheBackupPath;
        private readonly MethodInfo _savePositionsMethod;
        private readonly MethodInfo _loadPositionsMethod;
        private readonly FieldInfo _openPositionsField;

        public MetaTrader5PositionsFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            CachePath = Path.Combine(_engineDirPath, "MetaTrader5PositionsCache.txt");
            _cacheBackupPath = CachePath + ".codex.bak";

            _savePositionsMethod = typeof(MetaTrader5ServerRealization).GetMethod("SavePositionsInFile", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SavePositionsInFile not found.");
            _loadPositionsMethod = typeof(MetaTrader5ServerRealization).GetMethod("LoadPositionsFromFile", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadPositionsFromFile not found.");
            _openPositionsField = typeof(MetaTrader5ServerRealization).GetField("_dictionaryOpenPositions", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _dictionaryOpenPositions not found.");

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

        public void SetOpenPositions(MetaTrader5ServerRealization server, Dictionary<int, ulong> positions)
        {
            _openPositionsField.SetValue(server, positions);
        }

        public Dictionary<int, ulong> GetOpenPositions(MetaTrader5ServerRealization server)
        {
            return (Dictionary<int, ulong>)_openPositionsField.GetValue(server)!;
        }

        public void InvokePrivateSavePositionsInFile(MetaTrader5ServerRealization server)
        {
            _savePositionsMethod.Invoke(server, null);
        }

        public void InvokePrivateLoadPositionsFromFile(MetaTrader5ServerRealization server)
        {
            _loadPositionsMethod.Invoke(server, null);
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
