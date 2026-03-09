#nullable enable
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.OsData;
using OsEngine.OsData.BinaryEntity;

namespace OsEngine.Tests;

public sealed class OsDataSetMarketDepthPersistenceTests
{
    [Fact]
    public void CreateStream_ShouldPersistHeaderImmediately()
    {
        using MarketDepthLoaderScope scope = new();
        MarketDepthLoader loader = scope.CreateLoader();
        MarketDepth marketDepth = scope.CreateMarketDepth(new DateTime(2026, 3, 9, 10, 11, 12, DateTimeKind.Utc));

        scope.Invoke(loader, "CreateStream", marketDepth);

        FileStream? stream = scope.GetField<FileStream>(loader, "_fileStream");

        Assert.NotNull(stream);
        Assert.NotNull(scope.GetField<DataBinaryWriter>(loader, "_binaryWriter"));
        Assert.True(stream!.Length > 0);

        scope.Invoke(loader, "OffStream");

        byte[] prefix = File.ReadAllBytes(scope.FilePath);

        Assert.StartsWith("QScalp History Data", System.Text.Encoding.UTF8.GetString(prefix));
        Assert.Null(scope.GetField<FileStream>(loader, "_fileStream"));
        Assert.Null(scope.GetField<DataBinaryWriter>(loader, "_binaryWriter"));
    }

    [Fact]
    public void EnsureWritableStream_WithCorruptedExistingFile_ShouldRecreateHeader()
    {
        using MarketDepthLoaderScope scope = new();
        File.WriteAllText(scope.FilePath, "corrupted payload");

        MarketDepthLoader loader = scope.CreateLoader();
        MarketDepth marketDepth = scope.CreateMarketDepth(new DateTime(2026, 3, 9, 12, 13, 14, DateTimeKind.Utc));

        scope.Invoke(loader, "EnsureWritableStream", marketDepth);

        FileStream? stream = scope.GetField<FileStream>(loader, "_fileStream");

        Assert.NotNull(stream);
        Assert.NotNull(scope.GetField<DataBinaryWriter>(loader, "_binaryWriter"));
        Assert.Null(scope.GetField<MarketDepth>(loader, "_lastMarketDepth"));
        Assert.True(stream!.Length > 0);

        scope.Invoke(loader, "OffStream");

        byte[] bytes = File.ReadAllBytes(scope.FilePath);
        string headerPrefix = System.Text.Encoding.UTF8.GetString(bytes, 0, "QScalp History Data".Length);

        Assert.Equal("QScalp History Data", headerPrefix);
    }

    private sealed class MarketDepthLoaderScope : IDisposable
    {
        private readonly string _root;
        private readonly MethodInfo _createStreamMethod;
        private readonly MethodInfo _ensureWritableStreamMethod;
        private readonly MethodInfo _offStreamMethod;
        private readonly System.Collections.Generic.List<MarketDepthLoader> _loaders = [];

        public MarketDepthLoaderScope()
        {
            _root = Path.Combine(Path.GetTempPath(), "osengine-mdloader-" + Guid.NewGuid());
            Directory.CreateDirectory(_root);
            FilePath = Path.Combine(_root, "TEST.2026-03-09.Quotes.qsh");

            Type loaderType = typeof(MarketDepthLoader);
            _createStreamMethod = loaderType.GetMethod("CreateStream", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("CreateStream method not found.");
            _ensureWritableStreamMethod = loaderType.GetMethod("EnsureWritableStream", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("EnsureWritableStream method not found.");
            _offStreamMethod = loaderType.GetMethod("OffStream", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("OffStream method not found.");
        }

        public string FilePath { get; }

        public MarketDepthLoader CreateLoader()
        {
            MarketDepthLoader loader = (MarketDepthLoader)RuntimeHelpers.GetUninitializedObject(typeof(MarketDepthLoader));
            loader.NewLogMessageEvent += static (_, _) => { };

            SetField(loader, "_secName", "TEST");
            SetField(loader, "_secClass", "TQBR");
            SetField(loader, "_serverType", ServerType.Tester);
            SetField(loader, "_serverName", "Tester");
            SetField(loader, "_depth", 5);
            SetField(loader, "_priceStep", 1m);
            SetField(loader, "_volumeStep", 1m);
            SetField(loader, "_pathSecurityFolder", _root);
            SetField(loader, "_filePath", FilePath);
            SetField(loader, "_prefix", System.Text.Encoding.UTF8.GetBytes("QScalp History Data"));

            _loaders.Add(loader);
            return loader;
        }

        public MarketDepth CreateMarketDepth(DateTime time)
        {
            return new MarketDepth
            {
                Time = time,
                Asks = [],
                Bids = []
            };
        }

        public void Invoke(MarketDepthLoader loader, string methodName, MarketDepth? marketDepth = null)
        {
            MethodInfo method = methodName switch
            {
                "CreateStream" => _createStreamMethod,
                "EnsureWritableStream" => _ensureWritableStreamMethod,
                "OffStream" => _offStreamMethod,
                _ => throw new InvalidOperationException("Unexpected method: " + methodName)
            };

            object?[] args = marketDepth == null ? [] : [marketDepth];
            method.Invoke(loader, args);
        }

        public T? GetField<T>(MarketDepthLoader loader, string fieldName)
            where T : class
        {
            FieldInfo field = typeof(MarketDepthLoader).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);

            return field.GetValue(loader) as T;
        }

        private static void SetField(MarketDepthLoader loader, string fieldName, object value)
        {
            FieldInfo field = typeof(MarketDepthLoader).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);

            field.SetValue(loader, value);
        }

        public void Dispose()
        {
            foreach (MarketDepthLoader loader in _loaders)
            {
                try
                {
                    Invoke(loader, "OffStream");
                }
                catch
                {
                    // best-effort cleanup for reflection-created loaders
                }
            }

            if (Directory.Exists(_root))
            {
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        Directory.Delete(_root, recursive: true);
                        break;
                    }
                    catch (IOException) when (attempt < 2)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        System.Threading.Thread.Sleep(50);
                    }
                }
            }
        }
    }
}
