#nullable enable

using System;
using System.IO;
using System.Reflection;
using System.Text;
using OsEngine.Entity;
using OsEngine.Market.Servers.GateIoData;
using Xunit;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public class GateIoDataArchiveWriteTests
{
    [Fact]
    public void ParseCsvFileToDailyArchives_ShouldSplitByDay_PreserveOrder_AndDeleteMonthlySource()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-gateiodata-archive-" + Guid.NewGuid());
        string tempDirectory = EnsureTrailingSeparator(root);
        string monthlyPath = Path.Combine(root, "monthly.csv");

        try
        {
            Directory.CreateDirectory(root);

            string firstDayFirstTrade = BuildTradeLine(new DateTime(2026, 3, 8, 12, 0, 0), "101");
            string firstDaySecondTrade = BuildTradeLine(new DateTime(2026, 3, 8, 23, 59, 59), "102");
            string secondDayTrade = BuildTradeLine(new DateTime(2026, 3, 9, 0, 0, 1), "103");

            File.WriteAllLines(
                monthlyPath,
                new[]
                {
                    "create_time,id,price,amount,side",
                    string.Empty,
                    firstDayFirstTrade,
                    firstDaySecondTrade,
                    secondDayTrade
                },
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            GateIoDataServerRealization realization = CreateRealization(tempDirectory);
            Security security = new Security { Name = "BTC_USDT" };

            string[] createdFiles = InvokeParseCsvFileToDailyArchives(realization, monthlyPath, security).ToArray();
            string firstDayPath = Path.Combine(root, "BTC_USDT20260308.csv");
            string secondDayPath = Path.Combine(root, "BTC_USDT20260309.csv");

            Assert.Equal(new[] { firstDayPath, secondDayPath }, createdFiles);
            Assert.Equal(new[] { firstDayFirstTrade, firstDaySecondTrade }, File.ReadAllLines(firstDayPath));
            Assert.Equal(new[] { secondDayTrade }, File.ReadAllLines(secondDayPath));
            Assert.False(File.Exists(monthlyPath));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void ParseCsvFileToDailyArchives_ShouldDisposeWriters_AndWriteUtf8BomFiles()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-gateiodata-archive-" + Guid.NewGuid());
        string tempDirectory = EnsureTrailingSeparator(root);
        string monthlyPath = Path.Combine(root, "monthly.csv");

        try
        {
            Directory.CreateDirectory(root);

            string tradeLine = BuildTradeLine(new DateTime(2026, 3, 8, 9, 15, 30), "201");

            File.WriteAllLines(
                monthlyPath,
                new[]
                {
                    "create_time,id,price,amount,side",
                    tradeLine
                },
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            GateIoDataServerRealization realization = CreateRealization(tempDirectory);
            Security security = new Security { Name = "ETH_USDT" };

            string archivePath = Assert.Single(InvokeParseCsvFileToDailyArchives(realization, monthlyPath, security));

            byte[] bytes = File.ReadAllBytes(archivePath);

            Assert.True(bytes.Length >= 3);
            Assert.Equal(0xEF, bytes[0]);
            Assert.Equal(0xBB, bytes[1]);
            Assert.Equal(0xBF, bytes[2]);

            using (FileStream stream = new FileStream(archivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.True(stream.Length > 0);
            }
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private static GateIoDataServerRealization CreateRealization(string tempDirectory)
    {
        GateIoDataServerRealization realization = new GateIoDataServerRealization();
        GetTempDirectoryField().SetValue(realization, tempDirectory);
        Directory.CreateDirectory(tempDirectory);
        return realization;
    }

    private static System.Collections.Generic.List<string> InvokeParseCsvFileToDailyArchives(
        GateIoDataServerRealization realization,
        string csvFilePath,
        Security security)
    {
        return (System.Collections.Generic.List<string>)GetParseMethod().Invoke(realization, new object[] { csvFilePath, security })!;
    }

    private static MethodInfo GetParseMethod()
    {
        return typeof(GateIoDataServerRealization).GetMethod(
                   "ParseCsvFileToDailyArchives",
                   BindingFlags.Instance | BindingFlags.NonPublic)
               ?? throw new InvalidOperationException("Method not found: ParseCsvFileToDailyArchives");
    }

    private static FieldInfo GetTempDirectoryField()
    {
        return typeof(GateIoDataServerRealization).GetField(
                   "_tempDirectory",
                   BindingFlags.Instance | BindingFlags.NonPublic)
               ?? throw new InvalidOperationException("Field not found: _tempDirectory");
    }

    private static string BuildTradeLine(DateTime time, string tradeId)
    {
        return $"{ToUnixSeconds(time)}.123,{tradeId},100.5,0.25,1";
    }

    private static long ToUnixSeconds(DateTime time)
    {
        return Convert.ToInt64((time - new DateTime(1970, 1, 1)).TotalSeconds);
    }

    private static string EnsureTrailingSeparator(string path)
    {
        if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            || path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            return path;
        }

        return path + Path.DirectorySeparatorChar;
    }
}
