/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using System.Windows;
using OsEngine.Language;

#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8604, CS8618, CS8622, CS8625

namespace OsEngine.Entity
{
    public partial class SecurityUi
    {
        private Security _security;

        public bool IsChanged;

        public SecurityUi(Security security)
        {
            _security = security;
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);

            CultureInfo culture = new CultureInfo("ru-RU");

            TextBoxGoPrice.Text = (security.MarginBuy).ToString(culture);
            TextBoxMarginSell.Text = (security.MarginSell).ToString(culture);
            TextBoxLot.Text = security.Lot.ToString(culture);
            TextBoxStep.Text = security.PriceStep.ToString(culture);
            TextBoxStepCost.Text = security.PriceStepCost.ToString(culture);
            TextBoxVolumeDecimals.Text = security.DecimalsVolume.ToString(culture);
            TextBoxExpiration.Text = security.Expiration.ToString(culture);

            Title = OsLocalization.Entity.TitleSecurityUi;
            SecuritiesColumn3.Content = OsLocalization.Entity.SecuritiesColumn3;
            SecuritiesColumn4.Content = OsLocalization.Entity.SecuritiesColumn4;
            SecuritiesColumn5.Content = OsLocalization.Entity.SecuritiesColumn5;
            SecuritiesColumn6.Content = OsLocalization.Entity.SecuritiesColumn6;
            LabelSecuritiesMarginSell.Content = OsLocalization.Entity.SecuritiesColumn21;
            SecuritiesExpiration.Content = OsLocalization.Entity.SecuritiesColumn18;

            SecuritiesVolumeDecimals.Content = OsLocalization.Entity.SecuritiesColumn7;
            ButtonAccept.Content = OsLocalization.Entity.ButtonAccept;

            LabelName.Content = security.Name;

            this.Activate();
            this.Focus();

            Closed += SecurityUi_Closed;
        }

        private void SecurityUi_Closed(object sender, EventArgs e)
        {
            _security = null;
        }

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            if (TryReadSecurityValues(
                    out decimal marginBuy,
                    out decimal marginSell,
                    out decimal lot,
                    out decimal step,
                    out decimal stepCost,
                    out int volDecimals,
                    out DateTime expiration) == false)
            {
                CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Message.HintMessageError5);
                ui.ShowDialog();
                return;
            }

            string message = OsLocalization.Message.HintMessageError5 + "\n";

            int index = 0;

            if (step < 0)
            {
                message += index + 1 + ") " + OsLocalization.Message.HintMessageError0 + "\n";
                index++;
            }

            if (stepCost < 0)
            {
                message += index + 1 + ") " + OsLocalization.Message.HintMessageError1 + "\n";
                index++;
            }

            if (marginBuy < 0
                || marginSell < 0)
            {
                message += index + 1 + ") " + OsLocalization.Message.HintMessageError2 + "\n";
                index++;
            }

            if (lot < 0)
            {
                message += index + 1 + ") " + OsLocalization.Message.HintMessageError3 + "\n";
                index++;
            }

            if (volDecimals < 0)
            {
                message += index + 1 + ") " + OsLocalization.Message.HintMessageError4 + "\n";
                index++;
            }

            if (message != OsLocalization.Message.HintMessageError5 + "\n")
            {
                CustomMessageBoxUi ui = new CustomMessageBoxUi(message);
                ui.ShowDialog();
                return;
            }

            _security.MarginBuy = marginBuy;
            _security.MarginSell = marginSell;
            _security.Lot = lot;
            _security.PriceStep = step;
            _security.PriceStepCost = stepCost;
            _security.DecimalsVolume = volDecimals;
            _security.Expiration = expiration;
            IsChanged = true;
            Close();
        }

        private void ButtonInfoPriceStep_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Message.HintMessageLabel0);
            ui.ShowDialog();
        }

        private void ButtonInfoPriceStepPrice_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Message.HintMessageLabel1);
            ui.ShowDialog();
        }

        private void ButtonInfoLotPrice_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Message.HintMessageLabel2);
            ui.ShowDialog();
        }

        private void ButtonInfoMarginSell_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Message.HintMessageLabel2);
            ui.ShowDialog();
        }

        private void ButtonInfoLot_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Message.HintMessageLabel3);
            ui.ShowDialog();
        }

        private void ButtonInfoVolume_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Message.HintMessageLabel4);
            ui.ShowDialog();
        }

        private void ButtonInfoExpiration_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Message.HintMessageLabel8);
            ui.ShowDialog();
        }

        private bool TryReadSecurityValues(
            out decimal marginBuy,
            out decimal marginSell,
            out decimal lot,
            out decimal step,
            out decimal stepCost,
            out int volDecimals,
            out DateTime expiration)
        {
            marginBuy = 0;
            marginSell = 0;
            lot = 0;
            step = 0;
            stepCost = 0;
            volDecimals = 0;
            expiration = DateTime.MinValue;

            return TryParseDecimalFlexible(TextBoxGoPrice.Text, out marginBuy)
                && TryParseDecimalFlexible(TextBoxMarginSell.Text, out marginSell)
                && TryParseDecimalFlexible(TextBoxLot.Text, out lot)
                && TryParseDecimalFlexible(TextBoxStep.Text, out step)
                && TryParseDecimalFlexible(TextBoxStepCost.Text, out stepCost)
                && int.TryParse(TextBoxVolumeDecimals.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out volDecimals)
                && TryParseDateFlexible(TextBoxExpiration.Text, out expiration);
        }

        private static bool TryParseDecimalFlexible(string value, out decimal parsed)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
            {
                return true;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
            {
                return true;
            }

            return decimal.TryParse(value, NumberStyles.Any, new CultureInfo("ru-RU"), out parsed);
        }

        private static bool TryParseDateFlexible(string value, out DateTime parsedDate)
        {
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed))
            {
                parsedDate = parsed;
                return true;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                parsedDate = parsed;
                return true;
            }

            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
            {
                parsedDate = parsed;
                return true;
            }

            if (DateTime.TryParse(value, new CultureInfo("ru-RU"), DateTimeStyles.None, out parsed))
            {
                parsedDate = parsed;
                return true;
            }

            parsedDate = DateTime.MinValue;
            return false;
        }
    }
}
