using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Layout;
using Xunit;

namespace OsEngine.Tests;

public class GlobalGUILayoutPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using GlobalLayoutFileScope scope = new GlobalLayoutFileScope();

        GlobalGUILayout.UiOpenWindows = new List<OpenWindow>
        {
            new OpenWindow
            {
                Name = "MainWindow",
                Layout = new OpenWindowLayout
                {
                    Height = 600,
                    Widht = 1200,
                    Left = 100,
                    Top = 80,
                    IsExpand = false
                }
            }
        };

        scope.InvokeSave();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        GlobalGUILayout.UiOpenWindows = new List<OpenWindow>();
        scope.InvokeLoad();

        Assert.Single(GlobalGUILayout.UiOpenWindows);
        Assert.Equal("MainWindow", GlobalGUILayout.UiOpenWindows[0].Name);
        Assert.Equal(600, GlobalGUILayout.UiOpenWindows[0].Layout.Height);
        Assert.Equal(1200, GlobalGUILayout.UiOpenWindows[0].Layout.Widht);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using GlobalLayoutFileScope scope = new GlobalLayoutFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "LegacyWindow#500$10$20$900$True"
        });

        GlobalGUILayout.UiOpenWindows = new List<OpenWindow>();
        scope.InvokeLoad();

        Assert.Single(GlobalGUILayout.UiOpenWindows);
        Assert.Equal("LegacyWindow", GlobalGUILayout.UiOpenWindows[0].Name);
        Assert.Equal(500, GlobalGUILayout.UiOpenWindows[0].Layout.Height);
        Assert.True(GlobalGUILayout.UiOpenWindows[0].Layout.IsExpand);
    }

    private sealed class GlobalLayoutFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly MethodInfo _saveMethod;
        private readonly MethodInfo _loadMethod;
        private readonly List<OpenWindow> _originalWindows;

        public GlobalLayoutFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "LayoutGui.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _saveMethod = typeof(GlobalGUILayout).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method Save not found.");
            _loadMethod = typeof(GlobalGUILayout).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method Load not found.");
            _originalWindows = GlobalGUILayout.UiOpenWindows;

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _settingsFileExisted = File.Exists(SettingsPath);
            if (_settingsFileExisted)
            {
                File.Copy(SettingsPath, _settingsBackup, overwrite: true);
            }
            else if (File.Exists(_settingsBackup))
            {
                File.Delete(_settingsBackup);
            }
        }

        public string SettingsPath { get; }

        public void InvokeSave()
        {
            _saveMethod.Invoke(null, null);
        }

        public void InvokeLoad()
        {
            _loadMethod.Invoke(null, null);
        }

        public void Dispose()
        {
            GlobalGUILayout.UiOpenWindows = _originalWindows;

            if (_settingsFileExisted)
            {
                if (File.Exists(_settingsBackup))
                {
                    File.Copy(_settingsBackup, SettingsPath, overwrite: true);
                    File.Delete(_settingsBackup);
                }
            }
            else
            {
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }

                if (File.Exists(_settingsBackup))
                {
                    File.Delete(_settingsBackup);
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
