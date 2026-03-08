#nullable enable

using System;
using System.IO;
using System.Linq;
using OsEngine.Market.Servers.Polygon;
using OsEngine.Market.Servers.Polygon.Entity;

namespace OsEngine.Tests;

public class PolygonSecurityCacheWriteTests
{
    [Fact]
    public void FormatSecurityCacheLine_ShouldPreserveExistingCommaSeparatedShape()
    {
        Tickers ticker = new Tickers
        {
            ticker = "AAPL",
            name = "Apple Inc.",
            type = "CS",
            primary_exchange = "XNAS"
        };

        string line = PolygonServerRealization.FormatSecurityCacheLine(ticker);

        Assert.Equal("AAPL,Apple Inc.,CS,XNAS", line);
    }

    [Fact]
    public void CreateSecuritiesCacheWriter_ShouldWriteUtf8WithoutBom()
    {
        using PolygonSecurityCacheScope scope = new PolygonSecurityCacheScope();

        using (StreamWriter writer = PolygonServerRealization.CreateSecuritiesCacheWriter(scope.TempPath))
        {
            writer.WriteLine("AAPL,Apple Inc.,CS,XNAS");
            PolygonServerRealization.FlushSecuritiesCacheWriter(writer);
        }

        byte[] bytes = File.ReadAllBytes(scope.TempPath);

        Assert.False(bytes.Take(3).SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF }));
        Assert.Equal("AAPL,Apple Inc.,CS,XNAS" + Environment.NewLine, File.ReadAllText(scope.TempPath));
    }

    [Fact]
    public void ReplaceSecuritiesCacheFromTemp_ExistingTarget_ShouldPromoteTempAndKeepBackup()
    {
        using PolygonSecurityCacheScope scope = new PolygonSecurityCacheScope();
        Directory.CreateDirectory(Path.GetDirectoryName(scope.TargetPath)!);
        File.WriteAllText(scope.TargetPath, "old-line" + Environment.NewLine);

        using (StreamWriter writer = PolygonServerRealization.CreateSecuritiesCacheWriter(scope.TempPath))
        {
            writer.WriteLine("new-line");
            PolygonServerRealization.FlushSecuritiesCacheWriter(writer);
        }

        PolygonServerRealization.ReplaceSecuritiesCacheFromTemp(scope.TempPath, scope.TargetPath);

        Assert.Equal("new-line" + Environment.NewLine, File.ReadAllText(scope.TargetPath));
        Assert.Equal("old-line" + Environment.NewLine, File.ReadAllText(scope.BackupPath));
        Assert.False(File.Exists(scope.TempPath));
    }

    [Fact]
    public void ReplaceSecuritiesCacheFromTemp_MissingTarget_ShouldMoveTempIntoPlaceWithoutBackup()
    {
        using PolygonSecurityCacheScope scope = new PolygonSecurityCacheScope();

        using (StreamWriter writer = PolygonServerRealization.CreateSecuritiesCacheWriter(scope.TempPath))
        {
            writer.WriteLine("new-line");
            PolygonServerRealization.FlushSecuritiesCacheWriter(writer);
        }

        PolygonServerRealization.ReplaceSecuritiesCacheFromTemp(scope.TempPath, scope.TargetPath);

        Assert.Equal("new-line" + Environment.NewLine, File.ReadAllText(scope.TargetPath));
        Assert.False(File.Exists(scope.BackupPath));
        Assert.False(File.Exists(scope.TempPath));
    }

    private sealed class PolygonSecurityCacheScope : IDisposable
    {
        private readonly string _rootPath;

        public PolygonSecurityCacheScope()
        {
            _rootPath = Path.Combine(Path.GetTempPath(), "osengine-polygon-cache-" + Guid.NewGuid());
            Directory.CreateDirectory(_rootPath);
            TargetPath = Path.Combine(_rootPath, "Engine", "PolygonSecurities.csv");
            TempPath = PolygonServerRealization.GetSecuritiesCacheTempPath(TargetPath);
            BackupPath = Path.GetFullPath(TargetPath) + ".bak";
        }

        public string TargetPath { get; }

        public string TempPath { get; }

        public string BackupPath { get; }

        public void Dispose()
        {
            if (Directory.Exists(_rootPath))
            {
                Directory.Delete(_rootPath, true);
            }
        }
    }
}
