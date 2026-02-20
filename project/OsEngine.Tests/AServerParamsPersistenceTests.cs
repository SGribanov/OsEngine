#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Market.Servers;
using OsEngine.Market.Servers.Entity;
using OsEngine.Market.Servers.YahooFinance;
using Xunit;

namespace OsEngine.Tests;

public class AServerParamsPersistenceTests
{
    [Fact]
    public void SaveParam_ShouldPersistJson_AndLoadParamRoundTrip()
    {
        using AServerParamsFileScope scope = new AServerParamsFileScope("YahooFinance");

        YahooServer source = scope.CreateServerWithoutConstructor();
        source.ServerParameters = new List<IServerParameter>
        {
            new ServerParameterString
            {
                Name = "ApiKey",
                Value = "json_value"
            }
        };

        scope.InvokePrivateSaveParam(source);

        string content = File.ReadAllText(scope.ParamsPath);
        Assert.StartsWith("{", content.TrimStart());

        YahooServer loaded = scope.CreateServerWithoutConstructor();
        IServerParameter loadedParam = scope.InvokePrivateLoadParam(
            loaded,
            new ServerParameterString
            {
                Name = "ApiKey",
                Value = "default"
            });

        ServerParameterString loadedString = Assert.IsType<ServerParameterString>(loadedParam);
        Assert.Equal("json_value", loadedString.Value);
    }

    [Fact]
    public void LoadParam_ShouldSupportLegacyLineBasedFormat()
    {
        using AServerParamsFileScope scope = new AServerParamsFileScope("YahooFinance");

        File.WriteAllLines(scope.ParamsPath, new[] { "String^ApiKey^legacy_value" });

        YahooServer loaded = scope.CreateServerWithoutConstructor();
        IServerParameter loadedParam = scope.InvokePrivateLoadParam(
            loaded,
            new ServerParameterString
            {
                Name = "ApiKey",
                Value = "default"
            });

        ServerParameterString loadedString = Assert.IsType<ServerParameterString>(loadedParam);
        Assert.Equal("legacy_value", loadedString.Value);
    }

    private sealed class AServerParamsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _paramsFileExisted;
        private readonly string _paramsBackupPath;
        private readonly FieldInfo _serverRealizationField;
        private readonly MethodInfo _saveParamMethod;
        private readonly MethodInfo _loadParamMethod;

        public AServerParamsFileScope(string serverNameUnique)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            ParamsPath = Path.Combine(_engineDirPath, serverNameUnique + "Params.txt");
            _paramsBackupPath = ParamsPath + ".codex.bak";

            _serverRealizationField = typeof(AServer).GetField("_serverRealization", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _serverRealization not found.");
            _saveParamMethod = typeof(AServer).GetMethod("SaveParam", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SaveParam not found.");
            _loadParamMethod = typeof(AServer).GetMethod("LoadParam", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadParam not found.");

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _paramsFileExisted = File.Exists(ParamsPath);
            if (_paramsFileExisted)
            {
                File.Copy(ParamsPath, _paramsBackupPath, overwrite: true);
            }
            else if (File.Exists(_paramsBackupPath))
            {
                File.Delete(_paramsBackupPath);
            }
        }

        public string ParamsPath { get; }

        public YahooServer CreateServerWithoutConstructor()
        {
            YahooServer server = (YahooServer)RuntimeHelpers.GetUninitializedObject(typeof(YahooServer));
            _serverRealizationField.SetValue(server, new YahooServerRealization());
            server.ServerParameters = new List<IServerParameter>();
            return server;
        }

        public void InvokePrivateSaveParam(YahooServer server)
        {
            _saveParamMethod.Invoke(server, null);
        }

        public IServerParameter InvokePrivateLoadParam(YahooServer server, IServerParameter parameter)
        {
            return (IServerParameter)_loadParamMethod.Invoke(server, new object[] { parameter })!;
        }

        public void Dispose()
        {
            if (_paramsFileExisted)
            {
                if (File.Exists(_paramsBackupPath))
                {
                    File.Copy(_paramsBackupPath, ParamsPath, overwrite: true);
                    File.Delete(_paramsBackupPath);
                }
            }
            else
            {
                if (File.Exists(ParamsPath))
                {
                    File.Delete(ParamsPath);
                }

                if (File.Exists(_paramsBackupPath))
                {
                    File.Delete(_paramsBackupPath);
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
