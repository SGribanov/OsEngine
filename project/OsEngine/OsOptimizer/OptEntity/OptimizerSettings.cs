/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.Market.Servers.Tester;
using OsEngine.OsOptimizer;

#nullable enable

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Persistent optimizer configuration: filters, trade settings, phase config,
    /// clearing times, non-trade periods. Load/Save.
    /// Постоянная конфигурация оптимизатора: фильтры, настройки торговли, фазы,
    /// клиринги, неторговые периоды.
    /// </summary>
    public class OptimizerSettings
    {
        public OptimizerSettings()
        {
            _threadsCount = 1;
            _startDeposit = 100000;
            _filterProfitValue = 10;
            _filterProfitIsOn = false;
            _filterMaxDrawDownValue = -10;
            _filterMaxDrawDownIsOn = false;
            _filterMiddleProfitValue = 0.001m;
            _filterMiddleProfitIsOn = false;
            _filterProfitFactorValue = 1;
            _filterProfitFactorIsOn = false;
            _percentOnFiltration = 30;
            _iterationCount = 1;

            Load();
            LoadClearingInfo();
            LoadNonTradePeriods();
        }

        #region Main settings

        public int ThreadsCount
        {
            get => _threadsCount;
            set { _threadsCount = value; Save(); }
        }
        private int _threadsCount;

        public string StrategyName
        {
            get => _strategyName;
            set { _strategyName = value; Save(); }
        }
        private string _strategyName = string.Empty;

        public bool IsScript
        {
            get => _isScript;
            set { _isScript = value; Save(); }
        }
        private bool _isScript;

        public decimal StartDeposit
        {
            get => _startDeposit;
            set { _startDeposit = value; Save(); }
        }
        private decimal _startDeposit;

        #endregion

        #region Trade server settings

        public OrderExecutionType OrderExecutionType
        {
            get => _orderExecutionType;
            set { _orderExecutionType = value; Save(); }
        }
        private OrderExecutionType _orderExecutionType;

        public int SlippageToSimpleOrder
        {
            get => _slippageToSimpleOrder;
            set
            {
                if (_slippageToSimpleOrder == value) return;
                _slippageToSimpleOrder = value;
                Save();
            }
        }
        private int _slippageToSimpleOrder;

        public int SlippageToStopOrder
        {
            get => _slippageToStopOrder;
            set
            {
                if (_slippageToStopOrder == value) return;
                _slippageToStopOrder = value;
                Save();
            }
        }
        private int _slippageToStopOrder;

        public CommissionType CommissionType
        {
            get => _commissionType;
            set
            {
                if (_commissionType == value) return;
                _commissionType = value;
                Save();
                CommissionChanged?.Invoke();
            }
        }
        private CommissionType _commissionType;

        public decimal CommissionValue
        {
            get => _commissionValue;
            set
            {
                if (_commissionValue == value) return;
                _commissionValue = value;
                Save();
                CommissionChanged?.Invoke();
            }
        }
        private decimal _commissionValue;

        /// <summary>
        /// Fired when CommissionType or CommissionValue changes.
        /// </summary>
        public event Action? CommissionChanged;

        #endregion

        #region Filters

        public decimal FilterProfitValue
        {
            get => _filterProfitValue;
            set { _filterProfitValue = value; Save(); }
        }
        private decimal _filterProfitValue;

        public bool FilterProfitIsOn
        {
            get => _filterProfitIsOn;
            set { _filterProfitIsOn = value; Save(); }
        }
        private bool _filterProfitIsOn;

        public decimal FilterMaxDrawDownValue
        {
            get => _filterMaxDrawDownValue;
            set { _filterMaxDrawDownValue = value; Save(); }
        }
        private decimal _filterMaxDrawDownValue;

        public bool FilterMaxDrawDownIsOn
        {
            get => _filterMaxDrawDownIsOn;
            set { _filterMaxDrawDownIsOn = value; Save(); }
        }
        private bool _filterMaxDrawDownIsOn;

        public decimal FilterMiddleProfitValue
        {
            get => _filterMiddleProfitValue;
            set { _filterMiddleProfitValue = value; Save(); }
        }
        private decimal _filterMiddleProfitValue;

        public bool FilterMiddleProfitIsOn
        {
            get => _filterMiddleProfitIsOn;
            set { _filterMiddleProfitIsOn = value; Save(); }
        }
        private bool _filterMiddleProfitIsOn;

        public decimal FilterProfitFactorValue
        {
            get => _filterProfitFactorValue;
            set { _filterProfitFactorValue = value; Save(); }
        }
        private decimal _filterProfitFactorValue;

        public bool FilterProfitFactorIsOn
        {
            get => _filterProfitFactorIsOn;
            set { _filterProfitFactorIsOn = value; Save(); }
        }
        private bool _filterProfitFactorIsOn;

        public int FilterDealsCountValue
        {
            get => _filterDealsCountValue;
            set { _filterDealsCountValue = value; Save(); }
        }
        private int _filterDealsCountValue;

        public bool FilterDealsCountIsOn
        {
            get => _filterDealsCountIsOn;
            set { _filterDealsCountIsOn = value; Save(); }
        }
        private bool _filterDealsCountIsOn;

        #endregion

        #region Phase settings

        public DateTime TimeStart
        {
            get => _timeStart;
            set
            {
                _timeStart = value;
                Save();
                DateTimeStartEndChange?.Invoke();
            }
        }
        private DateTime _timeStart;

        public DateTime TimeEnd
        {
            get => _timeEnd;
            set
            {
                _timeEnd = value;
                Save();
                DateTimeStartEndChange?.Invoke();
            }
        }
        private DateTime _timeEnd;

        public decimal PercentOnFiltration
        {
            get => _percentOnFiltration;
            set { _percentOnFiltration = value; Save(); }
        }
        private decimal _percentOnFiltration;

        public int IterationCount
        {
            get => _iterationCount;
            set
            {
                _iterationCount = ClampPositiveInt(value);
                Save();
            }
        }
        private int _iterationCount;

        public bool LastInSample
        {
            get => _lastInSample;
            set { _lastInSample = value; Save(); }
        }
        private bool _lastInSample;

        public event Action? DateTimeStartEndChange;

        #endregion

        #region Clearing system

        public List<OrderClearing> ClearingTimes = new List<OrderClearing>();

        public void SaveClearingInfo()
        {
            try
            {
                List<string> lines = new List<string>();
                for (int i = 0; i < ClearingTimes.Count; i++)
                {
                    lines.Add(ClearingTimes[i].GetSaveString());
                }

                SafeFileWriter.WriteAllLines(GetClearingsPath(), lines);
            }
            catch (Exception ex)
            {
                LogMessageEvent?.Invoke(ex.ToString(), LogMessageType.Error);
            }
        }

        private void LoadClearingInfo()
        {
            if (!File.Exists(GetClearingsPath()))
            {
                return;
            }

            try
            {
                using (StreamReader reader = new StreamReader(GetClearingsPath()))
                {
                    while (reader.EndOfStream == false)
                    {
                        string? str = reader.ReadLine();

                        if (!string.IsNullOrEmpty(str))
                        {
                            OrderClearing clearings = new OrderClearing();
                            clearings.SetFromString(str);
                            ClearingTimes.Add(clearings);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessageEvent?.Invoke(ex.ToString(), LogMessageType.Error);
            }
        }

        public void CreateNewClearing()
        {
            OrderClearing newClearing = new OrderClearing();
            newClearing.Time = new DateTime(2000, 1, 1, 19, 0, 0);
            ClearingTimes.Add(newClearing);
            SaveClearingInfo();
        }

        public void RemoveClearing(int num)
        {
            if (num > ClearingTimes.Count)
            {
                return;
            }

            ClearingTimes.RemoveAt(num);
            SaveClearingInfo();
        }

        #endregion

        #region Non-trade periods

        public List<NonTradePeriod> NonTradePeriods = new List<NonTradePeriod>();

        public void SaveNonTradePeriods()
        {
            try
            {
                List<string> lines = new List<string>();
                for (int i = 0; i < NonTradePeriods.Count; i++)
                {
                    lines.Add(NonTradePeriods[i].GetSaveString());
                }

                SafeFileWriter.WriteAllLines(GetNonTradePeriodsPath(), lines);
            }
            catch (Exception ex)
            {
                LogMessageEvent?.Invoke(ex.ToString(), LogMessageType.Error);
            }
        }

        private void LoadNonTradePeriods()
        {
            if (!File.Exists(GetNonTradePeriodsPath()))
            {
                return;
            }

            try
            {
                using (StreamReader reader = new StreamReader(GetNonTradePeriodsPath()))
                {
                    while (reader.EndOfStream == false)
                    {
                        string? str = reader.ReadLine();

                        if (!string.IsNullOrEmpty(str))
                        {
                            NonTradePeriod period = new NonTradePeriod();
                            period.SetFromString(str);
                            NonTradePeriods.Add(period);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessageEvent?.Invoke(ex.ToString(), LogMessageType.Error);
            }
        }

        public void CreateNewNonTradePeriod()
        {
            NonTradePeriod newClearing = new NonTradePeriod();
            NonTradePeriods.Add(newClearing);
            SaveNonTradePeriods();
        }

        public void RemoveNonTradePeriod(int num)
        {
            if (num > NonTradePeriods.Count)
            {
                return;
            }

            NonTradePeriods.RemoveAt(num);
            SaveNonTradePeriods();
        }

        #endregion

        #region Optimization method settings

        public OptimizationMethodType OptimizationMethod
        {
            get => _optimizationMethod;
            set { _optimizationMethod = value; Save(); }
        }
        private OptimizationMethodType _optimizationMethod = OptimizationMethodType.BruteForce;

        public SortBotsType ObjectiveMetric
        {
            get => _objectiveMetric;
            set { _objectiveMetric = value; Save(); }
        }
        private SortBotsType _objectiveMetric = SortBotsType.TotalProfit;

        public int BayesianInitialSamples
        {
            get => _bayesianInitialSamples;
            set
            {
                _bayesianInitialSamples = ClampPositiveInt(value);
                Save();
            }
        }
        private int _bayesianInitialSamples = 20;

        public int BayesianMaxIterations
        {
            get => _bayesianMaxIterations;
            set
            {
                _bayesianMaxIterations = ClampPositiveInt(value);
                Save();
            }
        }
        private int _bayesianMaxIterations = 100;

        public int BayesianBatchSize
        {
            get => _bayesianBatchSize;
            set
            {
                _bayesianBatchSize = ClampPositiveInt(value);
                Save();
            }
        }
        private int _bayesianBatchSize = 5;

        public ObjectiveDirectionType ObjectiveDirection
        {
            get => _objectiveDirection;
            set { _objectiveDirection = value; Save(); }
        }
        private ObjectiveDirectionType _objectiveDirection = ObjectiveDirectionType.Maximize;

        public BayesianAcquisitionModeType BayesianAcquisitionMode
        {
            get => _bayesianAcquisitionMode;
            set { _bayesianAcquisitionMode = value; Save(); }
        }
        private BayesianAcquisitionModeType _bayesianAcquisitionMode = BayesianAcquisitionModeType.Ucb;

        public decimal BayesianAcquisitionKappa
        {
            get => _bayesianAcquisitionKappa;
            set
            {
                _bayesianAcquisitionKappa = ClampNonNegativeDecimal(value);
                Save();
            }
        }
        private decimal _bayesianAcquisitionKappa = 0.25m;

        public bool BayesianUseTailPass
        {
            get => _bayesianUseTailPass;
            set { _bayesianUseTailPass = value; Save(); }
        }
        private bool _bayesianUseTailPass = true;

        public int BayesianTailSharePercent
        {
            get => _bayesianTailSharePercent;
            set
            {
                _bayesianTailSharePercent = ClampTailSharePercent(value);
                Save();
            }
        }
        private int _bayesianTailSharePercent = 20;

        #endregion

        #region Save / Load

        private void Save()
        {
            try
            {
                List<string> lines = new List<string>
                {
                    _threadsCount.ToString(),
                    _strategyName,
                    _startDeposit.ToString(),

                    _filterProfitValue.ToString(),
                    _filterProfitIsOn.ToString(),
                    _filterMaxDrawDownValue.ToString(),
                    _filterMaxDrawDownIsOn.ToString(),
                    _filterMiddleProfitValue.ToString(),
                    _filterMiddleProfitIsOn.ToString(),
                    _filterProfitFactorValue.ToString(),
                    _filterProfitFactorIsOn.ToString(),

                    _timeStart.ToString(CultureInfo.InvariantCulture),
                    _timeEnd.ToString(CultureInfo.InvariantCulture),
                    _percentOnFiltration.ToString(),

                    _filterDealsCountValue.ToString(),
                    _filterDealsCountIsOn.ToString(),
                    _isScript.ToString(),
                    _iterationCount.ToString(),
                    _commissionType.ToString(),
                    _commissionValue.ToString(),
                    _lastInSample.ToString(),
                    _orderExecutionType.ToString(),
                    _slippageToSimpleOrder.ToString(),
                    _slippageToStopOrder.ToString(),

                    // V2 fields
                    _optimizationMethod.ToString(),
                    _objectiveMetric.ToString(),
                    _bayesianInitialSamples.ToString(),
                    _bayesianMaxIterations.ToString(),
                    _bayesianBatchSize.ToString(),
                    _objectiveDirection.ToString(),
                    _bayesianAcquisitionMode.ToString(),
                    _bayesianAcquisitionKappa.ToString(),
                    _bayesianUseTailPass.ToString(),
                    _bayesianTailSharePercent.ToString()
                };

                SafeFileWriter.WriteAllLines(GetSettingsPath(), lines);
            }
            catch (Exception error)
            {
                LogMessageEvent?.Invoke(error.ToString(), LogMessageType.Error);
            }
        }

        private void Load()
        {
            if (!File.Exists(GetSettingsPath()))
            {
                return;
            }
            try
            {
                using (StreamReader reader = new StreamReader(GetSettingsPath()))
                {
                    _threadsCount = Convert.ToInt32(reader.ReadLine() ?? "0");
                    _strategyName = reader.ReadLine() ?? string.Empty;
                    _startDeposit = (reader.ReadLine() ?? "0").ToDecimal();
                    _filterProfitValue = (reader.ReadLine() ?? "0").ToDecimal();
                    _filterProfitIsOn = Convert.ToBoolean(reader.ReadLine() ?? bool.FalseString);
                    _filterMaxDrawDownValue = (reader.ReadLine() ?? "0").ToDecimal();
                    _filterMaxDrawDownIsOn = Convert.ToBoolean(reader.ReadLine() ?? bool.FalseString);
                    _filterMiddleProfitValue = (reader.ReadLine() ?? "0").ToDecimal();
                    _filterMiddleProfitIsOn = Convert.ToBoolean(reader.ReadLine() ?? bool.FalseString);
                    _filterProfitFactorValue = (reader.ReadLine() ?? "0").ToDecimal();
                    _filterProfitFactorIsOn = Convert.ToBoolean(reader.ReadLine() ?? bool.FalseString);

                    _timeStart = Convert.ToDateTime(reader.ReadLine() ?? DateTime.MinValue.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                    _timeEnd = Convert.ToDateTime(reader.ReadLine() ?? DateTime.MinValue.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                    _percentOnFiltration = (reader.ReadLine() ?? "0").ToDecimal();

                    _filterDealsCountValue = Convert.ToInt32(reader.ReadLine() ?? "0");
                    _filterDealsCountIsOn = Convert.ToBoolean(reader.ReadLine() ?? bool.FalseString);
                    _isScript = Convert.ToBoolean(reader.ReadLine() ?? bool.FalseString);
                    _iterationCount = ClampPositiveInt(Convert.ToInt32(reader.ReadLine() ?? "1"));
                    _commissionType = (CommissionType)Enum.Parse(typeof(CommissionType),
                        reader.ReadLine() ?? CommissionType.None.ToString());
                    _commissionValue = (reader.ReadLine() ?? "0").ToDecimal();
                    _lastInSample = Convert.ToBoolean(reader.ReadLine() ?? bool.FalseString);

                    string? orderExecutionLine = reader.ReadLine();
                    if (TryParseDefinedEnum(orderExecutionLine, out OrderExecutionType orderExecutionType))
                    {
                        _orderExecutionType = orderExecutionType;
                    }
                    _slippageToSimpleOrder = Convert.ToInt32(reader.ReadLine() ?? "0");
                    _slippageToStopOrder = Convert.ToInt32(reader.ReadLine() ?? "0");

                    // V2 fields - optional for backward compatibility
                    string? line = reader.ReadLine();
                    if (line != null)
                    {
                        if (TryParseDefinedEnum(line, out OptimizationMethodType optimizationMethod))
                        {
                            _optimizationMethod = optimizationMethod;
                        }
                        line = reader.ReadLine();
                        if (line != null && TryParseDefinedEnum(line, out SortBotsType objectiveMetric))
                        {
                            _objectiveMetric = objectiveMetric;
                        }
                        line = reader.ReadLine();
                        if (TryParseInt(line, out int bayesianInitialSamples))
                        {
                            _bayesianInitialSamples = ClampPositiveInt(bayesianInitialSamples);
                        }
                        line = reader.ReadLine();
                        if (TryParseInt(line, out int bayesianMaxIterations))
                        {
                            _bayesianMaxIterations = ClampPositiveInt(bayesianMaxIterations);
                        }
                        line = reader.ReadLine();
                        if (TryParseInt(line, out int bayesianBatchSize))
                        {
                            _bayesianBatchSize = ClampPositiveInt(bayesianBatchSize);
                        }
                        line = reader.ReadLine();
                        if (line != null && TryParseDefinedEnum(line, out ObjectiveDirectionType objectiveDirection))
                        {
                            _objectiveDirection = objectiveDirection;
                        }
                        line = reader.ReadLine();
                        if (line != null && TryParseDefinedEnum(line, out BayesianAcquisitionModeType bayesianAcquisitionMode))
                        {
                            _bayesianAcquisitionMode = bayesianAcquisitionMode;
                        }
                        line = reader.ReadLine();
                        if (TryParseDecimal(line, out decimal bayesianAcquisitionKappa))
                        {
                            _bayesianAcquisitionKappa = ClampNonNegativeDecimal(bayesianAcquisitionKappa);
                        }
                        line = reader.ReadLine();
                        if (TryParseBool(line, out bool bayesianUseTailPass))
                        {
                            _bayesianUseTailPass = bayesianUseTailPass;
                        }
                        line = reader.ReadLine();
                        if (TryParseInt(line, out int bayesianTailSharePercent))
                        {
                            _bayesianTailSharePercent = ClampTailSharePercent(bayesianTailSharePercent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessageEvent?.Invoke(ex.ToString(), LogMessageType.Error);
            }
        }

        private static string GetClearingsPath()
        {
            return GetOptimizerSettingsFilePath("MasterClearings.txt");
        }

        private static string GetNonTradePeriodsPath()
        {
            return GetOptimizerSettingsFilePath("MasterNonTradePeriods.txt");
        }

        private static string GetSettingsPath()
        {
            return GetOptimizerSettingsFilePath("Settings.txt");
        }

        private static string GetOptimizerSettingsFilePath(string suffix)
        {
            return @"Engine\Optimizer" + suffix;
        }

        public event Action<string, LogMessageType>? LogMessageEvent;

        private static int ClampPositiveInt(int value)
        {
            return value < 1 ? 1 : value;
        }

        private static decimal ClampNonNegativeDecimal(decimal value)
        {
            return value < 0 ? 0 : value;
        }

        private static int ClampTailSharePercent(int value)
        {
            if (value < 1)
            {
                return 1;
            }

            if (value > 50)
            {
                return 50;
            }

            return value;
        }

        private static bool TryParseDefinedEnum<TEnum>(string? value, out TEnum result)
            where TEnum : struct, Enum
        {
            if (Enum.TryParse(value, out result) && Enum.IsDefined(typeof(TEnum), result))
            {
                return true;
            }

            result = default;
            return false;
        }

        private static bool TryParseInt(string? value, out int result)
        {
            return int.TryParse(value, out result);
        }

        private static bool TryParseBool(string? value, out bool result)
        {
            return bool.TryParse(value, out result);
        }

        private static bool TryParseDecimal(string? value, out decimal result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0;
                return false;
            }

            return decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out result)
                || decimal.TryParse(
                    value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out result);
        }

        #endregion
    }

    public enum OptimizationMethodType
    {
        BruteForce,
        Bayesian
    }

    public enum ObjectiveDirectionType
    {
        Maximize,
        Minimize
    }

    public enum BayesianAcquisitionModeType
    {
        Ucb,
        ExpectedImprovement,
        Greedy
    }
}
