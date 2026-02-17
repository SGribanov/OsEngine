/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

#nullable enable

namespace OsEngine.OsOptimizer
{
    /// <summary>
    /// How to sort optimization results.
    /// Способ сортировки результатов оптимизации.
    /// </summary>
    public enum SortBotsType
    {
        TotalProfit,

        BotName,

        PositionCount,

        MaxDrawDawn,

        AverageProfit,

        AverageProfitPercent,

        ProfitFactor,

        PayOffRatio,

        Recovery,

        SharpRatio
    }
}
