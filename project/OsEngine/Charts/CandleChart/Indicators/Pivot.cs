#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использования кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
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
    public class Pivot: IIndicator
    {
        /// <summary>
        /// constructor with parameters. Indicator will be saved
        /// конструктор с параметрами. Индикатор будет сохраняться
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Pivot(string uniqName, bool canDelete)
        {
            Name = uniqName;
            TypeIndicator = IndicatorChartPaintType.Line;

            ColorP = Color.LawnGreen;

            ColorS1 = Color.DarkRed;
            ColorS2 = Color.DarkRed;
            ColorS3 = Color.DarkRed;
            ColorS4 = Color.DarkRed;

            ColorR1 = Color.DodgerBlue;
            ColorR2 = Color.DodgerBlue;
            ColorR3 = Color.DodgerBlue;
            ColorR4 = Color.DodgerBlue;

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
        public Pivot(bool canDelete)
        {
            Name = Guid.NewGuid().ToString();

            TypeIndicator = IndicatorChartPaintType.Line;

            ColorP = Color.LawnGreen;

            ColorS1 = Color.DarkRed;
            ColorS2 = Color.DarkRed;
            ColorS3 = Color.DarkRed;
            ColorS4 = Color.DarkRed;

            ColorR1 = Color.DodgerBlue;
            ColorR2 = Color.DodgerBlue;
            ColorR3 = Color.DodgerBlue;
            ColorR4 = Color.DodgerBlue;

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
                list.Add(ValuesS1);
                list.Add(ValuesS2);
                list.Add(ValuesS3);
                list.Add(ValuesS4);
                list.Add(ValuesR1);
                list.Add(ValuesR2);
                list.Add(ValuesR3);
                list.Add(ValuesR4);
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
                colors.Add(ColorS1);
                colors.Add(ColorS2);
                colors.Add(ColorS3);
                colors.Add(ColorS4);
                colors.Add(ColorR1);
                colors.Add(ColorR2);
                colors.Add(ColorR3);
                colors.Add(ColorR4);

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
        /// имя серии на графике для прорисовки индикатора
        /// </summary>
        public string NameSeries { get; set; }

        /// <summary>
        /// name of data area where indicator will be drawn
        /// имя области на графике для прорисовки индикатора
        /// </summary>
        public string NameArea { get; set; }

        /// <summary>
        /// first resistance
        ///   первое сопротивление
        /// </summary> 
        public List<decimal> ValuesR1 
        { get; set; }

        /// <summary>
        /// first support
        /// первая поддержка
        /// </summary>
        public List<decimal> ValuesS1 
        { get; set; }

        /// <summary>
        /// second resistance
        ///   второе сопротивление
        /// </summary> 
        public List<decimal> ValuesR2
        { get; set; }

        /// <summary>
        /// second support
        /// вторая поддержка
        /// </summary>
        public List<decimal> ValuesS2
        { get; set; }

        /// <summary>
        /// third resistance
        ///   третье сопротивление
        /// </summary> 
        public List<decimal> ValuesR3
        { get; set; }

        /// <summary>
        /// fourth resistance
        ///   четвёртое сопротивление
        /// </summary> 
        public List<decimal> ValuesR4
        { get; set; }

        /// <summary>
        /// third support
        /// третья поддержка
        /// </summary>
        public List<decimal> ValuesS3
        { get; set; }

        /// <summary>
        /// fourth support
        /// четвёртая поддержка
        /// </summary>
        public List<decimal> ValuesS4
        { get; set; }

        /// <summary>
        /// unique indicator name
        /// уникальное имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// color of first resistance
        /// цвет сопротивления 1
        /// </summary>
        public Color ColorR1 { get; set; }

        /// <summary>
        /// color of second resistance
        /// цвет сопротивления 2
        /// </summary>
        public Color ColorR2 { get; set; }

        /// <summary>
        /// color of third resistance
        /// цвет сопротивления 3
        /// </summary>
        public Color ColorR3 { get; set; }

        /// <summary>
        /// color of fourth resistance
        /// цвет сопротивления 4
        /// </summary>
        public Color ColorR4 { get; set; }

        /// <summary>
        /// top line color
        /// цвет верхней линии
        /// </summary>
        public Color ColorP { get; set; }

        /// <summary>
        /// color of first support
        /// цвет поддержки 1
        /// </summary>
        public Color ColorS1 { get; set; }

        /// <summary>
        /// color of second support
        /// цвет поддержки 2
        /// </summary>
        public Color ColorS2 { get; set; }

        /// <summary>
        /// color of third support
        /// цвет поддержки 3
        /// </summary>
        public Color ColorS3 { get; set; }

        /// <summary>
        /// color of fourth support
        /// цвет поддержки 4
        /// </summary>
        public Color ColorS4 { get; set; }

        /// <summary>
        /// candles to calculate indicator
        /// вкллючена ли прорисовка индикатора на графике
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
                if (string.IsNullOrWhiteSpace(Name))
                {
                    return;
                }

                SettingsManager.Save(
                    GetSettingsPath(),
                    new PivotSettingsDto
                    {
                        ColorPArgb = ColorP.ToArgb(),
                        ColorS1Argb = ColorS1.ToArgb(),
                        ColorS2Argb = ColorS2.ToArgb(),
                        ColorS3Argb = ColorS3.ToArgb(),
                        ColorS4Argb = ColorS4.ToArgb(),
                        ColorR1Argb = ColorR1.ToArgb(),
                        ColorR2Argb = ColorR2.ToArgb(),
                        ColorR3Argb = ColorR3.ToArgb(),
                        ColorR4Argb = ColorR4.ToArgb(),
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
                PivotSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                ColorP = Color.FromArgb(settings.ColorPArgb);
                ColorS1 = Color.FromArgb(settings.ColorS1Argb);
                ColorS2 = Color.FromArgb(settings.ColorS2Argb);
                ColorS3 = Color.FromArgb(settings.ColorS3Argb);
                ColorS4 = Color.FromArgb(settings.ColorS4Argb);
                ColorR1 = Color.FromArgb(settings.ColorR1Argb);
                ColorR2 = Color.FromArgb(settings.ColorR2Argb);
                ColorR3 = Color.FromArgb(settings.ColorR3Argb);
                ColorR4 = Color.FromArgb(settings.ColorR4Argb);
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
        }

        private string GetSettingsPath()
        {
            return @"Engine\" + Name + @".txt";
        }

        private static PivotSettingsDto ParseLegacySettings(string content)
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

            if (lines.Length < 10)
            {
                return null;
            }

            return new PivotSettingsDto
            {
                ColorPArgb = ParseColorLegacy(lines[0]).ToArgb(),
                ColorS1Argb = ParseColorLegacy(lines[1]).ToArgb(),
                ColorS2Argb = ParseColorLegacy(lines[2]).ToArgb(),
                ColorS3Argb = ParseColorLegacy(lines[3]).ToArgb(),
                ColorS4Argb = ParseColorLegacy(lines[4]).ToArgb(),
                ColorR1Argb = ParseColorLegacy(lines[5]).ToArgb(),
                ColorR2Argb = ParseColorLegacy(lines[6]).ToArgb(),
                ColorR3Argb = ParseColorLegacy(lines[7]).ToArgb(),
                ColorR4Argb = ParseColorLegacy(lines[8]).ToArgb(),
                PaintOn = Convert.ToBoolean(lines[9])
            };
        }

        private static Color ParseColorLegacy(string value)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int argb))
            {
                return Color.FromArgb(argb);
            }

            const string prefix = "Color [";
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && value.EndsWith("]", StringComparison.Ordinal))
            {
                string inner = value.Substring(prefix.Length, value.Length - prefix.Length - 1);

                if (!inner.Contains("="))
                {
                    return Color.FromName(inner);
                }

                int a = 255;
                int r = 0;
                int g = 0;
                int b = 0;
                string[] parts = inner.Split(',');

                for (int i = 0; i < parts.Length; i++)
                {
                    string token = parts[i].Trim();
                    int separatorIndex = token.IndexOf('=');
                    if (separatorIndex <= 0 || separatorIndex == token.Length - 1)
                    {
                        continue;
                    }

                    string key = token.Substring(0, separatorIndex).Trim();
                    string number = token.Substring(separatorIndex + 1).Trim();

                    if (!int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedValue))
                    {
                        continue;
                    }

                    if (string.Equals(key, "A", StringComparison.OrdinalIgnoreCase))
                    {
                        a = parsedValue;
                    }
                    else if (string.Equals(key, "R", StringComparison.OrdinalIgnoreCase))
                    {
                        r = parsedValue;
                    }
                    else if (string.Equals(key, "G", StringComparison.OrdinalIgnoreCase))
                    {
                        g = parsedValue;
                    }
                    else if (string.Equals(key, "B", StringComparison.OrdinalIgnoreCase))
                    {
                        b = parsedValue;
                    }
                }

                return Color.FromArgb(a, r, g, b);
            }

            return Color.FromName(value);
        }

        private sealed class PivotSettingsDto
        {
            public int ColorPArgb { get; set; }

            public int ColorS1Argb { get; set; }

            public int ColorS2Argb { get; set; }

            public int ColorS3Argb { get; set; }

            public int ColorS4Argb { get; set; }

            public int ColorR1Argb { get; set; }

            public int ColorR2Argb { get; set; }

            public int ColorR3Argb { get; set; }

            public int ColorR4Argb { get; set; }

            public bool PaintOn { get; set; }
        }

        /// <summary>
        /// delete data
        /// удалить данные
        /// </summary>
        public void Clear()
        {
            if (ValuesS1 != null)
            {
                ValuesS1.Clear();
                ValuesS2.Clear();
                ValuesS3.Clear();
                ValuesS4.Clear();
                ValuesR1.Clear();
                ValuesR2.Clear();
                ValuesR3.Clear();
                ValuesR4.Clear();
            }
            _myCandles = null;
        }

        /// <summary>
        /// display settings window
        /// показать окно настроек
        /// </summary>
        public void ShowDialog()
        {
            PivotUi ui = new PivotUi(this);
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
        /// candles to calculate indicator
        /// свечи для рассчёта индикатора
        /// </summary>
        private List<Candle> _myCandles;

        /// <summary>
        /// calculate indicator
        /// рассчитать индикатор
        /// </summary>
        /// <param name="candles">candles/свечи</param>
        public void Process(List<Candle> candles)
        {
            _myCandles = candles;

            if (ValuesS1 != null &&
                ValuesS1.Count + 1 == candles.Count)
            {
                ProcessOne(candles);
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
            if (ValuesS1 == null)
            {
                ValuesS1 = new List<decimal>();
                ValuesR1 = new List<decimal>();
                ValuesR2 = new List<decimal>();
                ValuesR3 = new List<decimal>();
                ValuesR4 = new List<decimal>();
                ValuesS1 = new List<decimal>();
                ValuesS2 = new List<decimal>();
                ValuesS3 = new List<decimal>();
                ValuesS4 = new List<decimal>();
            }

            if (candles.Count != 1 &&
                candles[candles.Count - 1].TimeStart.Day != candles[candles.Count - 2].TimeStart.Day &&
                _lastTimeUpdete != candles[candles.Count - 1].TimeStart)
            {
                _lastTimeUpdete = candles[candles.Count - 1].TimeStart;
                Reload(candles);
            }

            ValuesR1.Add(_r1);
            ValuesR2.Add(_r2);
            ValuesR3.Add(_r3);
            ValuesR4.Add(_r4);
            ValuesS1.Add(_s1);
            ValuesS2.Add(_s2);
            ValuesS3.Add(_s3);
            ValuesS4.Add(_s4);
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

            ValuesR1 = new List<decimal>();
            ValuesR2 = new List<decimal>();
            ValuesR3 = new List<decimal>();
            ValuesR4 = new List<decimal>();
            ValuesS1 = new List<decimal>();
            ValuesS2 = new List<decimal>();
            ValuesS3 = new List<decimal>();
            ValuesS4 = new List<decimal>();

            List<Candle> newCandles = new List<Candle>();

            for (int i = 0; i < candles.Count; i++)
            {
                newCandles.Add(candles[i]);
                if (newCandles.Count != 1 &&
                    newCandles[newCandles.Count - 1].TimeStart.Day != newCandles[newCandles.Count - 2].TimeStart.Day &&
                    _lastTimeUpdete != newCandles[newCandles.Count - 1].TimeStart)
                {
                    _lastTimeUpdete = newCandles[newCandles.Count - 1].TimeStart;
                    Reload(newCandles);
                }

                ValuesR1.Add(_r1);
                ValuesR2.Add(_r2);
                ValuesR3.Add(_r3);
                ValuesR4.Add(_r4);
                ValuesS1.Add(_s1);
                ValuesS2.Add(_s2);
                ValuesS3.Add(_s3);
                ValuesS4.Add(_s4);
            }

        }


        private void Reload(List<Candle> candles)
        {
            /*
             
RANGE = H — L

R1 = C + RANGE * 1.1/12

R2 = C + RANGE * 1.1/6

R3 = C + RANGE * 1.1/4

R4 = C + RANGE * 1.1/2

S1 = C — RANGE * 1.1/12

S2 = C — RANGE * 1.1/6

S3 = C — RANGE * 1.1/4

S4 = C — RANGE * 1.1/2
            
             */

            decimal H = 0;

            decimal L = decimal.MaxValue;

            decimal C = 0;

            for (int i = candles.Count - 2; i > 0; i++)
            {
                if (C == 0)
                {
                    C = candles[i].Close;
                }

                if (candles[i].High > H)
                {
                    H = candles[i].High;
                }

                if (candles[i].Low < L)
                {
                    L = candles[i].Low;
                }

                if (i != 1 && candles[i].TimeStart.Day != candles[i - 1].TimeStart.Day)
                {
                    break;
                }

            }

            decimal range = H - L;

            _r1 = C + range * 1.1m / 12;
            _r2 = C + range * 1.1m / 6;
            _r3 = C + range * 1.1m / 4;
            _r4 = C + range * 1.1m / 2;

            _s1 = C - range * 1.1m / 12;
            _s2 = C - range * 1.1m / 6;
            _s3 = C - range * 1.1m / 4;
            _s4 = C - range * 1.1m / 2;
        }

        private DateTime _lastTimeUpdete;

        private decimal _r1;
        private decimal _r2;
        private decimal _r3;
        private decimal _r4;

        private decimal _s1;
        private decimal _s2;
        private decimal _s3;
        private decimal _s4;
    }
}

