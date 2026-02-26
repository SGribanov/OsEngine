#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 *Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
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
    /// PriceChannel indicator
    /// PriceChannel Индикатор
    /// </summary>
    public class PriceChannel: IIndicator
    {
        /// <summary>
        /// constructor with parameters. Indicator will be saved
        /// конструктор с параметрами. Индикатор будет сохраняться
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public PriceChannel(string uniqName,bool canDelete)
        {
            Name = uniqName;

            TypeIndicator = IndicatorChartPaintType.Line;
            ColorUp = Color.DodgerBlue;
            ColorDown = Color.DarkRed;
            LengthUpLine = 12;
            LengthDownLine = 12;
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
        public PriceChannel(bool canDelete)
        {
            Name = Guid.NewGuid().ToString();

            TypeIndicator = IndicatorChartPaintType.Line;
            ColorUp = Color.DodgerBlue;
            ColorDown = Color.DarkRed;
            LengthUpLine = 12;
            LengthDownLine = 12;
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
        /// имя серии данных на которой прорисовывается индикатор
        /// </summary>
        public string NameSeries
        { get; set; }

        /// <summary>
        /// name of data area where indicator will be drawn
        /// имя области данных на которой прорисовывается индикатор
        /// </summary>
        public string NameArea
        { get; set; }

        /// <summary>
        /// upper channel
        /// верхний канал
        /// </summary>
        public List<decimal> ValuesUp
        { get; set; }

        /// <summary>
        /// lower channel
        /// нижний канал
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
        /// upper channel calculation length
        /// длина расчета верхнего канала
        /// </summary>
        public int LengthUpLine
        { get; set; }

        /// <summary>
        /// bottom channel calculation length
        /// длина расчета нижнего канала
        /// </summary>
        public int LengthDownLine
        { get; set; }

        /// <summary>
        /// channel top edge color
        /// цвет верхней границы канала
        /// </summary>
        public Color ColorUp
        { get; set; }

        /// <summary>
        /// channel bottom edge color
        /// цвет нижней границы канала
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
                PriceChannelSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                ColorUp = Color.FromArgb(settings.ColorUpArgb);
                ColorDown = Color.FromArgb(settings.ColorDownArgb);
                LengthUpLine = settings.LengthUpLine;
                LengthDownLine = settings.LengthDownLine;
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
                if (string.IsNullOrWhiteSpace(Name))
                {
                    return;
                }

                SettingsManager.Save(
                    GetSettingsPath(),
                    new PriceChannelSettingsDto
                    {
                        ColorUpArgb = ColorUp.ToArgb(),
                        ColorDownArgb = ColorDown.ToArgb(),
                        LengthUpLine = LengthUpLine,
                        LengthDownLine = LengthDownLine,
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

        private static PriceChannelSettingsDto ParseLegacySettings(string content)
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

            return new PriceChannelSettingsDto
            {
                ColorUpArgb = Convert.ToInt32(lines[0], CultureInfo.InvariantCulture),
                ColorDownArgb = Convert.ToInt32(lines[1], CultureInfo.InvariantCulture),
                LengthUpLine = Convert.ToInt32(lines[2], CultureInfo.InvariantCulture),
                LengthDownLine = Convert.ToInt32(lines[3], CultureInfo.InvariantCulture),
                PaintOn = Convert.ToBoolean(lines[4])
            };
        }

        private sealed class PriceChannelSettingsDto
        {
            public int ColorUpArgb { get; set; }

            public int ColorDownArgb { get; set; }

            public int LengthUpLine { get; set; }

            public int LengthDownLine { get; set; }

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
            PriceChannelUi ui = new PriceChannelUi(this);
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
        /// indicator needs to be redrawn
        /// нужно перерисовать индикатор
        /// </summary>
        public event Action<IIndicator> NeedToReloadEvent;

        /// <summary>
        /// candles for which indicator calculated
        /// свечи для которых рассчитывается индикатор
        /// </summary>
        private List<Candle> _myCandles;
        //calculation
        // вычисления

        /// <summary>
        /// to upload new candles
        /// прогрузить новыми свечками
        /// </summary>
        public void Process(List<Candle> candles)
        {
            if (candles == null)
            {
                return;
            }
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
        /// count the last candle
        /// пересчитать последнюю
        /// </summary>
        private void ProcessLast(List<Candle> candles)
        {
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
            // consider the upper value
            // считаем верхнее значение
            decimal[] lines = new decimal[2];

            if (index - LengthUpLine <= 0 || 
                candles.Count <= LengthUpLine)
            {
                lines[0] = 0;
            }
            else
            {
                decimal upLine = 0;

                for (int i = index; i > -1 && i > index - LengthUpLine; i--)
                {
                    if (upLine < candles[i].High)
                    {
                        upLine = candles[i].High;
                    }
                }

                lines[0] = upLine;
            }
            //consider the upper value
            // считаем верхнее значение

            if (index - LengthDownLine <= 0 ||
                candles.Count <= LengthDownLine)
            {
                lines[1] = 0;
            }
            else
            {
                decimal downLine = decimal.MaxValue;

                for (int i = index; i > -1 && i > index - LengthDownLine; i--)
                {
                    if (downLine > candles[i].Low)
                    {
                        downLine = candles[i].Low;
                    }
                }

                lines[1] = downLine;
            }

            return lines;
        }
    }
}

