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
                OptimizerClearingsSettingsDto settings = new OptimizerClearingsSettingsDto
                {
                    Items = new List<OrderClearingDto>()
                };

                for (int i = 0; i < ClearingTimes.Count; i++)
                {
                    settings.Items.Add(new OrderClearingDto
                    {
                        Time = ClearingTimes[i].Time,
                        IsOn = ClearingTimes[i].IsOn
                    });
                }

                SettingsManager.Save(GetClearingsPath(), settings);
            }
            catch (Exception ex)
            {
                LogMessageEvent?.Invoke(ex.ToString(), LogMessageType.Error);
            }
        }

        private void LoadClearingInfo()
        {
            string path = GetClearingsPath();
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                OptimizerClearingsSettingsDto? settings = SettingsManager.Load(
                    path,
                    defaultValue: null as OptimizerClearingsSettingsDto,
                    legacyLoader: ParseLegacyClearingsContent);

                if (settings?.Items == null)
                {
                    return;
                }

                ClearingTimes.Clear();
                for (int i = 0; i < settings.Items.Count; i++)
                {
                    OrderClearingDto item = settings.Items[i];
                    if (item == null)
                    {
                        continue;
                    }

                    ClearingTimes.Add(new OrderClearing
                    {
                        Time = item.Time,
                        IsOn = item.IsOn
                    });
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
                OptimizerNonTradePeriodsSettingsDto settings = new OptimizerNonTradePeriodsSettingsDto
                {
                    Items = new List<NonTradePeriodDto>()
                };

                for (int i = 0; i < NonTradePeriods.Count; i++)
                {
                    settings.Items.Add(new NonTradePeriodDto
                    {
                        DateStart = NonTradePeriods[i].DateStart,
                        DateEnd = NonTradePeriods[i].DateEnd,
                        IsOn = NonTradePeriods[i].IsOn
                    });
                }

                SettingsManager.Save(GetNonTradePeriodsPath(), settings);
            }
            catch (Exception ex)
            {
                LogMessageEvent?.Invoke(ex.ToString(), LogMessageType.Error);
            }
        }

        private void LoadNonTradePeriods()
        {
            string path = GetNonTradePeriodsPath();
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                OptimizerNonTradePeriodsSettingsDto? settings = SettingsManager.Load(
                    path,
                    defaultValue: null as OptimizerNonTradePeriodsSettingsDto,
                    legacyLoader: ParseLegacyNonTradePeriodsContent);

                if (settings?.Items == null)
                {
                    return;
                }

                NonTradePeriods.Clear();
                for (int i = 0; i < settings.Items.Count; i++)
                {
                    NonTradePeriodDto item = settings.Items[i];
                    if (item == null)
                    {
                        continue;
                    }

                    NonTradePeriods.Add(new NonTradePeriod
                    {
                        DateStart = item.DateStart,
                        DateEnd = item.DateEnd,
                        IsOn = item.IsOn
                    });
                }
            }
            catch (Exception ex)
            {
                LogMessageEvent?.Invoke(ex.ToString(), LogMessageType.Error);
            }
        }

        private static OptimizerClearingsSettingsDto ParseLegacyClearingsContent(string content)
        {
            OptimizerClearingsSettingsDto settings = new OptimizerClearingsSettingsDto
            {
                Items = new List<OrderClearingDto>()
            };

            string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                OrderClearing clearing = new OrderClearing();
                clearing.SetFromString(line);

                settings.Items.Add(new OrderClearingDto
                {
                    Time = clearing.Time,
                    IsOn = clearing.IsOn
                });
            }

            return settings;
        }

        private static OptimizerNonTradePeriodsSettingsDto ParseLegacyNonTradePeriodsContent(string content)
        {
            OptimizerNonTradePeriodsSettingsDto settings = new OptimizerNonTradePeriodsSettingsDto
            {
                Items = new List<NonTradePeriodDto>()
            };

            string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                NonTradePeriod period = new NonTradePeriod();
                period.SetFromString(line);

                settings.Items.Add(new NonTradePeriodDto
                {
                    DateStart = period.DateStart,
                    DateEnd = period.DateEnd,
                    IsOn = period.IsOn
                });
            }

            return settings;
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

        public bool UseIndicatorCache
        {
            get => _useIndicatorCache;
            set
            {
                _useIndicatorCache = value;
                Save();
            }
        }
        private bool _useIndicatorCache = true;

        #endregion

        #region Save / Load

        private void Save()
        {
            try
            {
                SettingsManager.Save(GetSettingsPath(), BuildSettingsDto());
            }
            catch (Exception error)
            {
                LogMessageEvent?.Invoke(error.ToString(), LogMessageType.Error);
            }
        }

        private void Load()
        {
            string path = GetSettingsPath();

            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                string content = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return;
                }

                if (LooksLikeJson(content))
                {
                    OptimizerSettingsDto? settings = SettingsManager.Load<OptimizerSettingsDto?>(
                        path,
                        null);

                    if (settings != null)
                    {
                        ApplySettingsDto(settings);
                        return;
                    }
                }

                LoadLegacy(content);
            }
            catch (Exception ex)
            {
                LogMessageEvent?.Invoke(ex.ToString(), LogMessageType.Error);
            }
        }

        private OptimizerSettingsDto BuildSettingsDto()
        {
            return new OptimizerSettingsDto
            {
                ThreadsCount = _threadsCount,
                StrategyName = _strategyName,
                IsScript = _isScript,
                StartDeposit = _startDeposit,
                OrderExecutionType = _orderExecutionType,
                SlippageToSimpleOrder = _slippageToSimpleOrder,
                SlippageToStopOrder = _slippageToStopOrder,
                CommissionType = _commissionType,
                CommissionValue = _commissionValue,
                FilterProfitValue = _filterProfitValue,
                FilterProfitIsOn = _filterProfitIsOn,
                FilterMaxDrawDownValue = _filterMaxDrawDownValue,
                FilterMaxDrawDownIsOn = _filterMaxDrawDownIsOn,
                FilterMiddleProfitValue = _filterMiddleProfitValue,
                FilterMiddleProfitIsOn = _filterMiddleProfitIsOn,
                FilterProfitFactorValue = _filterProfitFactorValue,
                FilterProfitFactorIsOn = _filterProfitFactorIsOn,
                FilterDealsCountValue = _filterDealsCountValue,
                FilterDealsCountIsOn = _filterDealsCountIsOn,
                TimeStart = _timeStart,
                TimeEnd = _timeEnd,
                PercentOnFiltration = _percentOnFiltration,
                IterationCount = _iterationCount,
                LastInSample = _lastInSample,
                OptimizationMethod = _optimizationMethod,
                ObjectiveMetric = _objectiveMetric,
                BayesianInitialSamples = _bayesianInitialSamples,
                BayesianMaxIterations = _bayesianMaxIterations,
                BayesianBatchSize = _bayesianBatchSize,
                ObjectiveDirection = _objectiveDirection,
                BayesianAcquisitionMode = _bayesianAcquisitionMode,
                BayesianAcquisitionKappa = _bayesianAcquisitionKappa,
                BayesianUseTailPass = _bayesianUseTailPass,
                BayesianTailSharePercent = _bayesianTailSharePercent,
                UseIndicatorCache = _useIndicatorCache
            };
        }

        private void ApplySettingsDto(OptimizerSettingsDto dto)
        {
            if (dto.ThreadsCount.HasValue)
            {
                _threadsCount = dto.ThreadsCount.Value;
            }

            if (dto.StrategyName != null)
            {
                _strategyName = dto.StrategyName;
            }

            if (dto.IsScript.HasValue)
            {
                _isScript = dto.IsScript.Value;
            }

            if (dto.StartDeposit.HasValue)
            {
                _startDeposit = dto.StartDeposit.Value;
            }

            if (dto.OrderExecutionType.HasValue
                && Enum.IsDefined(typeof(OrderExecutionType), dto.OrderExecutionType.Value))
            {
                _orderExecutionType = dto.OrderExecutionType.Value;
            }

            if (dto.SlippageToSimpleOrder.HasValue)
            {
                _slippageToSimpleOrder = dto.SlippageToSimpleOrder.Value;
            }

            if (dto.SlippageToStopOrder.HasValue)
            {
                _slippageToStopOrder = dto.SlippageToStopOrder.Value;
            }

            if (dto.CommissionType.HasValue
                && Enum.IsDefined(typeof(CommissionType), dto.CommissionType.Value))
            {
                _commissionType = dto.CommissionType.Value;
            }

            if (dto.CommissionValue.HasValue)
            {
                _commissionValue = dto.CommissionValue.Value;
            }

            if (dto.FilterProfitValue.HasValue)
            {
                _filterProfitValue = dto.FilterProfitValue.Value;
            }

            if (dto.FilterProfitIsOn.HasValue)
            {
                _filterProfitIsOn = dto.FilterProfitIsOn.Value;
            }

            if (dto.FilterMaxDrawDownValue.HasValue)
            {
                _filterMaxDrawDownValue = dto.FilterMaxDrawDownValue.Value;
            }

            if (dto.FilterMaxDrawDownIsOn.HasValue)
            {
                _filterMaxDrawDownIsOn = dto.FilterMaxDrawDownIsOn.Value;
            }

            if (dto.FilterMiddleProfitValue.HasValue)
            {
                _filterMiddleProfitValue = dto.FilterMiddleProfitValue.Value;
            }

            if (dto.FilterMiddleProfitIsOn.HasValue)
            {
                _filterMiddleProfitIsOn = dto.FilterMiddleProfitIsOn.Value;
            }

            if (dto.FilterProfitFactorValue.HasValue)
            {
                _filterProfitFactorValue = dto.FilterProfitFactorValue.Value;
            }

            if (dto.FilterProfitFactorIsOn.HasValue)
            {
                _filterProfitFactorIsOn = dto.FilterProfitFactorIsOn.Value;
            }

            if (dto.FilterDealsCountValue.HasValue)
            {
                _filterDealsCountValue = dto.FilterDealsCountValue.Value;
            }

            if (dto.FilterDealsCountIsOn.HasValue)
            {
                _filterDealsCountIsOn = dto.FilterDealsCountIsOn.Value;
            }

            if (dto.TimeStart.HasValue)
            {
                _timeStart = dto.TimeStart.Value;
            }

            if (dto.TimeEnd.HasValue)
            {
                _timeEnd = dto.TimeEnd.Value;
            }

            if (dto.PercentOnFiltration.HasValue)
            {
                _percentOnFiltration = dto.PercentOnFiltration.Value;
            }

            if (dto.IterationCount.HasValue)
            {
                _iterationCount = ClampPositiveInt(dto.IterationCount.Value);
            }

            if (dto.LastInSample.HasValue)
            {
                _lastInSample = dto.LastInSample.Value;
            }

            if (dto.OptimizationMethod.HasValue
                && Enum.IsDefined(typeof(OptimizationMethodType), dto.OptimizationMethod.Value))
            {
                _optimizationMethod = dto.OptimizationMethod.Value;
            }

            if (dto.ObjectiveMetric.HasValue
                && Enum.IsDefined(typeof(SortBotsType), dto.ObjectiveMetric.Value))
            {
                _objectiveMetric = dto.ObjectiveMetric.Value;
            }

            if (dto.BayesianInitialSamples.HasValue)
            {
                _bayesianInitialSamples = ClampPositiveInt(dto.BayesianInitialSamples.Value);
            }

            if (dto.BayesianMaxIterations.HasValue)
            {
                _bayesianMaxIterations = ClampPositiveInt(dto.BayesianMaxIterations.Value);
            }

            if (dto.BayesianBatchSize.HasValue)
            {
                _bayesianBatchSize = ClampPositiveInt(dto.BayesianBatchSize.Value);
            }

            if (dto.ObjectiveDirection.HasValue
                && Enum.IsDefined(typeof(ObjectiveDirectionType), dto.ObjectiveDirection.Value))
            {
                _objectiveDirection = dto.ObjectiveDirection.Value;
            }

            if (dto.BayesianAcquisitionMode.HasValue
                && Enum.IsDefined(typeof(BayesianAcquisitionModeType), dto.BayesianAcquisitionMode.Value))
            {
                _bayesianAcquisitionMode = dto.BayesianAcquisitionMode.Value;
            }

            if (dto.BayesianAcquisitionKappa.HasValue)
            {
                _bayesianAcquisitionKappa = ClampNonNegativeDecimal(dto.BayesianAcquisitionKappa.Value);
            }

            if (dto.BayesianUseTailPass.HasValue)
            {
                _bayesianUseTailPass = dto.BayesianUseTailPass.Value;
            }

            if (dto.BayesianTailSharePercent.HasValue)
            {
                _bayesianTailSharePercent = ClampTailSharePercent(dto.BayesianTailSharePercent.Value);
            }

            if (dto.UseIndicatorCache.HasValue)
            {
                _useIndicatorCache = dto.UseIndicatorCache.Value;
            }
        }

        private void LoadLegacy(string content)
        {
            string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int i = 0;

            _threadsCount = Convert.ToInt32(ReadLegacyLine(lines, ref i) ?? "0", CultureInfo.InvariantCulture);
            _strategyName = ReadLegacyLine(lines, ref i) ?? string.Empty;
            _startDeposit = (ReadLegacyLine(lines, ref i) ?? "0").ToDecimal();
            _filterProfitValue = (ReadLegacyLine(lines, ref i) ?? "0").ToDecimal();
            _filterProfitIsOn = Convert.ToBoolean(ReadLegacyLine(lines, ref i) ?? bool.FalseString);
            _filterMaxDrawDownValue = (ReadLegacyLine(lines, ref i) ?? "0").ToDecimal();
            _filterMaxDrawDownIsOn = Convert.ToBoolean(ReadLegacyLine(lines, ref i) ?? bool.FalseString);
            _filterMiddleProfitValue = (ReadLegacyLine(lines, ref i) ?? "0").ToDecimal();
            _filterMiddleProfitIsOn = Convert.ToBoolean(ReadLegacyLine(lines, ref i) ?? bool.FalseString);
            _filterProfitFactorValue = (ReadLegacyLine(lines, ref i) ?? "0").ToDecimal();
            _filterProfitFactorIsOn = Convert.ToBoolean(ReadLegacyLine(lines, ref i) ?? bool.FalseString);

            _timeStart = ParseDateInvariantOrCurrent(ReadLegacyLine(lines, ref i));
            _timeEnd = ParseDateInvariantOrCurrent(ReadLegacyLine(lines, ref i));
            _percentOnFiltration = (ReadLegacyLine(lines, ref i) ?? "0").ToDecimal();

            _filterDealsCountValue = Convert.ToInt32(ReadLegacyLine(lines, ref i) ?? "0", CultureInfo.InvariantCulture);
            _filterDealsCountIsOn = Convert.ToBoolean(ReadLegacyLine(lines, ref i) ?? bool.FalseString);
            _isScript = Convert.ToBoolean(ReadLegacyLine(lines, ref i) ?? bool.FalseString);
            _iterationCount = ClampPositiveInt(Convert.ToInt32(ReadLegacyLine(lines, ref i) ?? "1", CultureInfo.InvariantCulture));
            _commissionType = (CommissionType)Enum.Parse(typeof(CommissionType),
                ReadLegacyLine(lines, ref i) ?? CommissionType.None.ToString());
            _commissionValue = (ReadLegacyLine(lines, ref i) ?? "0").ToDecimal();
            _lastInSample = Convert.ToBoolean(ReadLegacyLine(lines, ref i) ?? bool.FalseString);

            string? orderExecutionLine = ReadLegacyLine(lines, ref i);
            if (TryParseDefinedEnum(orderExecutionLine, out OrderExecutionType orderExecutionType))
            {
                _orderExecutionType = orderExecutionType;
            }

            _slippageToSimpleOrder = Convert.ToInt32(ReadLegacyLine(lines, ref i) ?? "0", CultureInfo.InvariantCulture);
            _slippageToStopOrder = Convert.ToInt32(ReadLegacyLine(lines, ref i) ?? "0", CultureInfo.InvariantCulture);

            // V2 fields - optional for backward compatibility
            string? line = ReadLegacyLine(lines, ref i);
            if (line != null)
            {
                if (TryParseDefinedEnum(line, out OptimizationMethodType optimizationMethod))
                {
                    _optimizationMethod = optimizationMethod;
                }

                line = ReadLegacyLine(lines, ref i);
                if (line != null && TryParseDefinedEnum(line, out SortBotsType objectiveMetric))
                {
                    _objectiveMetric = objectiveMetric;
                }

                line = ReadLegacyLine(lines, ref i);
                if (TryParseInt(line, out int bayesianInitialSamples))
                {
                    _bayesianInitialSamples = ClampPositiveInt(bayesianInitialSamples);
                }

                line = ReadLegacyLine(lines, ref i);
                if (TryParseInt(line, out int bayesianMaxIterations))
                {
                    _bayesianMaxIterations = ClampPositiveInt(bayesianMaxIterations);
                }

                line = ReadLegacyLine(lines, ref i);
                if (TryParseInt(line, out int bayesianBatchSize))
                {
                    _bayesianBatchSize = ClampPositiveInt(bayesianBatchSize);
                }

                line = ReadLegacyLine(lines, ref i);
                if (line != null && TryParseDefinedEnum(line, out ObjectiveDirectionType objectiveDirection))
                {
                    _objectiveDirection = objectiveDirection;
                }

                line = ReadLegacyLine(lines, ref i);
                if (line != null && TryParseDefinedEnum(line, out BayesianAcquisitionModeType bayesianAcquisitionMode))
                {
                    _bayesianAcquisitionMode = bayesianAcquisitionMode;
                }

                line = ReadLegacyLine(lines, ref i);
                if (TryParseDecimal(line, out decimal bayesianAcquisitionKappa))
                {
                    _bayesianAcquisitionKappa = ClampNonNegativeDecimal(bayesianAcquisitionKappa);
                }

                line = ReadLegacyLine(lines, ref i);
                if (TryParseBool(line, out bool bayesianUseTailPass))
                {
                    _bayesianUseTailPass = bayesianUseTailPass;
                }

                line = ReadLegacyLine(lines, ref i);
                if (TryParseInt(line, out int bayesianTailSharePercent))
                {
                    _bayesianTailSharePercent = ClampTailSharePercent(bayesianTailSharePercent);
                }

                line = ReadLegacyLine(lines, ref i);
                if (TryParseBool(line, out bool useIndicatorCache))
                {
                    _useIndicatorCache = useIndicatorCache;
                }
            }
        }

        private static string? ReadLegacyLine(string[] lines, ref int index)
        {
            if (index >= lines.Length)
            {
                index++;
                return null;
            }

            string line = lines[index];
            index++;
            return line;
        }

        private static bool LooksLikeJson(string content)
        {
            for (int i = 0; i < content.Length; i++)
            {
                if (!char.IsWhiteSpace(content[i]))
                {
                    return content[i] == '{' || content[i] == '[';
                }
            }

            return false;
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
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
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

        private static DateTime ParseDateInvariantOrCurrent(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DateTime.MinValue;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(value, new CultureInfo("ru-RU"), DateTimeStyles.None, out parsed))
            {
                return parsed;
            }

            return DateTime.MinValue;
        }

        private sealed class OptimizerSettingsDto
        {
            public int? ThreadsCount { get; set; }
            public string? StrategyName { get; set; }
            public bool? IsScript { get; set; }
            public decimal? StartDeposit { get; set; }
            public OrderExecutionType? OrderExecutionType { get; set; }
            public int? SlippageToSimpleOrder { get; set; }
            public int? SlippageToStopOrder { get; set; }
            public CommissionType? CommissionType { get; set; }
            public decimal? CommissionValue { get; set; }
            public decimal? FilterProfitValue { get; set; }
            public bool? FilterProfitIsOn { get; set; }
            public decimal? FilterMaxDrawDownValue { get; set; }
            public bool? FilterMaxDrawDownIsOn { get; set; }
            public decimal? FilterMiddleProfitValue { get; set; }
            public bool? FilterMiddleProfitIsOn { get; set; }
            public decimal? FilterProfitFactorValue { get; set; }
            public bool? FilterProfitFactorIsOn { get; set; }
            public int? FilterDealsCountValue { get; set; }
            public bool? FilterDealsCountIsOn { get; set; }
            public DateTime? TimeStart { get; set; }
            public DateTime? TimeEnd { get; set; }
            public decimal? PercentOnFiltration { get; set; }
            public int? IterationCount { get; set; }
            public bool? LastInSample { get; set; }
            public OptimizationMethodType? OptimizationMethod { get; set; }
            public SortBotsType? ObjectiveMetric { get; set; }
            public int? BayesianInitialSamples { get; set; }
            public int? BayesianMaxIterations { get; set; }
            public int? BayesianBatchSize { get; set; }
            public ObjectiveDirectionType? ObjectiveDirection { get; set; }
            public BayesianAcquisitionModeType? BayesianAcquisitionMode { get; set; }
            public decimal? BayesianAcquisitionKappa { get; set; }
            public bool? BayesianUseTailPass { get; set; }
            public int? BayesianTailSharePercent { get; set; }
            public bool? UseIndicatorCache { get; set; }
        }

        private sealed class OptimizerClearingsSettingsDto
        {
            public List<OrderClearingDto>? Items { get; set; }
        }

        private sealed class OrderClearingDto
        {
            public DateTime Time { get; set; }
            public bool IsOn { get; set; }
        }

        private sealed class OptimizerNonTradePeriodsSettingsDto
        {
            public List<NonTradePeriodDto>? Items { get; set; }
        }

        private sealed class NonTradePeriodDto
        {
            public DateTime DateStart { get; set; }
            public DateTime DateEnd { get; set; }
            public bool IsOn { get; set; }
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
