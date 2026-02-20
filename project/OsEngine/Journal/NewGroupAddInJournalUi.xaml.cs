#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Collections.Generic;
using System.Windows;
using OsEngine.Language;

namespace OsEngine.Journal
{
    /// <summary>
    /// Логика взаимодействия для NewGroupAddInJournalUi.xaml
    /// </summary>
    public partial class NewGroupAddInJournalUi : Window
    {
        public NewGroupAddInJournalUi(List<string> oldGroupNames)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);
            _oldGroupNames = oldGroupNames;
            Title = OsLocalization.Journal.Label13;
            ButtonAccept.Content = OsLocalization.Journal.Label14;

            this.Activate();
            this.Focus();
        }

        public bool IsAccepted;

        public string NewGroupName;

        private List<string> _oldGroupNames;

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            string textInTextBox = TextBoxNewGroupName.Text;

            if(string.IsNullOrEmpty(textInTextBox))
            {
                return;
            }

            for(int i = 0;i < _oldGroupNames.Count;i++)
            {
                if(_oldGroupNames[i].Equals(textInTextBox))
                {
                    return;
                }
            }

            NewGroupName = textInTextBox;
            IsAccepted = true;
            Close();
        }
    }
}
