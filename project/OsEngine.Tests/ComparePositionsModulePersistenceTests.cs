using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using OsEngine.Market.Servers;
using OsEngine.Market.Servers.YahooFinance;
using Xunit;

namespace OsEngine.Tests;

public class ComparePositionsModulePersistenceTests
{
    [Fact]
    public void SaveLoad_ShouldPersistJsonForMainSettings()
    {
        YahooServer server = CreateTestServer();
        using ComparePositionsFilesScope scope = new ComparePositionsFilesScope(server.ServerNameUnique);

        ComparePositionsModule source = CreateModule(server);
        SetVerificationPeriod(source, ComparePositionsVerificationPeriod.Min30);
        source.TimeDelaySeconds = 17;
        source.PortfoliosToWatch.Add("P1");
        source.PortfoliosToWatch.Add("P2");
        source.Save();

        string content = File.ReadAllText(scope.MainSettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        ComparePositionsModule loaded = CreateModule(server);
        InvokePrivateLoad(loaded);

        Assert.Equal(ComparePositionsVerificationPeriod.Min30, loaded.VerificationPeriod);
        Assert.Equal(17, loaded.TimeDelaySeconds);
        Assert.Equal(new[] { "P1", "P2" }, loaded.PortfoliosToWatch);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedMainSettings()
    {
        YahooServer server = CreateTestServer();
        using ComparePositionsFilesScope scope = new ComparePositionsFilesScope(server.ServerNameUnique);

        File.WriteAllLines(scope.MainSettingsPath, new[]
        {
            "Min10#15",
            "PX",
            "PY"
        });

        ComparePositionsModule loaded = CreateModule(server);
        InvokePrivateLoad(loaded);

        Assert.Equal(ComparePositionsVerificationPeriod.Min10, loaded.VerificationPeriod);
        Assert.Equal(15, loaded.TimeDelaySeconds);
        Assert.Equal(new[] { "PX", "PY" }, loaded.PortfoliosToWatch);
    }

    [Fact]
    public void SaveLoadIgnoredSecurities_ShouldPersistJson()
    {
        YahooServer server = CreateTestServer();
        using ComparePositionsFilesScope scope = new ComparePositionsFilesScope(server.ServerNameUnique);

        ComparePositionsModule source = CreateModule(server);
        source.IgnoredSecurities.Add(new ComparePositionsSecurity { Security = "SBER", IsIgnored = true });
        source.IgnoredSecurities.Add(new ComparePositionsSecurity { Security = "GAZP", IsIgnored = false });
        source.SaveIgnoredSecurities();

        string content = File.ReadAllText(scope.IgnoreSettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        ComparePositionsModule loaded = CreateModule(server);
        loaded.LoadIgnoredSecurities();

        Assert.Equal(2, loaded.IgnoredSecurities.Count);
        Assert.Equal("SBER", loaded.IgnoredSecurities[0].Security);
        Assert.True(loaded.IgnoredSecurities[0].IsIgnored);
        Assert.Equal("GAZP", loaded.IgnoredSecurities[1].Security);
        Assert.False(loaded.IgnoredSecurities[1].IsIgnored);
    }

    [Fact]
    public void LoadIgnoredSecurities_ShouldSupportLegacyLineBasedFormat()
    {
        YahooServer server = CreateTestServer();
        using ComparePositionsFilesScope scope = new ComparePositionsFilesScope(server.ServerNameUnique);

        File.WriteAllLines(scope.IgnoreSettingsPath, new[]
        {
            "VTBR%True%",
            "LKOH%False%"
        });

        ComparePositionsModule loaded = CreateModule(server);
        loaded.LoadIgnoredSecurities();

        Assert.Equal(2, loaded.IgnoredSecurities.Count);
        Assert.Equal("VTBR", loaded.IgnoredSecurities[0].Security);
        Assert.True(loaded.IgnoredSecurities[0].IsIgnored);
        Assert.Equal("LKOH", loaded.IgnoredSecurities[1].Security);
        Assert.False(loaded.IgnoredSecurities[1].IsIgnored);
    }

    private static YahooServer CreateTestServer()
    {
        YahooServer server = new YahooServer();
        server.ServerNum = 500000 + Random.Shared.Next(1, 100000);
        return server;
    }

    private static ComparePositionsModule CreateModule(YahooServer server)
    {
#pragma warning disable SYSLIB0050
        ComparePositionsModule module =
            (ComparePositionsModule)FormatterServices.GetUninitializedObject(typeof(ComparePositionsModule));
#pragma warning restore SYSLIB0050
        module.Server = server;
        module.TimeDelaySeconds = 20;
        module.PortfoliosToWatch = new List<string>();
        module.IgnoredSecurities = new List<ComparePositionsSecurity>();
        return module;
    }

    private static void SetVerificationPeriod(
        ComparePositionsModule module,
        ComparePositionsVerificationPeriod value)
    {
        FieldInfo field = typeof(ComparePositionsModule).GetField(
                              "_verificationPeriod",
                              BindingFlags.NonPublic | BindingFlags.Instance)
                          ?? throw new InvalidOperationException("Field _verificationPeriod not found.");
        field.SetValue(module, value);
    }

    private static void InvokePrivateLoad(ComparePositionsModule module)
    {
        MethodInfo method = typeof(ComparePositionsModule).GetMethod(
                                "Load",
                                BindingFlags.NonPublic | BindingFlags.Instance)
                            ?? throw new InvalidOperationException("Method Load not found.");
        method.Invoke(module, null);
    }

    private sealed class ComparePositionsFilesScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _mainExisted;
        private readonly bool _ignoreExisted;
        private readonly string _mainBackup;
        private readonly string _ignoreBackup;

        public ComparePositionsFilesScope(string serverNameUnique)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            MainSettingsPath = Path.Combine(_engineDirPath, serverNameUnique + "CompareModule.txt");
            IgnoreSettingsPath = Path.Combine(_engineDirPath, serverNameUnique + "CompareModule_IgnoreSec.txt");
            _mainBackup = MainSettingsPath + ".codex.bak";
            _ignoreBackup = IgnoreSettingsPath + ".codex.bak";

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _mainExisted = File.Exists(MainSettingsPath);
            if (_mainExisted)
            {
                File.Copy(MainSettingsPath, _mainBackup, overwrite: true);
            }
            else if (File.Exists(_mainBackup))
            {
                File.Delete(_mainBackup);
            }

            _ignoreExisted = File.Exists(IgnoreSettingsPath);
            if (_ignoreExisted)
            {
                File.Copy(IgnoreSettingsPath, _ignoreBackup, overwrite: true);
            }
            else if (File.Exists(_ignoreBackup))
            {
                File.Delete(_ignoreBackup);
            }
        }

        public string MainSettingsPath { get; }

        public string IgnoreSettingsPath { get; }

        public void Dispose()
        {
            RestoreFile(MainSettingsPath, _mainBackup, _mainExisted);
            RestoreFile(IgnoreSettingsPath, _ignoreBackup, _ignoreExisted);

            if (!_engineDirExisted
                && Directory.Exists(_engineDirPath)
                && !Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
            {
                Directory.Delete(_engineDirPath);
            }
        }

        private static void RestoreFile(string path, string backupPath, bool existed)
        {
            if (existed)
            {
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, path, overwrite: true);
                    File.Delete(backupPath);
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
        }
    }
}
