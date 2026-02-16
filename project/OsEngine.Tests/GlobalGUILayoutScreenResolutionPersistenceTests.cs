using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OsEngine.Layout;
using Xunit;

namespace OsEngine.Tests;

public class GlobalGUILayoutScreenResolutionPersistenceTests
{
    [Fact]
    public void SaveResolution_ShouldPersistJson_AndScreenSettingsCheckReturnTrue()
    {
        using ScreenResolutionFileScope scope = new ScreenResolutionFileScope();

        Screen primaryScreen = Screen.PrimaryScreen
            ?? throw new InvalidOperationException("Primary screen is not available.");

        int width = primaryScreen.Bounds.Size.Width;
        int height = primaryScreen.Bounds.Size.Height;
        int monitors = Screen.AllScreens.Length;

        scope.InvokeSaveResolution(width, height, monitors);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());
        Assert.True(scope.InvokeScreenSettingsIsAllRight());
    }

    [Fact]
    public void ScreenSettingsIsAllRight_ShouldSupportLegacyLineBasedFormat()
    {
        using ScreenResolutionFileScope scope = new ScreenResolutionFileScope();

        Screen primaryScreen = Screen.PrimaryScreen
            ?? throw new InvalidOperationException("Primary screen is not available.");

        int width = primaryScreen.Bounds.Size.Width;
        int height = primaryScreen.Bounds.Size.Height;
        int monitors = Screen.AllScreens.Length;

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            width.ToString(),
            height.ToString(),
            monitors.ToString()
        });

        Assert.True(scope.InvokeScreenSettingsIsAllRight());
    }

    private sealed class ScreenResolutionFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly MethodInfo _saveResolutionMethod;
        private readonly MethodInfo _screenSettingsIsAllRightMethod;

        public ScreenResolutionFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "ScreenResolution.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _saveResolutionMethod = typeof(GlobalGUILayout).GetMethod("SaveResolution", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method SaveResolution not found.");
            _screenSettingsIsAllRightMethod = typeof(GlobalGUILayout).GetMethod("ScreenSettingsIsAllRight", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ScreenSettingsIsAllRight not found.");

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

        public void InvokeSaveResolution(int width, int height, int monitors)
        {
            _saveResolutionMethod.Invoke(null, new object[] { width, height, monitors });
        }

        public bool InvokeScreenSettingsIsAllRight()
        {
            return (bool)_screenSettingsIsAllRightMethod.Invoke(null, null)!;
        }

        public void Dispose()
        {
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
