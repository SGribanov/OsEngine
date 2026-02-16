using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Market.Proxy;
using Xunit;

namespace OsEngine.Tests;

public class ProxyMasterProxyHubPersistenceTests
{
    [Fact]
    public void SaveProxy_ShouldPersistJson_AndLoadRoundTrip()
    {
        using ProxyHubFileScope scope = new ProxyHubFileScope();

        ProxyMaster source = new ProxyMaster();
        source.Proxies.Add(new ProxyOsa
        {
            IsOn = true,
            Number = 3,
            Ip = "127.0.0.1",
            Port = 8080,
            Login = "user",
            UserPassword = "pass",
            Location = "US_NewYork",
            AutoPingLastStatus = "Connect",
            PingWebAddress = "http://example.com"
        });

        source.SaveProxy();

        string content = File.ReadAllText(scope.ProxyHubPath);
        Assert.StartsWith("{", content.TrimStart());

        ProxyMaster target = new ProxyMaster();
        scope.InvokePrivateLoadProxy(target);

        Assert.Single(target.Proxies);
        ProxyOsa loaded = target.Proxies[0];
        Assert.True(loaded.IsOn);
        Assert.Equal(3, loaded.Number);
        Assert.Equal("127.0.0.1", loaded.Ip);
        Assert.Equal(8080, loaded.Port);
        Assert.Equal("user", loaded.Login);
        Assert.Equal("pass", loaded.UserPassword);
        Assert.Equal("US_NewYork", loaded.Location);
        Assert.Equal("Connect", loaded.AutoPingLastStatus);
        Assert.Equal("http://example.com", loaded.PingWebAddress);
    }

    [Fact]
    public void LoadProxy_ShouldSupportLegacyLineBasedFormat()
    {
        using ProxyHubFileScope scope = new ProxyHubFileScope();

        ProxyOsa legacyProxy = new ProxyOsa
        {
            IsOn = false,
            Number = 11,
            Ip = "10.10.10.10",
            Port = 3128,
            Login = "legacy",
            UserPassword = "legacy-pass",
            Location = "RU_Moscow",
            AutoPingLastStatus = "Unknown",
            PingWebAddress = "http://ipinfo.io/"
        };
        File.WriteAllText(scope.ProxyHubPath, legacyProxy.GetStringToSave());

        ProxyMaster target = new ProxyMaster();
        scope.InvokePrivateLoadProxy(target);

        Assert.Single(target.Proxies);
        ProxyOsa loaded = target.Proxies[0];
        Assert.False(loaded.IsOn);
        Assert.Equal(11, loaded.Number);
        Assert.Equal("10.10.10.10", loaded.Ip);
        Assert.Equal(3128, loaded.Port);
        Assert.Equal("legacy", loaded.Login);
        Assert.Equal("legacy-pass", loaded.UserPassword);
        Assert.Equal("RU_Moscow", loaded.Location);
        Assert.Equal("Unknown", loaded.AutoPingLastStatus);
        Assert.Equal("http://ipinfo.io/", loaded.PingWebAddress);
    }

    private sealed class ProxyHubFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _proxyHubFileExisted;
        private readonly string _proxyHubBackupPath;
        private readonly MethodInfo _loadProxyMethod;

        public ProxyHubFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            ProxyHubPath = Path.Combine(_engineDirPath, "ProxyHub.txt");
            _proxyHubBackupPath = ProxyHubPath + ".codex.bak";

            _loadProxyMethod = typeof(ProxyMaster).GetMethod("LoadProxy", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadProxy not found.");

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _proxyHubFileExisted = File.Exists(ProxyHubPath);
            if (_proxyHubFileExisted)
            {
                File.Copy(ProxyHubPath, _proxyHubBackupPath, overwrite: true);
            }
            else if (File.Exists(_proxyHubBackupPath))
            {
                File.Delete(_proxyHubBackupPath);
            }
        }

        public string ProxyHubPath { get; }

        public void InvokePrivateLoadProxy(ProxyMaster target)
        {
            _loadProxyMethod.Invoke(target, null);
        }

        public void Dispose()
        {
            if (_proxyHubFileExisted)
            {
                if (File.Exists(_proxyHubBackupPath))
                {
                    File.Copy(_proxyHubBackupPath, ProxyHubPath, overwrite: true);
                    File.Delete(_proxyHubBackupPath);
                }
            }
            else
            {
                if (File.Exists(ProxyHubPath))
                {
                    File.Delete(ProxyHubPath);
                }

                if (File.Exists(_proxyHubBackupPath))
                {
                    File.Delete(_proxyHubBackupPath);
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
