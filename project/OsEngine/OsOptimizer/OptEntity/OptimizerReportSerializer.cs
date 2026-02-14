/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Text;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Version-aware serializer for OptimizerReport with legacy fallback.
    /// </summary>
    public static class OptimizerReportSerializer
    {
        private const string V2Prefix = "V2|";

        public static StringBuilder Serialize(OptimizerReport report)
        {
            return new StringBuilder(V2Prefix + SerializeLegacyBody(report));
        }

        public static void Deserialize(OptimizerReport report, string saveStr)
        {
            if (string.IsNullOrEmpty(saveStr))
            {
                return;
            }

            try
            {
                if (saveStr.StartsWith(V2Prefix))
                {
                    DeserializeLegacyBody(report, saveStr.Substring(V2Prefix.Length));
                    return;
                }

                // legacy format without version prefix
                DeserializeLegacyBody(report, saveStr);
            }
            catch
            {
                // keep backward-compatible behavior
            }
        }

        private static string SerializeLegacyBody(OptimizerReport report)
        {
            StringBuilder result = new StringBuilder();

            // Сохраняем основное
            result.Append(report.BotName + "@");
            result.Append(report.PositionsCount + "@");
            result.Append(report.TotalProfit + "@");
            result.Append(report.MaxDrawDawn + "@");
            result.Append(report.AverageProfit + "@");
            result.Append(report.AverageProfitPercentOneContract + "@");
            result.Append(report.ProfitFactor + "@");
            result.Append(report.PayOffRatio + "@");
            result.Append(report.Recovery + "@");
            result.Append(report.TotalProfitPercent + "@");
            result.Append(report.SharpRatio + "@");

            // сохраняем параметры в строковом представлении
            StringBuilder parameters = new StringBuilder();

            for (int i = 0; i < report.StrategyParameters.Count; i++)
            {
                parameters.Append(report.StrategyParameters[i] + "&");
            }

            result.Append(parameters + "@");

            // сохраняем отдельные репорты по вкладкам
            StringBuilder reportTabs = new StringBuilder();

            for (int i = 0; i < report.TabsReports.Count; i++)
            {
                reportTabs.Append(report.TabsReports[i].GetSaveString() + "&");
            }

            result.Append(reportTabs + "@");

            return result.ToString();
        }

        private static void DeserializeLegacyBody(OptimizerReport report, string saveStr)
        {
            string[] str = saveStr.Split('@');

            report.BotName = str[0];
            report.PositionsCount = Convert.ToInt32(str[1]);
            report.TotalProfit = str[2].ToDecimal();
            report.MaxDrawDawn = str[3].ToDecimal();
            report.AverageProfit = str[4].ToDecimal();
            report.AverageProfitPercentOneContract = str[5].ToDecimal();
            report.ProfitFactor = str[6].ToDecimal();
            report.PayOffRatio = str[7].ToDecimal();
            report.Recovery = str[8].ToDecimal();
            report.TotalProfitPercent = str[9].ToDecimal();
            report.SharpRatio = str[10].ToDecimal();

            report.StrategyParameters.Clear();
            string[] param = str[11].Split('&');
            for (int i = 0; i < param.Length - 1; i++)
            {
                report.StrategyParameters.Add(param[i]);
            }

            report.TabsReports.Clear();
            string[] reportTabs = str[12].Split('&');
            for (int i = 0; i < reportTabs.Length - 1; i++)
            {
                OptimizerReportTab faze = new OptimizerReportTab();
                faze.LoadFromSaveString(reportTabs[i]);
                report.TabsReports.Add(faze);
            }
        }
    }
}
