#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Market.Servers.Tester;
using Xunit;

namespace OsEngine.Tests;

public class TesterServerSecurityDopSettingsPersistenceTests
{
    [Fact]
    public void SaveSecurityDopSettings_ShouldPersistJson_AndLoadSettings()
    {
        TesterServer server = CreateServerConfigured();
        string settingsPath = InvokeGetSecuritiesSettingsPath(server);

        using TesterServerSecurityDopFileScope scope = new TesterServerSecurityDopFileScope(settingsPath);

        Security securityToSave = new Security
        {
            Name = "AAA",
            NameClass = "Stock",
            Lot = 2m,
            MarginBuy = 100m,
            PriceStepCost = 1.5m,
            PriceStep = 0.01m,
            DecimalsVolume = 3,
            MarginSell = 95m,
            Expiration = new DateTime(2025, 1, 2, 3, 4, 5)
        };

        server.SaveSecurityDopSettings(securityToSave);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        List<string[]> loaded = InvokePrivateLoadSecurityDopSettings(server, scope.SettingsPath);
        Assert.NotNull(loaded);
        Assert.Single(loaded);

        string[] item = loaded[0];
        Assert.Equal("AAA", item[0]);
        Assert.Equal("2", item[1]);
        Assert.Equal("100", item[2]);
        Assert.Equal("1.5", item[3]);
        Assert.Equal("0.01", item[4]);
        Assert.Equal("3", item[5]);
        Assert.Equal("95", item[6]);
        Assert.Equal(new DateTime(2025, 1, 2, 3, 4, 5), DateTime.Parse(item[7], CultureInfo.InvariantCulture));
    }

    [Fact]
    public void LoadSecurityDopSettings_ShouldSupportLegacyLineBasedFormat()
    {
        TesterServer server = CreateServerConfigured();
        string settingsPath = InvokeGetSecuritiesSettingsPath(server);

        using TesterServerSecurityDopFileScope scope = new TesterServerSecurityDopFileScope(settingsPath);

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "AAA$2$100$1.5$0.01$3$95$2025-01-02T03:04:05"
        });

        List<string[]> loaded = InvokePrivateLoadSecurityDopSettings(server, scope.SettingsPath);
        Assert.NotNull(loaded);
        Assert.Single(loaded);
        Assert.Equal("AAA", loaded[0][0]);
        Assert.Equal("2", loaded[0][1]);
        Assert.Equal("100", loaded[0][2]);
        Assert.Equal("1.5", loaded[0][3]);
    }

    private static TesterServer CreateServerConfigured()
    {
        TesterServer server = (TesterServer)RuntimeHelpers.GetUninitializedObject(typeof(TesterServer));

        SetPrivateField(server, "_sourceDataType", TesterSourceDataType.Folder);
        SetPrivateField(server, "_pathToFolder", Path.Combine(Path.GetFullPath("Engine"), "TestSecuritiesDop"));
        SetPrivateField(server, "_securities", new List<Security>
        {
            new Security
            {
                Name = "AAA",
                NameClass = "Stock"
            }
        });

        server.SecuritiesTester = new List<SecurityTester>
        {
            new SecurityTester
            {
                Security = new Security
                {
                    Name = "AAA",
                    NameClass = "Stock"
                }
            }
        };

        return server;
    }

    private static string InvokeGetSecuritiesSettingsPath(TesterServer server)
    {
        MethodInfo method = typeof(TesterServer).GetMethod("GetSecuritiesSettingsPath", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetSecuritiesSettingsPath not found.");
        return (string)method.Invoke(server, null)!;
    }

    private static List<string[]> InvokePrivateLoadSecurityDopSettings(TesterServer server, string path)
    {
        MethodInfo method = typeof(TesterServer).GetMethod("LoadSecurityDopSettings", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method LoadSecurityDopSettings not found.");
        return (List<string[]>)method.Invoke(server, new object[] { path })!;
    }

    private static void SetPrivateField(TesterServer server, string fieldName, object value)
    {
        FieldInfo field = typeof(TesterServer).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Field not found: " + fieldName);
        field.SetValue(server, value);
    }

    private sealed class TesterServerSecurityDopFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _folderExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public TesterServerSecurityDopFileScope(string settingsPath)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            FolderPath = Path.GetDirectoryName(settingsPath)!;
            SettingsPath = settingsPath;
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _folderExisted = Directory.Exists(FolderPath);
            if (!_folderExisted)
            {
                Directory.CreateDirectory(FolderPath);
            }

            _settingsFileExisted = File.Exists(SettingsPath);
            if (_settingsFileExisted)
            {
                File.Copy(SettingsPath, _settingsBackupPath, overwrite: true);
            }
            else if (File.Exists(_settingsBackupPath))
            {
                File.Delete(_settingsBackupPath);
            }
        }

        public string FolderPath { get; }

        public string SettingsPath { get; }

        public void Dispose()
        {
            if (_settingsFileExisted)
            {
                if (File.Exists(_settingsBackupPath))
                {
                    File.Copy(_settingsBackupPath, SettingsPath, overwrite: true);
                    File.Delete(_settingsBackupPath);
                }
            }
            else
            {
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }

                if (File.Exists(_settingsBackupPath))
                {
                    File.Delete(_settingsBackupPath);
                }
            }

            if (!_folderExisted
                && Directory.Exists(FolderPath)
                && !Directory.EnumerateFileSystemEntries(FolderPath).Any())
            {
                Directory.Delete(FolderPath);
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
