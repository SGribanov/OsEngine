#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использования кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;

namespace OsEngine.Charts.CandleChart.Elements
{
    /// <summary>
    /// interface for creating chart elements. Lines, points, etc.
    /// интерфейс для создания элементов чарта. Линий, точек и т.д.
    /// </summary>
    public interface IChartElement
    {
        /// <summary>
        /// base class name
        /// имя базового класса
        /// </summary>
        string TypeName();

        /// <summary>
        /// unique name on chart
        /// уникальное имя на чарте
        /// </summary>
        string UniqName { get; set; }

        /// <summary>
        /// chart area where element will be drawn
        /// область чарта на которой будет прорисован элемент
        /// </summary>
        string Area { get; set; }

        /// <summary>
        /// it is necessary to update item on chart
        /// необходимо обновить элемент на чарте
        /// </summary>
        event Action<IChartElement> UpdateEvent;

        /// <summary>
        /// it's necessary to remove an item from chart forever
        /// необходимо удалить элемент с чарта навсегда
        /// </summary>
        event Action<IChartElement> DeleteEvent;

        /// <summary>
        /// update item on chart
        /// обновить элемент на чарте
        /// </summary>
        void Refresh();

        /// <summary>
        /// uninstall an item from chart
        /// удалить элемент с чарта
        /// </summary>
        void Delete();

    }
}

