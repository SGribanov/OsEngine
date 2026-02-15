using System;
using System.Reflection;
using OsEngine.Entity;
using OsEngine.Entity.WebSocketOsEngine;
using OsEngine.Market.Servers.Entity;
using Xunit;

namespace OsEngine.Tests;

public class SecurityRefactorTests
{
    [Fact]
    public void CredentialProtector_ProtectAndTryUnprotect_ShouldRoundTrip()
    {
        string plain = "secret_value_123";

        string stored = CredentialProtector.Protect(plain);
        bool decrypted = CredentialProtector.TryUnprotect(stored, out string restored);

        Assert.True(decrypted);
        Assert.Equal(plain, restored);
    }

    [Fact]
    public void ServerParameterPassword_LoadLegacyPlainText_ShouldSetMigrationFlag()
    {
        ServerParameterPassword param = new ServerParameterPassword();

        param.LoadFromStr("Password^ApiSecret^legacy_plain_secret");

        Assert.Equal("ApiSecret", param.Name);
        Assert.Equal("legacy_plain_secret", param.Value);
        Assert.True(param.NeedMigrationSave);

        string saved = param.GetStringToSave();
        Assert.StartsWith("Password^ApiSecret^" + CredentialProtector.Prefix, saved, StringComparison.Ordinal);
    }

    [Fact]
    public void ServerParameterPassword_LoadEncryptedValue_ShouldNotRequireMigration()
    {
        ServerParameterPassword source = new ServerParameterPassword
        {
            Name = "ApiSecret",
            Value = "encrypted_secret"
        };

        string saved = source.GetStringToSave();

        ServerParameterPassword loaded = new ServerParameterPassword();
        loaded.LoadFromStr(saved);

        Assert.Equal("ApiSecret", loaded.Name);
        Assert.Equal("encrypted_secret", loaded.Value);
        Assert.False(loaded.NeedMigrationSave);
    }

    [Fact]
    public void WebSocket_IgnoreSslErrors_Property_ShouldBeMarkedObsolete()
    {
        PropertyInfo property = typeof(WebSocket).GetProperty(nameof(WebSocket.IgnoreSslErrors));
        Assert.NotNull(property);

        ObsoleteAttribute attribute = property.GetCustomAttribute<ObsoleteAttribute>();
        Assert.NotNull(attribute);
        Assert.Contains("security risk", attribute.Message, StringComparison.OrdinalIgnoreCase);
    }
}

