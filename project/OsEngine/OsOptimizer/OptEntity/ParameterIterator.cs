/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using OsEngine.Entity;
using OsEngine.Logging;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Parameter combination counting, enumeration, and copying.
    /// Eliminates 3x duplication of the same iteration logic.
    /// Подсчёт, перечисление и копирование комбинаций параметров.
    /// </summary>
    public class ParameterIterator
    {
        /// <summary>
        /// Count how many parameter combinations exist for enabled parameters.
        /// </summary>
        public int CountCombinations(List<IIStrategyParameter> parameters, List<bool> parametersOn)
        {
            List<IIStrategyParameter> allParam = parameters;

            for (int i = 0; i < allParam.Count; i++)
            {
                ReloadParam(allParam[i]);
            }

            List<IIStrategyParameter> optimizedParams = new List<IIStrategyParameter>();

            for (int i = 0; i < allParam.Count; i++)
            {
                if (parametersOn[i])
                {
                    optimizedParams.Add(allParam[i]);
                    ReloadParam(allParam[i]);
                }
            }

            optimizedParams = CopyParameters(optimizedParams);

            int countBots = 0;
            bool isStart = true;

            while (true)
            {
                if (countBots > 10000000)
                {
                    SendLogMessage("Iteration count > 10000000. Warning!!!", LogMessageType.Error);
                    return countBots;
                }

                bool isEndOfFaze = false;

                for (int i2 = 0; i2 < optimizedParams.Count + 1; i2++)
                {
                    if (i2 == optimizedParams.Count)
                    {
                        isEndOfFaze = true;
                        break;
                    }

                    if (isStart)
                    {
                        countBots++;
                        isStart = false;
                        break;
                    }

                    if (TryIncrementParameter(optimizedParams, i2))
                    {
                        countBots++;
                        break;
                    }
                }

                if (isEndOfFaze)
                {
                    break;
                }
            }

            return countBots;
        }

        /// <summary>
        /// Enumerate all parameter combinations for optimized parameters.
        /// Yields a list of parameter values for each combination.
        /// </summary>
        public IEnumerable<List<IIStrategyParameter>> EnumerateCombinations(
            List<IIStrategyParameter> optimizedParametersStart)
        {
            List<IIStrategyParameter> optimizeParamCurrent = CopyParameters(optimizedParametersStart);
            ReloadAllParam(optimizeParamCurrent);

            bool isStart = true;

            while (true)
            {
                bool isEndOfFaze = false;

                for (int i2 = 0; i2 < optimizeParamCurrent.Count + 1; i2++)
                {
                    if (i2 == optimizeParamCurrent.Count)
                    {
                        isEndOfFaze = true;
                        break;
                    }

                    if (isStart)
                    {
                        isStart = false;
                        break;
                    }

                    if (TryIncrementParameter(optimizeParamCurrent, i2))
                    {
                        break;
                    }
                }

                if (isEndOfFaze)
                {
                    yield break;
                }

                yield return optimizeParamCurrent;
            }
        }

        /// <summary>
        /// Try to increment a parameter at position idx. If successful, reset all prior parameters.
        /// Returns true if incremented.
        /// </summary>
        private bool TryIncrementParameter(List<IIStrategyParameter> parameters, int idx)
        {
            if (parameters[idx].Type == StrategyParameterType.Int)
            {
                StrategyParameterInt parameter = (StrategyParameterInt)parameters[idx];
                if (parameter.ValueInt < parameter.ValueIntStop)
                {
                    parameter.ValueInt = parameter.ValueInt + parameter.ValueIntStep;
                    for (int i3 = 0; i3 < idx; i3++)
                    {
                        ReloadParam(parameters[i3]);
                    }
                    return true;
                }
            }
            else if (parameters[idx].Type == StrategyParameterType.Decimal)
            {
                StrategyParameterDecimal parameter = (StrategyParameterDecimal)parameters[idx];
                if (parameter.ValueDecimal < parameter.ValueDecimalStop)
                {
                    parameter.ValueDecimal = parameter.ValueDecimal + parameter.ValueDecimalStep;
                    for (int i3 = 0; i3 < idx; i3++)
                    {
                        ReloadParam(parameters[i3]);
                    }
                    return true;
                }
            }
            else if (parameters[idx].Type == StrategyParameterType.DecimalCheckBox)
            {
                StrategyParameterDecimalCheckBox parameter = (StrategyParameterDecimalCheckBox)parameters[idx];
                if (parameter.ValueDecimal < parameter.ValueDecimalStop)
                {
                    parameter.ValueDecimal = parameter.ValueDecimal + parameter.ValueDecimalStep;
                    for (int i3 = 0; i3 < idx; i3++)
                    {
                        ReloadParam(parameters[i3]);
                    }
                    return true;
                }
            }

            return false;
        }

        public void ReloadAllParam(List<IIStrategyParameter> parameters)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                ReloadParam(parameters[i]);
            }
        }

        public void ReloadParam(IIStrategyParameter parameters)
        {
            if (parameters.Type == StrategyParameterType.Int)
            {
                ((StrategyParameterInt)parameters).ValueInt = ((StrategyParameterInt)parameters).ValueIntStart;
            }

            if (parameters.Type == StrategyParameterType.Decimal)
            {
                ((StrategyParameterDecimal)parameters).ValueDecimal = ((StrategyParameterDecimal)parameters).ValueDecimalStart;
            }

            if (parameters.Type == StrategyParameterType.DecimalCheckBox)
            {
                ((StrategyParameterDecimalCheckBox)parameters).ValueDecimal = ((StrategyParameterDecimalCheckBox)parameters).ValueDecimalStart;
            }
        }

        public List<IIStrategyParameter> CopyParameters(List<IIStrategyParameter> parametersToCopy)
        {
            List<IIStrategyParameter> newParameters = new List<IIStrategyParameter>();

            for (int i = 0; i < parametersToCopy.Count; i++)
            {
                IIStrategyParameter newParam = null;

                if (parametersToCopy[i].Type == StrategyParameterType.Bool)
                {
                    newParam = new StrategyParameterBool(parametersToCopy[i].Name, ((StrategyParameterBool)parametersToCopy[i]).ValueBool);
                }
                else if (parametersToCopy[i].Type == StrategyParameterType.String)
                {
                    newParam = new StrategyParameterString(parametersToCopy[i].Name, ((StrategyParameterString)parametersToCopy[i]).ValueString,
                        ((StrategyParameterString)parametersToCopy[i]).ValuesString);
                }
                else if (parametersToCopy[i].Type == StrategyParameterType.Int)
                {
                    newParam = new StrategyParameterInt(parametersToCopy[i].Name,
                        ((StrategyParameterInt)parametersToCopy[i]).ValueIntDefolt,
                        ((StrategyParameterInt)parametersToCopy[i]).ValueIntStart,
                        ((StrategyParameterInt)parametersToCopy[i]).ValueIntStop,
                        ((StrategyParameterInt)parametersToCopy[i]).ValueIntStep);
                    ((StrategyParameterInt)newParam).ValueInt = ((StrategyParameterInt)parametersToCopy[i]).ValueIntStart;
                }
                else if (parametersToCopy[i].Type == StrategyParameterType.Decimal)
                {
                    newParam = new StrategyParameterDecimal(parametersToCopy[i].Name,
                        ((StrategyParameterDecimal)parametersToCopy[i]).ValueDecimalDefolt,
                        ((StrategyParameterDecimal)parametersToCopy[i]).ValueDecimalStart,
                        ((StrategyParameterDecimal)parametersToCopy[i]).ValueDecimalStop,
                        ((StrategyParameterDecimal)parametersToCopy[i]).ValueDecimalStep);
                    ((StrategyParameterDecimal)newParam).ValueDecimal = ((StrategyParameterDecimal)parametersToCopy[i]).ValueDecimalStart;
                }
                else if (parametersToCopy[i].Type == StrategyParameterType.DecimalCheckBox)
                {
                    newParam = new StrategyParameterDecimalCheckBox(parametersToCopy[i].Name,
                        ((StrategyParameterDecimalCheckBox)parametersToCopy[i]).ValueDecimalDefolt,
                        ((StrategyParameterDecimalCheckBox)parametersToCopy[i]).ValueDecimalStart,
                        ((StrategyParameterDecimalCheckBox)parametersToCopy[i]).ValueDecimalStop,
                        ((StrategyParameterDecimalCheckBox)parametersToCopy[i]).ValueDecimalStep,
                        Convert.ToBoolean(((StrategyParameterDecimalCheckBox)parametersToCopy[i]).CheckState));
                    ((StrategyParameterDecimalCheckBox)newParam).ValueDecimal = ((StrategyParameterDecimalCheckBox)parametersToCopy[i]).ValueDecimalStart;
                }

                newParameters.Add(newParam);
            }
            return newParameters;
        }

        private void SendLogMessage(string message, LogMessageType type)
        {
            LogMessageEvent?.Invoke(message, type);
        }

        public event Action<string, LogMessageType> LogMessageEvent;
    }
}
