#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 *Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использования кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/


using OsEngine.Language;

namespace OsEngine.Alerts
{
    /// <summary>
    /// Message box
    /// Окно сообщений
    /// </summary>
    public partial class AlertMessageSimpleUi
    {

        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        /// <param name="message">сообщение</param>
        public AlertMessageSimpleUi(string message)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            TextBoxMessage.Text = message;

            Title = OsLocalization.Alerts.TitleAlertMessageSimpleUi;

            this.Activate();
            this.Focus();
        }
    }
}

