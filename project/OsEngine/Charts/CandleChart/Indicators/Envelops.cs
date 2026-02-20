#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using OsEngine.Entity;
using OsEngine.Indicators;

namespace OsEngine.Charts.CandleChart.Indicators
{
    public class Envelops: IIndicator
    {
        /// <summary>
        /// constructor with unique name. Indicator will be saved
        /// конструктор с уникальным именем. Индикатор будет сохраняться
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Envelops(string uniqName,bool canDelete)
        {
            Name = uniqName;
            TypeIndicator = IndicatorChartPaintType.Line;
            ColorUp = Color.DodgerBlue;
            ColorDown = Color.DarkRed;
            PaintOn = true;
            Deviation = 2;

            if (!File.Exists(GetSettingsPath()))
            {
                // if this is our first download.
                // если у нас первая загрузка
                MovingAverage = new MovingAverage(uniqName + "maSignal", false) { Length = 9, TypeCalculationAverage = MovingAverageTypeCalculation.Simple };
            } 
            else
            {
                MovingAverage = new MovingAverage(uniqName + "maSignal", false);
            }
            CanDelete = canDelete;
            Load();
        }

        /// <summary>
        /// constructor without parameters.Indicator will not saved/конструктор без параметров. Индикатор не будет сохраняться
        /// used ONLY to create composite indicators/используется ТОЛЬКО для создания составных индикаторов
        /// Don't use it from robot creation layer/не используйте его из слоя создания роботов!
        /// </summary>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Envelops(bool canDelete)
        {
            Name = Guid.NewGuid().ToString();

            TypeIndicator = IndicatorChartPaintType.Line;
            ColorUp = Color.DodgerBlue;
            ColorDown = Color.DarkRed;
            PaintOn = true;
            Deviation = 2;
            MovingAverage = new MovingAverage(false){Length = 9,TypeCalculationAverage = MovingAverageTypeCalculation.Simple};
            CanDelete = canDelete;
        }

        /// <summary>
        /// designer with ready MA. Will line up based on parameters specified in it
        /// конструктор с готовой машкой. Будет выстраиваться исходя из заданных в ней параметров
        /// </summary>
        /// <param name="moving">moving average for calculation/скользящая средняя для расчёта</param>
        /// <param name="uniqName">unique name/уникальное имя</param>
        public Envelops(MovingAverage moving, string uniqName)
        {
            Name = uniqName;
            TypeIndicator = IndicatorChartPaintType.Line;
            ColorUp = Color.DodgerBlue;
            ColorDown = Color.DarkRed;
            PaintOn = true;
            Deviation = 2;

            MovingAverage = moving;
            MovingAverage.Name = uniqName + "maSignal";

            Load();
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
                list.Add(ValuesUp);
                list.Add(ValuesDown);
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
        /// channel upper limit
        /// верхняя граница канала
        /// </summary>
        public List<decimal> ValuesUp { get; set; }

        /// <summary>
        /// channel bottom edge
        /// нижняя граница канала
        /// </summary>
        public List<decimal> ValuesDown { get; set; }

        /// <summary>
        /// unique indicator name
        /// уникальное имя индикатора
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// color of central data series
        /// цвет верхней серии данных
        /// </summary>
        public Color ColorUp { get; set; }

        /// <summary>
        /// lower data color
        /// цвет нижней серии данных
        /// </summary>
        public Color ColorDown { get; set; }

        /// <summary>
        /// is indicator tracing enabled
        /// включена ли прорисовка индикатора
        /// </summary>
        public bool PaintOn { get; set; }

        /// <summary>
        /// deviation for indicator calculation
        /// отклонение для расчёта индикатора
        /// </summary>
        public decimal Deviation;

        /// <summary>
        /// save settings to file
        /// сохранить настройки в файл
        /// </summary>
        public void Save()
        {
            MovingAverage.Save();
            try
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    return;
                }

                SettingsManager.Save(
                    GetSettingsPath(),
                    new EnvelopsSettingsDto
                    {
                        ColorUpArgb = ColorUp.ToArgb(),
                        ColorDownArgb = ColorDown.ToArgb(),
                        PaintOn = PaintOn,
                        Deviation = Deviation
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
                EnvelopsSettingsDto settings = SettingsManager.Load(
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
                Deviation = settings.Deviation;


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
            MovingAverage.Delete();
        }

        private string GetSettingsPath()
        {
            return @"Engine\" + Name + @".txt";
        }

        private static EnvelopsSettingsDto ParseLegacySettings(string content)
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

            if (lines.Length < 4)
            {
                return null;
            }

            return new EnvelopsSettingsDto
            {
                ColorUpArgb = Convert.ToInt32(lines[0]),
                ColorDownArgb = Convert.ToInt32(lines[1]),
                PaintOn = Convert.ToBoolean(lines[2]),
                Deviation = ParseDecimalInvariantOrCurrent(lines[3])
            };
        }

        private static decimal ParseDecimalInvariantOrCurrent(string value)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedInvariant))
            {
                return parsedInvariant;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal parsedCurrent))
            {
                return parsedCurrent;
            }

            return Convert.ToDecimal(value);
        }

        private sealed class EnvelopsSettingsDto
        {
            public int ColorUpArgb { get; set; }

            public int ColorDownArgb { get; set; }

            public bool PaintOn { get; set; }

            public decimal Deviation { get; set; }
        }

        /// <summary>
        /// delete data
        /// удалить данные
        /// </summary>
        public void Clear()
        {
            if (ValuesUp != null)
            {
                ValuesUp.Clear();
                ValuesDown.Clear();
            }
            _myCandles = null;
        }

        /// <summary>
        /// display settings window
        /// показать окно с настройками
        /// </summary>
        public void ShowDialog()
        {
            EnvelopsUi ui = new EnvelopsUi(this);
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
        /// Show signal settings MA
        /// показать настройки сигнальной машки
        /// </summary>
        public void ShowMaSignalDialog()
        {
            MovingAverage.ShowDialog();

            ProcessAll(_myCandles);

            if (NeedToReloadEvent != null)
            {
                NeedToReloadEvent(this);
            }
            MovingAverage.Save();
        }
        // calculation
        // расчёт

        /// <summary>
        /// candles to calculate indicator
        /// свечи для рассчёта индикатора
        /// </summary>
        private List<Candle> _myCandles;

        /// <summary>
        /// signal MA
        /// сигнальная машка
        /// </summary>
        public MovingAverage MovingAverage;

        /// <summary>
        /// calculate indicator
        /// рассчитать индикатор
        /// </summary>
        /// <param name="candles">candles/свечи</param>
        public void Process(List<Candle> candles)
        {

            _myCandles = candles;

            MovingAverage.Process(candles);

            if (ValuesDown != null &&
                ValuesDown.Count + 1 == candles.Count)
            {
                ProcessOne(candles);
            }
            else if (ValuesDown != null &&
                     ValuesDown.Count == candles.Count)
            {
                ProcessLast(candles);
            }
            else
            {
                ProcessAll(candles);
            }

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

            if (ValuesUp == null)
            {
                ValuesUp = new List<decimal>();
                ValuesDown= new List<decimal>();
            }

            ValuesUp.Add(GetUpValue(candles.Count-1));
            ValuesDown.Add(GetDownValue(candles.Count - 1));
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

            MovingAverage.Values = null;
            MovingAverage.Process(candles);

            ValuesUp = new List<decimal>();
            ValuesDown= new List<decimal>();

            for (int i = 0; i < candles.Count; i++)
            {
                ValuesUp.Add(GetUpValue(i));
                ValuesDown.Add(GetDownValue(i));
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

            ValuesUp[ValuesUp.Count - 1] = GetUpValue(candles.Count-1);
            ValuesDown[ValuesDown.Count - 1] = GetDownValue(candles.Count - 1);
        }

        private decimal GetUpValue(int index)
        {
            if (MovingAverage.Values.Count <= index)
            {
                index = MovingAverage.Values.Count - 1;
            }
            return Math.Round(MovingAverage.Values[index] + MovingAverage.Values[index]*(Deviation/100),5);
        }

        private decimal GetDownValue(int index)
        {
            if (MovingAverage.Values.Count <= index)
            {
                index = MovingAverage.Values.Count - 1;
            }
            return Math.Round(MovingAverage.Values[index] - MovingAverage.Values[index] * (Deviation / 100),5);
        }
    }
}

