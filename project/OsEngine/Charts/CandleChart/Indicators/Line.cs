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
    /// line based on array of values - decimal 
    /// линия построенная на основе массива значений decimal
    /// </summary>
    public class Line : IIndicator
    {
        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Line(string uniqName, bool canDelete)
        {
            Name = uniqName;
            TypeIndicator = IndicatorChartPaintType.Line;
            ColorBase = Color.DodgerBlue;
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
        public Line(bool canDelete)
        {
            Name = Guid.NewGuid().ToString();

            TypeIndicator = IndicatorChartPaintType.Line;
            ColorBase = Color.DodgerBlue;
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
        public IndicatorChartPaintType TypeIndicator
        { get; set; }

        /// <summary>
        /// имя серии на которой индикатор прорисовывается
        /// </summary>
        public string NameSeries
        { get; set; }

        /// <summary>
        /// name of data series on which indicator will be drawn
        /// имя области на котророй индикатор прорисовывается
        /// </summary>
        public string NameArea
        { get; set; }

        /// <summary>
        /// indicator value
        /// значение индикатора
        /// </summary>
        public List<decimal> Values 
        { get; set; }

        /// <summary>
        /// unique indicator name
        /// уникальное имя индикатора
        /// </summary>
        public string Name
        { get; set; }

        /// <summary>
        /// color for drawing base data point
        /// цвет для прорисовки базовой точки данных
        /// </summary>
        public Color ColorBase
        { get; set; }

        /// <summary>
        /// is indicator tracing enabled
        /// включена ли прорисовка индикатора
        /// </summary>
        public bool PaintOn
        { get; set; }

        /// <summary>
        /// save settings to file
        /// сохранить настройки
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return;
            }
            try
            {
                SettingsManager.Save(
                    GetSettingsPath(),
                    new LineSettingsDto
                    {
                        ColorArgb = ColorBase.ToArgb(),
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
        /// загрузить настройки
        /// </summary>
        public void Load()
        {
            if (!File.Exists(GetSettingsPath()))
            {
                return;
            }
            try
            {
                LineSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                ColorBase = Color.FromArgb(settings.ColorArgb);
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

        private static LineSettingsDto ParseLegacySettings(string content)
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

            if (lines.Length < 2)
            {
                return null;
            }

            return new LineSettingsDto
            {
                ColorArgb = Convert.ToInt32(lines[0], CultureInfo.InvariantCulture),
                PaintOn = Convert.ToBoolean(lines[1])
            };
        }

        private sealed class LineSettingsDto
        {
            public int ColorArgb { get; set; }

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
            //ignored. This type of indicator set up and created only from code
            // ignored. Этот тип индикатора настраивается и создаётся только из кода
        }

        /// <summary>
        /// indicator needs to be redrawn
        /// индикатор нужно перерисовать
        /// </summary>
        public event Action<IIndicator> NeedToReloadEvent { add { } remove { } }

        /// <summary>
        /// recalculate indicator. This indicator blocked.
        /// пересчитать индикатор. У данного индикатора блокировано.
        /// </summary>
        /// <param name="candles">candles/свечи</param>
        public void Process(List<Candle> candles)
        {
        
        }

        /// <summary>
        /// upload new values
        /// прогрузить новыми значениями
        /// </summary>
        /// <param name="decimals"></param>
        public void ProcessDesimals(List<Decimal> decimals)
        {
            Values = decimals;
        }

    }
}


