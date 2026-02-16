/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using OsEngine.Entity;
using OsEngine.Indicators;

namespace OsEngine.Charts.CandleChart.Indicators
{
    /// <summary>
    ///  Volume-tick oscillator of contract flow per interval
    ///  объемно-тиковый осциллятор потока контрактов за интервал
    /// </summary>
    public class TradeThread: IIndicator
    {

        /// <summary>
        /// constructor with parameters. Indicator will be saved
        /// конструктор с параметрами. Индикатор будет сохраняться
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public TradeThread(string uniqName, bool canDelete)
        {
            Name = uniqName;
            TypeIndicator = IndicatorChartPaintType.Line;
            TypePointsToSearch = PriceTypePoints.Typical;
            ColorBase = Color.DeepSkyBlue;
            Length = 20;
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
        public TradeThread(bool canDelete)
        {
            Name = Guid.NewGuid().ToString();
            TypeIndicator = IndicatorChartPaintType.Line;
            TypePointsToSearch = PriceTypePoints.Typical;
            ColorBase = Color.DeepSkyBlue;
            Length = 20;
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
                colors.Add(ColorBase);
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
        /// on what point indicator will be built on: Open, Close ...
        /// по какой точке будет строиться индикатор по: Open, Close ...
        /// </summary>
        public PriceTypePoints TypePointsToSearch;

        /// <summary>
        /// name of data series on which indicator will be drawn
        /// имя серии данных на которой индикатор прорисовывается
        /// </summary>
        public string NameSeries { get; set; }

        /// <summary>
        /// name of data area where indicator will be drawn
        /// имя области данных на которой индикатор прорисовывается
        /// </summary>
        public string NameArea { get; set; }

        /// <summary>
        /// CCI value
        /// значение CCI
        /// </summary>
        public List<decimal> Values { get; set; }

        /// <summary>
        /// unique indicator name
        /// уникальное имя индикатора
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// длинна расчёта индикатора
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// indicator calculation length
        /// цвет линии индикатора
        /// </summary>
        public Color ColorBase { get; set; }

        /// <summary>
        /// candles to calculate indicator
        /// включена ли прорисовка серии на чарте
        /// </summary>
        public bool PaintOn { get; set; }

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
                TradeThreadSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                ColorBase = Color.FromArgb(settings.ColorArgb);
                Length = settings.Length;
                PaintOn = settings.PaintOn;
                TypePointsToSearch = settings.TypePointsToSearch;
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
                    new TradeThreadSettingsDto
                    {
                        ColorArgb = ColorBase.ToArgb(),
                        Length = Length,
                        PaintOn = PaintOn,
                        TypePointsToSearch = TypePointsToSearch
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

        private static TradeThreadSettingsDto ParseLegacySettings(string content)
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

            PriceTypePoints typePointsToSearch = PriceTypePoints.Typical;
            Enum.TryParse(lines[3], true, out typePointsToSearch);

            return new TradeThreadSettingsDto
            {
                ColorArgb = Convert.ToInt32(lines[0]),
                Length = Convert.ToInt32(lines[1]),
                PaintOn = Convert.ToBoolean(lines[2]),
                TypePointsToSearch = typePointsToSearch
            };
        }

        private sealed class TradeThreadSettingsDto
        {
            public int ColorArgb { get; set; }

            public int Length { get; set; }

            public bool PaintOn { get; set; }

            public PriceTypePoints TypePointsToSearch { get; set; }
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
        }

        /// <summary>
        /// display settings window
        /// показать окно с настройками
        /// </summary>
        public void ShowDialog()
        {
            MessageBox.Show("У данного индикатора нет настроек");
        }

        /// <summary>
        /// it's necessary to redraw indicator on chart
        /// необходимо перерисовать индикатор на графике
        /// </summary>
        public event Action<IIndicator> NeedToReloadEvent { add { } remove { } }

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
            if (Values != null && Values.Count + 1 == candles.Count)
            {
                ProcessOne(candles);
            }
            else if (Values != null && Values.Count == candles.Count)
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
            if (candles == null) return;

            if (Values == null) Values = new List<decimal>();

            Values.Add(GetValue(candles, candles.Count - 1));
        }

        /// <summary>
        /// to upload from the beginning
        /// прогрузить с самого начала
        /// </summary>
        private void ProcessAll(List<Candle> candles)
        {
            if (candles == null) return;

            Values = new List<decimal>();

            for (int i = 0; i < candles.Count; i++)
            {
                Values.Add(GetValue(candles, i));

            }
        }

        /// <summary>
        /// overload the last value
        /// перегрузить последнее значение
        /// </summary>
        private void ProcessLast(List<Candle> candles)
        {
            if (candles == null) return;

            Values[Values.Count - 1] = GetValue(candles, candles.Count - 1);
        }

        /// <summary>
        /// take the indicator value by index
        /// взять значение индикаторм по индексу
        /// </summary>
        private decimal GetValue(List<Candle> candles, int index)
        {
            if (index - Length <= 0)
            {
                return 0;
            }

            List<Trade> trades = candles[index].Trades;

            if (trades == null ||
                trades.Count == 0)
            {
                return 0;
            }

            decimal nBuy = 0;

            decimal vBuy = 0;

            decimal nSell = 0;

            decimal vSell = 0;

            for (int i = 0; i < trades.Count; i++)
            {
                if (trades[i].Side == Side.Buy)
                {
                    nBuy++;
                    vBuy += trades[i].Volume;
                }
                if (trades[i].Side == Side.Sell)
                {
                    nSell++;
                    vSell += trades[i].Volume;
                }
            }

            decimal vto = (nBuy*vBuy - nSell*vSell)/(nBuy*vBuy + nSell*vSell);

            return Math.Round(vto, 5);
        }
    }
}
