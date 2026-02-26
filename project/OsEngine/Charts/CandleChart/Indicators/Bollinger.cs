#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использования кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OsEngine.Entity;
using OsEngine.Indicators;

namespace OsEngine.Charts.CandleChart.Indicators
{

    /// <summary>
    /// Bollinger. Bollinger indicator/Индикатор Боллинджер
    /// </summary>
    public class Bollinger: IIndicator
    {
        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Bollinger(string uniqName,bool canDelete)
        {
            Name = uniqName;

            TypeIndicator = IndicatorChartPaintType.Line;
            ColorUp = Color.DodgerBlue;
            ColorDown = Color.DarkRed;
            Deviation = 2;
            Length = 12;
            PaintOn = true;
            CanDelete = canDelete;
            Load();
        }

        /// <summary>
        /// constructor without parameters.Indicator will not saved/конструктор без параметров. Индикатор не будет сохраняться
        /// used ONLY to create composite indicators/используется ТОЛЬКО для создания составных индикаторов
        /// Don't use it from robot creation layer/не используйте его из слоя создания роботов!
        /// </summary>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Bollinger(bool canDelete)
        {
            Name = Guid.NewGuid().ToString();
            TypeIndicator = IndicatorChartPaintType.Line;
            ColorUp = Color.DodgerBlue;
            ColorDown = Color.DarkRed;
            Deviation = 2;
            Length = 12;
            PaintOn = true;
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
        /// indicator type
        /// тип индикатора
        /// </summary>
        public IndicatorChartPaintType TypeIndicator
        { get; set; }

        /// <summary>
        /// name of data series on which indicator will be drawn
        /// имя серии данных на которой будет прорисован индикатор
        /// </summary>
        public string NameSeries
        { get; set; }

        /// <summary>
        /// name of data area where indicator will be drawn
        /// имя области данных на которой будет прорисовываться индикатор
        /// </summary>
        public string NameArea
        { get; set; }

        /// <summary>
        /// top bollinger line
        /// верхняя линия боллинжера
        /// </summary>
        public List<decimal> ValuesUp
        { get; set; }

        /// <summary>
        /// bottom line of bollinger
        /// нижняя линия боллинджера
        /// </summary>
        public List<decimal> ValuesDown
        { get; set; }

        /// <summary>
        /// unique indicator name
        /// уникальное имя индикатора
        /// </summary>
        public string Name
        { get; set; }

        /// <summary>
        /// period length to calculate indicator
        /// длина расчёта индикатора
        /// </summary>
        public int Length
        { get; set; }

        /// <summary>
        /// deviation
        /// отклонение
        /// </summary>
        public decimal Deviation
        { get; set; }

        /// <summary>
        /// color of upper data series
        /// цвет верхней серии данных
        /// </summary>
        public Color ColorUp
        { get; set; }

        /// <summary>
        /// color of lower data series
        /// цвет нижней серии данных
        /// </summary>
        public Color ColorDown
        { get; set; }

        /// <summary>
        /// is indicator tracing enabled
        /// включена ли прорисовка индикатора
        /// </summary>
        public bool PaintOn
        { get; set; }

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
                BollingerSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                ColorUp = Color.FromArgb(settings.ColorUpArgb);
                ColorDown = Color.FromArgb(settings.ColorDownArgb);
                Length = settings.Length;
                Deviation = settings.Deviation;
                PaintOn = settings.PaintOn;
            }
            catch (Exception)
            {
                // send to log
                // отправить в лог
            }
        }

        /// <summary>
        /// save settings to file
        /// сохранить настройки в файл
        /// </summary>
        public void Save()
        {
            try
            {
                SettingsManager.Save(
                    GetSettingsPath(),
                    new BollingerSettingsDto
                    {
                        ColorUpArgb = ColorUp.ToArgb(),
                        ColorDownArgb = ColorDown.ToArgb(),
                        Length = Length,
                        Deviation = Deviation,
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
        /// delete file with settings
        /// удалить файл настроек
        /// </summary>
        public void Delete()
        {
            if (File.Exists(GetSettingsPath()))
            {
                File.Delete(GetSettingsPath());
            }

            ValuesUp = null;
            ValuesDown = null;
        }

        private string GetSettingsPath()
        {
            return @"Engine\" + Name + @".txt";
        }

        private static BollingerSettingsDto ParseLegacySettings(string content)
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

            if (lines.Length < 5)
            {
                return null;
            }

            return new BollingerSettingsDto
            {
                ColorUpArgb = Convert.ToInt32(lines[0], CultureInfo.InvariantCulture),
                ColorDownArgb = Convert.ToInt32(lines[1], CultureInfo.InvariantCulture),
                Length = Convert.ToInt32(lines[2], CultureInfo.InvariantCulture),
                Deviation = lines[3].ToDecimal(),
                PaintOn = Convert.ToBoolean(lines[4])
            };
        }

        private sealed class BollingerSettingsDto
        {
            public int ColorUpArgb { get; set; }

            public int ColorDownArgb { get; set; }

            public int Length { get; set; }

            public decimal Deviation { get; set; }

            public bool PaintOn { get; set; }
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
        /// показать окно настроек
        /// </summary>
        public void ShowDialog()
        {
            BollingerUi ui = new BollingerUi(this);
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
        /// to upload new candles
        /// прогрузить новыми свечками
        /// </summary>
        public void Process(List<Candle> candles)
        {
            _myCandles = candles;
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
        /// it's necessary to redraw indicator
        /// необходимо перерисовать индикатор
        /// </summary>
        public event Action<IIndicator> NeedToReloadEvent;

        /// <summary>
        /// candles used to build indicator
        /// свечи по которым строиться индикатор
        /// </summary>
        private List<Candle> _myCandles;

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
            if (ValuesDown == null)
            {
                ValuesUp = new List<decimal>();
                ValuesDown = new List<decimal>();

                decimal[] value = GetValueSimple(candles, candles.Count - 1);

                ValuesUp.Add(value[0]);
                ValuesDown.Add(value[1]);
            }
            else
            {
                decimal[] value = GetValueSimple(candles, candles.Count - 1);

                ValuesUp.Add(value[0]);
                ValuesDown.Add(value[1]);
            }
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
            ValuesUp = new List<decimal>();
            ValuesDown = new List<decimal>();

            decimal[][] newValues = new decimal[candles.Count][];

            for (int i = 0; i < candles.Count; i++)
            {
                newValues[i] = GetValueSimple(candles, i);
            }

            for (int i = 0; i < candles.Count; i++)
            {
                ValuesUp.Add(newValues[i][0]);
            }

            for (int i = 0; i < candles.Count; i++)
            {
                ValuesDown.Add(newValues[i][1]);
            }
        }

        /// <summary>
        /// overload last value
        /// перегрузить последнюю ячейку
        /// </summary>
        private void ProcessLast(List<Candle> candles)
        {
            if (candles == null)
            {
                return;
            }
            decimal[] value = GetValueSimple(candles, candles.Count - 1);
            ValuesUp[ValuesUp.Count - 1] = value[0];
            ValuesDown[ValuesDown.Count - 1] = value[1];
        }

        /// <summary>
        /// take indicator value by index
        /// взять значение индикатора по индексу
        /// </summary>
        private decimal[] GetValueSimple(List<Candle> candles, int index)
        {
            if (index - Length <= 0)
            {
                return new decimal[2];
            }

            decimal [] bollinger = new decimal[2];
// 1 count SMA
// 1 считаем СМА

            decimal valueSma = 0;

            for (int i = index - Length + 1; i < index + 1; i++)
            {
                // running through past periods and collecting values
                // бежим по прошлым периодам и собираем значения
                valueSma += candles[i].Close;
            }

            valueSma = valueSma / Length;
            // 2 count average deviation
            // 2 считаем среднее отклонение
            // find an array of deviations from mean
            // находим массив отклонений от средней
            decimal[] valueDev = new decimal[Length];
            for (int i = index - Length + 1, i2 = 0; i < index + 1; i++, i2++)
            {
                // running through past periods and collecting values
                // бежим по прошлым периодам и собираем значения
                valueDev[i2] = candles[i].Close - valueSma;
            }
            // square this array
            // возводим этот массив в квадрат
            for (int i = 0; i < valueDev.Length; i++)
            {
                valueDev[i] = Convert.ToDecimal(Math.Pow(Convert.ToDouble(valueDev[i]), 2));
            }
            // folding up
            // складываем

            double summ = 0;

            for (int i = 0; i < valueDev.Length; i++)
            {
                summ += Convert.ToDouble(valueDev[i]);
            }
            // divide amount by number of elements in sample( if on n-1,so if n> 30) 
            //делим полученную сумму на количество элементов в выборке (или на n-1, если n>30)
            if (Length > 30)
            {
                summ = summ/(Length - 1);
            }
            else
            {
                summ = summ/Length;
            }
            // calculating root.
            // вычисляем корень

            summ = Math.Sqrt(summ);
            // 3 count bollinger lines
            // 3 считаем линии боллинжера

            bollinger[0] = Math.Round(valueSma + Convert.ToDecimal(summ) * Deviation,6);

            bollinger[1] = Math.Round(valueSma -Convert.ToDecimal(summ) * Deviation,6);

            return bollinger;
        }
    }
}


