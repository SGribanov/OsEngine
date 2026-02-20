#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OsEngine.Entity;
using OsEngine.Indicators;

namespace OsEngine.Charts.CandleChart.Indicators
{

    /// <summary>
    /// MACD Histogram. Moving Average Convergence Divergence.
    /// Indicator for analysis of convergence and divergence of moving averages in the form of Histogram
    /// Индикатор для анализа схождения-расхождения скользящих средних, в виде Гистограммы
    /// </summary>
    public class MacdHistogram: IIndicator
    {
        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public MacdHistogram(string uniqName,bool canDelete)
        {
            Name = uniqName;
            TypeIndicator = IndicatorChartPaintType.Column;
            ColorUp = Color.DodgerBlue;
            ColorDown = Color.DarkRed;
            PaintOn = true;

            if (!File.Exists(GetSettingsPath()))
            {// если у нас первая загрузка
                _maShort = new MovingAverage(uniqName + "ma1", false) { Length = 12, TypeCalculationAverage = MovingAverageTypeCalculation.Exponential };
                _maLong = new MovingAverage(uniqName + "ma2", false) { Length = 26, TypeCalculationAverage = MovingAverageTypeCalculation.Exponential };
                _maSignal = new MovingAverage(uniqName + "maSignal", false) { Length = 9, TypeCalculationAverage = MovingAverageTypeCalculation.Simple };
            } 
            else
            {
                _maShort = new MovingAverage(uniqName + "ma1", false);
                _maLong = new MovingAverage(uniqName + "ma2", false);
                _maSignal = new MovingAverage(uniqName + "maSignal", false);
            }
            CanDelete = canDelete;
            Load();
        }

        /// <summary>
        /// constructor without parameters.Indicator will not saved/конструктор без параметров. Индикатор не будет сохраняться
        /// used ONLY to create composite indicators/используется ТОЛЬКО для создания составных индикаторов
        /// Don't use it from robot creation layer/не используйте его из слоя создания роботов!
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="movingShort">short moving average/короткий мувинг</param>
        /// <param name="movingLong">long moving average/длинный мувинг</param>
        /// <param name="movingSignal">signal ma/сигнальный мувинг</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public MacdHistogram(string uniqName, MovingAverage movingShort, MovingAverage movingLong, MovingAverage movingSignal, bool canDelete)
        {
            Name = uniqName;
            TypeIndicator = IndicatorChartPaintType.Column;
            ColorUp = Color.DodgerBlue;
            ColorDown= Color.DarkRed;
            PaintOn = true;

            _maShort = movingShort;
            _maLong = movingLong;
            _maSignal = movingSignal;

            _maShort.Name = uniqName + "ma1";
            _maLong.Name = uniqName + "ma12";
            _maSignal.Name = uniqName + "maSignal";
            CanDelete = canDelete;
            Load();
        }

        /// <summary>
        /// constructor without parameters.Indicator will not saved/конструктор без параметров. Индикатор не будет сохраняться
        /// used ONLY to create composite indicators/используется ТОЛЬКО для создания составных индикаторов
        /// Don't use it from robot creation layer/не используйте его из слоя создания роботов!
        /// </summary>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public MacdHistogram(bool canDelete)
        {
            Name = Guid.NewGuid().ToString();

            TypeIndicator = IndicatorChartPaintType.Column;
            ColorUp = Color.DodgerBlue;
            ColorDown = Color.DarkRed;
            PaintOn = true;
            _maShort = new MovingAverage( false) {Length = 12,TypeCalculationAverage = MovingAverageTypeCalculation.Exponential};
            _maLong = new MovingAverage( false) { Length = 26, TypeCalculationAverage = MovingAverageTypeCalculation.Exponential };
            _maSignal = new MovingAverage( false){Length = 9,TypeCalculationAverage = MovingAverageTypeCalculation.Simple};
            CanDelete = canDelete;
        }

        /// <summary>
        /// all indicator values
        /// все значения индикатора
        /// </summary>
        List<List<decimal>> IIndicator.ValuesToChart
        {
            get
            {
                List<List<decimal>> list = new List<List<decimal>>();
                list.Add(Values);
                return list;
            }
        }

        /// <summary>
        /// indicator colors
        /// цвета для индикатора
        /// </summary>
        List<Color> IIndicator.Colors
        {
            get
            {
                List<Color> colors = new List<Color>();
                colors.Add(ColorUp);
                colors.Add(ColorDown);
                return colors;
            }

        }

        /// <summary>
        /// whether indicator can be removed from chart. This is necessary so that robots can't be removed /можно ли удалить индикатор с графика. Это нужно для того чтобы у роботов нельзя было удалить 
        /// indicators he needs in trading/индикаторы которые ему нужны в торговле
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// indicator drawing type
        /// тип прорисовки индикатора
        /// </summary>
        public IndicatorChartPaintType TypeIndicator { get; set; }

        /// <summary>
        /// name of data series on which indicator will be drawn
        /// имя серии данных на которой будет прорисован индикатор
        /// </summary>
        public string NameSeries { get; set; }

        /// <summary>
        /// name of data area where indicator will be drawn
        /// имя области данных на которой будет прорисовываться индикатор
        /// </summary>
        public string NameArea { get; set; }

        /// <summary>
        /// Macd Histogram
        /// </summary>
        public List<decimal> Values { get; set; }

        /// <summary>
        /// unique indicator name
        /// уникальное имя индикатора
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// growing column color
        /// цвет растущего столбика
        /// </summary>
        public Color ColorUp { get; set; }

        /// <summary>
        /// falling column color
        /// цвет падуещего столбца
        /// </summary>
        public Color ColorDown { get; set; }

        /// <summary>
        /// is indicator tracing enabled
        /// включена ли прорисовка индикатора
        /// </summary>
        public bool PaintOn { get; set; }

        /// <summary>
        /// save settings to file
        /// сохранить настройки в файл
        /// </summary>
        public void Save()
        {
            try
            {
                _maSignal.Save();
                _maShort.Save();
                _maLong.Save();
                if (string.IsNullOrWhiteSpace(Name))
                {
                    return;
                }

                SettingsManager.Save(
                    GetSettingsPath(),
                    new MacdHistogramSettingsDto
                    {
                        ColorUpArgb = ColorUp.ToArgb(),
                        ColorDownArgb = ColorDown.ToArgb(),
                        PaintOn = PaintOn
                    });
            }
            catch (Exception)
            {
                // send to log
                // отправить в лог
            }
        }

        /// <summary>
        /// upload settings from file
        /// загрузить настройки из файла
        /// </summary>
        public void Load()
        {
            if (!File.Exists(GetSettingsPath()))
            {
                return;
            }
            try
            {
                MacdHistogramSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                ColorUp = Color.FromArgb(settings.ColorUpArgb);
                ColorDown = Color.FromArgb(settings.ColorDownArgb);
                PaintOn = settings.PaintOn;

            }
            catch (Exception)
            {
                // send to log
                // отправить в лог
            }
        }

        /// <summary>
        /// delete file with settings
        /// удалить файл с настройками
        /// </summary>
        public void Delete()
        {
            if (File.Exists(GetSettingsPath()))
            {
                File.Delete(GetSettingsPath());
            }
            _maShort.Delete();
            _maLong.Delete();
            _maSignal.Delete();
        }

        private string GetSettingsPath()
        {
            return @"Engine\" + Name + @".txt";
        }

        private static MacdHistogramSettingsDto ParseLegacySettings(string content)
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

            if (lines.Length < 3)
            {
                return null;
            }

            return new MacdHistogramSettingsDto
            {
                ColorUpArgb = Convert.ToInt32(lines[0]),
                ColorDownArgb = Convert.ToInt32(lines[1]),
                PaintOn = Convert.ToBoolean(lines[2])
            };
        }

        private sealed class MacdHistogramSettingsDto
        {
            public int ColorUpArgb { get; set; }

            public int ColorDownArgb { get; set; }

            public bool PaintOn { get; set; }
        }

        /// <summary>
        /// delete data
        /// удалить данные
        /// </summary>
        public void Clear()
        {
            if (Values != null)
            {
                Values.Clear();
            }
            _myCandles = null;
        }

        /// <summary>
        /// display settings window
        /// показать окно с настройками
        /// </summary>
        public void ShowDialog()
        {
            MacdHistogramUi ui = new MacdHistogramUi(this);
            ui.ShowDialog();

            if (ui.IsChange && _myCandles != null)
            {
                Reload();
            }
        }

        /// <summary>
        /// reload indicator
        /// перезагрузить индикатор
        /// </summary>
        public void Reload()
        {
            if (_myCandles == null)
            {
                return;
            }
            ProcessAll(_myCandles);

            if (NeedToReloadEvent != null)
            {
                NeedToReloadEvent(this);
            }
        }

        /// <summary>
        /// show short ma settings
        /// показать настройки короткой машки
        /// </summary>
        public void ShowMaShortDialog()
        {
            _maShort.ShowDialog();

            ProcessAll(_myCandles);

            if (NeedToReloadEvent != null)
            {
                NeedToReloadEvent(this);
            }
        }

        /// <summary>
        /// show long ma settings
        /// показать настройки длинной машки
        /// </summary>
        public void ShowMaLongDialog()
        {
            _maLong.ShowDialog();

            ProcessAll(_myCandles);

            if (NeedToReloadEvent != null)
            {
                NeedToReloadEvent(this);
            }
        }

        /// <summary>
        /// show settings of signal MA
        /// показать настройки сигнальной машки
        /// </summary>
        public void ShowMaSignalDialog()
        {
            _maSignal.ShowDialog();

            ProcessAll(_myCandles);

            if (NeedToReloadEvent != null)
            {
                NeedToReloadEvent(this);
            }
        }
        // calculating
        // расчёт

        /// <summary>
        /// candles to calculate indicator
        /// свечи для рассчёта индикатора
        /// </summary>
        private List<Candle> _myCandles;

        /// <summary>
        /// short ma
        /// короткая машка
        /// </summary>
        private MovingAverage _maShort;

        /// <summary>
        /// long ma
        /// длинная машка
        /// </summary>
        private MovingAverage _maLong;

        /// <summary>
        /// signal ma
        /// сигнальная машка
        /// </summary>
        private MovingAverage _maSignal;

        private List<decimal> _macd;

        /// <summary>
        /// calculate indicator
        /// рассчитать индикатор
        /// </summary>
        /// <param name="candles">candles/свечи</param>
        public void Process(List<Candle> candles)
        {
            _myCandles = candles;

            _maShort.Process(candles);
            _maLong.Process(candles);


            if (Values != null &&
                Values.Count + 1 == candles.Count)
            {
                ProcessOne(candles);
            }
            else if (Values != null &&
                     Values.Count == candles.Count)
            {
                ProcessLast(candles);
            }
            else
            {
                ProcessAll(candles);
            }

           // Values = _maSignal.Values;
        }

        /// <summary>
        /// indicator needs to be redrawn
        /// индикатор нужно перерисовать
        /// </summary>
        public event Action<IIndicator> NeedToReloadEvent;

        /// <summary>
        /// load only last candle
        /// прогрузить только последнюю свечку
        /// </summary>
        private void ProcessOne(List<Candle> candles)
        {
            if (candles == null)
            {
                return;
            }

            if (_macd == null)
            {
                _macd = new List<decimal>();
                Values = new List<decimal>();
                _macd.Add(GetMacd(candles.Count - 1));
            }
            else
            {
                _macd.Add(GetMacd(candles.Count - 1));
            }
            _maSignal.Process(_macd);

            Values.Add(GetMacdHistogram(candles.Count - 1));
        }

        /// <summary>
        /// to upload from the beginning
        /// прогрузить с самого начала
        /// </summary>
        private void ProcessAll(List<Candle> candles)
        {
            if (candles == null)
            {
                return;
            }

            _maShort.Values = null;
            _maLong.Values = null;
            _maSignal.Values = null;

            _maShort.Process(candles);
            _maLong.Process(candles);

            _macd = new List<decimal>();
            Values = new List<decimal>();

            for (int i = 0; i < candles.Count; i++)
            {
                _macd.Add(GetMacd(i));
                _maSignal.Process(_macd);
                Values.Add(GetMacdHistogram(i));
            }
        }

        /// <summary>
        /// overload last value
        /// перегрузить последнее значение
        /// </summary>
        private void ProcessLast(List<Candle> candles)
        {
            if (candles == null)
            {
                return;
            }
            _macd[_macd.Count - 1] = GetMacd(candles.Count - 1);
            _maSignal.Process(_macd);
            Values[Values.Count - 1] = GetMacdHistogram(Values.Count - 1);
        }

        /// <summary>
        /// take indicator value by index
        /// взять значение индикатора по индексу
        /// </summary>
        /// <param name="index">index/индекс</param>
        /// <returns>index value/значение индикатора по индексу</returns>
        private decimal GetMacd(int index)
        {
            if (_maShort == null || _maShort.Values[index] == 0 ||
                _maShort.Values.Count - 1 < index)
            {
                return 0;
            }

            if (_maLong == null || _maLong.Values[index] == 0 ||
                _maLong.Values.Count - 1 < index)
            {
                return 0;
            }

            return _maShort.Values[index] - _maLong.Values[index];
           
        }

        private decimal GetMacdHistogram(int index)
        {
            if (_maSignal.Values[index] == 0
                || _macd[index] == 0)
            {
                return 0;
            }
            else
            {
                return Math.Round(_macd[index] - _maSignal.Values[index], 7);
            }
        }

    }
}

