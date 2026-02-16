using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Market.AutoFollow;
using Xunit;

namespace OsEngine.Tests;

public class CopyMasterHubPersistenceTests
{
    [Fact]
    public void SaveCopyTraders_ShouldPersistJson_AndLoadRoundTrip()
    {
        using CopyMasterHubFileScope scope = new CopyMasterHubFileScope();

        CopyMaster source = new CopyMaster();
        source.CopyTraders.Add(scope.CreateFakeCopyTrader(5, "TraderA"));
        source.SaveCopyTraders();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        CopyMaster target = new CopyMaster();
        scope.InvokePrivateLoadCopyTraders(target);

        Assert.Single(target.CopyTraders);
        Assert.Equal(5, target.CopyTraders[0].Number);
        Assert.Equal("TraderA", target.CopyTraders[0].Name);
        Assert.False(target.CopyTraders[0].IsOn);

        scope.StopLoadedCopyTraders(target);
    }

    [Fact]
    public void LoadCopyTraders_ShouldSupportLegacyLineBasedFormat()
    {
        using CopyMasterHubFileScope scope = new CopyMasterHubFileScope();

        CopyTrader legacyTrader = scope.CreateFakeCopyTrader(9, "LegacyTrader");
        File.WriteAllText(scope.SettingsPath, legacyTrader.GetStringToSave());

        CopyMaster target = new CopyMaster();
        scope.InvokePrivateLoadCopyTraders(target);

        Assert.Single(target.CopyTraders);
        Assert.Equal(9, target.CopyTraders[0].Number);
        Assert.Equal("LegacyTrader", target.CopyTraders[0].Name);
        Assert.False(target.CopyTraders[0].IsOn);

        scope.StopLoadedCopyTraders(target);
    }

    private sealed class CopyMasterHubFileScope : IDisposable
    {
        private readonly string _copyTraderDirPath;
        private readonly bool _copyTraderDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _loadCopyTradersMethod;

        public CopyMasterHubFileScope()
        {
            _copyTraderDirPath = Path.GetFullPath(Path.Combine("Engine", "CopyTrader"));
            SettingsPath = Path.Combine(_copyTraderDirPath, "CopyTradersHub.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _loadCopyTradersMethod = typeof(CopyMaster).GetMethod("LoadCopyTraders", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadCopyTraders not found.");

            _copyTraderDirExisted = Directory.Exists(_copyTraderDirPath);
            if (!_copyTraderDirExisted)
            {
                Directory.CreateDirectory(_copyTraderDirPath);
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

        public CopyTrader CreateFakeCopyTrader(int number, string name)
        {
            CopyTrader trader = (CopyTrader)RuntimeHelpers.GetUninitializedObject(typeof(CopyTrader));
            trader.Number = number;
            trader.Name = name;
            trader.IsOn = false;
            trader.PanelsPosition = "1,1,1,1,1";
            trader.MasterRobotsNames = new List<string>();
            trader.PortfolioToCopy = new List<PortfolioToCopy>();
            return trader;
        }

        public void InvokePrivateLoadCopyTraders(CopyMaster master)
        {
            _loadCopyTradersMethod.Invoke(master, null);
        }

        public void StopLoadedCopyTraders(CopyMaster master)
        {
            for (int i = 0; i < master.CopyTraders.Count; i++)
            {
                master.CopyTraders[i].ClearDelete();
            }
        }

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

            if (!_copyTraderDirExisted
                && Directory.Exists(_copyTraderDirPath)
                && !Directory.EnumerateFileSystemEntries(_copyTraderDirPath).Any())
            {
                Directory.Delete(_copyTraderDirPath);
            }
        }
    }
}
