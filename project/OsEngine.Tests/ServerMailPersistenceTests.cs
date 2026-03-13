#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Logging;
using Xunit;

namespace OsEngine.Tests;

public class ServerMailPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistToml_AndLoadRoundTrip()
    {
        using ServerMailFileScope scope = new ServerMailFileScope();

        ServerMail source = ServerMail.GetServer();
        source.MyAdress = "sender@example.com";
        source.MyPassword = "mail_password";
        source.Smtp = "smtp.example.com";
        source.Adress = new[] { "one@example.com", "two@example.com" };
        source.Save();

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("MyAdress = \"sender@example.com\"", content);

        scope.ResetSingleton();
        ServerMail loaded = ServerMail.GetServer();

        Assert.Equal("sender@example.com", loaded.MyAdress);
        Assert.Equal("mail_password", loaded.MyPassword);
        Assert.Equal("smtp.example.com", loaded.Smtp);
        Assert.NotNull(loaded.Adress);
        Assert.Equal(2, loaded.Adress.Length);
        Assert.Equal("one@example.com", loaded.Adress[0]);
        Assert.Equal("two@example.com", loaded.Adress[1]);
        Assert.True(ServerMail.IsReady);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        using ServerMailFileScope scope = new ServerMailFileScope();

        File.WriteAllLines(scope.LegacyTxtPath, new[]
        {
            "legacy_sender@example.com",
            "legacy_password",
            "legacy.smtp.example.com",
            "first@example.com",
            "second@example.com"
        });

        scope.ResetSingleton();
        ServerMail loaded = ServerMail.GetServer();

        Assert.Equal("legacy_sender@example.com", loaded.MyAdress);
        Assert.Equal("legacy_password", loaded.MyPassword);
        Assert.Equal("legacy.smtp.example.com", loaded.Smtp);
        Assert.NotNull(loaded.Adress);
        Assert.Equal(2, loaded.Adress.Length);
        Assert.Equal("first@example.com", loaded.Adress[0]);
        Assert.Equal("second@example.com", loaded.Adress[1]);
        Assert.True(ServerMail.IsReady);

        loaded.Save();
        Assert.True(File.Exists(scope.CanonicalPath));
        Assert.Contains("Smtp = \"legacy.smtp.example.com\"", File.ReadAllText(scope.CanonicalPath));
    }

    private sealed class ServerMailFileScope : IDisposable
    {
        private readonly StructuredSettingsFileScope _settingsScope;
        private readonly FieldInfo _serverField;
        private readonly object? _originalServer;
        private readonly bool _originalIsReady;

        public ServerMailFileScope()
        {
            _serverField = typeof(ServerMail).GetField("_server", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field _server not found.");
            _originalServer = _serverField.GetValue(null);
            _originalIsReady = ServerMail.IsReady;

            _serverField.SetValue(null, null);
            ServerMail.IsReady = false;

            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", "mailSet.toml"));
        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

        public void ResetSingleton()
        {
            _serverField.SetValue(null, null);
            ServerMail.IsReady = false;
        }

        public void Dispose()
        {
            _serverField.SetValue(null, _originalServer);
            ServerMail.IsReady = _originalIsReady;

            _settingsScope.Dispose();
        }
    }
}
