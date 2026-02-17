/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.IO;
using System.Text.Json;

#nullable enable

namespace OsEngine.Entity
{
    /// <summary>
    /// Json-based settings persistence with optional legacy fallback loader.
    /// </summary>
    public static class SettingsManager
    {
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static void Save<T>(string path, T settings, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            string json = JsonSerializer.Serialize(settings, options ?? DefaultOptions);
            SafeFileWriter.WriteAllText(path, json);
        }

        public static T? Load<T>(
            string path,
            T? defaultValue = default,
            Func<string, T?>? legacyLoader = null,
            JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return defaultValue;
            }

            if (!File.Exists(path))
            {
                return defaultValue;
            }

            string content;

            try
            {
                content = File.ReadAllText(path);
            }
            catch
            {
                return defaultValue;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return defaultValue;
            }

            try
            {
                T? model = JsonSerializer.Deserialize<T>(content, options ?? DefaultOptions);

                if (model == null)
                {
                    return defaultValue;
                }

                return model;
            }
            catch
            {
                if (legacyLoader != null)
                {
                    try
                    {
                        return legacyLoader(content);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }
    }
}
