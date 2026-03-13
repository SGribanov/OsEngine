#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использования кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OsEngine.Entity;
using OsEngine.Properties;
using System.Windows.Forms;

namespace OsEngine.Alerts
{
    public class AlertToPrice: IIAlert
    {
        public AlertToPrice(string name)
        {
            TypeAlert = AlertType.PriceAlert;
            SignalType = SignalType.None;
            MusicType = AlertMusic.Duck;

            Name = name;
            Load();
        }

        public void Save()
        {
            try
            {
                SettingsManager.Save(GetSettingsPath(), BuildSettings());
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        public void Load()
        {
            AlertToPriceSettingsDto settings = SettingsManager.Load(
                GetSettingsPath(),
                defaultValue: null,
                legacyLoader: ParseLegacySettings);

            if (settings == null)
            {
                return;
            }

            ApplySettings(settings);
        }

        private void ApplySettings(AlertToPriceSettingsDto settings)
        {
            try
            {
                Message = settings.Message;
                IsOn = settings.IsOn;
                MessageIsOn = settings.MessageIsOn;
                MusicType = settings.MusicType;
                SignalType = settings.SignalType;
                VolumeReaction = settings.VolumeReaction;
                Slippage = settings.Slippage;
                NumberClosePosition = settings.NumberClosePosition;
                OrderPriceType = settings.OrderPriceType;
                TypeActivation = settings.TypeActivation;
                PriceActivation = settings.PriceActivation;
                SlippageType = settings.SlippageType;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private AlertToPriceSettingsDto BuildSettings()
        {
            return new AlertToPriceSettingsDto
            {
                Message = Message,
                IsOn = IsOn,
                MessageIsOn = MessageIsOn,
                MusicType = MusicType,
                SignalType = SignalType,
                VolumeReaction = VolumeReaction,
                Slippage = Slippage,
                NumberClosePosition = NumberClosePosition,
                OrderPriceType = OrderPriceType,
                TypeActivation = TypeActivation,
                PriceActivation = PriceActivation,
                SlippageType = SlippageType
            };
        }

        private string GetSettingsPath()
        {
            return @"Engine\" + Name + @"Alert.toml";
        }

        private static AlertToPriceSettingsDto ParseLegacySettings(string content)
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

            AlertMusic musicType = AlertMusic.Duck;
            if (lines.Length > 3)
            {
                Enum.TryParse(lines[3], out musicType);
            }

            SignalType signalType = SignalType.None;
            if (lines.Length > 4)
            {
                Enum.TryParse(lines[4], true, out signalType);
            }

            OrderPriceType orderPriceType = OrderPriceType.Limit;
            if (lines.Length > 8)
            {
                Enum.TryParse(lines[8], true, out orderPriceType);
            }

            PriceAlertTypeActivation typeActivation = PriceAlertTypeActivation.PriceHigherOrEqual;
            if (lines.Length > 9)
            {
                Enum.TryParse(lines[9], true, out typeActivation);
            }

            AlertSlippageType slippageType = AlertSlippageType.Absolute;
            if (lines.Length > 11)
            {
                Enum.TryParse(lines[11], true, out slippageType);
            }

            return new AlertToPriceSettingsDto
            {
                Message = lines.Length > 0 ? lines[0] : string.Empty,
                IsOn = lines.Length > 1 && lines[1].Equals("true", StringComparison.OrdinalIgnoreCase),
                MessageIsOn = lines.Length > 2 && lines[2].Equals("true", StringComparison.OrdinalIgnoreCase),
                MusicType = musicType,
                SignalType = signalType,
                VolumeReaction = lines.Length > 5 ? lines[5].ToDecimal() : 0,
                Slippage = lines.Length > 6 ? lines[6].ToDecimal() : 0,
                NumberClosePosition = lines.Length > 7 ? Convert.ToInt32(lines[7], CultureInfo.InvariantCulture) : 0,
                OrderPriceType = orderPriceType,
                TypeActivation = typeActivation,
                PriceActivation = lines.Length > 10 ? lines[10].ToDecimal() : 0,
                SlippageType = slippageType
            };
        }

        private sealed class AlertToPriceSettingsDto
        {
            public string Message { get; set; }

            public bool IsOn { get; set; }

            public bool MessageIsOn { get; set; }

            public AlertMusic MusicType { get; set; }

            public SignalType SignalType { get; set; }

            public decimal VolumeReaction { get; set; }

            public decimal Slippage { get; set; }

            public int NumberClosePosition { get; set; }

            public OrderPriceType OrderPriceType { get; set; }

            public PriceAlertTypeActivation TypeActivation { get; set; }

            public decimal PriceActivation { get; set; }

            public AlertSlippageType SlippageType { get; set; }
        }

        public void Delete()
        {
            SettingsManager.Delete(GetSettingsPath());
        }

        public void ShowDialog()
        {
            AlertToPriceCreateUi ui = new AlertToPriceCreateUi(this);
            ui.ShowDialog();
        }

        public bool IsOn { get; set; }

        public string Name { get; set; }

        public AlertType TypeAlert { get; set; }

        public AlertSignal CheckSignal(List<Candle> candles, Security sec)
        {
            if (IsOn == false || candles == null)
            {
                return null;
            }

            // 3 run along allert lines and check triggering
            // 3 бежим по линиям аллерта и проверяем срабатывание

            if (TypeActivation == PriceAlertTypeActivation.PriceLowerOrEqual &&
                candles[candles.Count - 1].Close <= PriceActivation ||
                TypeActivation == PriceAlertTypeActivation.PriceHigherOrEqual &&
                candles[candles.Count - 1].Close >= PriceActivation)
            {
                IsOn = false;
                if (MessageIsOn)
                {
                    UnmanagedMemoryStream stream = Resources.Bird;

                    if (MusicType == AlertMusic.Duck)
                    {
                        stream = Resources.Duck;
                    }
                    if (MusicType == AlertMusic.Wolf)
                    {
                        stream = Resources.wolf01;
                    }

                    AlertMessageManager.ThrowAlert(stream, Name, Message);
                }
                if (SignalType != SignalType.None)
                {
                    decimal realSlippage = 0;

                    if (SlippageType == AlertSlippageType.Absolute)
                    {
                        realSlippage = Slippage;
                    }
                    else if (SlippageType == AlertSlippageType.PriceStep)
                    {
                        realSlippage = Slippage * sec.PriceStep;
                    }
                    else if (SlippageType == AlertSlippageType.Persent)
                    {
                        realSlippage = (candles[candles.Count - 1].Close / 100) * Slippage;
                    }

                    return new AlertSignal
                    {
                        SignalType = SignalType,
                        Volume = VolumeReaction,
                        NumberClosingPosition = NumberClosePosition,
                        PriceType = OrderPriceType,
                        Slippage = realSlippage
                    };
                }
                if (SignalType == SignalType.None)
                {
                    return new AlertSignal();
                }
            }

            return null;
        }
        // custom settings
        // индивидуальные настройки

        public PriceAlertTypeActivation TypeActivation;

        public decimal PriceActivation;

        public SignalType SignalType;

        /// <summary>
        /// execution volume
        /// объём для исполнения
        /// </summary>
        public decimal VolumeReaction;

        /// <summary>
        /// slippage
        /// проскальзывание
        /// </summary>
        public decimal Slippage;

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
        /// whether ejecting is enabled Message windows
        /// влкючено ли выбрасывание Окна сообщения
        /// </summary>
        public bool MessageIsOn;

        /// <summary>
        /// text of message thrown out when alert triggered
        /// текст сообщения, выбрасываемый при срабатывании Алерта
        /// </summary>
        public string Message;

        /// <summary>
        /// path to music file
        /// путь к файлу с музыкой
        /// </summary>
        public AlertMusic MusicType;

    }

    /// <summary>
    /// Price condition for activation of Alert
    /// условие активации Алерта по цене
    /// </summary>
    public enum PriceAlertTypeActivation
    {
        /// <summary>
        /// price higher or equal to value
        /// цена выше или равна значению
        /// </summary>
        PriceHigherOrEqual,

        /// <summary>
        /// Price lower or equal to
        /// цена ниже или равна значению
        /// </summary>
        PriceLowerOrEqual
    }
}

