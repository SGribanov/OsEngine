#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using OsEngine.Market.Servers.Tester;
using OsEngine.OsOptimizer.OptEntity;
using Xunit;

namespace OsEngine.Tests;

[Collection("OptimizerSettingsFileSerial")]
public class OptimizerSettingsCollectionsPersistenceTests
{
    private static readonly object FilesLock = new object();

    [Fact]
    public void Stage2Step2_2_OptimizerSettings_LoadClearingInfo_ShouldSupportLegacyLineBasedPayload()
    {
        lock (FilesLock)
        {
            using OptimizerSettingsCollectionsFileScope scope = new OptimizerSettingsCollectionsFileScope();

            OrderClearing first = new OrderClearing
            {
                Time = new DateTime(2000, 1, 1, 10, 15, 0),
                IsOn = true
            };
            string legacyRuDate = "01.01.2000 19:30:00$False";

            File.WriteAllLines(scope.ClearingsPath, new[]
            {
                first.GetSaveString(),
                legacyRuDate
            });

            OptimizerSettings settings = new OptimizerSettings();

            Assert.Equal(2, settings.ClearingTimes.Count);
            Assert.Equal(first.Time, settings.ClearingTimes[0].Time);
            Assert.True(settings.ClearingTimes[0].IsOn);
            Assert.Equal(new DateTime(2000, 1, 1, 19, 30, 0), settings.ClearingTimes[1].Time);
            Assert.False(settings.ClearingTimes[1].IsOn);
        }
    }

    [Fact]
    public void Stage2Step2_2_OptimizerSettings_LoadNonTradePeriods_ShouldSupportLegacyLineBasedPayload()
    {
        lock (FilesLock)
        {
            using OptimizerSettingsCollectionsFileScope scope = new OptimizerSettingsCollectionsFileScope();

            NonTradePeriod first = new NonTradePeriod
            {
                DateStart = new DateTime(2025, 1, 10, 9, 0, 0),
                DateEnd = new DateTime(2025, 1, 10, 10, 0, 0),
                IsOn = true
            };
            string legacyRuDate = "11.01.2025 12:00:00$11.01.2025 13:45:00$False";

            File.WriteAllLines(scope.NonTradePeriodsPath, new[]
            {
                first.GetSaveString(),
                legacyRuDate
            });

            OptimizerSettings settings = new OptimizerSettings();

            Assert.Equal(2, settings.NonTradePeriods.Count);
            Assert.Equal(first.DateStart, settings.NonTradePeriods[0].DateStart);
            Assert.Equal(first.DateEnd, settings.NonTradePeriods[0].DateEnd);
            Assert.True(settings.NonTradePeriods[0].IsOn);
            Assert.Equal(new DateTime(2025, 11, 1, 12, 0, 0), settings.NonTradePeriods[1].DateStart);
            Assert.Equal(new DateTime(2025, 11, 1, 13, 45, 0), settings.NonTradePeriods[1].DateEnd);
            Assert.False(settings.NonTradePeriods[1].IsOn);
        }
    }

    [Fact]
    public void Stage2Step2_2_OptimizerSettings_SaveCollections_ShouldRoundTrip()
    {
        lock (FilesLock)
        {
            using OptimizerSettingsCollectionsFileScope scope = new OptimizerSettingsCollectionsFileScope();

            OptimizerSettings writer = new OptimizerSettings();
            writer.ClearingTimes = new List<OrderClearing>
            {
                new OrderClearing { Time = new DateTime(2000, 1, 1, 11, 0, 0), IsOn = true },
                new OrderClearing { Time = new DateTime(2000, 1, 1, 21, 30, 0), IsOn = false }
            };
            writer.NonTradePeriods = new List<NonTradePeriod>
            {
                new NonTradePeriod
                {
                    DateStart = new DateTime(2025, 2, 1, 8, 0, 0),
                    DateEnd = new DateTime(2025, 2, 1, 9, 30, 0),
                    IsOn = true
                },
                new NonTradePeriod
                {
                    DateStart = new DateTime(2025, 2, 2, 14, 0, 0),
                    DateEnd = new DateTime(2025, 2, 2, 16, 0, 0),
                    IsOn = false
                }
            };

            writer.SaveClearingInfo();
            writer.SaveNonTradePeriods();

            OptimizerSettings reader = new OptimizerSettings();

            Assert.Equal(2, reader.ClearingTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 11, 0, 0), reader.ClearingTimes[0].Time);
            Assert.True(reader.ClearingTimes[0].IsOn);
            Assert.Equal(new DateTime(2000, 1, 1, 21, 30, 0), reader.ClearingTimes[1].Time);
            Assert.False(reader.ClearingTimes[1].IsOn);

            Assert.Equal(2, reader.NonTradePeriods.Count);
            Assert.Equal(new DateTime(2025, 2, 1, 8, 0, 0), reader.NonTradePeriods[0].DateStart);
            Assert.Equal(new DateTime(2025, 2, 1, 9, 30, 0), reader.NonTradePeriods[0].DateEnd);
            Assert.True(reader.NonTradePeriods[0].IsOn);
            Assert.Equal(new DateTime(2025, 2, 2, 14, 0, 0), reader.NonTradePeriods[1].DateStart);
            Assert.Equal(new DateTime(2025, 2, 2, 16, 0, 0), reader.NonTradePeriods[1].DateEnd);
            Assert.False(reader.NonTradePeriods[1].IsOn);
        }
    }

    private sealed class OptimizerSettingsCollectionsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly Dictionary<string, string> _backupByPath = new Dictionary<string, string>();
        private readonly HashSet<string> _fileExistedBefore = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public OptimizerSettingsCollectionsFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            ClearingsPath = Path.Combine(_engineDirPath, "OptimizerMasterClearings.txt");
            NonTradePeriodsPath = Path.Combine(_engineDirPath, "OptimizerMasterNonTradePeriods.txt");

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            BackupFile(ClearingsPath);
            BackupFile(NonTradePeriodsPath);
        }

        public string ClearingsPath { get; }
        public string NonTradePeriodsPath { get; }

        public void Dispose()
        {
            RestoreFile(ClearingsPath);
            RestoreFile(NonTradePeriodsPath);

            if (!_engineDirExisted
                && Directory.Exists(_engineDirPath)
                && !Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
            {
                Directory.Delete(_engineDirPath);
            }
        }

        private void BackupFile(string path)
        {
            string backupPath = path + ".codex.bak";
            _backupByPath[path] = backupPath;

            if (File.Exists(path))
            {
                _fileExistedBefore.Add(path);
                File.Copy(path, backupPath, overwrite: true);
            }
            else if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }

        private void RestoreFile(string path)
        {
            string backupPath = _backupByPath[path];

            if (_fileExistedBefore.Contains(path))
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

[CollectionDefinition("OptimizerSettingsFileSerial", DisableParallelization = true)]
public class OptimizerSettingsFileSerialCollection
{
}
