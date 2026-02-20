#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Windows;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.OsTrader.Panels;

namespace OsEngine.Robots.Engines
{
    // Blank strategy for manual trading
    public class CandleEngine : BotPanel
    {
        public CandleEngine(string name, StartProgram startProgram)
            : base(name, startProgram)
        {
            // Create tabs
            TabCreate(BotTabType.Simple);

            Description = OsLocalization.Description.DescriptionLabel28;
        }

        // The name of the robot in OsEngine
        public override string GetNameStrategyType()
        {
            return "Engine";
        }

        // Show settings GUI
        public override void ShowIndividualSettingsDialog()
        {
            MessageBox.Show(OsLocalization.Trader.Label57);
        }
    }
}
