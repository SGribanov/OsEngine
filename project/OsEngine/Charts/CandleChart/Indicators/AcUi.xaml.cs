#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использования кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
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
    /// Interaction logic for AcUi.xaml
    /// Логика взаимодействия для AcUi.xaml
    /// </summary>
    public partial class AcUi
    { 
        
        /// <summary>
        /// indocator
        /// индикатор
        /// </summary>
        private Ac _ac;

        /// <summary>
        /// whether indicator settings have been changed
        /// изменялись ли настройки индикатора
        /// </summary>
        public bool IsChange;

        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        public AcUi(Ac ac)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);
            _ac = ac;

            TextBoxLength.Text = _ac.LengthLong.ToString();
            TextBoxLengthAverage.Text = _ac.LengthShort.ToString();
            CheckBoxPaintOnOff.IsChecked = _ac.PaintOn;

            HostColorUp.Child = new TextBox();
            HostColorUp.Child.BackColor = _ac.ColorUp;

            HostColorDown.Child = new TextBox();
            HostColorDown.Child.BackColor = _ac.ColorDown;

            ButtonColorUp.Content = OsLocalization.Charts.LabelButtonIndicatorColorUp;
            ButtonColorDown.Content = OsLocalization.Charts.LabelButtonIndicatorColorDown;
            CheckBoxPaintOnOff.Content = OsLocalization.Charts.LabelPaintIntdicatorIsVisible;
            ButtonAccept.Content = OsLocalization.Charts.LabelButtonIndicatorAccept;
            LabelIndicatorLongPeriod.Content = OsLocalization.Charts.LabelIndicatorLongPeriod;
            LabelIndicatorShortPeriod.Content = OsLocalization.Charts.LabelIndicatorShortPeriod;

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
                if (Convert.ToInt32(TextBoxLength.Text, CultureInfo.InvariantCulture) <= 0
                    || Convert.ToInt32(TextBoxLengthAverage.Text, CultureInfo.InvariantCulture) <= 0)
                {
                    throw new Exception("error");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Процесс сохранения прерван. В одном из полей недопустимые значения");
                return;
            }

            _ac.ColorUp = HostColorUp.Child.BackColor;
            _ac.ColorDown = HostColorDown.Child.BackColor;

            _ac.LengthLong = Convert.ToInt32(TextBoxLength.Text, CultureInfo.InvariantCulture);
            _ac.PaintOn = CheckBoxPaintOnOff.IsChecked.Value;
            _ac.LengthShort = Convert.ToInt32(TextBoxLengthAverage.Text, CultureInfo.InvariantCulture);
            _ac.Save();

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
            dialog.Color = HostColorUp.Child.BackColor;
            dialog.ShowDialog();
            HostColorUp.Child.BackColor = dialog.Color;
        }

        private void ButtonColorDown_Click(object sender, RoutedEventArgs e)
        {
            ColorCustomDialog dialog = new ColorCustomDialog();
            dialog.Color = HostColorDown.Child.BackColor;
            dialog.ShowDialog();
            HostColorDown.Child.BackColor = dialog.Color;
        }
    }
}

