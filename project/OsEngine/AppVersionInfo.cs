#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Reflection;

namespace OsEngine
{
    public static class AppVersionInfo
    {
        private static readonly string _displayVersion = ResolveDisplayVersion();

        public static string DisplayVersion => _displayVersion;

        private static string ResolveDisplayVersion()
        {
            Assembly assembly = typeof(AppVersionInfo).Assembly;

            string infoVersion = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            if (string.IsNullOrWhiteSpace(infoVersion) == false)
            {
                return infoVersion;
            }

            return assembly.GetName().Version?.ToString() ?? "unknown";
        }
    }
}
