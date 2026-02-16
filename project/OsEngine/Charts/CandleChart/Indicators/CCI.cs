/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использования кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
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
    /// Calculation type indicator Moving average
    /// Тип расчета индикатор Скользящая средняя
    /// </summary>


    /// <summary>
    /// indicator Standard Deviation / Индикатор Среднеквадратическое отклонение
    /// </summary>
    public class Cci : IIndicator
    {

        /// <summary>
        /// constructor with parameters. Indicator will save settings.
        /// конструктор с параметрами. Индикатор будет сохранять настройки
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Cci(string uniqName, bool canDelete)
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
        public Cci(bool canDelete)
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
        /// </summar
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
        /// period length to calculate indicator
        /// длинна расчёта индикатора
        /// </summary>
        public int Length { get; set; } 

        /// <summary>
        /// indicator line color
        /// цвет линии индикатора
        /// </summary>
        public Color ColorBase { get; set; }

        /// <summary>
        /// is indicator tracing enabled
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
                CciSettingsDto settings = SettingsManager.Load(
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
                SettingsManager.Save(
                    GetSettingsPath(),
                    new CciSettingsDto
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

        private static CciSettingsDto ParseLegacySettings(string content)
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

            return new CciSettingsDto
            {
                ColorArgb = Convert.ToInt32(lines[0]),
                Length = Convert.ToInt32(lines[1]),
                PaintOn = Convert.ToBoolean(lines[2]),
                TypePointsToSearch = typePointsToSearch
            };
        }

        private sealed class CciSettingsDto
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
            _myCandles = null;
        }

        /// <summary>
        /// candles to calculate indicator
        /// свечи по которым строится индикатор
        /// </summary>
        private List<Candle> _myCandles;

        /// <summary>
        /// display settings window
        /// показать окно с настройками
        /// </summary>
        public void ShowDialog()
        {
            CciUi ui = new CciUi(this);
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
        /// it's necessary to redraw indicator on chart
        /// необходимо перерисовать индикатор на графике
        /// </summary>
        public event Action<IIndicator> NeedToReloadEvent;

        /// <summary>
        /// take point values to calculate data
        /// взять значения точки для расчета данных
        /// </summary>
        /// <param name="candles">candles/свечи</param>
        /// <param name="index">index/индекс</param>
        /// <returns>index value/значение индикатора по индексу</returns>
        private decimal GetPoint(List<Candle> candles, int index)
        {
            if (TypePointsToSearch == PriceTypePoints.Close)
            {
                return candles[index].Close;
            }
            else if (TypePointsToSearch == PriceTypePoints.High)
            {
                return candles[index].High;
            }
            else if (TypePointsToSearch == PriceTypePoints.Low)
            {
                return candles[index].Low;
            }
            else if (TypePointsToSearch == PriceTypePoints.Open)
            {
                return candles[index].Open;
            }
            else if (TypePointsToSearch == PriceTypePoints.Median)
            {
                return (candles[index].High + candles[index].Low) / 2;
            }
            else if (TypePointsToSearch == PriceTypePoints.Typical)
            {
                return (candles[index].High + candles[index].Low + candles[index].Close) / 3;
            }
            return 0;
        }

        /// <summary>
        /// to load new candles
        /// прогрузить новыми свечками
        /// </summary>        
        public void Process(List<Candle> candles)
        {
            if (candles == null)
            {
                return;
            }
            _myCandles = candles;
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

            Values.Add(GetValueCci(candles, candles.Count - 1));
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
                Values.Add(GetValueCci(candles, i));

            }
        }

        /// <summary>
        /// overload last value
        /// перегрузить последнее значение
        /// </summary>
        private void ProcessLast(List<Candle> candles)
        {
            if (candles == null) return;

            Values[Values.Count - 1] = GetValueCci(candles, candles.Count - 1);
        }

        /// <summary>
        /// take indicator value by index
        /// взять значение индикаторм по индексу
        /// </summary>
        private decimal GetValueCci(List<Candle> candles, int index)
        {

            if (index - Length <= 0)
            {
                return 0;
            }

            decimal sum = 0;
            for (int i = index; i > index - Length; i--)
            {
                sum += GetPoint(candles, i);
            }
            // average count
            // подсчет средней
            var ma = sum / Length;       

            decimal md = 0;
            for (int i = index; i > index - Length; i--)
            {
                md += Math.Abs(ma - GetPoint(candles, i));
            }

            var cciP = (GetPoint(candles, index) - ma) / (md * 0.015m / Length);
            return Math.Round(cciP, 5);
        }
    }
}
