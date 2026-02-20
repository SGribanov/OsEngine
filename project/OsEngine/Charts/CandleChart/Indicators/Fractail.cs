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
    /// Fractals. Indicator Fractal bu Bill Williams
    /// Fractal. индикатор фрактал. В интерпритации Билла Вильямса
    /// </summary>
    public class Fractal : IIndicator
    {

        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Fractal(string uniqName,bool canDelete)
        {
            Name = uniqName;

            TypeIndicator = IndicatorChartPaintType.Point;
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
        public Fractal(bool canDelete) 
        {
            Name = Guid.NewGuid().ToString();

            TypeIndicator = IndicatorChartPaintType.Point;
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
        /// тип индикатора
        /// </summary>
        public IndicatorChartPaintType TypeIndicator { get; set; }

        /// <summary>
        ///  name of data series on which indicator will be drawn
        /// имя серии данных на которой будет прорисовываться индикатор
        /// </summary>
        public string NameSeries { get; set; }

        /// <summary>
        /// name of data area where indicator will be drawn
        /// имя области данных на которой будет прорисовываться индикатор
        /// </summary>
        public string NameArea
        { get; set; }

        /// <summary>
        /// indicator name
        /// имя индикатора
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// color of upper data series
        /// цвет верхней серии данных
        /// </summary>
        public Color ColorUp { get; set; }

        /// <summary>
        /// color of lower data series
        /// цвет нижней серии данных
        /// </summary>
        public Color ColorDown { get; set; }

        /// <summary>
        /// is indicator tracing enabled
        /// включена ли прорисовка индикаторов
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
                FractalSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                PaintOn = settings.PaintOn;
                ColorUp = Color.FromArgb(settings.ColorUpArgb);
                ColorDown = Color.FromArgb(settings.ColorDownArgb);


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
                    new FractalSettingsDto
                    {
                        PaintOn = PaintOn,
                        ColorUpArgb = ColorUp.ToArgb(),
                        ColorDownArgb = ColorDown.ToArgb()
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

        private static FractalSettingsDto ParseLegacySettings(string content)
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

            return new FractalSettingsDto
            {
                PaintOn = Convert.ToBoolean(lines[0]),
                ColorUpArgb = Convert.ToInt32(lines[1]),
                ColorDownArgb = Convert.ToInt32(lines[2])
            };
        }

        private sealed class FractalSettingsDto
        {
            public bool PaintOn { get; set; }

            public int ColorUpArgb { get; set; }

            public int ColorDownArgb { get; set; }
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
        }

        /// <summary>
        /// display settings window
        /// показать окно настроек
        /// </summary>
        public void ShowDialog()
        {
            FractalUi ui = new FractalUi(this);
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
        /// upper fractals
        /// верхние фракталы
        /// </summary>
        public List<decimal> ValuesUp { get; set; }

        /// <summary>
        /// bottom fractals
        /// нижние фракталы
        /// </summary>
        public List<decimal> ValuesDown { get; set; }

        /// <summary>
        /// to upload new candles
        /// прогрузить новыми свечками
        /// </summary>
        public void Process(List<Candle> candles)
        {
            if (candles.Count <= 5 || ValuesUp == null)
            {
                ValuesUp = new List<decimal>();
                ValuesDown = new List<decimal>();

                for (int i = 0; i < candles.Count; i++)
                {
                    ValuesUp.Add(0);
                    ValuesDown.Add(0);
                }
                ProcessAll(candles);
                return;
            }

            if (ValuesUp != null &&
                ValuesUp.Count + 1 == candles.Count)
            {
                ProcessOne(candles);
            }
            else if (ValuesUp != null && ValuesUp.Count != candles.Count)
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
        /// load only last candle
        /// прогрузить только последнюю свечку
        /// </summary>
        private void ProcessOne(List<Candle> candles)
        {
            if (ValuesUp == null)
            {
                ValuesUp = new List<decimal>();
                ValuesDown = new List<decimal>();
                ValuesUp.Add(GetValueUp(candles, candles.Count - 1));
                ValuesDown.Add(GetValueDown(candles, candles.Count - 1));
            }
            else
            {
                ValuesUp.Add(0);
                ValuesDown.Add(0);

                ValuesUp[ValuesUp.Count -3] = (GetValueUp(candles, candles.Count - 1));
                ValuesDown[ValuesDown.Count - 3] = (GetValueDown(candles, candles.Count - 1));

                if (ValuesDown[ValuesDown.Count - 3] != 0)
                {
                    ValuesDown[ValuesDown.Count - 4] = 0;
                    ValuesDown[ValuesDown.Count - 5] = 0;
                }

                if (ValuesUp[ValuesUp.Count - 3] != 0)
                {
                    ValuesUp[ValuesUp.Count - 4] = 0;
                    ValuesUp[ValuesUp.Count - 5] = 0;
                }
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
            ValuesDown= new List<decimal>();
            for (int i = 0; i < candles.Count; i++)
            {
                ValuesUp.Add(0);
                ValuesDown.Add(0);
            }

            for (int i = 2; i < candles.Count; i++)
            {

                    ValuesUp[i - 2] = GetValueUp(candles, i);
                    if (ValuesUp[i - 2] != 0)
                    {
                        ValuesUp[i - 2 - 1] = 0;
                        ValuesUp[i - 2 - 2] = 0;
                    }

                    ValuesDown[i - 2] = GetValueDown(candles, i);
                    if (ValuesDown[i - 2] != 0)
                    {
                        ValuesDown[i - 2 - 1] = 0;
                        ValuesDown[i - 2 - 2] = 0;
                    }
                
            }
        }

        /// <summary>
        ///  take upper value of indicator by index
        /// взять верхнее значение индикатора по индексу
        /// </summary>
        private decimal GetValueUp(List<Candle> candles, int index)
        {
            // fractal considered to be formed only after two candles have passed
            // фрактал у нас считается сформированным только после прошедших уже двух свечей
            // looking at trird candle from index
            // т.ч. смотрим трейтью свечу от индекса
            if (index - 5 <= 0)
            {
                return 0;
            }

            if (candles[index - 2].High >= candles[index - 1].High &&
                candles[index - 2].High >= candles[index].High &&
                candles[index - 2].High >= candles[index - 3].High &&
                candles[index - 2].High >= candles[index - 4].High)
            {
                return candles[index - 2].High;
            }



            return 0;
        }

        /// <summary>
        /// take lower value of indicator by index
        /// взять нижнее значение индикатора по индексу
        /// </summary>
        private decimal GetValueDown(List<Candle> candles, int index)
        {
            // fractal considered to be formed only after two candles have passed
            // фрактал у нас считается сформированным только после прошедших уже двух свечей
            // looking at trird candle from index
            // т.ч. смотрим трейтью свечу от индекса
            if (index - 5 <= 0)
            {
                return 0;
            }

            if (candles[index - 2].Low <= candles[index - 1].Low &&
                candles[index - 2].Low <= candles[index].Low &&
                candles[index - 2].Low <= candles[index - 3].Low &&
                candles[index - 2].Low <= candles[index - 4].Low)
            {
                return candles[index - 2].Low;
            }



            return 0;
        }

    }
}

