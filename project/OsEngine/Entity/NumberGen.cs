/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace OsEngine.Entity
{
    /// <summary>
    /// number generator for deals and orders inside the robot
    /// </summary>
    public class NumberGen
    {
        private static bool _isFirstTime = true;
        private static async void SaverSpace()
        {
            while (true)
            {
                await Task.Delay(500);

                if (_needToSave)
                {
                    _needToSave = false;
                    Save();
                }

                if (!MainWindow.ProccesIsWorked)
                {
                    return;
                }
            }
        }

        private static bool _needToSave;

        /// <summary>
        /// current number of the last transaction
        /// </summary>
        private static int _numberDealForRealTrading;

        /// <summary>
        /// current number of the last order
        /// </summary>
        private static int _numberOrderForRealTrading;

        /// <summary>
        /// current number of the last transaction for tests
        /// </summary>
        private static int _numberDealForTesting;

        /// <summary>
        /// current number of the last order for tests
        /// </summary>
        private static int _numberOrderForTesting;

        private static readonly Lock _locker = new();

        /// <summary>
        /// take a number for a deal
        /// </summary>
        public static int GetNumberDeal(StartProgram startProgram)
        {
            lock (_locker)
            {
                if (startProgram == StartProgram.IsOsTrader)
                {
                    return GetNumberForRealTrading();
                }
                else
                {
                    return GetNumberForTesting();
                }
            }
        }

        private static int GetNumberForRealTrading()
        {
            if (_isFirstTime)
            {
                _isFirstTime = false;
                Load();

                Task task = new Task(SaverSpace);
                task.Start();
            }

            _numberDealForRealTrading++;

            _needToSave = true;
            return _numberDealForRealTrading;
        }

        private static int GetNumberForTesting()
        {
            _numberDealForTesting++;
            return _numberDealForTesting;
        }

        /// <summary>
        /// take the order number
        /// </summary>
        public static int GetNumberOrder(StartProgram startProgram)
        {
            lock (_locker)
            {
                if (startProgram == StartProgram.IsOsTrader)
                {
                    return GetNumberOrderForRealTrading();
                }
                else
                {
                    return GetNumberOrderForTesting();
                }
            }
        }

        private static int GetNumberOrderForRealTrading()
        {
            if (_isFirstTime)
            {
                _isFirstTime = false;
                Load();

                _dayOfYear
                    = (1000 - DateTime.Now.DayOfYear).ToString();

                Task task = new Task(SaverSpace);
                task.Start();
            }

            string resInString = _dayOfYear + _numberOrderForRealTrading.ToString();

            int result = 0;

            try
            {
                result = Convert.ToInt32(resInString, CultureInfo.InvariantCulture);
            }
            catch
            {
                // интежер кончился сбрасываем на ноль нумерацию
                _numberOrderForRealTrading = 0;
                resInString = _dayOfYear + _numberOrderForRealTrading.ToString();
                result = Convert.ToInt32(resInString, CultureInfo.InvariantCulture);
            }
            _numberOrderForRealTrading++;
            _needToSave = true;
            return result;
        }

        private static string _dayOfYear = string.Empty;

        private static int GetNumberOrderForTesting()
        {
            _numberOrderForTesting++;
            return _numberOrderForTesting;
        }

        private static void Load()
        {
            try
            {
                NumberGenSettings? settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings != null)
                {
                    _numberDealForRealTrading = settings.NumberDealForRealTrading;
                    _numberOrderForRealTrading = settings.NumberOrderForRealTrading;
                }
            }
            catch (Exception)
            {
                //send to log
                // отправить в лог
            }
        }

        private static void Save()
        {
            try
            {
                NumberGenSettings settings = new NumberGenSettings
                {
                    NumberDealForRealTrading = _numberDealForRealTrading,
                    NumberOrderForRealTrading = _numberOrderForRealTrading
                };

                SettingsManager.Save(GetSettingsPath(), settings);
            }
            catch (Exception)
            {
                //send to log
                // отправить в лог
            }
        }

        private static string GetSettingsPath()
        {
            return @"Engine\" + @"NumberGen.txt";
        }

        private static NumberGenSettings? ParseLegacySettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string normalized = content.Replace("\r", string.Empty);
            string[] lines = normalized.Split('\n');

            if (lines.Length < 2)
            {
                return null;
            }

            if (!int.TryParse(lines[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int dealNumber))
            {
                return null;
            }

            if (!int.TryParse(lines[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int orderNumber))
            {
                return null;
            }

            return new NumberGenSettings
            {
                NumberDealForRealTrading = dealNumber,
                NumberOrderForRealTrading = orderNumber
            };
        }

        private class NumberGenSettings
        {
            public int NumberDealForRealTrading { get; set; }

            public int NumberOrderForRealTrading { get; set; }
        }

        public static void ResetToZeroInTester()
        {
            _numberDealForRealTrading = 0;
            _numberOrderForRealTrading = 0;
            _numberDealForTesting = 0;
            _numberOrderForTesting = 0;
        }
    }
}
