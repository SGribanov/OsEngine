#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Diagnostics;
using System.IO;
using OsEngine.Entity;

namespace OsEngine.PrimeSettings
{
    public class PrimeSettingsMaster
    {

        public static bool ErrorLogMessageBoxIsActive
        {
            get
            {
                if (_isLoad == false)
                {
                    Load();
                }
                return _errorLogMessageBoxIsActive;
            }
            set
            {
                _errorLogMessageBoxIsActive = value;
                Save();
            }
        }
        private static bool _errorLogMessageBoxIsActive = true;

        public static bool ErrorLogBeepIsActive
        {
            get
            {
                if (_isLoad == false)
                {
                    Load();
                }
                return _errorLogBeepIsActive;
            }
            set { _errorLogBeepIsActive = value; Save(); }
        }
        private static bool _errorLogBeepIsActive = true;

        public static bool TransactionBeepIsActive
        {
            get
            {
                if (_isLoad == false)
                {
                    Load();
                }
                return _transactionBeepIsActive;
            }
            set
            {
                _transactionBeepIsActive = value;
                Save();
            }
        }
        private static bool _transactionBeepIsActive;

        public static bool RebootTradeUiLight
        {
            get
            {
                if (_isLoad == false)
                {
                    Load();
                }
                return _rebootTradeUiLight;
            }
            set
            {
                _rebootTradeUiLight = value;
                Save();
            }
        }
        private static bool _rebootTradeUiLight;

        public static bool ReportCriticalErrors
        {
            get
            {
                return _reportCriticalErrors;
            }
            set
            {
                if(_reportCriticalErrors == value)
                {
                    return;
                }
                _reportCriticalErrors = value;
                Save();
            }
        }
        private static bool _reportCriticalErrors = true;

        public static string LabelInHeaderBotStation
        {
            get
            {
                if (_isLoad == false)
                {
                    Load();
                }
                return _labelInHeaderBotStation;
            }
            set
            {
                _labelInHeaderBotStation = value;
                Save();
            }
        }
        private static string _labelInHeaderBotStation;

        public static MemoryCleanerRegime MemoryCleanerRegime
        {
            get
            {
                if (_isLoad == false)
                {
                    Load();
                }
                return _memoryCleanerRegime;
            }
            set
            {
                if(_memoryCleanerRegime == value)
                {
                    return;
                }
                _memoryCleanerRegime = value;
                Save();
            }
        }
        public static MemoryCleanerRegime _memoryCleanerRegime;

        public static void Save()
        {
            try
            {
                SettingsManager.Save(
                    GetSettingsPath(),
                    new PrimeSettingsDto
                    {
                        TransactionBeepIsActive = _transactionBeepIsActive,
                        ErrorLogBeepIsActive = _errorLogBeepIsActive,
                        ErrorLogMessageBoxIsActive = _errorLogMessageBoxIsActive,
                        LabelInHeaderBotStation = _labelInHeaderBotStation,
                        RebootTradeUiLight = _rebootTradeUiLight,
                        ReportCriticalErrors = _reportCriticalErrors,
                        MemoryCleanerRegime = _memoryCleanerRegime
                    });
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }
        }

        private static bool _isLoad;

        private static void Load()
        {
            _isLoad = true;

            try
            {
                PrimeSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings != null)
                {
                    _transactionBeepIsActive = settings.TransactionBeepIsActive;
                    _errorLogBeepIsActive = settings.ErrorLogBeepIsActive;
                    _errorLogMessageBoxIsActive = settings.ErrorLogMessageBoxIsActive;
                    _labelInHeaderBotStation = settings.LabelInHeaderBotStation;
                    _rebootTradeUiLight = settings.RebootTradeUiLight;
                    _reportCriticalErrors = settings.ReportCriticalErrors;
                    _memoryCleanerRegime = settings.MemoryCleanerRegime;
                }
            }
            catch (Exception ex)
            {
                _reportCriticalErrors = true;
                Trace.TraceWarning(ex.ToString());
            }
        }

        private static string GetSettingsPath()
        {
            return @"Engine\PrimeSettings.txt";
        }

        private static PrimeSettingsDto ParseLegacySettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string normalized = content.Replace("\r", string.Empty);
            string[] lines = normalized.Split('\n');

            if (lines.Length > 0 && lines[lines.Length - 1] == string.Empty)
            {
                Array.Resize(ref lines, lines.Length - 1);
            }

            string labelInHeader = lines.Length > 3 ? lines[3] : string.Empty;
            if (labelInHeader == "True" || labelInHeader == "False")
            {
                labelInHeader = string.Empty;
            }

            MemoryCleanerRegime memoryCleanerRegime = MemoryCleanerRegime.Disable;
            if (lines.Length > 6)
            {
                Enum.TryParse(lines[6], out memoryCleanerRegime);
            }

            return new PrimeSettingsDto
            {
                TransactionBeepIsActive = lines.Length > 0 && lines[0].Equals("true", StringComparison.OrdinalIgnoreCase),
                ErrorLogBeepIsActive = lines.Length > 1 && lines[1].Equals("true", StringComparison.OrdinalIgnoreCase),
                ErrorLogMessageBoxIsActive = lines.Length > 2 && lines[2].Equals("true", StringComparison.OrdinalIgnoreCase),
                LabelInHeaderBotStation = labelInHeader,
                RebootTradeUiLight = lines.Length > 4 && lines[4].Equals("true", StringComparison.OrdinalIgnoreCase),
                ReportCriticalErrors = lines.Length > 5
                    ? lines[5].Equals("true", StringComparison.OrdinalIgnoreCase)
                    : true,
                MemoryCleanerRegime = memoryCleanerRegime
            };
        }

        private sealed class PrimeSettingsDto
        {
            public bool TransactionBeepIsActive { get; set; }

            public bool ErrorLogBeepIsActive { get; set; }

            public bool ErrorLogMessageBoxIsActive { get; set; }

            public string LabelInHeaderBotStation { get; set; }

            public bool RebootTradeUiLight { get; set; }

            public bool ReportCriticalErrors { get; set; }

            public MemoryCleanerRegime MemoryCleanerRegime { get; set; }
        }
    }

    public enum MemoryCleanerRegime
    {
        Disable,
        At5Minutes,
        At30Minutes,
        AtDay
    }
}

