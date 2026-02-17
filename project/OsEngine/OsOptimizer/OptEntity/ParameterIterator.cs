/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using OsEngine.Entity;
using OsEngine.Logging;

#nullable enable

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

            if (optimizedParams.Count == 0)
            {
                return 0;
            }

            long totalCombinations = 1;

            for (int i = 0; i < optimizedParams.Count; i++)
            {
                long variantsForParam = GetParameterVariantsCount(optimizedParams[i]);

                if (variantsForParam <= 0)
                {
                    return 0;
                }

                if (totalCombinations > int.MaxValue / variantsForParam)
                {
                    SendLogMessage("Iteration count exceeds Int32.MaxValue. Estimate was clamped.", LogMessageType.Error);
                    return int.MaxValue;
                }

                totalCombinations *= variantsForParam;
            }

            return (int)totalCombinations;
        }

        private long GetParameterVariantsCount(IIStrategyParameter parameter)
        {
            if (parameter == null)
            {
                return 0;
            }

            if (parameter.Type == StrategyParameterType.Int)
            {
                StrategyParameterInt p = (StrategyParameterInt)parameter;

                if (p.ValueIntStart == p.ValueIntStop)
                {
                    return 1;
                }

                if (p.ValueIntStep <= 0 || p.ValueIntStart > p.ValueIntStop)
                {
                    return 0;
                }

                long diff = (long)p.ValueIntStop - p.ValueIntStart;
                long increments = (diff + p.ValueIntStep - 1) / p.ValueIntStep;
                return increments + 1;
            }

            if (parameter.Type == StrategyParameterType.Decimal)
            {
                StrategyParameterDecimal p = (StrategyParameterDecimal)parameter;
                return GetDecimalVariantsCount(p.ValueDecimalStart, p.ValueDecimalStop, p.ValueDecimalStep);
            }

            if (parameter.Type == StrategyParameterType.DecimalCheckBox)
            {
                StrategyParameterDecimalCheckBox p = (StrategyParameterDecimalCheckBox)parameter;
                return GetDecimalVariantsCount(p.ValueDecimalStart, p.ValueDecimalStop, p.ValueDecimalStep);
            }

            return 1;
        }

        private long GetDecimalVariantsCount(decimal start, decimal stop, decimal step)
        {
            if (start == stop)
            {
                return 1;
            }

            if (step <= 0 || start > stop)
            {
                return 0;
            }

            decimal raw = (stop - start) / step;
            decimal incrementsDecimal = Math.Ceiling(raw);

            if (incrementsDecimal > int.MaxValue - 1)
            {
                return int.MaxValue;
            }

            int increments = (int)incrementsDecimal;
            return (long)increments + 1;
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
                if (parameter.ValueIntStep <= 0)
                {
                    return false;
                }

                if (parameter.ValueInt < parameter.ValueIntStop)
                {
                    int nextValue = parameter.ValueInt + parameter.ValueIntStep;
                    if (nextValue <= parameter.ValueInt)
                    {
                        return false;
                    }

                    if (nextValue > parameter.ValueIntStop)
                    {
                        nextValue = parameter.ValueIntStop;
                    }

                    parameter.ValueInt = nextValue;
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
                if (parameter.ValueDecimalStep <= 0)
                {
                    return false;
                }

                if (parameter.ValueDecimal < parameter.ValueDecimalStop)
                {
                    decimal nextValue = parameter.ValueDecimal + parameter.ValueDecimalStep;
                    if (nextValue <= parameter.ValueDecimal)
                    {
                        return false;
                    }

                    if (nextValue > parameter.ValueDecimalStop)
                    {
                        nextValue = parameter.ValueDecimalStop;
                    }

                    parameter.ValueDecimal = nextValue;
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
                if (parameter.ValueDecimalStep <= 0)
                {
                    return false;
                }

                if (parameter.ValueDecimal < parameter.ValueDecimalStop)
                {
                    decimal nextValue = parameter.ValueDecimal + parameter.ValueDecimalStep;
                    if (nextValue <= parameter.ValueDecimal)
                    {
                        return false;
                    }

                    if (nextValue > parameter.ValueDecimalStop)
                    {
                        nextValue = parameter.ValueDecimalStop;
                    }

                    parameter.ValueDecimal = nextValue;
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
                IIStrategyParameter? newParam = null;

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

                newParameters.Add(newParam!);
            }
            return newParameters;
        }

        private void SendLogMessage(string message, LogMessageType type)
        {
            LogMessageEvent?.Invoke(message, type);
        }

        public event Action<string, LogMessageType>? LogMessageEvent;
    }
}
