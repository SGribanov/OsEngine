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
            SortBotsType objectiveMetric,
            ObjectiveDirectionType objectiveDirection,
            int bayesianInitialSamples,
            int bayesianMaxIterations,
            int bayesianBatchSize,
            BayesianAcquisitionModeType bayesianAcquisitionMode,
            decimal bayesianAcquisitionKappa,
            bool bayesianUseTailPass,
            int bayesianTailSharePercent,
            out string infoMessage)
        {
            if (method == OptimizationMethodType.Bayesian)
            {
                infoMessage =
                    "Bayesian strategy skeleton is active. " +
                    "Method=Bayesian, " +
                    "Objective=" + objectiveMetric + ", " +
                    "Direction=" + objectiveDirection + ", " +
                    "InitialSamples=" + bayesianInitialSamples + ", " +
                    "MaxIterations=" + bayesianMaxIterations + ", " +
                    "BatchSize=" + bayesianBatchSize + ", " +
                    "AcquisitionMode=" + bayesianAcquisitionMode + ", " +
                    "Kappa=" + bayesianAcquisitionKappa + ", " +
                    "TailPass=" + bayesianUseTailPass + ", " +
                    "TailSharePercent=" + bayesianTailSharePercent + ".";
                return new BayesianOptimizationStrategy(
                    parameterIterator,
                    evaluator,
                    maxParallel,
                    objectiveMetric,
                    objectiveDirection,
                    bayesianInitialSamples,
                    bayesianMaxIterations,
                    bayesianBatchSize,
                    bayesianAcquisitionMode,
                    bayesianAcquisitionKappa,
                    bayesianUseTailPass,
                    bayesianTailSharePercent);
            }

            infoMessage = null;
            return new BruteForceStrategy(parameterIterator, evaluator, maxParallel);
        }
    }
}
