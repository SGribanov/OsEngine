#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 *Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using System.Drawing;
using System.IO;
using System.Threading;
using OsEngine.Entity;
using OsEngine.Logging;

namespace OsEngine.Charts.ColorKeeper
{
    /// <summary>
    /// Хранилище цветов для чарта
    /// </summary>
    public class ChartMasterColorKeeper
    {
        
        /// <summary>
        /// имя
        /// </summary>
        private readonly string _name;

        public ChartColorScheme ColorScheme
        {
            get { return _colorScheme; }
        }

        private ChartColorScheme _colorScheme;

        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="name">имя панели, которой принадлежит</param>
        public ChartMasterColorKeeper(string name) 
        {
            _name = name;
            _pointType = PointType.Cross;
            Load();
        }

        /// <summary>
        ///  загрузить из файла
        /// </summary>
        private void Load()
        {
            try
            {
                EnsureDirectoryExists();

                ChartColorKeeperSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings != null)
                {
                    ColorUpBodyCandle = Color.FromArgb(settings.ColorUpBodyCandleArgb);
                    ColorUpBorderCandle = Color.FromArgb(settings.ColorUpBorderCandleArgb);
                    ColorDownBodyCandle = Color.FromArgb(settings.ColorDownBodyCandleArgb);
                    ColorDownBorderCandle = Color.FromArgb(settings.ColorDownBorderCandleArgb);
                    ColorBackSecond = Color.FromArgb(settings.ColorBackSecondArgb);
                    ColorBackChart = Color.FromArgb(settings.ColorBackChartArgb);
                    ColorBackCursor = Color.FromArgb(settings.ColorBackCursorArgb);
                    ColorText = Color.FromArgb(settings.ColorTextArgb);
                    _pointType = settings.PointType;
                    _colorScheme = settings.ColorScheme;
                }
                else
                {
                    ColorUpBodyCandle = Color.FromArgb(57, 157, 54);
                    ColorUpBorderCandle = Color.FromArgb(57, 157, 54);

                    ColorDownBodyCandle = Color.FromArgb(17, 18, 23);
                    ColorDownBorderCandle = Color.FromArgb(255, 83, 0);

                    ColorBackSecond = Color.FromArgb(17, 18, 23);
                    ColorBackChart = Color.FromArgb(17, 18, 23);
                    ColorBackCursor = Color.FromArgb(255, 83, 0);

                    ColorText = Color.FromArgb(255, 147, 147, 147);

                    _colorScheme = ChartColorScheme.Black;
                }
            }
            catch (Exception error)
            {
                SendNewMessage(error.ToString(),LogMessageType.Error);
            }
        }

        /// <summary>
        /// сохранить в файл
        /// </summary>
        public void Save() 
        {
            try
            {
                EnsureDirectoryExists();

                SettingsManager.Save(
                    GetSettingsPath(),
                    new ChartColorKeeperSettingsDto
                    {
                        ColorUpBodyCandleArgb = ColorUpBodyCandle.ToArgb(),
                        ColorUpBorderCandleArgb = ColorUpBorderCandle.ToArgb(),
                        ColorDownBodyCandleArgb = ColorDownBodyCandle.ToArgb(),
                        ColorDownBorderCandleArgb = ColorDownBorderCandle.ToArgb(),
                        ColorBackSecondArgb = ColorBackSecond.ToArgb(),
                        ColorBackChartArgb = ColorBackChart.ToArgb(),
                        ColorBackCursorArgb = ColorBackCursor.ToArgb(),
                        ColorTextArgb = ColorText.ToArgb(),
                        PointType = _pointType,
                        ColorScheme = _colorScheme
                    });

                if (NeedToRePaintFormEvent != null)
                {
                    NeedToRePaintFormEvent();
                }
            }
            catch (Exception error)
            {
                SendNewMessage(error.ToString(),LogMessageType.Error);
            }
        }

        /// <summary>
        /// удалить файл с настройками
        /// </summary>
        public void Delete()
        {
            try
            {
                SettingsManager.Delete(GetSettingsPath());
            }
            catch (Exception error)
            {
                SendNewMessage(error.ToString(),LogMessageType.Error);
            }
        }

        /// <summary>
        /// загрузить чёрную схему
        /// </summary>
        public void SetBlackScheme()
        {
            ColorUpBodyCandle = Color.FromArgb(57, 157, 54);
            ColorUpBorderCandle = Color.FromArgb(57, 157, 54);

            ColorDownBodyCandle = Color.FromArgb(17, 18, 23);
            ColorDownBorderCandle = Color.FromArgb(255, 83, 0);

            ColorBackSecond = Color.FromArgb(17, 18, 23);
            ColorBackChart = Color.FromArgb(17, 18, 23);
            ColorBackCursor = Color.FromArgb(255, 83, 0);
            ColorText = Color.FromArgb(255, 147, 147, 147);

            _colorScheme = ChartColorScheme.Black;

            Save();

            if (NeedToRePaintFormEvent != null)
            {
                NeedToRePaintFormEvent();
            }
        }

        /// <summary>
        /// загрузить белую схему
        /// </summary>
        public void SetWhiteScheme()
        {
            ColorUpBodyCandle = Color.Azure;
            ColorUpBorderCandle = Color.Azure;

            ColorDownBodyCandle = Color.Black;
            ColorDownBorderCandle = Color.Black;

            ColorBackSecond = Color.Black;

            ColorBackChart = Color.FromArgb(255, 147, 147, 147);
            //ColorBackCursor = Color.DarkOrange;
            ColorBackCursor = Color.FromArgb(255, 255, 107, 0);

            ColorText = Color.Black;

            _colorScheme = ChartColorScheme.White;

            Save();

            if (NeedToRePaintFormEvent != null)
            {
                NeedToRePaintFormEvent();
            }
        }

 // цвета

        public Color ColorUpBodyCandle;

        public Color ColorDownBodyCandle;

        public Color ColorUpBorderCandle;

        public Color ColorDownBorderCandle;

        public Color ColorBackSecond;

        public Color ColorBackChart;

        public Color ColorBackCursor;

        public Color ColorText;

 // спецификация прорисовки позиций на графике

        /// <summary>
        /// размер для точки обозначающей позицию
        /// </summary>
        public ChartPositionTradeSize PointsSize;

        /// <summary>
        /// тип точки
        /// </summary>
        public PointType PointType
        {
            get { return _pointType; }
            set
            {
                _pointType = value;
                Save();
            }
        }

        private PointType _pointType;

        /// <summary>
        /// событие изменения цвета в хранилище
        /// </summary>
        public event Action NeedToRePaintFormEvent;

        /// <summary>
        /// выслать наверх сообщение об ошибке
        /// </summary>
        private void SendNewMessage(string message, LogMessageType type)
        {
            if (LogMessageEvent != null)
            {
                LogMessageEvent(message, LogMessageType.Error);
            }
            else if (type == LogMessageType.Error)
            { // если никто на нас не подписан и происходит ошибка
                System.Windows.MessageBox.Show(message);
            }
        }

        /// <summary>
        /// исходящее сообщение для лога
        /// </summary>
        public event Action<string,LogMessageType> LogMessageEvent;

        private string GetSettingsPath()
        {
            return Path.Combine(GetSettingsDirectoryPath(), _name + "Color.toml");
        }

        private static string GetSettingsDirectoryPath()
        {
            return @"Engine\Color";
        }

        private static ChartColorKeeperSettingsDto ParseLegacySettings(string content)
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

            PointType pointType = PointType.Cross;
            if (lines.Length > 8)
            {
                Enum.TryParse(lines[8], true, out pointType);
            }

            ChartColorScheme colorScheme = ChartColorScheme.Black;
            if (lines.Length > 9)
            {
                Enum.TryParse(lines[9], true, out colorScheme);
            }

            return new ChartColorKeeperSettingsDto
            {
                ColorUpBodyCandleArgb = lines.Length > 0 ? Convert.ToInt32(lines[0], CultureInfo.InvariantCulture) : Color.FromArgb(57, 157, 54).ToArgb(),
                ColorUpBorderCandleArgb = lines.Length > 1 ? Convert.ToInt32(lines[1], CultureInfo.InvariantCulture) : Color.FromArgb(57, 157, 54).ToArgb(),
                ColorDownBodyCandleArgb = lines.Length > 2 ? Convert.ToInt32(lines[2], CultureInfo.InvariantCulture) : Color.FromArgb(17, 18, 23).ToArgb(),
                ColorDownBorderCandleArgb = lines.Length > 3 ? Convert.ToInt32(lines[3], CultureInfo.InvariantCulture) : Color.FromArgb(255, 83, 0).ToArgb(),
                ColorBackSecondArgb = lines.Length > 4 ? Convert.ToInt32(lines[4], CultureInfo.InvariantCulture) : Color.FromArgb(17, 18, 23).ToArgb(),
                ColorBackChartArgb = lines.Length > 5 ? Convert.ToInt32(lines[5], CultureInfo.InvariantCulture) : Color.FromArgb(17, 18, 23).ToArgb(),
                ColorBackCursorArgb = lines.Length > 6 ? Convert.ToInt32(lines[6], CultureInfo.InvariantCulture) : Color.FromArgb(255, 83, 0).ToArgb(),
                ColorTextArgb = lines.Length > 7 ? Convert.ToInt32(lines[7], CultureInfo.InvariantCulture) : Color.FromArgb(255, 147, 147, 147).ToArgb(),
                PointType = pointType,
                ColorScheme = colorScheme
            };
        }

        private static void EnsureDirectoryExists()
        {
            string settingsDirectoryPath = GetSettingsDirectoryPath();
            if (!Directory.Exists(settingsDirectoryPath))
            {
                Directory.CreateDirectory(settingsDirectoryPath);
            }
        }

        private sealed class ChartColorKeeperSettingsDto
        {
            public int ColorUpBodyCandleArgb { get; set; }
            public int ColorDownBodyCandleArgb { get; set; }
            public int ColorUpBorderCandleArgb { get; set; }
            public int ColorDownBorderCandleArgb { get; set; }
            public int ColorBackSecondArgb { get; set; }
            public int ColorBackChartArgb { get; set; }
            public int ColorBackCursorArgb { get; set; }
            public int ColorTextArgb { get; set; }
            public PointType PointType { get; set; }
            public ChartColorScheme ColorScheme { get; set; }
        }

    }

    /// <summary>
    /// тип прорисовки позиций на графике
    /// </summary>
    public enum PointType
    {
        /// <summary>
        /// перекрестие
        /// </summary>
        Cross,

        /// <summary>
        /// в дебаггере перекрестие, без него картинка треугольника
        /// </summary>
        Auto,

        /// <summary>
        /// круг
        /// </summary>
        Circle,

        /// <summary>
        /// треугольник
        /// </summary>
        TriAngle,

        /// <summary>
        /// ромб
        /// </summary>
        Romb

    }

    /// <summary>
    /// схема раскраски чарта
    /// </summary>
    public enum ChartColorScheme
    {
        /// <summary>
        /// чёрная
        /// </summary>
        Black,
        /// <summary>
        /// белая
        /// </summary>
        White,
        /// <summary>
        /// тёмная
        /// </summary>
        Dark,
    }

    /// <summary>
    /// Размер точки данных на чарте для трейда. 1 - самая маленькая
    /// </summary>
    public enum ChartPositionTradeSize
    {
        Size1,

        Size2,

        Size3,

        Size4
    }
}

