#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

﻿namespace OsEngine.OsTrader.Panels.Attributes
{
    /// <summary>
    /// Attribute for applying bot to terminal
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class BotAttribute : System.Attribute
    {
        public string Name { get; }

        public BotAttribute(string name)
        {
            Name = name;
        }
    }
}
