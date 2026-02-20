#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;

namespace OsEngine.Indicators
{
    /// <summary>
    /// Attribute for applying indicators to terminal
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Class)]
    public class IndicatorAttribute : Attribute
    {
        public string Name { get; }

        public IndicatorAttribute(string name)
        {
            Name = name;
        }
    }
}
