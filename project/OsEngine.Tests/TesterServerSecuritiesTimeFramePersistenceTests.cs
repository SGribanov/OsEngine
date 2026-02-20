#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Market.Servers.Tester;
using Xunit;

namespace OsEngine.Tests;

public class TesterServerSecuritiesTimeFramePersistenceTests
{
    [Fact]
    public void SaveSetSecuritiesTimeFrameSettings_ShouldPersistJson_AndLoadRoundTrip()
    {
        TesterServer source = CreateServer();
        ConfigureFolderMode(source);
        source.SecuritiesTester = new List<SecurityTester>
        {
            new SecurityTester
            {
                Security = new Security { Name = "AAA" },
                TimeFrame = TimeFrame.Min5
            },
            new SecurityTester
            {
                Security = new Security { Name = "BBB" },
                TimeFrame = TimeFrame.Min15
            }
        };

        using TesterServerSecuritiesTfFileScope scope = new TesterServerSecuritiesTfFileScope(InvokeGetSettingsPath(source));

        source.SaveSetSecuritiesTimeFrameSettings();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        TesterServer loaded = CreateServer();
        ConfigureFolderMode(loaded);
        loaded.SecuritiesTester = new List<SecurityTester>
        {
            new SecurityTester
            {
                Security = new Security { Name = "AAA" },
                TimeFrame = TimeFrame.Sec1
            },
            new SecurityTester
            {
                Security = new Security { Name = "BBB" },
                TimeFrame = TimeFrame.Sec1
            }
        };

        InvokePrivateLoadSetSecuritiesTimeFrameSettings(loaded);

        Assert.Equal(TimeFrame.Min5, loaded.SecuritiesTester[0].TimeFrame);
        Assert.Equal(TimeFrame.Min15, loaded.SecuritiesTester[1].TimeFrame);
    }

    [Fact]
    public void LoadSetSecuritiesTimeFrameSettings_ShouldSupportLegacyLineBasedFormat()
    {
        TesterServer loaded = CreateServer();
        ConfigureFolderMode(loaded);
        loaded.SecuritiesTester = new List<SecurityTester>
        {
            new SecurityTester
            {
                Security = new Security { Name = "AAA" },
                TimeFrame = TimeFrame.Sec1
            },
            new SecurityTester
            {
                Security = new Security { Name = "BBB" },
                TimeFrame = TimeFrame.Sec1
            }
        };

        string settingsPath = InvokeGetSettingsPath(loaded);
        using TesterServerSecuritiesTfFileScope scope = new TesterServerSecuritiesTfFileScope(settingsPath);

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "AAA#Min30",
            "BBB#Hour1"
        });

        InvokePrivateLoadSetSecuritiesTimeFrameSettings(loaded);

        Assert.Equal(TimeFrame.Min30, loaded.SecuritiesTester[0].TimeFrame);
        Assert.Equal(TimeFrame.Hour1, loaded.SecuritiesTester[1].TimeFrame);
    }

    private static TesterServer CreateServer()
    {
        return (TesterServer)RuntimeHelpers.GetUninitializedObject(typeof(TesterServer));
    }

    private static void ConfigureFolderMode(TesterServer server)
    {
        SetPrivateField(server, "_sourceDataType", TesterSourceDataType.Folder);
        SetPrivateField(server, "_typeTesterData", TesterDataType.Candle);
        SetPrivateField(server, "_pathToFolder", "C:\\Data\\TfSettings");
    }

    private static string InvokeGetSettingsPath(TesterServer server)
    {
        MethodInfo method = typeof(TesterServer).GetMethod("GetSecuritiesTimeFrameSettingsPath", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method GetSecuritiesTimeFrameSettingsPath not found.");
        return (string)method.Invoke(server, null)!;
    }

    private static void InvokePrivateLoadSetSecuritiesTimeFrameSettings(TesterServer server)
    {
        MethodInfo method = typeof(TesterServer).GetMethod("LoadSetSecuritiesTimeFrameSettings", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method LoadSetSecuritiesTimeFrameSettings not found.");
        method.Invoke(server, null);
    }

    private static void SetPrivateField(TesterServer server, string fieldName, object value)
    {
        FieldInfo field = typeof(TesterServer).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Field not found: " + fieldName);
        field.SetValue(server, value);
    }

    private sealed class TesterServerSecuritiesTfFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public TesterServerSecuritiesTfFileScope(string settingsPath)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = settingsPath;
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
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

            if (!_engineDirExisted
                && Directory.Exists(_engineDirPath)
                && !Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
            {
                Directory.Delete(_engineDirPath);
            }
        }
    }
}
