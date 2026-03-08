/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using System.Windows;
using OsEngine.Language;

#nullable enable

namespace OsEngine.Entity
{
    public partial class DateTimeSelectionDialog : Window
    {
        public DateTimeSelectionDialog(DateTime initTime)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);
            Time = initTime;

            DateTimePicker.SelectedDate = Time;
            TextBoxHour.Text = initTime.Hour.ToString();
            TextBoxMinute.Text = initTime.Minute.ToString();
            TextBoxSecond.Text = initTime.Second.ToString();

            Title = OsLocalization.Entity.TimeChangeDialogLabel1;
            ButtonSave.Content = OsLocalization.Entity.TimeChangeDialogLabel2;
            LabelHour.Content = OsLocalization.Entity.TimeChangeDialogLabel3;
            LabelMinute.Content = OsLocalization.Entity.TimeChangeDialogLabel4;
            LabelSecond.Content = OsLocalization.Entity.TimeChangeDialogLabel5;

            this.Activate();
            this.Focus();
        }

        public DateTime Time;

        public bool IsSaved;

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (DateTimePicker.SelectedDate is DateTime selectedDate
                && TryParseTimePart(TextBoxHour.Text, out int hour)
                && TryParseTimePart(TextBoxMinute.Text, out int min)
                && TryParseTimePart(TextBoxSecond.Text, out int sec))
            {
                Time = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hour, min, sec);
                IsSaved = true;
            }
            else
            {
                return;
            }

            Close();
        }

        private static bool TryParseTimePart(string? value, out int parsed)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed);
        }
    }
}
