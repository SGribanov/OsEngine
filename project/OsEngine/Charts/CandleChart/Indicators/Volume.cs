#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
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
    /// Volume. Candle volume. Indicator
    ///  Volume. Объём свечек. Индикатор
    /// </summary>
    public class Volume:IIndicator
    {

        /// <summary>
        /// constructor with parameters. Indicator will be saved
        /// конструктор с параметрами. Индикатор будет сохраняться
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Volume(string uniqName,bool canDelete)
        {
            Name = uniqName;
            TypeIndicator = IndicatorChartPaintType.Column;
            ColorUp = Color.DodgerBlue;
            ColorDown = Color.DarkRed;
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
        public Volume(bool canDelete)
        {
            Name = Guid.NewGuid().ToString();
            TypeIndicator = IndicatorChartPaintType.Column;
            ColorUp = Color.DodgerBlue;
            ColorDown = Color.DarkRed;
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
        /// тип индикатора
        /// </summary>
        public IndicatorChartPaintType TypeIndicator
        { get; set; }

        /// <summary>
        /// name of data series on which indicator will be drawn
        /// имя серии данных на которой будет прорисовываться индикатор
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
        /// volume
        /// объём
        /// </summary>
        public List<decimal> Values
        { get; set; }

        /// <summary>
        /// unique indicator name
        /// уникальное имя
        /// </summary>
        public string Name
        { get; set; }

        /// <summary>
        /// color of increasing volume
        /// цвет растущего объёма
        /// </summary>
        public Color ColorUp
        { get; set; }

        /// <summary>
        /// color of falling volume
        /// цвет падающего объёма
        /// </summary>
        public Color ColorDown
        { get; set; }

        /// <summary>
        /// candles to calculate indicator
        /// включена ли прорисовка индикатора на чарте
        /// </summary>
        public bool PaintOn
        { get; set; }

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
                    new VolumeSettingsDto
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
                VolumeSettingsDto settings = SettingsManager.Load(
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
        }

        private string GetSettingsPath()
        {
            return @"Engine\" + Name + @".txt";
        }

        private static VolumeSettingsDto ParseLegacySettings(string content)
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

            return new VolumeSettingsDto
            {
                ColorUpArgb = Convert.ToInt32(lines[0], CultureInfo.InvariantCulture),
                ColorDownArgb = Convert.ToInt32(lines[1], CultureInfo.InvariantCulture),
                PaintOn = Convert.ToBoolean(lines[2])
            };
        }

        private sealed class VolumeSettingsDto
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
        }

        /// <summary>
        /// display settings window
        /// показать окно настроек
        /// </summary>
        public void ShowDialog()
        {
            VolumeUi ui = new VolumeUi(this);
            ui.ShowDialog();

            if (ui.IsChange)
            {
                if (NeedToReloadEvent != null)
                {
                    NeedToReloadEvent(this);
                }
            }
        }

        /// <summary>
        /// indicator needs to be redrawn
        /// нужно перерисовать индикатор
        /// </summary>
        public event Action<IIndicator> NeedToReloadEvent;
        // calculating
        // вычисления

        /// <summary>
        /// load indicator with candles
        /// прогрузить индикатор свечками
        /// </summary>
        public void Process(List<Candle> candles)
        {
            if (Values != null &&
                           Values.Count + 1 == candles.Count)
            {
                ProcessOneCandle(candles);
            }
            else if (Values != null &&
                Values.Count == candles.Count)
            {
                ProcessLastCanlde(candles);
            }
            else
            {
                ProcessAllCandle(candles);
            }
        }

        /// <summary>
        /// load only last candle
        /// прогрузить только последнюю свечку
        /// </summary>
        private void ProcessOneCandle(List<Candle> candles)
        {
            if (Values == null)
            {
                Values = new List<decimal>();
                Values.Add(candles[candles.Count-1].Volume);
            }
            else
            {
                Values.Add(candles[candles.Count - 1].Volume);
            }
        }

        /// <summary>
        /// load all candles
        /// прогрузить все свечи
        /// </summary>
        private void ProcessAllCandle(List<Candle> candles)
        {
            Values = new List<decimal>();

            for (int i = 0; i < candles.Count; i++)
            {
                Values.Add(candles[i].Volume);
            }
        }

        /// <summary>
        /// load only last candle
        /// перегрузить последнюю свечу
        /// </summary>
        private void ProcessLastCanlde(List<Candle> candles)
        {
            Values[Values.Count-1] = (candles[candles.Count - 1].Volume);
        }
    }
}

