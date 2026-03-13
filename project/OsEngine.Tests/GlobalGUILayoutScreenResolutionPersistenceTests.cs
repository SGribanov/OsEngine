#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

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
    public void SaveResolution_ShouldPersistToml_AndScreenSettingsCheckReturnTrue()
    {
        using ScreenResolutionFileScope scope = new ScreenResolutionFileScope();

        Screen primaryScreen = Screen.PrimaryScreen
            ?? throw new InvalidOperationException("Primary screen is not available.");

        int width = primaryScreen.Bounds.Size.Width;
        int height = primaryScreen.Bounds.Size.Height;
        int monitors = Screen.AllScreens.Length;

        scope.InvokeSaveResolution(width, height, monitors);

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("Width =", content);
        Assert.True(scope.InvokeScreenSettingsIsAllRight());
    }

    [Fact]
    public void ScreenSettingsIsAllRight_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        using ScreenResolutionFileScope scope = new ScreenResolutionFileScope();

        Screen primaryScreen = Screen.PrimaryScreen
            ?? throw new InvalidOperationException("Primary screen is not available.");

        int width = primaryScreen.Bounds.Size.Width;
        int height = primaryScreen.Bounds.Size.Height;
        int monitors = Screen.AllScreens.Length;

        File.WriteAllLines(scope.LegacyTxtPath, new[]
        {
            width.ToString(),
            height.ToString(),
            monitors.ToString()
        });

        Assert.True(scope.InvokeScreenSettingsIsAllRight());
        Assert.True(File.Exists(scope.CanonicalPath));
    }

    private sealed class ScreenResolutionFileScope : IDisposable
    {
        private readonly MethodInfo _saveResolutionMethod;
        private readonly MethodInfo _screenSettingsIsAllRightMethod;
        private readonly StructuredSettingsFileScope _settingsScope;

        public ScreenResolutionFileScope()
        {
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", "ScreenResolution.toml"));

            _saveResolutionMethod = typeof(GlobalGUILayout).GetMethod("SaveResolution", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method SaveResolution not found.");
            _screenSettingsIsAllRightMethod = typeof(GlobalGUILayout).GetMethod("ScreenSettingsIsAllRight", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ScreenSettingsIsAllRight not found.");
        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

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
            _settingsScope.Dispose();
        }
    }
}
