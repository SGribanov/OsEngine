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
    /// Indicator Ishimoku by Bill Williams
    /// Индикатор Ishimoku. Билла Вильямса
    /// </summary>
    public class Ichimoku : IIndicator
    {
        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Ichimoku(string uniqName, bool canDelete)
        {
            Name = uniqName;
            TypeIndicator = IndicatorChartPaintType.Line;

            LengthFirst = 9;
            LengthSecond = 26;
            LengthFird = 52;
            LengthSdvig = 26;
            LengthChinkou = 26;

            ColorEtalonLine = Color.BlueViolet;
            ColorLineRounded = Color.OrangeRed;
            ColorLineLate = Color.DarkRed;
            ColorLineFirst = Color.LimeGreen;
            ColorLineSecond = Color.DodgerBlue;

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
        public Ichimoku(bool canDelete)
        {
            Name = Guid.NewGuid().ToString();

            TypeIndicator = IndicatorChartPaintType.Line;

            LengthFirst = 9;
            LengthSecond = 26;
            LengthFird = 52;
            LengthSdvig = 26;

            ColorEtalonLine = Color.BlueViolet;
            ColorLineRounded = Color.OrangeRed;
            ColorLineLate = Color.DarkRed;
            ColorLineFirst = Color.LimeGreen;
            ColorLineSecond = Color.DodgerBlue;

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
                list.Add(ValuesEtalonLine_Kejun_sen);
                list.Add(ValuesLineRounded_Teken_sen);
                list.Add(ValuesLineLate_Chinkou_span);
                list.Add(ValuesLineFirst_Senkkou_span_A);
                list.Add(ValuesLineSecond_Senkou_span_B);

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
                colors.Add(ColorEtalonLine);
                colors.Add(ColorLineRounded);
                colors.Add(ColorLineLate);
                colors.Add(ColorLineFirst);
                colors.Add(ColorLineSecond);

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
        /// reference line
        /// эталонная линия
        /// </summary>
        public List<decimal> ValuesEtalonLine_Kejun_sen
        { get; set; }

        /// <summary>
        /// rotation line
        /// линия вращения
        /// </summary> 
        public List<decimal> ValuesLineRounded_Teken_sen
        { get; set; }

        /// <summary>
        /// delayed line
        /// запаздывающая линия
        /// </summary>
        public List<decimal> ValuesLineLate_Chinkou_span
        { get; set; }

        /// <summary>
        /// first warning line
        /// первая предупреждающая
        /// </summary>
        public List<decimal> ValuesLineFirst_Senkkou_span_A
        { get; set; }

        /// <summary>
        /// second warning line
        /// вторая предупреждающая
        /// </summary>
        public List<decimal> ValuesLineSecond_Senkou_span_B
        { get; set; }

        /// <summary>
        /// unique indicator name
        /// уникальное имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// reference line color
        /// цвет эталонной линии
        /// </summary>
        public Color ColorEtalonLine { get; set; }

        /// <summary>
        /// rotation line color
        /// цвет линии вращения
        /// </summary>
        public Color ColorLineRounded { get; set; }

        /// <summary>
        /// delayed line color
        /// цвет запаздывающей линии
        /// </summary>
        public Color ColorLineLate { get; set; }

        /// <summary>
        /// color of first warning line
        /// цвет первой упреждающей
        /// </summary>
        public Color ColorLineFirst { get; set; }

        /// <summary>
        /// color of second warning line
        /// цвет второй упреждающей
        /// </summary>
        public Color ColorLineSecond { get; set; }

        /// <summary>
        /// period one
        /// период один
        /// </summary>
        public int LengthFirst;

        /// <summary>
        /// period two
        /// период два
        /// </summary>
        public int LengthSecond;

        /// <summary>
        /// period three
        /// период три
        /// </summary>
        public int LengthFird;

        /// <summary>
        /// shift
        /// сдвиг
        /// </summary>
        public int LengthSdvig;

        /// <summary>
        /// shift
        /// сдвиг
        /// </summary>
        public int LengthChinkou;

        /// <summary>
        /// is indicator tracing enabled
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
                    new IchimokuSettingsDto
                    {
                        LengthFirst = LengthFirst,
                        LengthSecond = LengthSecond,
                        LengthFird = LengthFird,
                        ColorEtalonLineArgb = ColorEtalonLine.ToArgb(),
                        ColorLineRoundedArgb = ColorLineRounded.ToArgb(),
                        ColorLineLateArgb = ColorLineLate.ToArgb(),
                        ColorLineFirstArgb = ColorLineFirst.ToArgb(),
                        ColorLineSecondArgb = ColorLineSecond.ToArgb(),
                        PaintOn = PaintOn,
                        LengthSdvig = LengthSdvig,
                        LengthChinkou = LengthChinkou
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
                IchimokuSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                LengthFirst = settings.LengthFirst;
                LengthSecond = settings.LengthSecond;
                LengthFird = settings.LengthFird;
                ColorEtalonLine = Color.FromArgb(settings.ColorEtalonLineArgb);
                ColorLineRounded = Color.FromArgb(settings.ColorLineRoundedArgb);
                ColorLineLate = Color.FromArgb(settings.ColorLineLateArgb);
                ColorLineFirst = Color.FromArgb(settings.ColorLineFirstArgb);
                ColorLineSecond = Color.FromArgb(settings.ColorLineSecondArgb);
                PaintOn = settings.PaintOn;
                LengthSdvig = settings.LengthSdvig;
                LengthChinkou = settings.LengthChinkou;

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

        private static IchimokuSettingsDto ParseLegacySettings(string content)
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

            int lengthSdvig = Convert.ToInt32(lines[9]);
            int lengthChinkou = lines.Length >= 11
                ? Convert.ToInt32(lines[10])
                : lengthSdvig;

            return new IchimokuSettingsDto
            {
                LengthFirst = Convert.ToInt32(lines[0]),
                LengthSecond = Convert.ToInt32(lines[1]),
                LengthFird = Convert.ToInt32(lines[2]),
                ColorEtalonLineArgb = Convert.ToInt32(lines[3]),
                ColorLineRoundedArgb = Convert.ToInt32(lines[4]),
                ColorLineLateArgb = Convert.ToInt32(lines[5]),
                ColorLineFirstArgb = Convert.ToInt32(lines[6]),
                ColorLineSecondArgb = Convert.ToInt32(lines[7]),
                PaintOn = Convert.ToBoolean(lines[8]),
                LengthSdvig = lengthSdvig,
                LengthChinkou = lengthChinkou
            };
        }

        private sealed class IchimokuSettingsDto
        {
            public int LengthFirst { get; set; }

            public int LengthSecond { get; set; }

            public int LengthFird { get; set; }

            public int ColorEtalonLineArgb { get; set; }

            public int ColorLineRoundedArgb { get; set; }

            public int ColorLineLateArgb { get; set; }

            public int ColorLineFirstArgb { get; set; }

            public int ColorLineSecondArgb { get; set; }

            public bool PaintOn { get; set; }

            public int LengthSdvig { get; set; }

            public int LengthChinkou { get; set; }
        }

        /// <summary>
        /// delete data
        /// удалить данные
        /// </summary>
        public void Clear()
        {
            if (ValuesEtalonLine_Kejun_sen != null)
            {
                ValuesEtalonLine_Kejun_sen.Clear();
                ValuesLineLate_Chinkou_span.Clear();
                ValuesLineRounded_Teken_sen.Clear();
            }
            _myCandles = null;
        }

        /// <summary>
        /// display settings window
        /// показать окно настроек
        /// </summary>
        public void ShowDialog()
        {
            IshimokuUi ui = new IshimokuUi(this);
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

            if (ValuesEtalonLine_Kejun_sen != null &&
                ValuesEtalonLine_Kejun_sen.Count + 1 == candles.Count)
            {
                ProcessOne(candles);
            }
            else if (ValuesEtalonLine_Kejun_sen != null &&
                     ValuesEtalonLine_Kejun_sen.Count == candles.Count)
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
            if (ValuesEtalonLine_Kejun_sen == null)
            {
                ValuesEtalonLine_Kejun_sen = new List<decimal>();
                ValuesLineRounded_Teken_sen = new List<decimal>();
                ValuesLineLate_Chinkou_span = new List<decimal>();
                ValuesLineFirst_Senkkou_span_A = new List<decimal>();
                ValuesLineSecond_Senkou_span_B = new List<decimal>();
            }

            ValuesEtalonLine_Kejun_sen.Add(GetLine(candles, candles.Count - 1, LengthSecond, 0));
            ValuesLineRounded_Teken_sen.Add(GetLine(candles, candles.Count - 1, LengthFirst, 0));

            if (candles.Count - 1 >= LengthChinkou)
            {
                ValuesLineLate_Chinkou_span.Add(GetLineLate(candles, candles.Count - 1 - LengthChinkou));
            }

            ValuesLineFirst_Senkkou_span_A.Add(GetLineFirst(candles, candles.Count - 1));
            ValuesLineSecond_Senkou_span_B.Add(GetLine(candles, candles.Count - 1, LengthFird, LengthSdvig));
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
            ValuesEtalonLine_Kejun_sen = new List<decimal>();
            ValuesLineRounded_Teken_sen = new List<decimal>();
            ValuesLineLate_Chinkou_span = new List<decimal>();
            ValuesLineFirst_Senkkou_span_A = new List<decimal>();
            ValuesLineSecond_Senkou_span_B = new List<decimal>();

            for (int i = 0; i < candles.Count; i++)
            {

                ValuesEtalonLine_Kejun_sen.Add(GetLine(candles, i, LengthSecond, 0));
                ValuesLineRounded_Teken_sen.Add(GetLine(candles, i, LengthFirst, 0));

                if (i >= LengthChinkou)
                {
                    ValuesLineLate_Chinkou_span.Add(GetLineLate(candles, i - LengthChinkou));
                }

                ValuesLineFirst_Senkkou_span_A.Add(GetLineFirst(candles, i));
                ValuesLineSecond_Senkou_span_B.Add(GetLine(candles, i, LengthFird, LengthSdvig));
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
            ValuesEtalonLine_Kejun_sen[ValuesEtalonLine_Kejun_sen.Count - 1] = (GetLine(candles, candles.Count - 1, LengthSecond, 0));
            ValuesLineRounded_Teken_sen[ValuesLineRounded_Teken_sen.Count - 1] = (GetLine(candles, candles.Count - 1, LengthFirst, 0));

            if (candles.Count >= LengthChinkou)
            {
                ValuesLineLate_Chinkou_span[ValuesLineLate_Chinkou_span.Count - 1] = (GetLineLate(candles, candles.Count - 1 - LengthChinkou));
            }

            ValuesLineLate_Chinkou_span[ValuesLineLate_Chinkou_span.Count - 1] = (GetLineLate(candles, candles.Count - 1));
            ValuesLineFirst_Senkkou_span_A[ValuesLineFirst_Senkkou_span_A.Count - 1] = (GetLineFirst(candles, candles.Count - 1));
            ValuesLineSecond_Senkou_span_B[ValuesLineSecond_Senkou_span_B.Count - 1] = (GetLine(candles, candles.Count - 1, LengthFird, LengthSdvig));

        }

        private decimal GetLine(List<Candle> candles, int index, int length, int shift)
        {
            index = index - shift;

            if (index < 0)
            {
                return candles[candles.Count - 1].Close;
            }

            decimal high = 0;
            decimal low = decimal.MaxValue;

            for (int i = index; i > -1 && i > index - length; i--)
            {
                if (candles[i].High > high)
                {
                    high = candles[i].High;
                }
                if (candles[i].Low < low)
                {
                    low = candles[i].Low;
                }
            }
            decimal val = (low + high) / 2;
            return (low + high) / 2;
        }

        public decimal GetLineLate(List<Candle> candles, int index)
        {

            if (index + LengthChinkou >= candles.Count)
            {
                return candles[candles.Count - 1].Close;
            }

            return candles[index + LengthChinkou].Close;
        }

        public decimal GetLineFirst(List<Candle> candles, int index)
        {
            if (LengthSdvig >= index + 1 ||
                LengthFirst >= index + 1 ||
                index - LengthSdvig < LengthSdvig ||
                index - LengthSdvig < LengthFirst)
            {
                return 0;
            }

            return (ValuesEtalonLine_Kejun_sen[index - LengthSdvig] + ValuesLineRounded_Teken_sen[index - LengthSdvig]) / 2;
        }
    }
}

