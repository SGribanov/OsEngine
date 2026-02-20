#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using System;
using System.Linq;

namespace OsEngine.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ButtonAttribute : ParameterElementAttribute
    {
        /// <summary> Displays a button parameter in the bot parameters </summary>
        public ButtonAttribute(string name = null, string tabControlName = null)
        {
            _name = name;
            TabControlName = tabControlName;
        }

        public override void BindToBot(BotPanel bot, AttributeInitializer.AttributeMember member, AttributeInitializer initializer)
        {
            ParameterElementAttribute[] attributs = member.CustomAttributes;

            if (attributs.OfType<ParameterAttribute>().Count() > 0)
                return;

            StrategyParameterButton parameter = bot.CreateParameterButton(Name, TabControlName);

            if (member.Type == typeof(StrategyParameterButton))
                member.SetValue(parameter);
            else
                parameter.UserClickOnButtonEvent += member.InvokeIfMethod;
        }
    }
}

