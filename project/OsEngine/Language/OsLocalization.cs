#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using OsEngine.Entity;

namespace OsEngine.Language
{
    public class OsLocalization
    {
        public enum OsLocalType
        {
            None,

            Ru,

            Eng,
        }

        public static List<OsLocalType> GetExistLocalizationTypes()
        {
            List<OsLocalType> localizations = new List<OsLocalType>();

            localizations.Add(OsLocalType.Eng);
            localizations.Add(OsLocalType.Ru);

            return localizations;
        }

        public static string CurLocalizationCode
        {
            get
            {
                if (_curLocalization == OsLocalType.Eng)
                {
                    return "en-US";
                }
                else if(_curLocalization == OsLocalType.Ru)
                {
                    return "ru-RU";
                }

                return "en-US";
            }
        }

        // h:mm:ss tt
        // H:mm:ss
        public static string LongTimePattern
        {
            get
            {
                return _longTimePattern;
            }
            set
            {
                if (_longTimePattern != value)
                {
                    _longTimePattern = value;
                    Save();
                }
            }
        }

        private static string _longTimePattern;

        // M/d/yyyy
        // dd.MM.yyyy
        public static string ShortDatePattern
        {
            get
            {
                return _shortDatePattern;
            }
            set
            {
                if (_shortDatePattern != value)
                {
                    _shortDatePattern = value;
                    Save();
                }
            }
        }

        private static string _shortDatePattern;

        public static CultureInfo CurCulture
        {
            get
            {
                CultureInfo culture = new CultureInfo(CurLocalizationCode);

                if(_longTimePattern == null)
                {
                    Load();
                }

                if(_longTimePattern != null)
                {
                    culture.DateTimeFormat.LongTimePattern = _longTimePattern;
                }

                if (_shortDatePattern != null)
                {
                    culture.DateTimeFormat.ShortDatePattern = _shortDatePattern;
                    if(_shortDatePattern == "M/d/yyyy")
                    {
                        culture.DateTimeFormat.DateSeparator = "/";
                        culture.DateTimeFormat.AMDesignator = "AM";
                        culture.DateTimeFormat.PMDesignator = "PM";
                    }
                    else
                    {
                        culture.DateTimeFormat.DateSeparator = ".";
                    }
                }
                

                return culture;
            }
        }

        public static string ShortDateFormatString
        {
            get
            {
                CultureInfo culture = CurCulture;

                return CurCulture.DateTimeFormat.ShortDatePattern;
            }
        }

        public static OsLocalType CurLocalization
        {
            get
            {
                if (_curLocalization == OsLocalType.None)
                {
                    Load();
                    if (_curLocalization == OsLocalType.None)
                    {
                        _curLocalization = OsLocalType.Ru;
                    }
                }
                return _curLocalization;
            }
            set
            {
                if (_curLocalization == value)
                {
                    return;
                }
                _curLocalization = value;

                // System.Threading.Thread.CurrentThread.CurrentUICulture = OsLocalization.CurCulture;
                // System.Threading.Thread.CurrentThread.CurrentCulture = OsLocalization.CurCulture;

                LocalizationTypeChangeEvent?.Invoke();
                Save();
            }
        }

        private static OsLocalType _curLocalization;

        /// <summary>
        /// сохранить настройки
        /// </summary>
        public static void Save()
        {
            try
            {
                SettingsManager.Save(
                    GetSettingsPath(),
                    new OsLocalizationSettingsDto
                    {
                        CurLocalization = _curLocalization,
                        LongTimePattern = _longTimePattern,
                        ShortDatePattern = _shortDatePattern
                    });
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning(ex.ToString());
            }
        }

        /// <summary>
        /// загрузить настройки
        /// </summary>
        private static void Load()
        {
            if (!File.Exists(GetSettingsPath()))
            {
                _longTimePattern = "H:mm:ss";
                _shortDatePattern = "dd.MM.yyyy";
                return;
            }
            try
            {
                OsLocalizationSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings != null)
                {
                    _curLocalization = settings.CurLocalization;
                    _longTimePattern = settings.LongTimePattern;
                    _shortDatePattern = settings.ShortDatePattern;
                }

                // System.Threading.Thread.CurrentThread.CurrentUICulture = OsLocalization.CurCulture;
                // System.Threading.Thread.CurrentThread.CurrentCulture = OsLocalization.CurCulture;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning(ex.ToString());
            }

            if(string.IsNullOrEmpty(_longTimePattern))
            {
                _longTimePattern = "H:mm:ss";
            }
            if(string.IsNullOrEmpty(_shortDatePattern))
            {
                _shortDatePattern = "dd.MM.yyyy";
            }
        }

        private static string GetSettingsPath()
        {
            return @"Engine\local.txt";
        }

        private static OsLocalizationSettingsDto ParseLegacySettings(string content)
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

            OsLocalType localization = OsLocalType.None;
            if (lines.Length > 0)
            {
                Enum.TryParse(lines[0], true, out localization);
            }

            return new OsLocalizationSettingsDto
            {
                CurLocalization = localization,
                LongTimePattern = lines.Length > 1 ? lines[1] : null,
                ShortDatePattern = lines.Length > 2 ? lines[2] : null
            };
        }

        private sealed class OsLocalizationSettingsDto
        {
            public OsLocalType CurLocalization { get; set; }

            public string LongTimePattern { get; set; }

            public string ShortDatePattern { get; set; }
        }

        public static event Action LocalizationTypeChangeEvent;

        public static string ConvertToLocString(string str)
        {
            try
            {
                //"Eng:Main&Ru:Главное меню&"

                string[] locStrings = str.Split('_');

                string engLoc = "";

                for (int i = 0; i < locStrings.Length; i++)
                {
                    if (locStrings[i] == "" || locStrings[i] == " ")
                    {
                        continue;
                    }

                    string [] locCur = locStrings[i].Split(':');

                    OsLocalType cultureTypeCur;
                    if (Enum.TryParse(locCur[0], out cultureTypeCur))
                    {
                        if (cultureTypeCur == CurLocalization)
                        {
                            return locCur[1];
                        }
                        if (cultureTypeCur == OsLocalType.Eng)
                        {
                            engLoc = locCur[1];
                        }
                    }
                }

                return engLoc;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return "error";
            }

        }

        public static MainWindowLocal MainWindow = new MainWindowLocal();

        public static PrimeSettingsMasterUiLocal PrimeSettings = new PrimeSettingsMasterUiLocal();

        public static AlertsLocal Alerts = new AlertsLocal();

        public static ChartsLocal Charts = new ChartsLocal();

        public static EntityLocal Entity = new EntityLocal();

        public static JournalLocal Journal = new JournalLocal();

        public static LoggingLocal Logging = new LoggingLocal();

        public static MarketLocal Market = new MarketLocal();

        public static ConverterLocal Converter = new ConverterLocal();

        public static DataLocal Data = new DataLocal();

        public static OptimizerLocal Optimizer = new OptimizerLocal();

        public static TraderLocal Trader = new TraderLocal();

        public static HintMessage Message = new HintMessage();

        public static DescriptionRobotsLocal Description = new DescriptionRobotsLocal();

    }
}

