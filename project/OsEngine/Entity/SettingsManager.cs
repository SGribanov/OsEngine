/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.IO;
using System.Text.Json;
using Tomlyn;

#nullable enable

namespace OsEngine.Entity
{
    /// <summary>
    /// Structured settings persistence with TOML as canonical format and
    /// JSON/legacy fallback support for backward compatibility.
    /// </summary>
    public static class SettingsManager
    {
        private const string TomlExtension = ".toml";

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

            if (IsTomlPath(path))
            {
                string toml = TomlSerializer.Serialize(settings);
                SafeFileWriter.WriteAllText(path, toml);
                return;
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

            if (IsTomlPath(path))
            {
                if (TryLoadToml(path, defaultValue, out T? tomlModel))
                {
                    return tomlModel;
                }

                string jsonPath = Path.ChangeExtension(path, ".json");
                if (TryLoadJsonOrLegacy(jsonPath, defaultValue, legacyLoader, options, out T? jsonModel))
                {
                    return jsonModel;
                }

                string txtPath = Path.ChangeExtension(path, ".txt");
                if (TryLoadJsonOrLegacy(txtPath, defaultValue, legacyLoader, options, out T? txtModel))
                {
                    return txtModel;
                }

                return defaultValue;
            }

            if (TryLoadJsonOrLegacy(path, defaultValue, legacyLoader, options, out T? model))
            {
                return model;
            }

            return defaultValue;
        }

        private static bool TryLoadToml<T>(string path, T? defaultValue, out T? model)
        {
            model = defaultValue;

            string? content = TryReadContent(path);
            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            try
            {
                T? loaded = TomlSerializer.Deserialize<T>(content);

                if (loaded == null)
                {
                    return false;
                }

                model = loaded;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryLoadJsonOrLegacy<T>(
            string path,
            T? defaultValue,
            Func<string, T?>? legacyLoader,
            JsonSerializerOptions? options,
            out T? model)
        {
            model = defaultValue;

            string? content = TryReadContent(path);
            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            try
            {
                T? loaded = JsonSerializer.Deserialize<T>(content, options ?? DefaultOptions);

                if (loaded == null)
                {
                    return false;
                }

                model = loaded;
                return true;
            }
            catch
            {
                if (legacyLoader != null)
                {
                    try
                    {
                        T? legacyModel = legacyLoader(content);
                        if (legacyModel == null)
                        {
                            return false;
                        }

                        model = legacyModel;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                return false;
            }
        }

        private static string? TryReadContent(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return File.ReadAllText(path);
            }
            catch
            {
                return null;
            }
        }

        private static bool IsTomlPath(string path)
        {
            return string.Equals(Path.GetExtension(path), TomlExtension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
