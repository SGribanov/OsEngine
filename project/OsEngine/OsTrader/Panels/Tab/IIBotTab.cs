#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using OsEngine.Logging;

namespace OsEngine.OsTrader.Panels.Tab
{
    /// <summary>
    /// Tab interface for robot panel
    /// </summary>
    public interface IIBotTab
    {
        /// <summary>
        /// source type
        /// </summary>
        BotTabType TabType { get; }

        /// <summary>
        /// Remove tab and all child structures
        /// </summary>
        void Delete();

        /// <summary>
        /// Clear
        /// </summary>
        void Clear();

        /// <summary>
        /// Stop drawing this robot
        /// </summary>
        void StopPaint();

        /// <summary>
        /// Unique robot name
        /// </summary>
        string TabName { get; set; }

        /// <summary>
        /// Tab number
        /// </summary>
        int TabNum { get; set; }

        /// <summary>
        /// are events sent to the top from the tab?
        /// </summary>
        bool EventsIsOn { get; set; }

        /// <summary>
        /// are events sent to the top from the tab?
        /// </summary>
        bool EmulatorIsOn { get; set; }

        /// <summary>
        /// Time of the last update of the candle
        /// </summary>
        DateTime LastTimeCandleUpdate { get; set; }

        /// <summary>
        /// Source removed
        /// </summary>
        event Action TabDeletedEvent;

        /// <summary>
        /// New log message event
        /// </summary>
        event Action<string, LogMessageType> LogMessageEvent;
    }
}
