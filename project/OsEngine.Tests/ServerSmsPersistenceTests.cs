#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Logging;
using Xunit;

namespace OsEngine.Tests;

public class ServerSmsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistToml_AndLoadRoundTrip()
    {
        using ServerSmsFileScope scope = new ServerSmsFileScope();

        ServerSms source = ServerSms.GetSmsServer();
        source.SmscLogin = "json_login";
        source.SmscPassword = "json_password";
        source.Phones = "+10000000000";
        source.Save();

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("SmscLogin = \"json_login\"", content);

        scope.ResetSingleton();
        ServerSms loaded = ServerSms.GetSmsServer();

        Assert.Equal("json_login", loaded.SmscLogin);
        Assert.Equal("json_password", loaded.SmscPassword);
        Assert.Equal("+10000000000", loaded.Phones);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        using ServerSmsFileScope scope = new ServerSmsFileScope();

        File.WriteAllLines(scope.LegacyTxtPath, new[]
        {
            "legacy_login",
            "legacy_password",
            "+79990000000"
        });

        scope.ResetSingleton();
        ServerSms loaded = ServerSms.GetSmsServer();

        Assert.Equal("legacy_login", loaded.SmscLogin);
        Assert.Equal("legacy_password", loaded.SmscPassword);
        Assert.Equal("+79990000000", loaded.Phones);

        loaded.Save();
        Assert.True(File.Exists(scope.CanonicalPath));
        Assert.Contains("Phones = \"+79990000000\"", File.ReadAllText(scope.CanonicalPath));
    }

    private sealed class ServerSmsFileScope : IDisposable
    {
        private readonly StructuredSettingsFileScope _settingsScope;
        private readonly FieldInfo _serverField;
        private readonly object? _originalServer;

        public ServerSmsFileScope()
        {
            _serverField = typeof(ServerSms).GetField("_server", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field _server not found.");
            _originalServer = _serverField.GetValue(null);
            _serverField.SetValue(null, null);
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", "smsSet.toml"));
        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

        public void ResetSingleton()
        {
            _serverField.SetValue(null, null);
        }

        public void Dispose()
        {
            _serverField.SetValue(null, _originalServer);

            _settingsScope.Dispose();
        }
    }
}
