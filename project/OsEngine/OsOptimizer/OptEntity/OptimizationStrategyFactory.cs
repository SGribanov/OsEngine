/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

namespace OsEngine.OsOptimizer.OptEntity
{
    public static class OptimizationStrategyFactory
    {
        public static IOptimizationStrategy CreateInSampleStrategy(
            OptimizationMethodType method,
            ParameterIterator parameterIterator,
            IBotEvaluator evaluator,
            int maxParallel,
            out string infoMessage)
        {
            if (method == OptimizationMethodType.Bayesian)
            {
                infoMessage = "Bayesian strategy is not integrated yet. Fallback to BruteForce.";
                return new BruteForceStrategy(parameterIterator, evaluator, maxParallel);
            }

            infoMessage = null;
            return new BruteForceStrategy(parameterIterator, evaluator, maxParallel);
        }
    }
}
