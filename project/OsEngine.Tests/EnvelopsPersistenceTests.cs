#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Charts.CandleChart.Indicators;
using Xunit;

namespace OsEngine.Tests;

public class EnvelopsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexEnvelopsJson";
        using EnvelopsFileScope scope = new EnvelopsFileScope(name);

        Envelops source = new Envelops(name, canDelete: true)
        {
            ColorUp = Color.DarkOrange,
            ColorDown = Color.DarkSlateBlue,
            PaintOn = false,
            Deviation = 2.25m
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        Envelops loaded = new Envelops(name, canDelete: true);

        Assert.Equal(Color.DarkOrange.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.DarkSlateBlue.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.False(loaded.PaintOn);
        Assert.Equal(2.25m, loaded.Deviation);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexEnvelopsLegacy";
        using EnvelopsFileScope scope = new EnvelopsFileScope(name);

        string legacy = string.Join(
            "\n",
            Color.OrangeRed.ToArgb().ToString(),
            Color.Navy.ToArgb().ToString(),
            "True",
            "1.75",
            "legacy-ignored-line") + "\n";
        File.WriteAllText(scope.SettingsPath, legacy);

        Envelops loaded = new Envelops(name, canDelete: true);

        Assert.Equal(Color.OrangeRed.ToArgb(), loaded.ColorUp.ToArgb());
        Assert.Equal(Color.Navy.ToArgb(), loaded.ColorDown.ToArgb());
        Assert.True(loaded.PaintOn);
        Assert.Equal(1.75m, loaded.Deviation);
    }

    private sealed class EnvelopsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly List<FileState> _states;

        public EnvelopsFileScope(string name)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            SettingsPath = Path.Combine(_engineDirPath, name + ".txt");
            _states = new List<FileState>
            {
                Capture(SettingsPath),
                Capture(Path.Combine(_engineDirPath, name + "maSignal.txt"))
            };
        }

        public string SettingsPath { get; }

        public void Dispose()
        {
            foreach (FileState state in _states)
            {
                if (state.FileExisted)
                {
                    if (File.Exists(state.BackupPath))
                    {
                        File.Copy(state.BackupPath, state.Path, overwrite: true);
                        File.Delete(state.BackupPath);
                    }
                }
                else
                {
                    if (File.Exists(state.Path))
                    {
                        File.Delete(state.Path);
                    }

                    if (File.Exists(state.BackupPath))
                    {
                        File.Delete(state.BackupPath);
                    }
                }
            }

            if (!_engineDirExisted
                && Directory.Exists(_engineDirPath)
                && !Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
            {
                Directory.Delete(_engineDirPath);
            }
        }

        private static FileState Capture(string path)
        {
            string backupPath = path + ".codex.bak";
            bool fileExisted = File.Exists(path);

            if (fileExisted)
            {
                File.Copy(path, backupPath, overwrite: true);
            }
            else if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            return new FileState(path, backupPath, fileExisted);
        }

        private sealed record FileState(string Path, string BackupPath, bool FileExisted);
    }
}
