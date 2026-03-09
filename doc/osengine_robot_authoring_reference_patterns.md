# OsEngine Robot Authoring Reference Patterns

Curated excerpts extracted on 2026-03-09 from the temporary local set `D:\001`.

Use this document as a stable local reference for the `osengine-robot-authoring` skill.

Rules for using these excerpts:

- Treat them as patterns and boundary examples, not as mandatory copy-paste templates.
- Prefer the guidance in `SKILL.md`; use this file only when a task clearly needs a non-standard robot shape.
- Some source snippets intentionally show legacy or compromise decisions. Reuse the idea only after checking it against current project rules.

## Multi-Tab And Screener Orchestration

Source files:

- `D:\001\ArbitrageQuoterScreenerNew.cs`
- `D:\001\LordOfTheImpulse.cs`
- `D:\001\ArbitrageBitGet.cs`

Pattern: fixed-role tabs plus screener-level sync pass before per-security execution.

```csharp
// ArbitrageQuoterScreenerNew: fixed set of screener buckets + custom UI manager
TabCreate(BotTabType.Screener);
_tab0 = TabsScreener[0];
TabCreate(BotTabType.Screener);
_tab1 = TabsScreener[1];
TabCreate(BotTabType.Screener);
_tab2 = TabsScreener[2];
TabCreate(BotTabType.Screener);
_tab3 = TabsScreener[3];

this.ParamGuiSettings.Title = "Arbitrage Quoter Screener";
CustomTabToParametersUi customTabOpen = ParamGuiSettings.CreateCustomTab(" Торговля ");
CreateTableOpen();
customTabOpen.AddChildren(_hostTableOpen);

for (int i = 0; i < TabsScreener.Count; i++)
{
    TabsScreener[i].NewTabCreateEvent += ArbitrageQuoterScreener_NewTabCreateEvent;
}
```

```csharp
// LordOfTheImpulse: screener sync event split into ranking pass and per-security logic pass
private void _screener_CandlesSyncFinishedEvent(List<BotTabSimple> sources)
{
    for (int i = 0; i < sources.Count; i++)
    {
        SetValueInRanking1Min(sources[i].CandlesFinishedOnly, sources[i]);
    }

    CalculateBollingerIndexes();

    for (int i = 0; i < sources.Count; i++)
    {
        _screener_CandleFinishedEvent(sources[i].CandlesFinishedOnly, sources[i]);
    }
}
```

Use this pattern when:

- one robot coordinates multiple buckets, legs, or exchanges;
- ranking/filter state must be built across the whole screener before making per-symbol decisions;
- tabs have explicit roles and are not interchangeable.

## Custom Parameter UI And Operator Dashboards

Source files:

- `D:\001\ArbitrageBitGet.cs`
- `D:\001\GridBot.cs`
- `D:\001\ArbitrageQuoterScreenerNew.cs`

Pattern: `ParamGuiSettings.CreateCustomTab(...)` plus `WindowsFormsHost` for dashboards, grids, and operator controls.

```csharp
// ArbitrageBitGet: custom monitoring and grid tabs in robot settings
this.ParamGuiSettings.Title = "ArbitrageBitGet";
this.ParamGuiSettings.Height = 400;
this.ParamGuiSettings.Width = 400;

string tabName = " Параметры ";

_setRatioForBuy = CreateParameter("Соотношение стоимости активов для выставления ордеров на покупку", 0m, 0m, 0m, 0m, tabName);
_setRatioForStop = CreateParameter("Соотношение стоимости активов для остановки бота", 0m, 0m, 0m, 0m, tabName);

CustomTabToParametersUi customTabMonitoring = ParamGuiSettings.CreateCustomTab(" Мониторинг ");
CreateTableMonitoring();
customTabMonitoring.AddChildren(_hostMonitoring);

CustomTabToParametersUi customTabGrid = ParamGuiSettings.CreateCustomTab(" Сетка ордеров ");
CreateTableGrid();
customTabGrid.AddChildren(_hostGrid);
LoadTableGrid();
```

```csharp
// GridBot: compose operator UI from buttons + editable grid
private void CreateTableGrid()
{
    _hostTableGrid = new WindowsFormsHost();

    _gridButtonGrid = AddManualGridButton();
    _gridTableGrid = AddManualGridTable();

    TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
    tableLayoutPanel.Dock = DockStyle.Fill;
    tableLayoutPanel.ColumnCount = 1;
    tableLayoutPanel.RowCount = 2;
    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

    tableLayoutPanel.Controls.Add(_gridButtonGrid, 0, 0);
    tableLayoutPanel.Controls.Add(_gridTableGrid, 0, 1);

    _gridTableGrid.CellClick += GridTableGrid_CellClick;
    _gridTableGrid.CellValueChanged += GridTableGrid_CellValueChanged;
    _gridTableGrid.DataError += _gridTableGrid_DataError;

    _hostTableGrid.Child = tableLayoutPanel;
}
```

Use this pattern when:

- standard parameter tabs cannot express the workflow;
- the robot needs an operator-facing matrix, recovery table, or manual grid editor;
- UI state must be saved and restored independently from trading state.

## External Connector Lifecycle

Source files:

- `D:\001\ReverseAdaptivePriceChannelPlus_2.cs`
- `D:\001\CustomConnectors\Coinglass\CoinglassConnector.cs`
- `D:\001\Ver2_Without_api_key_MemoryOpt\ReverseAdaptivePriceChannelPlus.cs`

Pattern: keep HTTP/rate-limit/queue logic in a dedicated connector; let the robot own only subscription lifecycle and request wiring.

```csharp
// CoinglassConnector: queue + rate gate + background worker
private static CoinglassConnector _server;

private readonly HttpClient _httpClient;
private static string _apikey;
private readonly ConcurrentQueue<RequestContent> _requestsQueue = new ConcurrentQueue<RequestContent>();
private readonly RateGate _rateGateGetData = new RateGate(30, TimeSpan.FromSeconds(60));

public static CoinglassConnector GetServer(string apiKey)
{
    if (_server != null) return _server;
    _server = new CoinglassConnector(apiKey);
    _apikey = apiKey;
    return _server;
}

private CoinglassConnector(string apiKey)
{
    _httpClient = new HttpClient();
    _httpClient.DefaultRequestHeaders.Accept.Clear();
    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    _httpClient.DefaultRequestHeaders.Add("CG-API-KEY", apiKey);

    Thread worker = new Thread(PullRequests);
    worker.CurrentCulture = new CultureInfo("ru-RU");
    worker.IsBackground = true;
    worker.Start();
}
```

```csharp
// ReverseAdaptivePriceChannelPlus_2: lazy connector init in robot lifecycle
if (_conn == null)
{
    _conn = CoinglassConnector.GetServer(ApiKey.ValueString);
    _conn.CoinglassUpdateEvent += CoinglassUpdateEvent;

    if (StartProgram == StartProgram.IsOsOptimizer)
    {
        OptimizerServer server = (OptimizerServer)_tab.Connector.MyServer;
        server.TestingEndEvent += Server_TestingEndEvent;
    }
}

if (LongShortRatioFilterIsOn.ValueBool)
{
    _requestContent.Exchange = ExchageForLongShort.ValueString;
    _requestContent.Interval = MinIntervalTimeFrame.ValueString;
    _requestContent.BotName = _tab.TabName;
    _requestContent.ResponseType = ResponseType.LongShortRatio;
    _requestContent.Symbol = _tab.Securiti.Name;
}
```

Use this pattern when:

- the strategy consumes analytics or service data from a non-exchange source;
- live mode should remain event-driven, while tester/optimizer may need cached or preloaded service data;
- rate limiting and transport concerns would otherwise pollute the robot class.

## Recovery Worker And Stateful Orchestration

Source files:

- `D:\001\GridBot.cs`
- `D:\001\CrossTwoEMA.cs`
- `D:\001\TmonScalp.cs`

Pattern: background worker is acceptable when the robot must reconcile open orders, recovery state, or throttled quote-driven actions.

```csharp
// GridBot: restore state and choose recovery path after restart
private void AutoStart()
{
    RuntimeState savedState = LoadRuntimeState();
    bool wasWorkBeforeRestart = savedState != null && savedState.WasWork;
    bool hasOpenPosition = _tab.PositionOpenLong.Count > 0 || _tab.PositionOpenShort.Count > 0;

    if (!hasOpenPosition && !wasWorkBeforeRestart)
    {
        SendNewLogMessage("Автостарт пропущен: нет открытых позиций и сохраненного рабочего состояния.", Logging.LogMessageType.User);
        return;
    }

    SetFalseAllFlags();
    LoadRecoveryGrid();

    if (savedState != null)
    {
        ApplyRuntimeState(savedState);
    }

    SyncActiveOrdersForRecovery();
}
```

```csharp
// GridBot: separate RSI block state from entry/exit logic
private void SyncRsiBlockState()
{
    if (_rsiOnOff.ValueString == GetDescription(RsiRegime.Off))
    {
        if (_rsiBlocked)
        {
            _rsiBlocked = false;
            ResumeAfterRsiBlock();
        }

        return;
    }

    bool rsiAllowed = CheckRSI();

    if (!rsiAllowed && !_rsiBlocked)
    {
        _rsiBlocked = true;
        EnterRsiBlock();
    }
    else if (rsiAllowed && _rsiBlocked)
    {
        _rsiBlocked = false;
        ResumeAfterRsiBlock();
    }
}
```

```csharp
// CrossTwoEMA: background worker for stop/profit/order supervision
private void MainThread()
{
    while (true)
    {
        try
        {
            if (_startProgram == StartProgram.IsOsTrader)
            {
                Thread.Sleep(1000);
            }
            else
            {
                Thread.Sleep(100);
            }

            if (_tab == null || _tab.Security == null)
            {
                continue;
            }

            if (_regime == "Off")
            {
                WithdrawOrders();
                continue;
            }

            LogicLongPosition();
            LogicShortPosition();
        }
        catch (Exception ex)
        {
            SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error);
            Thread.Sleep(5000);
        }
    }
}
```

Use this pattern when:

- the robot must restore or reconcile state after restart;
- quote-driven throttling does not fit a pure candle event model;
- service logic is separate from signal math and has clear shutdown ownership.

## Helper Trade Mechanics

Source files:

- `D:\001\ParabolicSarClassicTrade.cs`
- `D:\001\Classes\TrailingStop.cs`

Pattern: move reusable order-management mechanics into helper classes when the helper has a stable interface and at least one real robot integration.

```csharp
// ParabolicSarClassicTrade: helper wiring through robot parameters
TrailingStopIsOn = CreateParameter("Is Trailing stop On", false, "Trailing Stop");
TrailingStopTypeOrder = CreateParameter("Type order", OrderPriceType.Market.ToString(), new[] { OrderPriceType.Market.ToString(), OrderPriceType.Limit.ToString() }, "Trailing Stop");
PointOrPercent = CreateParameter("Choise Points or Percent", "Points", new[] { "Points", "Percent" }, "Trailing Stop");
ChangeStepStop = CreateParameter("Stop level change step", 1, 1, 10000, 001m, "Trailing Stop");
MinDist = CreateParameter("Minimum distance to price", 1, 1, 10000, 0.01m, "Trailing Stop");
QuantityStepsPrices = CreateParameter("Quantity steps prices for limit order", 0m, 0, 10000, 1, "Trailing Stop");

_trailingStop = new TrailingStop(
    _tab,
    TrailingStopTypeOrder.ValueString,
    ChangeStepStop.ValueDecimal,
    MinDist.ValueDecimal,
    QuantityStepsPrices.ValueDecimal,
    PointOrPercent.ValueString);
```

```csharp
// TrailingStop helper: encapsulate activation/order-price math
public void SetTrailingStop(decimal lastPrice)
{
    for (int i = 0; i < _tab.PositionsOpenAll.Count; i++)
    {
        Position pos = _tab.PositionsOpenAll[i];

        decimal minDist = GetMinDist(pos);
        decimal stepStop = GetStepStop(pos);
        decimal slippageOrder = _tab.Securiti.PriceStep * _quantityStepsPrices;

        if (pos.State != PositionStateType.Open)
        {
            continue;
        }

        if (_orderType == "Limit")
        {
            _tab.CloseAtTrailingStop(_tab.PositionsOpenAll[i], priceActivation, priceOrder);
        }
        else if (_orderType == "Market")
        {
            _tab.CloseAtTrailingStopMarket(_tab.PositionsOpenAll[i], priceActivation);
        }
    }
}
```

Use this pattern when:

- stop/trailing/recovery logic repeats across robots;
- the helper can depend only on `BotTabSimple`, stable parameters, and entity types;
- the robot remains responsible for lifecycle and parameter changes.

## Advanced Volume And Risk Sizing

Source files:

- `D:\001\CrossTwoEMA.cs`
- `D:\001\TmonScalp.cs`
- `D:\001\LordOfTheImpulse.cs`

Pattern: explicit volume modes plus risk-aware sizing from stop distance or selected portfolio asset.

```csharp
// CrossTwoEMA: risk per trade from known stop-loss distance
private decimal GetVolume()
{
    Portfolio myPortfolio = _tab.Portfolio;

    decimal moneyOnPosition = myPortfolio.ValueCurrent * (_riskDeal.ValueDecimal / 100);
    decimal qty = moneyOnPosition / Math.Abs(_tab.PriceBestAsk - _valueSL) / _tab.Security.Lot;

    if (_tab.Security.UsePriceStepCostToCalculateVolume == true
        && _tab.Security.PriceStep != _tab.Security.PriceStepCost
        && _tab.PriceBestAsk != 0
        && _tab.Security.PriceStep != 0
        && _tab.Security.PriceStepCost != 0)
    {
        qty = moneyOnPosition / (Math.Abs(_tab.PriceBestAsk - _valueSL) / _tab.Security.PriceStep * _tab.Security.PriceStepCost);
    }

    qty = Math.Round(qty, _tab.Security.DecimalsVolume);
    return qty;
}
```

```csharp
// TmonScalp: deposit-percent sizing from a chosen portfolio asset
case "Deposit percent":
{
    var myPortfolio = tab.Portfolio;
    decimal portfolioPrimeAsset = 0;

    if (_tradeAssetInPortfolio.ValueString == "Prime")
    {
        portfolioPrimeAsset = myPortfolio.ValueCurrent;
    }
    else
    {
        var positionOnBoard = myPortfolio.GetPositionOnBoard();

        for (int i = 0; i < positionOnBoard.Count; i++)
        {
            if (positionOnBoard[i].SecurityNameCode == _tradeAssetInPortfolio.ValueString)
            {
                portfolioPrimeAsset = positionOnBoard[i].ValueCurrent;
                break;
            }
        }
    }

    var moneyOnPosition = portfolioPrimeAsset * (_volume.ValueDecimal / 100);
    var qty = moneyOnPosition / tab.PriceBestAsk / tab.Security.Lot;
    return Math.Round(qty, tab.Security.DecimalsVolume);
}
```

Use this pattern when:

- position size depends on stop distance, not only notional size;
- the portfolio base asset is configurable;
- live mode needs exchange-specific lot and step-cost normalization.

## Built-In Multi-Asset Outliers

Source files:

- `project/OsEngine/bin/Debug/Custom/Robots/PriceChannelScreener.cs`
- `project/OsEngine/bin/Debug/Custom/Robots/SimpleArbitrage.cs`
- `project/OsEngine/bin/Debug/Custom/Robots/ClusterTrend.cs`

These are the small set of shipped custom robots that add genuinely different tab topologies compared with the majority of `Custom/Robots` scripts.

```csharp
// PriceChannelScreener: screener-managed per-security trading with indicator bootstrap
TabCreate(BotTabType.Screener);
_screenerTab = TabsScreener[0];

_screenerTab.NewTabCreateEvent += ScreenerNewTabCreateEvent;
_screenerTab.CandleFinishedEvent += ScreenerTabCandleFinishedEvent;
_screenerTab.PositionOpeningSuccesEvent += ScreenerPositionOpeningSuccesEvent;

List<string> indicatorParams = new List<string>()
{ _upLine.ValueInt.ToString(), _downLine.ValueInt.ToString() };

_screenerTab.CreateCandleIndicator(1, "PriceChannel", indicatorParams, "Prime");
```

```csharp
// SimpleArbitrage: index spread tab plus two execution legs
TabCreate(BotTabType.Index);
_index = TabsIndex.Last();

TabCreate(BotTabType.Simple);
_firstLeg = TabsSimple.Last();

TabCreate(BotTabType.Simple);
_secondLeg = TabsSimple.Last();

_dayMiddle = IndicatorsFactory.CreateIndicatorByName("LastDayMiddle", name + "LastDayMiddle", false);
_dayMiddle.ParametersDigit[0].Value = _deviation.ValueDecimal;
_dayMiddle = (Aindicator)_index.CreateCandleIndicator(_dayMiddle, "Prime");

_index.SpreadChangeEvent += IndexSpreadChangeEvent;
```

```csharp
// ClusterTrend: cluster-analysis tab drives a separate execution tab
TabCreate(BotTabType.Cluster);
_tabCluster = TabsCluster.Last();

TabCreate(BotTabType.Simple);
_tabToTrade = TabsSimple.Last();

_tabCluster.MaxBuyClusterChangeEvent += TabClusterMaxBuyClusterChangeEvent;
_tabToTrade.CandleFinishedEvent += TabToTradeCandleFinishedEvent;
```

Use these patterns when:

- a robot needs a non-`Simple` analytical tab (`Screener`, `Index`, `Cluster`) plus one or more execution tabs;
- the traded leg is not the same tab that computes the primary signal;
- the majority single-tab custom robot template is no longer expressive enough.

## Notification And Connectivity Side Effects

Source file:

- `project/OsEngine/bin/Debug/Custom/Robots/TelegramSample.cs`

This is a useful reference for optional side effects, but it is also a legacy-style example. Reuse the lifecycle idea, not every implementation detail.

```csharp
// TelegramSample: side effects are gated by mode and separated from signal logic
_tab.CandleFinishedEvent += _tab_CandleFinishedEvent;
_tab.PositionOpeningSuccesEvent += _tab_PositionOpeningSuccesEvent;
_tab.PositionClosingSuccesEvent += _tab_PositionClosingSuccesEvent;

if (startProgram == StartProgram.IsOsTrader)
{
    Thread worker = new Thread(CheckConnect);
    worker.IsBackground = true;
    worker.Start();
}
```

```csharp
private void _tab_PositionOpeningSuccesEvent(Position position)
{
    if (_alertsRegime.ValueString == "On")
    {
        string message = "Open long position (" + _tab.NameStrategy + ")" + "\r\n"
            + "By price: " + position.EntryPrice + "\r\n"
            + "Volume: " + position.OpenVolume;
        SendTelegramMessageAsync(message);
    }
}
```

Use this pattern when:

- notifications are optional and must not be mixed into the signal calculation path;
- background connectivity monitoring should exist only in live mode;
- entry/exit notifications can be driven from position lifecycle events rather than from polling.

## Manual Calculation And Explicit Cleanup

Source files:

- `project/OsEngine/bin/Debug/Custom/Robots/ImpulsV1.cs`
- `project/OsEngine/bin/Debug/Custom/Robots/FractalAndCciNewFormat2.cs`

These are stronger references than many indicator-based scripts when the task explicitly calls for manual math, cached series, and deterministic event cleanup.

```csharp
// ImpulsV1: manual buffers + delete-safe lifecycle
private readonly BotTabSimple _tab;
private readonly List<decimal> _momentumValues = new List<decimal>();
private readonly List<decimal> _secondMomentumValues = new List<decimal>();
private readonly List<decimal> _trueRangeValues = new List<decimal>();
private readonly List<decimal> _atrValues = new List<decimal>();
private bool _isDeleted;

_tab.CandleFinishedEvent += OnCandleFinished;
DeleteEvent += OnDelete;
```

```csharp
private void OnDelete()
{
    if (_isDeleted)
    {
        return;
    }

    _isDeleted = true;

    try
    {
        _tab.CandleFinishedEvent -= OnCandleFinished;
    }
    catch
    {
        // Tab can already be disposed during bot removal.
    }

    DeleteEvent -= OnDelete;
    ClearCalculatedValues();
}
```

```csharp
// FractalAndCciNewFormat2: manual signals without indicator objects
/*
The trend robot on Fractal And CCI without indicators.
Signal calculations are manual (approach 2) to avoid indicator object lifecycle overhead
and reduce extra per-candle allocations in hot path.
*/

_tab.CandleFinishedEvent += OnCandleFinished;
DeleteEvent += OnDelete;
```

Use these patterns when:

- the user explicitly wants approach `2` (manual calculation) or perf/allocation reasons justify it;
- cached arrays/lists are simpler than managing indicator object lifecycle;
- the robot must be explicit about event unsubscription and deleted-state guards.

## Variant And Fork Notes

Source files:

- `D:\001\ReverseAdaptivePriceChannelPlus_2.cs`
- `D:\001\Ver2_Without_api_key_MemoryOpt\ReverseAdaptivePriceChannelPlus.cs`

Pattern: keep a fork only when the infrastructure profile differs in a meaningful way.

```csharp
// _2 variant adds explicit reverse switch
ReverseLogic = CreateParameter("Reverse logic", false, "Base");
```

```csharp
// memory-opt / without-api-key variant keeps the same base trade shape
Regime = CreateParameter("Regime", "Off", new[] { "Off", "On", "OnlyLong", "OnlyShort", "OnlyClosePosition" }, "Base");
VolumeRegime = CreateParameter("Volume type", "Number of contracts", new[] { "Number of contracts", "Contract currency", "% of the total portfolio" }, "Base");
LongShortRatioFilterIsOn = CreateParameter("Is Long Short Ratio Filter On", false, "Filters");
```

When comparing or creating forks, document:

- what part of the trading logic remains invariant;
- what changed in infrastructure, memory behavior, external data dependency, or UX;
- why this is still one strategy family rather than two unrelated robots.
