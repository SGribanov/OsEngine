/*
 *Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использования кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms.Integration;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Properties;
using System.Windows.Forms;

namespace OsEngine.Alerts
{
    /// <summary>
    /// Alert
    /// Алерт
    /// </summary>
    public class AlertToChart:IIAlert
    {
        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        /// <param name="name">alert name/имя алерта</param>
        /// <param name="gridView">control for calling main thread/контрол для вызова основного потока</param>
        public AlertToChart(string name, WindowsFormsHost gridView)
        {
            _lastAlarm = DateTime.MinValue;
            Name = name;
            _gridView = gridView;
            VolumeReaction = 1;
            Load();
            TypeAlert = AlertType.ChartAlert;
        }

        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        /// <param name="gridView">control for calling main thread/контрол для вызова основного потока</param>
        public AlertToChart(WindowsFormsHost gridView)
        {
            _gridView = gridView;
            _lastAlarm = DateTime.MinValue;
            VolumeReaction = 1;
            TypeAlert = AlertType.ChartAlert;
        }

        /// <summary>
        /// download from file
        /// загрузить из файла
        /// </summary>
        public void Load()
        {
            AlertToChartSettingsDto settings = SettingsManager.Load(
                GetSettingsPath(),
                defaultValue: null,
                legacyLoader: ParseLegacySettings);

            if (settings == null)
            {
                return;
            }

            ApplySettings(settings);
        }

        private void ApplySettings(AlertToChartSettingsDto settings)
        {
            try
            {
                Type = settings.Type;

                string[] lineSaves = settings.Lines ?? Array.Empty<string>();
                Lines = new ChartAlertLine[lineSaves.Length];
                for (int i = 0; i < lineSaves.Length; i++)
                {
                    Lines[i] = new ChartAlertLine();
                    Lines[i].SetFromSaveString(lineSaves[i]);
                }

                Label = settings.Label;
                Message = settings.Message;
                BorderWidth = settings.BorderWidth;
                IsOn = settings.IsOn;
                IsMusicOn = settings.IsMusicOn;
                IsMessageOn = settings.IsMessageOn;

                ColorLine = Color.FromArgb(settings.ColorLineArgb);
                ColorLabel = Color.FromArgb(settings.ColorLabelArgb);
                Music = settings.Music;

                SignalType = settings.SignalType;
                VolumeReaction = settings.VolumeReaction;
                Slippage = settings.Slippage;
                NumberClosePosition = settings.NumberClosePosition;
                OrderPriceType = settings.OrderPriceType;
                SlippageType = settings.SlippageType;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        /// <summary>
        /// save to file
        /// сохранить в файл
        /// </summary>
        public void Save()
        {
            try
            {
                if(Lines == null ||
                    Lines.Length == 0)
                {
                    return;
                }

                SettingsManager.Save(GetSettingsPath(), BuildSettings());
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private AlertToChartSettingsDto BuildSettings()
        {
            string[] lines = new string[Lines.Length];
            for (int i = 0; i < Lines.Length; i++)
            {
                lines[i] = Lines[i].GetStringToSave();
            }

            return new AlertToChartSettingsDto
            {
                Type = Type,
                Lines = lines,
                Label = Label,
                Message = Message,
                BorderWidth = BorderWidth,
                IsOn = IsOn,
                IsMusicOn = IsMusicOn,
                IsMessageOn = IsMessageOn,
                ColorLineArgb = ColorLine.ToArgb(),
                ColorLabelArgb = ColorLabel.ToArgb(),
                Music = Music,
                SignalType = SignalType,
                VolumeReaction = VolumeReaction,
                Slippage = Slippage,
                NumberClosePosition = NumberClosePosition,
                OrderPriceType = OrderPriceType,
                SlippageType = SlippageType
            };
        }

        private string GetSettingsPath()
        {
            return @"Engine\" + Name + @"Alert.txt";
        }

        private static AlertToChartSettingsDto ParseLegacySettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string normalized = content.Replace("\r", string.Empty);
            string[] rows = normalized.Split('\n');

            if (rows.Length > 0 && rows[rows.Length - 1] == string.Empty)
            {
                Array.Resize(ref rows, rows.Length - 1);
            }

            ChartAlertType type = ChartAlertType.Line;
            if (rows.Length > 0)
            {
                Enum.TryParse(rows[0], true, out type);
            }

            string[] lines = Array.Empty<string>();
            if (rows.Length > 1 && !string.IsNullOrEmpty(rows[1]))
            {
                string[] raw = rows[1].Split('%');
                int count = raw.Length;
                if (count > 0 && raw[count - 1] == string.Empty)
                {
                    count--;
                }
                lines = new string[count];
                Array.Copy(raw, lines, count);
            }

            AlertMusic music = AlertMusic.Duck;
            if (rows.Length > 10)
            {
                Enum.TryParse(rows[10], out music);
            }

            SignalType signalType = SignalType.None;
            if (rows.Length > 11)
            {
                Enum.TryParse(rows[11], true, out signalType);
            }

            OrderPriceType orderPriceType = OrderPriceType.Limit;
            if (rows.Length > 15)
            {
                Enum.TryParse(rows[15], true, out orderPriceType);
            }

            AlertSlippageType slippageType = AlertSlippageType.Absolute;
            if (rows.Length > 16)
            {
                Enum.TryParse(rows[16], true, out slippageType);
            }

            return new AlertToChartSettingsDto
            {
                Type = type,
                Lines = lines,
                Label = rows.Length > 2 ? rows[2] : string.Empty,
                Message = rows.Length > 3 ? rows[3] : string.Empty,
                BorderWidth = rows.Length > 4 ? Convert.ToInt32(rows[4]) : 0,
                IsOn = rows.Length > 5 && rows[5].Equals("true", StringComparison.OrdinalIgnoreCase),
                IsMusicOn = rows.Length > 6 && rows[6].Equals("true", StringComparison.OrdinalIgnoreCase),
                IsMessageOn = rows.Length > 7 && rows[7].Equals("true", StringComparison.OrdinalIgnoreCase),
                ColorLineArgb = rows.Length > 8 ? Convert.ToInt32(rows[8]) : Color.Black.ToArgb(),
                ColorLabelArgb = rows.Length > 9 ? Convert.ToInt32(rows[9]) : Color.Black.ToArgb(),
                Music = music,
                SignalType = signalType,
                VolumeReaction = rows.Length > 12 ? rows[12].ToDecimal() : 0,
                Slippage = rows.Length > 13 ? rows[13].ToDecimal() : 0,
                NumberClosePosition = rows.Length > 14 ? Convert.ToInt32(rows[14]) : 0,
                OrderPriceType = orderPriceType,
                SlippageType = slippageType
            };
        }

        private sealed class AlertToChartSettingsDto
        {
            public ChartAlertType Type { get; set; }

            public string[] Lines { get; set; }

            public string Label { get; set; }

            public string Message { get; set; }

            public int BorderWidth { get; set; }

            public bool IsOn { get; set; }

            public bool IsMusicOn { get; set; }

            public bool IsMessageOn { get; set; }

            public int ColorLineArgb { get; set; }

            public int ColorLabelArgb { get; set; }

            public AlertMusic Music { get; set; }

            public SignalType SignalType { get; set; }

            public decimal VolumeReaction { get; set; }

            public decimal Slippage { get; set; }

            public int NumberClosePosition { get; set; }

            public OrderPriceType OrderPriceType { get; set; }

            public AlertSlippageType SlippageType { get; set; }
        }

        /// <summary>
        /// delete save file
        /// удалить файл сохранений
        /// </summary>
        public void Delete()
        {
            if (File.Exists(GetSettingsPath()))
            {
                File.Delete(GetSettingsPath());
            }
        }

        /// <summary>
        /// Alert field
        /// поле для записи Алертов
        /// </summary>
        private readonly WindowsFormsHost _gridView;

        /// <summary>
        /// Alert name
        /// Имя Алерта
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// is Alert enabled
        /// включен ли Алерт
        /// </summary>
        public bool IsOn { get; set; }

        public AlertType TypeAlert { get; set; }

        /// <summary>
        /// message text emitted when alert triggered
        /// текст сообщения, выбрасываемый при срабатывании Алерта
        /// </summary>
        public string Message;

        /// <summary>
        /// line width
        /// ширина линии
        /// </summary>
        public int BorderWidth;

        /// <summary>
        /// Is music on?
        /// включена ли Музыка
        /// </summary>
        public bool IsMusicOn;

        /// <summary>
        /// whether message window discard enabled
        /// влкючено ли выбрасывание Окна сообщения
        /// </summary>
        public bool IsMessageOn;

        /// <summary>
        /// line color
        /// цвет линии
        /// </summary>
        public Color ColorLine;

        /// <summary>
        /// signature colour
        /// цвет подписи
        /// </summary>
        public Color ColorLabel;

        /// <summary>
        /// path to music file
        /// путь к файлу с музыкой
        /// </summary>
        public AlertMusic Music;

        /// <summary>
        /// signal type
        /// тип сигнала
        /// </summary>
        public SignalType SignalType;

        /// <summary>
        /// volume for execution
        /// объём для исполнения
        /// </summary>
        public decimal VolumeReaction;

        /// <summary>
        /// slippage
        /// проскальзывание
        /// </summary>
        public decimal Slippage;

        /// <summary>
        /// slippage type
        /// тип проскальзывания для реакции алерта
        /// </summary>
        public AlertSlippageType SlippageType;

        /// <summary>
        /// position number that will be closed
        /// номер позиции которая будет закрыта
        /// </summary>
        public int NumberClosePosition;

        /// <summary>
        /// order type
        /// тип ордера 
        /// </summary>
        public OrderPriceType OrderPriceType;

        /// <summary>
        /// signature
        /// подпись
        /// </summary>
        public string Label;

        /// <summary>
        /// alert type
        /// Тип Алерта
        /// </summary>
        public ChartAlertType Type;

        /// <summary>
        /// Line set
        /// Набор линий
        /// </summary>
        public ChartAlertLine[] Lines;

        /// <summary>
        /// recent alert call
        /// последнее время вызыва аллерта
        /// </summary>
        private DateTime _lastAlarm;

        /// <summary>
        /// check alert for triggering
        /// проверить алерт на срабатывание
        /// </summary>
        public AlertSignal CheckSignal(List<Candle> candles, Security sec)
        {
            if (IsOn == false || candles == null)
            {
                return null;
            }

            if (_lastAlarm != DateTime.MinValue &&
                _lastAlarm == candles[candles.Count - 1].TimeStart)
            {
                // alert already triggered at this moment
                // алерт уже сработал в эту минуту
                return null;
            }
            // 1 need to find out if time lines are in current range
            // 1 надо выяснить. входят ли линии по времени в текущий диапазон
            if (Lines[0].TimeFirstPoint > candles[candles.Count - 1].TimeStart &&
                Lines[0].TimeSecondPoint > candles[candles.Count - 1].TimeStart ||
                Lines[0].TimeFirstPoint < candles[0].TimeStart &&
                Lines[0].TimeSecondPoint < candles[0].TimeStart)
            {
                return null;
            }
            // 2 find out which points of array allert built from
            // 2 узнаём какие в массиве точки из которых собран аллерт

            int numberCandleFirst = -1;
            int numberCandleSecond = -1;

            for (int i = 0; i < candles.Count; i++)
            {
                if (candles[i].TimeStart == Lines[0].TimeFirstPoint)
                {
                    numberCandleFirst = i;
                }

                if (candles[i].TimeStart == Lines[0].TimeSecondPoint)
                {
                    numberCandleSecond = i;
                }
            }

            if (numberCandleSecond == -1 ||
                numberCandleFirst == -1)
            {
                return null;
            }
            // 3 running along allert lines and checking for triggering
            // 3 бежим по линиям аллерта и проверяем срабатывание

            bool isAlarm = false;

            for (int i = 0; i < Lines.Length; i++)
            {
                // 1 see how long our line goes by candle
                // а узнаём, сколько наша линия проходит за свечку

                decimal stepCorner = (Lines[i].ValueFirstPoint - Lines[i].ValueSecondPoint) / (numberCandleFirst - numberCandleSecond);
                // 2 now build an array of line values parallel to candlestick array
                // б теперь строим массив значений линии параллельный свечному массиву

                decimal[] lineDecimals = new decimal[candles.Count];
                decimal point = Lines[i].ValueFirstPoint;

                for (int i2 = numberCandleFirst; i2 < lineDecimals.Length; i2++)
                {
                    // running ahead of array.
                    // бежим вперёд по массиву
                    lineDecimals[i2] = point;
                    point += stepCorner;
                }
                for (int i2 = numberCandleFirst; i2 > -1; i2--)
                {
                    // running backwards through array.
                    // бежим назад по массиву
                    lineDecimals[i2] = point;
                    point -= stepCorner;
                }

                decimal redLineUp = candles[candles.Count - 1].High;
                if (candles[candles.Count - 2].Close > redLineUp)
                {
                    redLineUp = candles[candles.Count - 2].Close;
                }

                decimal redLineDown = candles[candles.Count - 1].Low;
                if (candles[candles.Count - 2].Close < redLineDown)
                {
                    redLineDown = candles[candles.Count - 2].Close;
                }

                decimal lastPoint = lineDecimals[lineDecimals.Length - 1];


                if ((redLineUp > lastPoint &&
                     redLineDown < lastPoint) ||
                    (candles[candles.Count - 1].Close < lastPoint && candles[candles.Count - 1].High > lastPoint) ||
                    (candles[candles.Count - 1].Close > lastPoint && candles[candles.Count - 1].High < lastPoint) ||
                    (candles[candles.Count - 1].Close > lastPoint && candles[candles.Count - 2].Close < lastPoint)||
                    (candles[candles.Count - 1].Close < lastPoint && candles[candles.Count - 2].Close > lastPoint))
                {
                    // if the closing price is in zone of triggering of allert
                    // если цена закрытия вошла в зону срабатывания аллерта
                    _lastAlarm = candles[candles.Count - 1].TimeStart;
                    isAlarm = true;
                    SignalAlarm();
                }
            }

            if (isAlarm)
            {
                decimal realSlippage = 0;

                if(SlippageType == AlertSlippageType.Absolute)
                {
                    realSlippage = Slippage;
                }
                else if(SlippageType == AlertSlippageType.PriceStep)
                {
                    realSlippage = Slippage * sec.PriceStep;
                }
                else if (SlippageType == AlertSlippageType.Persent)
                {
                    realSlippage = (candles[candles.Count - 1].Close/100) * Slippage;
                }

                IsOn = false;

                return new AlertSignal
                {
                    SignalType = SignalType,
                    Volume = VolumeReaction,
                    NumberClosingPosition = NumberClosePosition,
                    PriceType = OrderPriceType,
                    Slippage = realSlippage
                };
            }

            return null;
        }

        /// <summary>
        /// start alert
        /// запустить оповещение
        /// </summary>
        private void SignalAlarm()
        {
            if (IsMusicOn)
            {
                UnmanagedMemoryStream stream = Resources.Bird;

                if (Music == AlertMusic.Duck)
                {
                    stream = Resources.Duck;
                }
                if (Music == AlertMusic.Wolf)
                {
                    stream = Resources.wolf01;
                }
                AlertMessageManager.ThrowAlert(stream, Name, Message);
            }

            if (IsMessageOn)
            {
                SetMessage();
            }

            IsOn = false;
            Save();
        }

        /// <summary>
        /// throw message in form of window
        /// выбросить сообщение в виде окошка
        /// </summary>
        private void SetMessage()
        {
            if(_gridView == null)
            {
                return;
            }

            if (!_gridView.Dispatcher.CheckAccess())
            {
                _gridView.Dispatcher.InvokeAsync((SetMessage));
                return;
            }

            if (!string.IsNullOrWhiteSpace(Message))
            {
                AlertMessageSimpleUi ui = new AlertMessageSimpleUi(Message);
                ui.Show();
            }
            else
            {
                AlertMessageSimpleUi ui = new AlertMessageSimpleUi(OsLocalization.Alerts.Message2 + Label);
                ui.Show();
            }
        }

    }

    /// <summary>
    /// alert type
    /// Тип Алерта
    /// </summary>
    public enum ChartAlertType
    {
        /// <summary>
        /// Line
        /// Линия
        /// </summary>
        Line,
        /// <summary>
        /// Fibonacci Channel
        /// Канал Фибоначи
        /// </summary>
        FibonacciChannel,
        /// <summary>
        ///  Fibonacci Speed Line
        /// Скоростная линия Фибоначчи
        /// </summary>
        FibonacciSpeedLine,
        /// <summary>
        /// Horizontal line
        /// Горизонтальная линия
        /// </summary>
        HorizontalLine,
    }

    /// <summary>
    /// alert line
    /// Линия Алерта
    /// </summary>
    public class ChartAlertLine
    {
        /// <summary>
        /// time of first point
        /// время первой точки
        /// </summary>
        public DateTime TimeFirstPoint;

        /// <summary>
        /// time of first point
        /// значение первой точки
        /// </summary>
        public decimal ValueFirstPoint;

        /// <summary>
        /// time of second point
        /// время второй точки
        /// </summary>
        public DateTime TimeSecondPoint;

        /// <summary>
        /// second point value
        /// значение второй точки
        /// </summary>
        public decimal ValueSecondPoint;

        /// <summary>
        /// line value on last candlestick of array
        /// значение линии на последней свече массива
        /// </summary>
        public decimal LastPoint;

        private readonly CultureInfo CultureInfo = new CultureInfo("ru-RU");

        /// <summary>
        /// take string to save
        /// взять строку для сохранение
        /// </summary>
        public string GetStringToSave()
        {
            string result = "";

            result += TimeFirstPoint.ToString(CultureInfo) + "@";
            result += ValueFirstPoint.ToString(CultureInfo) + "@";

            result += TimeSecondPoint.ToString(CultureInfo) + "@";
            result += ValueSecondPoint.ToString(CultureInfo) + "@";
            result += LastPoint.ToString(CultureInfo) + "@";

            return result;
        }

        /// <summary>
        /// set line from save line
        /// установить линию со cтроки сохранения
        /// </summary>
        public void SetFromSaveString(string saveString)
        {
            string[] saveStrings = saveString.Split('@');

            TimeFirstPoint = Convert.ToDateTime(saveStrings[0], CultureInfo);
            ValueFirstPoint = saveStrings[1].ToDecimal();

            TimeSecondPoint = Convert.ToDateTime(saveStrings[2], CultureInfo);
            ValueSecondPoint = saveStrings[3].ToDecimal();
            LastPoint = saveStrings[4].ToDecimal();
        }

    }
}
