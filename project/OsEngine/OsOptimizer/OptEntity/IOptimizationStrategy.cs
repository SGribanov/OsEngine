/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;

#nullable enable

namespace OsEngine.OsOptimizer.OptEntity
{
    public interface IOptimizationStrategy
    {
        int EstimateBotCount(List<IIStrategyParameter> allParameters, List<bool>? parametersToOptimization);

        Task<List<OptimizerReport>> OptimizeInSampleAsync(
            List<IIStrategyParameter> allParameters,
            List<bool>? parametersToOptimization,
            CancellationToken cancellationToken = default);
    }
}
