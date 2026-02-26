#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using OsEngine.Entity;
using OsEngine.Language;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Forms.TextBox;

namespace OsEngine.Charts.CandleChart.Indicators
{
    /// <summary>
    /// Interaction logic  for RVIUi.xaml
    /// Логика взаимодействия для RVIUi.xaml
    /// </summary>
    public partial class RviUi
    {
        /// <summary>
        /// indicator
        /// индикатор
        /// </summary>
        private Rvi _rvi;

        /// <summary>
        /// whether indicator settings have been changed
        /// изменялись ли настройки индикатора
        /// </summary>
        public bool IsChange;

        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        /// <param name="alb">configuration indicator/индикатор для настроек</param>
        public RviUi(Rvi rvi)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);
            _rvi = rvi;

            TextBoxLength.Text = _rvi.Period.ToString();

            HostColorBase.Child = new TextBox();
            HostColorBaseCopy.Child = new TextBox();
            HostColorBase.Child.BackColor = _rvi.ColorDown;
            HostColorBaseCopy.Child.BackColor = _rvi.ColorUp;
            CheckBoxPaintOnOff.IsChecked = _rvi.PaintOn;

            ButtonColorUp.Content = OsLocalization.Charts.LabelButtonIndicatorColorUp;
            ButtonColorDown.Content = OsLocalization.Charts.LabelButtonIndicatorColorDown;
            CheckBoxPaintOnOff.Content = OsLocalization.Charts.LabelPaintIntdicatorIsVisible;
            ButtonAccept.Content = OsLocalization.Charts.LabelButtonIndicatorAccept;
            LabelIndicatorPeriod.Content = OsLocalization.Charts.LabelIndicatorPeriod;

            this.Activate();
            this.Focus();
        }

        /// <summary>
        /// accept button
        /// кнопка принять
        /// </summary>
        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Convert.ToInt32(TextBoxLength.Text, CultureInfo.InvariantCulture) <= 0)
                {
                    throw new Exception("error");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Процесс сохранения прерван. В одном из полей недопустимые значения");
                return;
            }

            _rvi.ColorDown = HostColorBase.Child.BackColor;
            _rvi.ColorUp = HostColorBaseCopy.Child.BackColor;
            _rvi.Period = Convert.ToInt32(TextBoxLength.Text, CultureInfo.InvariantCulture);
            _rvi.PaintOn = CheckBoxPaintOnOff.IsChecked.Value;



            _rvi.Save();

            IsChange = true;
            Close();
        }

        /// <summary>
        /// color setting button
        /// кнопка настроить цвет
        /// </summary>
        private void ButtonColor_Click(object sender, RoutedEventArgs e)
        {
            ColorCustomDialog dialog = new ColorCustomDialog();
            dialog.Color = HostColorBase.Child.BackColor;
            dialog.ShowDialog();
            HostColorBase.Child.BackColor = dialog.Color;
        }
    }
}

