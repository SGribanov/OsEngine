/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;

#nullable enable

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Delegate-based evaluator adapter for optimization strategies.
    /// </summary>
    public class BotEvaluator : IBotEvaluator
    {
        private readonly Func<List<IIStrategyParameter>, List<IIStrategyParameter>, CancellationToken, Task<OptimizerReport>> _evaluateFunc;

        public BotEvaluator(Func<List<IIStrategyParameter>, List<IIStrategyParameter>, CancellationToken, Task<OptimizerReport>> evaluateFunc)
        {
            _evaluateFunc = evaluateFunc ?? throw new ArgumentNullException(nameof(evaluateFunc));
        }

        public Task<OptimizerReport> EvaluateAsync(
            List<IIStrategyParameter> allParameters,
            List<IIStrategyParameter> optimizedParameters,
            CancellationToken cancellationToken = default)
        {
            return _evaluateFunc(allParameters, optimizedParameters, cancellationToken);
        }
    }
}
