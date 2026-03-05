#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Logging;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace OsEngine.OsTrader.Grids
{
    public class TradeGrid
    {
        private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

        #region Service

        public TradeGrid(StartProgram startProgram, BotTabSimple tab, int number)
        {
            Tab = tab;
            Number = number;

            if (Tab.ManualPositionSupport != null)
            {
                Tab.ManualPositionSupport.DisableManualSupport();
            }

            Tab.NewTickEvent += Tab_NewTickEvent;
            Tab.PositionOpeningSuccesEvent += Tab_PositionOpeningSuccesEvent;
            Tab.PositionClosingSuccesEvent += Tab_PositionClosingSuccesEvent;
            Tab.PositionStopActivateEvent += Tab_PositionStopActivateEvent;
            Tab.Connector.TestStartEvent += Connector_TestStartEvent;

            Tab.PositionOpeningFailEvent += Tab_PositionOpeningFailEvent;
            Tab.PositionClosingFailEvent += Tab_PositionClosingFailEvent;

            StartProgram = startProgram;

            NonTradePeriods = new TradeGridNonTradePeriods(tab.TabName + "Grid" + number);
            NonTradePeriods.LogMessageEvent += SendNewLogMessage;

            StopBy = new TradeGridStopBy();
            StopBy.LogMessageEvent += SendNewLogMessage;

            StopAndProfit = new TradeGridStopAndProfit();
            StopAndProfit.LogMessageEvent += SendNewLogMessage;

            AutoStarter = new TradeGridAutoStarter();
            AutoStarter.LogMessageEvent += SendNewLogMessage;

            GridCreator = new TradeGridCreator();
            GridCreator.LogMessageEvent += SendNewLogMessage;

            ErrorsReaction = new TradeGridErrorsReaction(this);
            ErrorsReaction.LogMessageEvent += SendNewLogMessage;

            TrailingUp = new TrailingUp(this);
            TrailingUp.LogMessageEvent += SendNewLogMessage;

            if (StartProgram == StartProgram.IsOsTrader)
            {
                Thread worker = new Thread(ThreadWorkerPlace);
                worker.Name = "GridThread." + tab.TabName;
                worker.IsBackground = true;
                worker.Start();

                RegimeLogicEntry = TradeGridLogicEntryRegime.OncePerSecond;
                AutoClearJournalIsOn = true;
            }
            else
            {
                RegimeLogicEntry = TradeGridLogicEntryRegime.OnTrade;
                AutoClearJournalIsOn = false;
            }
        }

        public StartProgram StartProgram;

        public int Number;

        public BotTabSimple Tab;

        public TradeGridNonTradePeriods NonTradePeriods;

        public TradeGridStopBy StopBy;

        public TradeGridStopAndProfit StopAndProfit;

        public TradeGridAutoStarter AutoStarter;

        public TradeGridCreator GridCreator;

        public TradeGridErrorsReaction ErrorsReaction;

        public TrailingUp TrailingUp;

        public string GetSaveString()
        {
            TradeGridNonTradePeriods nonTradePeriods = NonTradePeriods;
            TradeGridStopBy stopBy = StopBy;
            TradeGridCreator gridCreator = GridCreator;
            TradeGridStopAndProfit stopAndProfit = StopAndProfit;
            TradeGridAutoStarter autoStarter = AutoStarter;
            TradeGridErrorsReaction errorsReaction = ErrorsReaction;
            TrailingUp trailingUp = TrailingUp;

            string result = "";

            // settings prime

            result += Number.ToString(CultureInfo.InvariantCulture) + "@";
            result += GridType + "@";
            result += Regime + "@";
            result += RegimeLogicEntry + "@";
            result += AutoClearJournalIsOn + "@";
            result += MaxClosePositionsInJournal.ToString(CultureInfo.InvariantCulture) + "@";
            result += MaxOpenOrdersInMarket.ToString(CultureInfo.InvariantCulture) + "@";
            result += MaxCloseOrdersInMarket.ToString(CultureInfo.InvariantCulture) + "@";
            result += _firstTradePrice.ToString(CultureInfo.InvariantCulture) + "@";
            result += _openPositionsBySession.ToString(CultureInfo.InvariantCulture) + "@";
            result += _firstTradeTime.ToString("O", CultureInfo.InvariantCulture) + "@";
            result += DelayInReal.ToString(CultureInfo.InvariantCulture) + "@";
            result += CheckMicroVolumes + "@";
            result += MaxDistanceToOrdersPercent.ToString(CultureInfo.InvariantCulture) + "@";
            result += OpenOrdersMakerOnly + "@";
            result += "@";
            result += "@";

            result += "%";

            // non trade periods
            result += nonTradePeriods?.GetSaveString() ?? string.Empty;
            result += "%";

            // trade days
            result += "";
            result += "%";

            // stop grid by event
            result += stopBy?.GetSaveString() ?? string.Empty;
            result += "%";

            // grid lines creation and storage
            result += gridCreator?.GetSaveString() ?? string.Empty;
            result += "%";

            // stop and profit 
            result += stopAndProfit?.GetSaveString() ?? string.Empty;
            result += "%";

            // auto start
            result += autoStarter?.GetSaveString() ?? string.Empty;
            result += "%";

            // errors reaction
            result += errorsReaction?.GetSaveString() ?? string.Empty;
            result += "%";

            // trailing up / down
            result += trailingUp?.GetSaveString() ?? string.Empty;
            result += "%";

            return result;
        }

        public void LoadFromString(string? value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                string[]? array = null;
                string primeSegment;
                int payloadSeparatorIndex = value.IndexOf('%');
                if (payloadSeparatorIndex >= 0)
                {
                    array = value.Split('%');
                    primeSegment = GetTrimmedToken(array, 0);
                }
                else
                {
                    primeSegment = value.Trim();
                }

                if (string.IsNullOrWhiteSpace(primeSegment))
                {
                    return;
                }

                ReadOnlySpan<char> primeValues = primeSegment.AsSpan();
                bool hasDelay = false;
                bool hasMicro = false;
                bool hasMaxDistance = false;
                bool hasMakerOnly = false;

                int tokenIndex = 0;
                int tokenStart = 0;

                while (tokenIndex <= 14 && tokenStart <= primeValues.Length)
                {
                    ReadOnlySpan<char> rest = primeValues.Slice(tokenStart);
                    int separatorOffset = rest.IndexOf('@');
                    int tokenEnd = separatorOffset >= 0
                        ? tokenStart + separatorOffset
                        : primeValues.Length;

                    ReadOnlySpan<char> token = primeValues.Slice(tokenStart, tokenEnd - tokenStart).Trim();

                    if (token.IsEmpty == false)
                    {
                        switch (tokenIndex)
                        {
                            case 0:
                                if (TryParseIntInvariant(token, out int numberParsed) && numberParsed >= 0)
                                {
                                    Number = numberParsed;
                                }
                                break;
                            case 1:
                                if (TryParseGridPrimeType(token, out TradeGridPrimeType gridTypeParsed)
                                    && (gridTypeParsed == TradeGridPrimeType.MarketMaking
                                        || gridTypeParsed == TradeGridPrimeType.OpenPosition))
                                {
                                    GridType = gridTypeParsed;
                                }
                                break;
                            case 2:
                                if (TryParseTradeGridRegime(token, out TradeGridRegime regimeParsed)
                                    && (regimeParsed == TradeGridRegime.Off
                                        || regimeParsed == TradeGridRegime.OffAndCancelOrders
                                        || regimeParsed == TradeGridRegime.On
                                        || regimeParsed == TradeGridRegime.CloseOnly
                                        || regimeParsed == TradeGridRegime.CloseForced))
                                {
                                    _regime = regimeParsed;
                                }
                                break;
                            case 3:
                                if (TryParseTradeGridLogicEntryRegime(token, out TradeGridLogicEntryRegime logicParsed)
                                    && (logicParsed == TradeGridLogicEntryRegime.OnTrade
                                        || logicParsed == TradeGridLogicEntryRegime.OncePerSecond))
                                {
                                    RegimeLogicEntry = logicParsed;
                                }
                                break;
                            case 4:
                                if (TryParseBoolFlexible(token, out bool autoClearParsed))
                                {
                                    AutoClearJournalIsOn = autoClearParsed;
                                }
                                break;
                            case 5:
                                if (TryParseIntInvariant(token, out int maxClosePositionsParsed) && maxClosePositionsParsed >= 0)
                                {
                                    MaxClosePositionsInJournal = maxClosePositionsParsed;
                                }
                                break;
                            case 6:
                                if (TryParseIntInvariant(token, out int maxOpenOrdersParsed) && maxOpenOrdersParsed >= 0)
                                {
                                    MaxOpenOrdersInMarket = maxOpenOrdersParsed;
                                }
                                break;
                            case 7:
                                if (TryParseIntInvariant(token, out int maxCloseOrdersParsed) && maxCloseOrdersParsed >= 0)
                                {
                                    MaxCloseOrdersInMarket = maxCloseOrdersParsed;
                                }
                                break;
                            case 8:
                                if (TryParseDecimal(token, out decimal firstTradePriceParsed) && firstTradePriceParsed >= 0)
                                {
                                    _firstTradePrice = firstTradePriceParsed;
                                }
                                break;
                            case 9:
                                if (TryParseIntInvariant(token, out int openPositionsParsed) && openPositionsParsed >= 0)
                                {
                                    _openPositionsBySession = openPositionsParsed;
                                }
                                break;
                            case 10:
                                if (TryParseDateInvariantOrCurrent(token, out DateTime firstTradeTimeParsed))
                                {
                                    _firstTradeTime = firstTradeTimeParsed;
                                }
                                break;
                            case 11:
                                hasDelay = true;
                                if (TryParseIntInvariant(token, out int delayParsed)
                                    && delayParsed > 0)
                                {
                                    DelayInReal = delayParsed;
                                }
                                else
                                {
                                    DelayInReal = 500;
                                }
                                break;
                            case 12:
                                hasMicro = true;
                                if (TryParseBoolFlexible(token, out bool microParsed))
                                {
                                    CheckMicroVolumes = microParsed;
                                }
                                break;
                            case 13:
                                hasMaxDistance = true;
                                if (TryParseDecimal(token, out decimal maxDistanceParsed))
                                {
                                    if (maxDistanceParsed >= 0)
                                    {
                                        MaxDistanceToOrdersPercent = maxDistanceParsed;
                                    }
                                    else
                                    {
                                        MaxDistanceToOrdersPercent = 1.5m;
                                    }
                                }
                                break;
                            case 14:
                                hasMakerOnly = true;
                                if (TryParseBoolFlexible(token, out bool makerOnlyParsed))
                                {
                                    OpenOrdersMakerOnly = makerOnlyParsed;
                                }
                                break;
                        }
                    }

                    tokenIndex++;
                    if (separatorOffset < 0)
                    {
                        break;
                    }

                    tokenStart = tokenEnd + 1;
                }

                if (hasDelay == false)
                {
                    DelayInReal = 500;
                }

                if (hasMicro == false)
                {
                    CheckMicroVolumes = true;
                }

                if (hasMaxDistance == false)
                {
                    MaxDistanceToOrdersPercent = 1.5m;
                }

                if (hasMakerOnly == false)
                {
                    OpenOrdersMakerOnly = true;
                }

                if (array != null)
                {
                    // non trade periods
                    if (NonTradePeriods != null
                        && TryGetPayloadSegment(array, 1, out string nonTradeSegment))
                    {
                        NonTradePeriods.LoadFromString(nonTradeSegment);
                    }

                    // trade days
                    // removed

                    // stop grid by event
                    if (StopBy != null
                        && TryGetPayloadSegment(array, 3, out string stopBySegment))
                    {
                        StopBy.LoadFromString(stopBySegment);
                    }

                    // grid lines creation and storage
                    if (GridCreator != null
                        && TryGetPayloadSegment(array, 4, out string gridCreatorSegment))
                    {
                        GridCreator.LoadFromString(gridCreatorSegment);
                    }

                    // stop and profit 
                    if (StopAndProfit != null
                        && TryGetPayloadSegment(array, 5, out string stopAndProfitSegment))
                    {
                        StopAndProfit.LoadFromString(stopAndProfitSegment);
                    }

                    // auto start
                    if (AutoStarter != null
                        && TryGetPayloadSegment(array, 6, out string autoStarterSegment))
                    {
                        AutoStarter.LoadFromString(autoStarterSegment);
                    }

                    // errors reaction
                    if (ErrorsReaction != null
                        && TryGetPayloadSegment(array, 7, out string errorsReactionSegment))
                    {
                        ErrorsReaction.LoadFromString(errorsReactionSegment);
                    }

                    // trailing up / down
                    if (TrailingUp != null
                        && TryGetPayloadSegment(array, 8, out string trailingUpSegment))
                    {
                        TrailingUp.LoadFromString(trailingUpSegment);
                    }
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        private static bool TryParseDateInvariantOrCurrent(ReadOnlySpan<char> value, out DateTime parsed)
        {
            ReadOnlySpan<char> valueSpan = value;
            if (valueSpan.IsEmpty)
            {
                parsed = default;
                return false;
            }

            char firstChar = valueSpan[0];
            if ((uint)(firstChar - '0') > 9 && firstChar != '-' && firstChar != '+')
            {
                parsed = default;
                return false;
            }

            if (TryParseRuFixedDateTime(valueSpan, out parsed))
            {
                return true;
            }

            bool hasT = valueSpan.IndexOf('T') >= 0;
            bool hasDash = valueSpan.IndexOf('-') >= 0;
            bool hasDot = valueSpan.IndexOf('.') >= 0;
            bool hasColon = valueSpan.IndexOf(':') >= 0;

            if (hasT
                && hasDash
                && DateTime.TryParse(valueSpan, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed))
            {
                return true;
            }

            if (hasDot
                && hasColon
                && hasDash == false
                && DateTime.TryParse(valueSpan, RuCulture, DateTimeStyles.None, out parsed))
            {
                return true;
            }

            if (DateTime.TryParse(valueSpan, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed))
            {
                return true;
            }

            if (DateTime.TryParse(valueSpan, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return true;
            }

            if (DateTime.TryParse(valueSpan, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
            {
                return true;
            }

            if (DateTime.TryParse(valueSpan, RuCulture, DateTimeStyles.None, out parsed))
            {
                return true;
            }

            parsed = default;
            return false;
        }

        private static bool TryParseRuFixedDateTime(ReadOnlySpan<char> value, out DateTime parsed)
        {
            parsed = default;

            if (value.Length != 19
                || value[2] != '.'
                || value[5] != '.'
                || value[10] != ' '
                || value[13] != ':'
                || value[16] != ':')
            {
                return false;
            }

            if (TryParseTwoDigits(value, 0, out int day) == false
                || TryParseTwoDigits(value, 3, out int month) == false
                || TryParseFourDigits(value, 6, out int year) == false
                || TryParseTwoDigits(value, 11, out int hour) == false
                || TryParseTwoDigits(value, 14, out int minute) == false
                || TryParseTwoDigits(value, 17, out int second) == false)
            {
                return false;
            }

            if (month < 1 || month > 12
                || day < 1
                || hour > 23
                || minute > 59
                || second > 59)
            {
                return false;
            }

            int daysInMonth = DateTime.DaysInMonth(year, month);
            if (day > daysInMonth)
            {
                return false;
            }

            parsed = new DateTime(year, month, day, hour, minute, second);
            return true;
        }

        private static bool TryParseTwoDigits(ReadOnlySpan<char> value, int startIndex, out int parsed)
        {
            parsed = 0;
            if (startIndex < 0 || startIndex + 1 >= value.Length)
            {
                return false;
            }

            char c0 = value[startIndex];
            char c1 = value[startIndex + 1];
            if (c0 < '0' || c0 > '9' || c1 < '0' || c1 > '9')
            {
                return false;
            }

            parsed = (c0 - '0') * 10 + (c1 - '0');
            return true;
        }

        private static bool TryParseFourDigits(ReadOnlySpan<char> value, int startIndex, out int parsed)
        {
            parsed = 0;
            if (startIndex < 0 || startIndex + 3 >= value.Length)
            {
                return false;
            }

            char c0 = value[startIndex];
            char c1 = value[startIndex + 1];
            char c2 = value[startIndex + 2];
            char c3 = value[startIndex + 3];
            if (c0 < '0' || c0 > '9'
                || c1 < '0' || c1 > '9'
                || c2 < '0' || c2 > '9'
                || c3 < '0' || c3 > '9')
            {
                return false;
            }

            parsed = (c0 - '0') * 1000
                + (c1 - '0') * 100
                + (c2 - '0') * 10
                + (c3 - '0');
            return true;
        }

        private static bool TryParseIntInvariant(ReadOnlySpan<char> value, out int parsed)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed);
        }

        private static bool TryGetPayloadSegment(string[] sections, int index, out string segment)
        {
            segment = string.Empty;

            if (sections == null || index < 0 || index >= sections.Length)
            {
                return false;
            }

            string candidate = sections[index];

            if (string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }

            segment = candidate.Trim();
            return true;
        }

        private static string GetTrimmedToken(string[] values, int index)
        {
            if (values == null || index < 0 || index >= values.Length)
            {
                return string.Empty;
            }

            string value = values[index];
            return value == null ? string.Empty : value.Trim();
        }

        private static bool TryParseEnumFlexible<TEnum>(ReadOnlySpan<char> value, out TEnum parsed)
            where TEnum : struct
        {
            ReadOnlySpan<char> trimmed = value.Trim();
            return Enum.TryParse(trimmed, true, out parsed);
        }

        private static bool TryParseGridPrimeType(ReadOnlySpan<char> value, out TradeGridPrimeType parsed)
        {
            ReadOnlySpan<char> trimmed = value.Trim();
            if (trimmed.SequenceEqual("MarketMaking".AsSpan()))
            {
                parsed = TradeGridPrimeType.MarketMaking;
                return true;
            }

            if (trimmed.SequenceEqual("OpenPosition".AsSpan()))
            {
                parsed = TradeGridPrimeType.OpenPosition;
                return true;
            }

            return TryParseEnumFlexible(trimmed, out parsed);
        }

        private static bool TryParseTradeGridRegime(ReadOnlySpan<char> value, out TradeGridRegime parsed)
        {
            ReadOnlySpan<char> trimmed = value.Trim();
            if (trimmed.SequenceEqual("Off".AsSpan()))
            {
                parsed = TradeGridRegime.Off;
                return true;
            }

            if (trimmed.SequenceEqual("OffAndCancelOrders".AsSpan()))
            {
                parsed = TradeGridRegime.OffAndCancelOrders;
                return true;
            }

            if (trimmed.SequenceEqual("On".AsSpan()))
            {
                parsed = TradeGridRegime.On;
                return true;
            }

            if (trimmed.SequenceEqual("CloseOnly".AsSpan()))
            {
                parsed = TradeGridRegime.CloseOnly;
                return true;
            }

            if (trimmed.SequenceEqual("CloseForced".AsSpan()))
            {
                parsed = TradeGridRegime.CloseForced;
                return true;
            }

            return TryParseEnumFlexible(trimmed, out parsed);
        }

        private static bool TryParseTradeGridLogicEntryRegime(ReadOnlySpan<char> value, out TradeGridLogicEntryRegime parsed)
        {
            ReadOnlySpan<char> trimmed = value.Trim();
            if (trimmed.SequenceEqual("OnTrade".AsSpan()))
            {
                parsed = TradeGridLogicEntryRegime.OnTrade;
                return true;
            }

            if (trimmed.SequenceEqual("OncePerSecond".AsSpan()))
            {
                parsed = TradeGridLogicEntryRegime.OncePerSecond;
                return true;
            }

            return TryParseEnumFlexible(trimmed, out parsed);
        }

        private static bool TryParseBoolFlexible(ReadOnlySpan<char> value, out bool parsed)
        {
            ReadOnlySpan<char> normalized = value;

            if (bool.TryParse(normalized, out parsed))
            {
                return true;
            }

            if (normalized.SequenceEqual("1".AsSpan())
                || normalized.Equals("yes".AsSpan(), StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("y".AsSpan(), StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("on".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                parsed = true;
                return true;
            }

            if (normalized.SequenceEqual("0".AsSpan())
                || normalized.Equals("no".AsSpan(), StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("n".AsSpan(), StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("off".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                parsed = false;
                return true;
            }

            parsed = false;
            return false;
        }

        private static bool TryParseDecimal(ReadOnlySpan<char> value, out decimal parsed)
        {
            ReadOnlySpan<char> valueSpan = value;
            if (valueSpan.IsEmpty)
            {
                parsed = 0;
                return false;
            }

            char firstChar = valueSpan[0];
            if ((uint)(firstChar - '0') > 9 && firstChar != '-' && firstChar != '+')
            {
                parsed = 0;
                return false;
            }

            bool hasComma = valueSpan.IndexOf(',') >= 0;
            bool hasDot = valueSpan.IndexOf('.') >= 0;

            if (hasComma && hasDot == false)
            {
                if (decimal.TryParse(valueSpan, NumberStyles.Any, RuCulture, out parsed))
                {
                    return true;
                }
            }

            if (decimal.TryParse(valueSpan, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
            {
                return true;
            }

            if (decimal.TryParse(valueSpan, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
            {
                return true;
            }

            if (decimal.TryParse(valueSpan, NumberStyles.Any, RuCulture, out parsed))
            {
                return true;
            }

            parsed = 0;
            return false;
        }

        public void Delete()
        {
            _isDeleted = true;

            BotTabSimple tab = Tab;
            TradeGridNonTradePeriods nonTradePeriods = NonTradePeriods;
            TradeGridStopBy stopBy = StopBy;
            TradeGridStopAndProfit stopAndProfit = StopAndProfit;
            TradeGridAutoStarter autoStarter = AutoStarter;
            TradeGridCreator gridCreator = GridCreator;
            TradeGridErrorsReaction errorsReaction = ErrorsReaction;
            TrailingUp trailingUp = TrailingUp;

            Tab = null;
            NonTradePeriods = null;
            StopBy = null;
            StopAndProfit = null;
            AutoStarter = null;
            GridCreator = null;
            ErrorsReaction = null;
            TrailingUp = null;

            if (tab != null)
            {
                try
                {
                    tab.NewTickEvent -= Tab_NewTickEvent;
                    tab.PositionOpeningSuccesEvent -= Tab_PositionOpeningSuccesEvent;
                    tab.PositionClosingSuccesEvent -= Tab_PositionClosingSuccesEvent;
                    tab.PositionStopActivateEvent -= Tab_PositionStopActivateEvent;
                    if (tab.Connector != null)
                    {
                        tab.Connector.TestStartEvent -= Connector_TestStartEvent;
                    }
                    tab.PositionOpeningFailEvent -= Tab_PositionOpeningFailEvent;
                    tab.PositionClosingFailEvent -= Tab_PositionClosingFailEvent;
                }
                catch (Exception)
                {
                    // Defensive cleanup path: ignore partial-state unsubscription failures.
                }
            }

            if (nonTradePeriods != null)
            {
                try
                {
                    nonTradePeriods.LogMessageEvent -= SendNewLogMessage;
                    nonTradePeriods.Delete();
                }
                catch (Exception)
                {
                    // Defensive cleanup path: ignore partial-state component failures.
                }
            }

            if (stopBy != null)
            {
                try
                {
                    stopBy.LogMessageEvent -= SendNewLogMessage;
                }
                catch (Exception)
                {
                    // Defensive cleanup path: ignore partial-state component failures.
                }
            }

            if (stopAndProfit != null)
            {
                try
                {
                    stopAndProfit.LogMessageEvent -= SendNewLogMessage;
                }
                catch (Exception)
                {
                    // Defensive cleanup path: ignore partial-state component failures.
                }
            }

            if (autoStarter != null)
            {
                try
                {
                    autoStarter.LogMessageEvent -= SendNewLogMessage;
                }
                catch (Exception)
                {
                    // Defensive cleanup path: ignore partial-state component failures.
                }
            }

            if (gridCreator != null)
            {
                try
                {
                    gridCreator.LogMessageEvent -= SendNewLogMessage;
                }
                catch (Exception)
                {
                    // Defensive cleanup path: ignore partial-state component failures.
                }
            }

            if (errorsReaction != null)
            {
                try
                {
                    errorsReaction.LogMessageEvent -= SendNewLogMessage;
                    errorsReaction.Delete();
                }
                catch (Exception)
                {
                    // Defensive cleanup path: ignore partial-state component failures.
                }
            }

            if (trailingUp != null)
            {
                try
                {
                    trailingUp.LogMessageEvent -= SendNewLogMessage;
                    trailingUp.Delete();
                }
                catch (Exception)
                {
                    // Defensive cleanup path: ignore partial-state component failures.
                }
            }
        }

        public void Save()
        {
            NeedToSaveEvent?.Invoke();
        }

        public void RePaintGrid()
        {
            RePaintSettingsEvent?.Invoke();
        }

        public void FullRePaintGrid()
        {
            FullRePaintGridEvent?.Invoke();
        }

        private void Connector_TestStartEvent()
        {
            try
            {
                TradeGridCreator gridCreator = GridCreator;
                if (gridCreator == null)
                {
                    return;
                }

                List<TradeGridLine> lines = gridCreator.Lines;

                if (lines == null)
                {
                    return;
                }

                for (int i = 0; i < lines.Count; i++)
                {
                    TradeGridLine line = lines[i];
                    if (line == null)
                    {
                        continue;
                    }

                    line.Position = null;
                    line.PositionNum = 0;
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        private void Tab_PositionClosingFailEvent(Position position)
        {
            try
            {
                if (position == null)
                {
                    return;
                }

                if (Regime != TradeGridRegime.Off)
                {
                    TradeGridCreator gridCreator = GridCreator;
                    List<TradeGridLine> lines = gridCreator?.Lines;
                    if (lines == null)
                    {
                        return;
                    }

                    bool isInArray = false;

                    for (int i = 0; i < lines.Count; i++)
                    {
                        TradeGridLine line = lines[i];
                        if (line == null)
                        {
                            continue;
                        }

                        if (line.Position != null
                            && line.Position.Number == position.Number)
                        {
                            isInArray = true;
                            break;
                        }
                    }

                    if (isInArray)
                    {
                        TradeGridErrorsReaction errorsReaction = ErrorsReaction;
                        if (errorsReaction == null)
                        {
                            return;
                        }

                        errorsReaction.PositionClosingFailEvent(position);
                    }
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        private void Tab_PositionOpeningFailEvent(Position position)
        {
            try
            {
                if (position == null)
                {
                    return;
                }

                if (Regime != TradeGridRegime.Off)
                {
                    TradeGridCreator gridCreator = GridCreator;
                    List<TradeGridLine> lines = gridCreator?.Lines;
                    if (lines == null)
                    {
                        return;
                    }

                    bool isInArray = false;

                    for (int i = 0; i < lines.Count; i++)
                    {
                        TradeGridLine line = lines[i];
                        if (line == null)
                        {
                            continue;
                        }

                        if (line.Position != null
                            && line.Position.Number == position.Number)
                        {
                            isInArray = true;
                            break;
                        }
                    }

                    if (isInArray)
                    {
                        TradeGridErrorsReaction errorsReaction = ErrorsReaction;
                        if (errorsReaction == null)
                        {
                            return;
                        }

                        errorsReaction.PositionOpeningFailEvent(position);
                    }
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        public event Action? NeedToSaveEvent;

        public event Action? RePaintSettingsEvent;

        public event Action? FullRePaintGridEvent;

        #endregion

        #region Settings Prime

        public TradeGridPrimeType GridType;

        public TradeGridRegime Regime
        {
            get
            {
                return _regime;
            }
            set
            {
                if (_regime == value)
                {
                    return;
                }

                _regime = value;

                FullRePaintGridEvent?.Invoke();
                RePaintSettingsEvent?.Invoke();
            }
        }
        private TradeGridRegime _regime;

        public TradeGridLogicEntryRegime RegimeLogicEntry;

        public bool AutoClearJournalIsOn;

        public int MaxClosePositionsInJournal = 100;

        public int MaxOpenOrdersInMarket = 5;

        public int MaxCloseOrdersInMarket = 5;

        public int DelayInReal = 500;

        public bool CheckMicroVolumes = true;

        public decimal MaxDistanceToOrdersPercent = 0;

        public bool OpenOrdersMakerOnly = true;

        #endregion

        #region Grid managment

        public void CreateNewGridSafe()
        {
            try
            {
                TradeGridCreator gridCreator = GridCreator;
                BotTabSimple tab = Tab;
                List<TradeGridLine> lines = gridCreator?.Lines;

                if (gridCreator == null || tab == null)
                {
                    return;
                }

                if (Regime != TradeGridRegime.Off &&
                    lines != null
                    && lines.Count > 0)
                {
                    // Сетка включена. Есть линии. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label510);
                    ui.Show();
                    return;
                }
                if (HaveOpenPositionsByGrid == true)
                {
                    // По сетке есть открытые позиции. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label511);
                    ui.Show();
                    return;
                }

                if (tab.IsConnected == false
                    || tab.IsReadyToTrade == false)
                {
                    // По сетке не подключены данные. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label512);
                    ui.Show();
                    return;
                }

                if (gridCreator.LineCountStart <= 0)
                {
                    // Количество линий в сетке не установлено. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label514);
                    ui.Show();
                    return;
                }

                if (gridCreator.LineStep <= 0)
                {
                    // Шаг сетки не указан. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label515);
                    ui.Show();
                    return;
                }

                if (GridType == TradeGridPrimeType.MarketMaking
                    && gridCreator.ProfitStep <= 0)
                {
                    // Шаг сетки для профита не указан. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label516);
                    ui.Show();
                    return;
                }

                if (gridCreator.StartVolume <= 0)
                {
                    // Стартовый объём не указан. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label517);
                    ui.Show();
                    return;
                }

                if (gridCreator.StepMultiplicator <= 0)
                {
                    // Мультипликатор шага ноль. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label518);
                    ui.Show();
                    return;
                }

                if (GridType == TradeGridPrimeType.MarketMaking
                    && gridCreator.ProfitMultiplicator <= 0)
                {
                    // Мультипликатор профита ноль. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label519);
                    ui.Show();
                    return;
                }

                if (gridCreator.MartingaleMultiplicator <= 0)
                {
                    // Мультипликатор объёма ноль. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label520);
                    ui.Show();
                    return;
                }

                if (lines != null && lines.Count > 0)
                {
                    AcceptDialogUi ui = new AcceptDialogUi(OsLocalization.Trader.Label522);

                    ui.ShowDialog();

                    if (ui.UserAcceptAction == false)
                    {
                        return;
                    }
                }

                gridCreator.CreateNewGrid(tab, GridType);
                Save();
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        public void DeleteGrid()
        {
            try
            {
                TradeGridCreator gridCreator = GridCreator;
                if (gridCreator == null)
                {
                    return;
                }

                if (HaveOpenPositionsByGrid == true
                    && StartProgram == StartProgram.IsOsTrader)
                {
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label524);
                    ui.Show();
                    return;
                }

                gridCreator.DeleteGrid();
                Save();
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        private bool _isDeleted;

        public void CreateNewLine()
        {
            try
            {
                TradeGridCreator gridCreator = GridCreator;
                if (gridCreator == null)
                {
                    return;
                }

                gridCreator.CreateNewLine();

                Save();
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        public void RemoveSelected(List<int> numbers)
        {
            try
            {
                TradeGridCreator gridCreator = GridCreator;
                BotTabSimple tab = Tab;
                if (gridCreator == null || numbers == null || numbers.Count == 0)
                {
                    return;
                }

                List<TradeGridLine> lines = gridCreator.Lines;
                if (lines == null || lines.Count == 0)
                {
                    return;
                }

                for (int i = numbers.Count - 1; i > -1; i--)
                {
                    int curNumber = numbers[i];

                    if (curNumber < 0 || curNumber >= lines.Count)
                    {
                        continue;
                    }

                    TradeGridLine line = lines[curNumber];
                    if (line == null)
                    {
                        continue;
                    }

                    if (line.Position != null
                        && line.Position.OpenActive
                        && tab != null)
                    {
                        if (TryGetLastOrder(line.Position.OpenOrders, out Order order))
                        {
                            tab.CloseOrder(order);
                        }
                    }
                }

                gridCreator.RemoveSelected(numbers);
                Save();
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        #endregion

        #region Trade logic. Entry in logic

        private void Tab_NewTickEvent(Trade trade)
        {
            if (trade == null)
            {
                return;
            }

            if (_isDeleted == true)
            {
                return;
            }
            if (RegimeLogicEntry == TradeGridLogicEntryRegime.OnTrade)
            {
                Process();
            }
        }

        private void ThreadWorkerPlace()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);

                    if (_isDeleted == true)
                    {
                        return;
                    }

                    if (RegimeLogicEntry == TradeGridLogicEntryRegime.OncePerSecond)
                    {
                        Process();
                    }

                    if (_needToSave)
                    {
                        _needToSave = false;
                        Save();
                    }
                }
                catch (Exception e)
                {
                    Thread.Sleep(1000);

                    try
                    {
                        SendNewLogMessage(e.ToString(), LogMessageType.Error);
                    }
                    catch
                    {
                        ServerMaster.SendNewLogMessage(e.ToString(), LogMessageType.Error);
                    }
                }
            }
        }

        private void Tab_PositionOpeningSuccesEvent(Position position)
        {
            if (position == null)
            {
                return;
            }

            if (Regime != TradeGridRegime.Off)
            {
                TradeGridCreator gridCreator = GridCreator;
                List<TradeGridLine> lines = gridCreator?.Lines;
                if (lines == null)
                {
                    return;
                }

                bool isInArray = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    TradeGridLine line = lines[i];
                    if (line == null)
                    {
                        continue;
                    }

                    if (line.Position != null
                        && line.Position.Number == position.Number)
                    {
                        isInArray = true;
                        break;
                    }
                }

                if (isInArray)
                {
                    _openPositionsBySession++;
                    _needToSave = true;
                }
            }

            if (Regime == TradeGridRegime.On)
            {
                _firstPositionIsOpen = true;
            }
            else
            {
                _firstPositionIsOpen = false;
            }
        }

        private void Tab_PositionClosingSuccesEvent(Position position)
        {
            if (position == null)
            {
                return;
            }

            if (Regime == TradeGridRegime.On)
            {
                _firstPositionIsOpen = true;
            }
            else
            {
                _firstPositionIsOpen = false;
            }
        }

        private bool _needToSave;

        private static bool TryGetLastOrder(List<Order> orders, out Order order)
        {
            order = null;

            if (orders == null || orders.Count == 0)
            {
                return false;
            }

            order = orders[^1];
            return order != null;
        }

        #endregion

        #region Trade logic. Main logic tree

        private DateTime _vacationTime;

        private void Process()
        {
            BotTabSimple tab = Tab;
            TradeGridCreator gridCreator = GridCreator;
            TradeGridErrorsReaction errorsReaction = ErrorsReaction;
            TradeGridAutoStarter autoStarter = AutoStarter;
            TradeGridNonTradePeriods nonTradePeriods = NonTradePeriods;
            TradeGridStopBy stopBy = StopBy;
            TrailingUp trailingUp = TrailingUp;
            List<TradeGridLine> lines = gridCreator?.Lines;

            if (tab == null
                || gridCreator == null
                || errorsReaction == null
                || autoStarter == null
                || nonTradePeriods == null
                || stopBy == null
                || trailingUp == null)
            {
                return;
            }

            if (tab.IsConnected == false
                || tab.IsReadyToTrade == false)
            {
                return;
            }

            if (tab.CandlesAll == null
                || tab.CandlesAll.Count == 0)
            {
                return;
            }

            if (lines == null
                || lines.Count == 0)
            {
                return;
            }

            if (tab.EventsIsOn == false)
            {
                return;
            }

            if (MainWindow.ProccesIsWorked == false)
            {
                return;
            }

            if (StartProgram == StartProgram.IsOsTrader)
            {
                if (tab.IsNonTradePeriodInConnector == true)
                {
                    return;
                }
            }

            if (StartProgram == StartProgram.IsOsTrader
               && errorsReaction.WaitOnStartConnectorIsOn == true)
            {
                var connector = tab.Connector;
                IServer server = connector?.MyServer;
                if (server == null)
                {
                    return;
                }

                if (server is AServer aServer)
                {
                    if (errorsReaction.AwaitOnStartConnector(aServer) == true)
                    {
                        return;
                    }
                }
            }

            if (StartProgram == StartProgram.IsOsTrader)
            {// сбрасываем кол-во ошибок по утрам и на старте сессии

                if (errorsReaction.TryResetErrorsAtStartOfDay(tab.TimeServerCurrent) == true)
                {
                    Save();
                }
            }

            TradeGridRegime baseRegime = Regime;

            // 1 Авто-старт сетки, если выключено
            if (baseRegime == TradeGridRegime.Off ||
                baseRegime == TradeGridRegime.OffAndCancelOrders)
            {
                _firstPositionIsOpen = false;

                if (StartProgram == StartProgram.IsOsTrader)
                {
                    if (_vacationTime > DateTime.Now)
                    {
                        return;
                    }
                }

                if (_openPositionsBySession != 0)
                {
                    _openPositionsBySession = 0;
                    _needToSave = true;
                }
                if (_firstTradePrice != 0)
                {
                    _firstTradePrice = 0;
                    _needToSave = true;
                }

                if (_firstTradeTime != DateTime.MinValue)
                {
                    _firstTradeTime = DateTime.MinValue;
                    _needToSave = true;
                }

                _firstStopIsActivate = false;

                if (errorsReaction.FailCancelOrdersCountFact != 0
                    || errorsReaction.FailOpenOrdersCountFact != 0)
                {
                    errorsReaction.FailCancelOrdersCountFact = 0;
                    errorsReaction.FailOpenOrdersCountFact = 0;
                    _needToSave = true;
                }

                if (GridType == TradeGridPrimeType.OpenPosition)
                {
                    TryDeleteDonePositions();
                }

                // отзываем ордера с рынка

                if (HaveOrdersTryToCancelLastSecond())
                {
                    return;
                }

                if (baseRegime == TradeGridRegime.OffAndCancelOrders)
                {
                    int countRejectOrders = TryCancelClosingOrders();

                    if (countRejectOrders > 0)
                    {
                        _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                        return;
                    }

                    countRejectOrders = TryCancelOpeningOrders();

                    if (countRejectOrders > 0)
                    {
                        _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                        return;
                    }
                }

                // проверяем работу авто-стартера, если он включен

                if (autoStarter.AutoStartRegime == TradeGridAutoStartRegime.Off
                    && autoStarter.StartGridByTimeOfDayIsOn == false)
                {
                    return;
                }

                DateTime serverTime = tab.TimeServerCurrent;

                TradeGridRegime nonTradePeriodsRegime = nonTradePeriods.GetNonTradePeriodsRegime(serverTime);

                if (nonTradePeriodsRegime != TradeGridRegime.On)
                { // авто-старт не может быть включен, если сейчас не торговый период
                    return;
                }

                if (autoStarter.HaveEventToStart(this))
                {
                    if (autoStarter.RebuildGridRegime == GridAutoStartShiftFirstPriceRegime.On_FullRebuild)
                    {// пересобираем сетку полностью
                        decimal newPriceStart = autoStarter.GetNewGridPriceStart(this);

                        if (newPriceStart != 0)
                        {
                            gridCreator.FirstPrice = newPriceStart;
                            gridCreator.CreateNewGrid(tab, GridType);
                            Save();
                            FullRePaintGrid();
                        }
                    }
                    else if (autoStarter.RebuildGridRegime == GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice)
                    {// просто сдвигаем сетку на новую цену

                        decimal newPriceStart = autoStarter.GetNewGridPriceStart(this);

                        if (newPriceStart != 0)
                        {
                            autoStarter.ShiftGridOnNewPrice(newPriceStart, this);
                            Save();
                            FullRePaintGrid();
                        }
                    }

                    baseRegime = TradeGridRegime.On;
                    Regime = TradeGridRegime.On;
                    Save();
                    RePaintGrid();
                }
                else
                {
                    return;
                }
            }

            // 2 проверяем ошибки и реагируем на них

            if (StartProgram == StartProgram.IsOsTrader)
            {
                TradeGridRegime reaction = errorsReaction.GetReactionOnErrors(this);

                if (reaction != TradeGridRegime.On)
                {
                    errorsReaction.FailCancelOrdersCountFact = 0;
                    errorsReaction.FailOpenOrdersCountFact = 0;
                    baseRegime = reaction;
                    Regime = reaction;

                    if (autoStarter.AutoStartRegime != TradeGridAutoStartRegime.Off
                        || autoStarter.StartGridByTimeOfDayIsOn == true)
                    {// Отключаем авто-стартер, если выключаемся по ошибкам
                        if (autoStarter.AutoStartRegime != TradeGridAutoStartRegime.Off)
                        {
                            autoStarter.AutoStartRegime = TradeGridAutoStartRegime.Off;
                        }

                        if (autoStarter.StartGridByTimeOfDayIsOn == true)
                        {
                            autoStarter.StartGridByTimeOfDayIsOn = false;
                        }

                        string message = "AutoStarter is OFF";
                        SendNewLogMessage(message, LogMessageType.Signal);
                    }

                    Save();
                    RePaintGrid();
                }
            }

            // 3 проверяем ожидание в бою. Только что были отозваны или выставлены N кол-во ордеров

            if (StartProgram == StartProgram.IsOsTrader)
            {
                if (_vacationTime > DateTime.Now)
                {
                    return;
                }
            }

            // 4 проверяем наличие ордеров без номеров в маркете. Для медленных подключений

            if (StartProgram == StartProgram.IsOsTrader)
            {
                if (HaveOrdersWithNoMarketOrders())
                {
                    return;
                }

                if (HaveOrdersTryToCancelLastSecond())
                {
                    return;
                }
            }

            // 5 попытка смены режима если блокировано по времени или по дням

            if (baseRegime != TradeGridRegime.Off)
            {
                DateTime serverTime = tab.TimeServerCurrent;

                TradeGridRegime nonTradePeriodsRegime = nonTradePeriods.GetNonTradePeriodsRegime(serverTime);

                if (nonTradePeriodsRegime != TradeGridRegime.On)
                {
                    baseRegime = nonTradePeriodsRegime;

                    if (baseRegime == TradeGridRegime.CloseForced)
                    {
                        Regime = baseRegime;
                        Save();
                        RePaintGrid();
                    }
                }
            }

            // 6 попытка смены режима по остановке торгов

            if (baseRegime == TradeGridRegime.On)
            {
                TradeGridRegime stopByRegime = stopBy.GetRegime(this, tab);

                if (stopByRegime != TradeGridRegime.On)
                {
                    baseRegime = stopByRegime;
                    Regime = stopByRegime;
                    Save();
                    RePaintGrid();
                }
            }

            // 7 попытка сместить сетку

            if (baseRegime == TradeGridRegime.On)
            {
                if (trailingUp.TryTrailingGrid())
                {
                    _needToSave = true;
                    RePaintGrid();
                    FullRePaintGrid();
                }
            }

            // 8 вход в различную логику различных сеток

            if (baseRegime == TradeGridRegime.On
                || baseRegime == TradeGridRegime.CloseOnly
                || baseRegime == TradeGridRegime.CloseForced)
            {
                if (GridType == TradeGridPrimeType.MarketMaking)
                {
                    GridTypeMarketMakingLogic(baseRegime);
                }
                else if (GridType == TradeGridPrimeType.OpenPosition)
                {
                    GridTypeOpenPositionLogic(baseRegime);
                }
            }
            else if (baseRegime == TradeGridRegime.OffAndCancelOrders)
            {
                int countRejectOrders = TryCancelClosingOrders();

                if (countRejectOrders > 0)
                {
                    _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                    return;
                }

                countRejectOrders = TryCancelOpeningOrders();

                if (countRejectOrders > 0)
                {
                    _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                    return;
                }
            }
        }

        #endregion

        #region Open Position end logic

        private void GridTypeOpenPositionLogic(TradeGridRegime baseRegime)
        {
            if (_firstStopIsActivate == true)
            {
                if (_firstStopActivateTime.AddSeconds(5) < DateTime.Now)
                {
                    string message = "First stop by grid is activate. \n";
                    message += "Stop trading" + "\n";
                    message += "New regime: CloseForced";

                    SendNewLogMessage(message, LogMessageType.Signal);

                    Regime = TradeGridRegime.CloseForced;
                    Save();
                    RePaintGrid();
                    _firstStopIsActivate = false;
                    _vacationTime = DateTime.Now.AddSeconds(5);
                }
                else
                {
                    return;
                }
            }

            // 1 сверям позиции в журнале и в сетке

            TryFindPositionsInJournalAfterReconnect();
            TryDeleteOpeningFailPositions();

            // 2 удаляем ордера стоящие не на своём месте

            int countRejectOrders = TryRemoveWrongOrders();

            if (countRejectOrders > 0)
            {
                _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                return;
            }

            // 3 торговая логика 

            if (baseRegime == TradeGridRegime.On)
            {
                if (_firstStopIsActivate == false)
                {
                    TradeGridStopAndProfit stopAndProfit = StopAndProfit;

                    // 1 пытаемся почистить журнал от лишних сделок
                    TryFreeJournal();

                    // 2 проверяем выставлены ли ордера на открытие
                    TrySetOpenOrders();

                    // 3 проверяем выставлены ли закрытия
                    TrySetStopAndProfit();

                    // 4 проверяем лимитки за закрытие по профиту
                    if (stopAndProfit != null
                        && stopAndProfit.ProfitRegime == OnOffRegime.On)
                    {
                        TrySetLimitProfit();

                        if (stopAndProfit.StopTradingAfterProfit == true)
                        {
                            CheckStopTradingAfterProfit();
                        }
                        else
                        {
                            TryDeleteDonePositions();
                        }
                    }
                }
            }
            else
            {
                countRejectOrders = TryCancelOpeningOrders();

                if (countRejectOrders > 0)
                {
                    _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                    return;
                }

                if (_openPositionsBySession != 0)
                {
                    _openPositionsBySession = 0;
                    _needToSave = true;
                }
                if (_firstTradePrice != 0)
                {
                    _firstTradePrice = 0;
                    _needToSave = true;
                }
                if (_firstTradeTime != DateTime.MinValue)
                {
                    _firstTradeTime = DateTime.MinValue;
                    _needToSave = true;
                }

                if (baseRegime == TradeGridRegime.CloseOnly)
                {
                    // закрываем позиции штатно
                    TrySetStopAndProfit();
                }
                else if (baseRegime == TradeGridRegime.CloseForced)
                {
                    countRejectOrders = TryCancelClosingOrders();

                    if (countRejectOrders > 0)
                    {
                        _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                        return;
                    }

                    // закрываем позиции насильно
                    TryForcedCloseGrid();
                }
            }
        }

        private void TrySetStopAndProfit()
        {
            TradeGridStopAndProfit stopAndProfit = StopAndProfit;
            if (stopAndProfit == null)
            {
                return;
            }

            if (stopAndProfit.ProfitRegime == OnOffRegime.Off
                && stopAndProfit.StopRegime == OnOffRegime.Off
                && stopAndProfit.TrailStopRegime == OnOffRegime.Off)
            {
                return;
            }

            stopAndProfit.Process(this);
        }

        private void TryDeleteOpeningFailPositions()
        {
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return;
            }

            List<TradeGridLine> lines = gridCreator.Lines;

            if (lines == null)
            {
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                if (line.Position != null)
                {
                    // Открывающий ордер был отозван
                    if (line.Position.State == PositionStateType.OpeningFail
                        && line.Position.OpenActive == false)
                    {
                        line.Position = null;
                        line.PositionNum = -1;
                    }
                }
            }
        }

        private bool _firstStopIsActivate = false;

        private DateTime _firstStopActivateTime;

        private bool _firstPositionIsOpen = false;

        private void Tab_PositionStopActivateEvent(Position obj)
        {
            if (obj == null)
            {
                return;
            }

            if (_firstStopIsActivate == false)
            {
                _firstStopIsActivate = true;
                _firstStopActivateTime = DateTime.Now;
            }
        }

        private void TrySetLimitProfit()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }

            // 1 проверяем отзыв не правильных лимиток

            int countRejectOrders = TryCancelWrongCloseProfitOrders();

            if (countRejectOrders > 0)
            {
                _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                return;
            }

            // 2 выставляем лимитки 

            TrySetClosingProfitOrders(tab.PriceBestAsk);

        }

        private int TryCancelWrongCloseProfitOrders()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return 0;
            }

            List<TradeGridLine> lines = GetLinesWithClosingOrdersFact();

            int cancelledOrders = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                Position pos = line.Position;

                if (pos == null
                    || pos.CloseActive == false)
                {
                    continue;
                }

                if (TryGetLastOrder(pos.CloseOrders, out Order order) == false)
                {
                    continue;
                }

                if (order.NumberMarket != null
                    && order.LastCancelTryLocalTime.AddSeconds(5) < DateTime.Now)
                {
                    if (order.Price != pos.ProfitOrderPrice
                        || order.Volume - order.VolumeExecute != pos.OpenVolume)
                    {
                        tab.CloseOrder(order);
                        cancelledOrders++;
                    }
                }
            }

            return cancelledOrders;
        }

        private void TrySetClosingProfitOrders(decimal lastPrice)
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }
            Security security = tab.Security;
            if (security == null)
            {
                return;
            }

            List<TradeGridLine> linesOpenPoses = GetLinesWithOpenPosition();

            for (int i = 0; i < linesOpenPoses.Count; i++)
            {
                TradeGridLine line = linesOpenPoses[i];
                if (line == null)
                {
                    continue;
                }

                Position pos = line.Position;
                if (pos == null)
                {
                    continue;
                }

                if (pos.CloseActive == true)
                {
                    continue;
                }

                if (pos.ProfitOrderPrice == 0)
                {
                    continue;
                }

                decimal volume = pos.OpenVolume;

                if (CheckMicroVolumes == true
                    && tab.CanTradeThisVolume(volume) == false)
                {
                    continue;
                }

                if (security.PriceLimitHigh != 0
                 && security.PriceLimitLow != 0)
                {
                    if (line.PriceExit > security.PriceLimitHigh
                        || line.PriceExit < security.PriceLimitLow)
                    {
                        continue;
                    }
                }

                if (tab.StartProgram == StartProgram.IsOsTrader
                    && MaxDistanceToOrdersPercent != 0
                    && lastPrice != 0)
                {
                    decimal maxPriceUp = lastPrice + lastPrice * (MaxDistanceToOrdersPercent / 100);
                    decimal minPriceDown = lastPrice - lastPrice * (MaxDistanceToOrdersPercent / 100);

                    if (line.PriceExit > maxPriceUp
                     || line.PriceExit < minPriceDown)
                    {
                        continue;
                    }
                }

                tab.CloseAtLimitUnsafe(pos, pos.ProfitOrderPrice, volume);
            }
        }

        private void CheckStopTradingAfterProfit()
        {
            List<TradeGridLine> linesOpenPoses = GetLinesWithOpenPosition();

            // И если линий с открытыми позами нет - переключаемся в CloseForced

            if (linesOpenPoses == null
                || linesOpenPoses.Count == 0)
            {
                if (_firstPositionIsOpen == true)
                {
                    Regime = TradeGridRegime.CloseForced;

                    string message = "Grid is stop by Profit. \n";
                    message += "Stop trading" + "\n";
                    message += "New regime: CloseForced";

                    SendNewLogMessage(message, LogMessageType.Signal);
                }
            }
        }

        #endregion

        #region MarketMaking end logic

        private void GridTypeMarketMakingLogic(TradeGridRegime baseRegime)
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }

            // 1 сверям позиции в журнале и в сетке

            TryFindPositionsInJournalAfterReconnect();
            TryDeleteDonePositions();

            // 2 удаляем ордера стоящие не на своём месте

            int countRejectOrders = TryRemoveWrongOrders();

            if (countRejectOrders > 0)
            {
                _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                return;
            }

            // 8 торговая логика 

            if (baseRegime == TradeGridRegime.On)
            {
                // 1 пытаемся почистить журнал от лишних сделок
                TryFreeJournal();

                // 2 проверяем выставлены ли ордера на открытие
                TrySetOpenOrders();

                // 3 проверяем выставлены ли закрытия
                TrySetClosingOrders(tab.PriceBestAsk);
            }
            else
            {
                countRejectOrders = TryCancelOpeningOrders();

                if (countRejectOrders > 0)
                {
                    _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                    return;
                }

                if (_openPositionsBySession != 0)
                {
                    _openPositionsBySession = 0;
                    _needToSave = true;
                }
                if (_firstTradePrice != 0)
                {
                    _firstTradePrice = 0;
                    _needToSave = true;
                }
                if (_firstTradeTime != DateTime.MinValue)
                {
                    _firstTradeTime = DateTime.MinValue;
                    _needToSave = true;
                }

                if (baseRegime == TradeGridRegime.CloseOnly)
                {
                    // закрываем позиции штатно
                    TrySetClosingOrders(tab.PriceBestAsk);
                }
                else if (baseRegime == TradeGridRegime.CloseForced)
                {
                    countRejectOrders = TryCancelClosingOrders();

                    if (countRejectOrders > 0)
                    {
                        _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                        return;
                    }

                    // закрываем позиции насильно 
                    TryForcedCloseGrid();
                }
            }
        }

        private int TryRemoveWrongOrders()
        {
            BotTabSimple tab = Tab;
            TradeGridCreator gridCreator = GridCreator;
            if (tab == null || gridCreator == null)
            {
                return 0;
            }

            if (TryGetLastCandle(tab.CandlesAll, out Candle lastCandle) == false)
            {
                return 0;
            }

            decimal lastPrice = lastCandle.Close;

            // 1 убираем ордера на открытие и закрытие с неправильной ценой.

            List<TradeGridLine> linesWithOrdersToOpenFact = GetLinesWithOpenOrdersFact();
            List<TradeGridLine> linesWithOrdersToCloseFact = GetLinesWithClosingOrdersFact();

            List<Order> ordersToCancelBadPrice = GetOrdersBadPriceToGridFromLines(linesWithOrdersToOpenFact, linesWithOrdersToCloseFact);

            if (ordersToCancelBadPrice != null
                && ordersToCancelBadPrice.Count > 0)
            {
                for (int i = 0; i < ordersToCancelBadPrice.Count; i++)
                {
                    //Tab.SetNewLogMessage("Отзыв ордера по не правильной цене", LogMessageType.System);
                    tab.CloseOrder(ordersToCancelBadPrice[i]);
                }

                return ordersToCancelBadPrice.Count;
            }

            // 2 убираем ордера лишние на открытие. Когда в сетке больше ордеров чем указал пользователь

            List<Order> ordersToCancelBadLines = GetOrdersBadLinesMaxCountFromLines(linesWithOrdersToOpenFact);

            if (ordersToCancelBadLines != null
                && ordersToCancelBadLines.Count > 0)
            {
                for (int i = 0; i < ordersToCancelBadLines.Count; i++)
                {
                    //Tab.SetNewLogMessage("Отзыв ордера по количеству", LogMessageType.System);
                    tab.CloseOrder(ordersToCancelBadLines[i]);
                }

                return ordersToCancelBadLines.Count;
            }

            // 3 убираем ордера на открытие, если имеет место дыра в сетке

            List<Order> ordersToCancelOpenOrders = GetOpenOrdersGridHoleFromLines(linesWithOrdersToOpenFact, lastPrice);

            if (ordersToCancelOpenOrders != null
                && ordersToCancelOpenOrders.Count > 0)
            {
                for (int i = 0; i < ordersToCancelOpenOrders.Count; i++)
                {
                    //Tab.SetNewLogMessage("Отзыв ордера по дыре в сетке", LogMessageType.System);
                    tab.CloseOrder(ordersToCancelOpenOrders[i]);
                }

                return ordersToCancelOpenOrders.Count;
            }

            // 4 убираем ордера лишние на закрытие.
            // Когда в сетке больше ордеров чем указал пользователь
            // И когда объём на закрытие не совпадает с тем что в ордере закрывающем

            if (GridType == TradeGridPrimeType.MarketMaking)
            {
                List<Order> ordersToCancelCloseOrders = GetCloseOrdersGridHole();

                if (ordersToCancelCloseOrders != null
                    && ordersToCancelCloseOrders.Count > 0)
                {
                    for (int i = 0; i < ordersToCancelCloseOrders.Count; i++)
                    {
                        tab.CloseOrder(ordersToCancelCloseOrders[i]);
                    }

                    return ordersToCancelCloseOrders.Count;
                }
            }

            return 0;
        }

        private List<Order> GetOrdersBadPriceToGrid()
        {
            return GetOrdersBadPriceToGridFromLines(GetLinesWithOpenOrdersFact(), GetLinesWithClosingOrdersFact());
        }

        private List<Order> GetOrdersBadPriceToGridFromLines(List<TradeGridLine> linesWithOrdersToOpenFact, List<TradeGridLine> linesWithOrdersToCloseFact)
        {
            // 1 смотрим совпадение цен у ордера на открытие с ценой открытия линии 
            // 2 смотрим совпадиние цен у ордера на закрытие с ценой закрытия линии

            List<Order> ordersToCancel = new List<Order>();

            for (int i = 0; linesWithOrdersToOpenFact != null && i < linesWithOrdersToOpenFact.Count; i++)
            {
                Position position = linesWithOrdersToOpenFact[i].Position;
                TradeGridLine currentLine = linesWithOrdersToOpenFact[i];

                if (position.OpenActive)
                {
                    if (TryGetLastOrder(position.OpenOrders, out Order openOrder) == false)
                    {
                        continue;
                    }

                    if (openOrder.Price != currentLine.PriceEnter)
                    {
                        ordersToCancel.Add(openOrder);
                    }
                }
            }

            for (int i = 0; linesWithOrdersToCloseFact != null && i < linesWithOrdersToCloseFact.Count; i++)
            {
                Position position = linesWithOrdersToCloseFact[i].Position;
                TradeGridLine currentLine = linesWithOrdersToCloseFact[i];

                if (position.CloseActive
                    && currentLine.CanReplaceExitOrder == true)
                {
                    if (TryGetLastOrder(position.CloseOrders, out Order closeOrder) == false)
                    {
                        continue;
                    }

                    if (GridType == TradeGridPrimeType.MarketMaking)
                    {
                        if (closeOrder.Price != currentLine.PriceExit
                         && closeOrder.TypeOrder != OrderPriceType.Market)
                        {
                            ordersToCancel.Add(closeOrder);
                        }
                    }
                    else if (GridType == TradeGridPrimeType.OpenPosition)
                    {
                        if (closeOrder.Price != position.ProfitOrderPrice
                        && closeOrder.TypeOrder != OrderPriceType.Market)
                        {
                            ordersToCancel.Add(closeOrder);
                        }
                    }
                }
            }

            return ordersToCancel;
        }

        private List<Order> GetOrdersBadLinesMaxCount()
        {
            return GetOrdersBadLinesMaxCountFromLines(GetLinesWithOpenOrdersFact());
        }

        private List<Order> GetOrdersBadLinesMaxCountFromLines(List<TradeGridLine> linesWithOrdersToOpenFact)
        {
            int maxOpenOrdersInMarket = Math.Max(0, MaxOpenOrdersInMarket);
            if (linesWithOrdersToOpenFact == null
                || linesWithOrdersToOpenFact.Count == 0
                || maxOpenOrdersInMarket >= linesWithOrdersToOpenFact.Count)
            {
                return new List<Order>();
            }

            List<Order> ordersToCancel = new List<Order>(linesWithOrdersToOpenFact.Count - maxOpenOrdersInMarket);

            // 1 Открытие. Смотрим чтобы не было ордеров больше чем указал пользователь

            for (int i = maxOpenOrdersInMarket; i < linesWithOrdersToOpenFact.Count; i++)
            {
                Position curPosition = linesWithOrdersToOpenFact[i].Position;
                if (TryGetLastOrder(curPosition.OpenOrders, out Order order))
                {
                    ordersToCancel.Add(order);
                }
            }

            return ordersToCancel;
        }

        private List<Order> GetOpenOrdersGridHole()
        {
            BotTabSimple tab = Tab;
            TradeGridCreator gridCreator = GridCreator;
            if (tab == null || gridCreator == null)
            {
                return null;
            }

            if (TryGetLastCandle(tab.CandlesAll, out Candle lastCandle) == false)
            {
                return null;
            }

            return GetOpenOrdersGridHoleFromLines(GetLinesWithOpenOrdersFact(), lastCandle.Close);
        }

        private List<Order> GetOpenOrdersGridHoleFromLines(List<TradeGridLine> linesWithOrdersToOpenFact, decimal lastPrice)
        {
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return null;
            }

            // 1 берём текущие линии с позициями

            List<TradeGridLine> linesWithOrdersToOpenNeed = GetLinesWithOpenOrdersNeed(lastPrice);

            if (linesWithOrdersToOpenFact == null ||
                linesWithOrdersToOpenFact.Count == 0)
            {
                return null;
            }

            if (linesWithOrdersToOpenNeed == null ||
                linesWithOrdersToOpenNeed.Count == 0)
            {
                return null;
            }

            // 2 смотрим, Стоит ли первый ордер на своём месте

            TradeGridLine firstLineFirstNeed = linesWithOrdersToOpenNeed[0];
            TradeGridLine firstLineFirstFact = linesWithOrdersToOpenFact[0];

            TradeGridLine firstLineLastNeed = linesWithOrdersToOpenNeed[^1];
            TradeGridLine firstLineLastFact = linesWithOrdersToOpenFact[^1];

            if (firstLineFirstNeed == null
                || firstLineFirstFact == null
                || firstLineLastNeed == null
                || firstLineLastFact == null)
            {
                return null;
            }

            if (firstLineFirstFact.PriceEnter == firstLineFirstNeed.PriceEnter
                && firstLineLastFact.PriceEnter == firstLineLastNeed.PriceEnter)
            {// всё в порядке
                return null;
            }

            if (linesWithOrdersToOpenFact.Count >= linesWithOrdersToOpenNeed.Count)
            {
                TradeGridLine lastLine = linesWithOrdersToOpenFact[^1];
                if (lastLine?.Position == null)
                {
                    return null;
                }

                if (TryGetLastOrder(lastLine.Position.OpenOrders, out Order order))
                {
                    return new List<Order>(1) { order };
                }
            }

            return null;
        }

        private List<Order> GetCloseOrdersGridHole()
        {
            List<TradeGridLine> linesOpenPoses = GetLinesWithOpenPosition();
            int maxCloseOrdersInMarket = Math.Max(0, MaxCloseOrdersInMarket);

            List<Order> ordersToCancel = new List<Order>();

            // 1 отправляем на отзыв ордера которые за пределами желаемого пользователем кол-ва

            for (int i = 0; i < linesOpenPoses.Count - maxCloseOrdersInMarket; i++)
            {
                Position pos = linesOpenPoses[i].Position;
                TradeGridLine line = linesOpenPoses[i];

                if (pos.CloseActive == true)
                {
                    if (TryGetLastOrder(pos.CloseOrders, out Order order))
                    {
                        ordersToCancel.Add(order);
                    }
                }
            }

            // 2 отправляем на отзыв ордера которые с не верным объёмом

            for (int i = 0; i < linesOpenPoses.Count; i++)
            {
                Position pos = linesOpenPoses[i].Position;
                TradeGridLine line = linesOpenPoses[i];

                if (pos.CloseActive == false)
                {
                    continue;
                }

                if (TryGetLastOrder(pos.CloseOrders, out Order orderToClose) == false)
                {
                    continue;
                }

                if (orderToClose.Volume != pos.OpenVolume)
                {
                    bool isInArray = false;

                    for (int j = 0; j < ordersToCancel.Count; j++)
                    {
                        if (ordersToCancel[j].NumberUser == orderToClose.NumberUser)
                        {
                            isInArray = true;
                            break;
                        }
                    }
                    if (isInArray == false)
                    {
                        ordersToCancel.Add(orderToClose);
                    }
                }
            }

            return ordersToCancel;
        }

        private int TryCancelOpeningOrders()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return 0;
            }

            List<TradeGridLine> lines = GetLinesWithOpenOrdersFact();

            int cancelledOrders = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                Position pos = line.Position;

                if (pos == null
                    || pos.OpenActive == false)
                {
                    continue;
                }

                if (TryGetLastOrder(pos.OpenOrders, out Order order) == false)
                {
                    continue;
                }

                if (order.NumberMarket != null)
                {
                    tab.CloseOrder(order);
                    cancelledOrders++;
                }
            }

            return cancelledOrders;
        }

        private void TrySetClosingOrders(decimal lastPrice)
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }
            Security security = tab.Security;
            if (security == null)
            {
                return;
            }

            CheckWrongCloseOrders();

            List<TradeGridLine> linesOpenPoses = GetLinesWithOpenPosition();

            int startIndex = linesOpenPoses.Count - MaxCloseOrdersInMarket;

            if (startIndex < 0)
            {
                startIndex = 0;
            }

            for (int i = startIndex; i < linesOpenPoses.Count; i++)
            {
                TradeGridLine line = linesOpenPoses[i];
                if (line == null)
                {
                    continue;
                }

                Position pos = line.Position;
                if (pos == null)
                {
                    continue;
                }

                if (pos.CloseActive == true)
                {
                    continue;
                }

                decimal volume = pos.OpenVolume;

                if (CheckMicroVolumes == true
                    && tab.CanTradeThisVolume(volume) == false)
                {
                    continue;
                }

                if (security.PriceLimitHigh != 0
                 && security.PriceLimitLow != 0)
                {
                    if (line.PriceExit > security.PriceLimitHigh
                        || line.PriceExit < security.PriceLimitLow)
                    {
                        continue;
                    }
                }

                if (tab.StartProgram == StartProgram.IsOsTrader
                    && MaxDistanceToOrdersPercent != 0
                    && lastPrice != 0)
                {
                    decimal maxPriceUp = lastPrice + lastPrice * (MaxDistanceToOrdersPercent / 100);
                    decimal minPriceDown = lastPrice - lastPrice * (MaxDistanceToOrdersPercent / 100);

                    if (line.PriceExit > maxPriceUp
                     || line.PriceExit < minPriceDown)
                    {
                        continue;
                    }
                }

                tab.CloseAtLimitUnsafe(pos, line.PriceExit, volume);
            }
        }

        private void CheckWrongCloseOrders()
        {
            BotTabSimple tab = Tab;
            TradeGridCreator gridCreator = GridCreator;
            if (tab == null || gridCreator == null || tab.StartProgram != StartProgram.IsOsTrader)
            {
                return;
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;
            if (linesAll == null || linesAll.Count == 0)
            {
                return;
            }

            for (int i = 0; i < linesAll.Count; i++)
            {
                TradeGridLine curLine = linesAll[i];
                if (curLine == null)
                {
                    continue;
                }
                Position pos = curLine.Position;

                if (pos == null)
                {
                    continue;
                }

                decimal volumePosOpen = pos.OpenVolume;

                if (pos.CloseActive == true)
                {
                    if (TryGetLastOrder(pos.CloseOrders, out Order orderToClose) == false)
                    {
                        continue;
                    }
                    decimal volumeCloseOrder = orderToClose.Volume;
                    decimal volumeExecuteCloseOrder = orderToClose.VolumeExecute;

                    if (volumePosOpen != (volumeCloseOrder - volumeExecuteCloseOrder))
                    {
                        tab.CloseOrder(orderToClose);
                    }
                }
            }
        }

        private int TryCancelClosingOrders()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return 0;
            }

            List<TradeGridLine> lines = GetLinesWithOpenPosition();

            int cancelledOrders = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                Position pos = line.Position;

                if (pos == null
                    || pos.CloseActive == false)
                {
                    continue;
                }

                if (TryGetLastOrder(pos.CloseOrders, out Order order) == false)
                {
                    continue;
                }

                if (order.NumberMarket != null
                   && order.TypeOrder != OrderPriceType.Market)
                {
                    tab.CloseOrder(order);
                    cancelledOrders++;
                }
            }

            return cancelledOrders;
        }

        private void TrySetOpenOrders()
        {
            BotTabSimple tab = Tab;
            TradeGridCreator gridCreator = GridCreator;
            if (tab == null || gridCreator == null)
            {
                return;
            }
            Security security = tab.Security;
            if (security == null)
            {
                return;
            }

            if (TryGetLastCandle(tab.CandlesAll, out Candle lastCandle) == false)
            {
                return;
            }

            decimal lastPrice = lastCandle.Close;

            if (lastPrice == 0)
            {
                return;
            }

            if (tab.PriceBestAsk == 0
                || tab.PriceBestBid == 0)
            {
                return;
            }

            // 1 берём текущие линии с позициями

            List<TradeGridLine> linesWithOrdersToOpenNeed = GetLinesWithOpenOrdersNeed(lastPrice);

            List<TradeGridLine> linesWithOrdersToOpenFact = GetLinesWithOpenOrdersFact();

            // 2 ничего не делаем если уже кол-во ордеров максимально

            if (linesWithOrdersToOpenFact.Count >= MaxOpenOrdersInMarket)
            {
                return;
            }

            // 3 открываемся по новой схеме

            for (int i = 0; i < linesWithOrdersToOpenNeed.Count; i++)
            {
                TradeGridLine curLineNeed = linesWithOrdersToOpenNeed[i];
                if (curLineNeed == null)
                {
                    continue;
                }

                if (curLineNeed.Position != null)
                {
                    continue;
                }

                // открываемся. Позиции по линии нет

                decimal volume = gridCreator.GetVolume(curLineNeed, tab);

                Position newPosition = null;

                if (curLineNeed.Side == Side.Buy)
                {
                    decimal price = curLineNeed.PriceEnter;

                    if (OpenOrdersMakerOnly == false
                        && security.PriceLimitHigh != 0
                        && price >= security.PriceLimitHigh)
                    {
                        price = security.PriceLimitHigh - (security.PriceStep * 10);
                    }

                    newPosition = tab.BuyAtLimit(volume, price);
                }
                else if (curLineNeed.Side == Side.Sell)
                {
                    decimal price = curLineNeed.PriceEnter;

                    if (OpenOrdersMakerOnly == false
                        && security.PriceLimitLow != 0
                        && price <= security.PriceLimitLow)
                    {
                        price = security.PriceLimitLow + (security.PriceStep * 10);
                    }

                    newPosition = tab.SellAtLimit(volume, price);
                }

                if (newPosition != null)
                {
                    curLineNeed.Position = newPosition;
                    curLineNeed.PositionNum = newPosition.Number;

                    if (_firstTradePrice == 0)
                    {
                        _firstTradePrice = curLineNeed.PriceEnter;
                    }

                    if (_firstTradeTime == DateTime.MinValue)
                    {
                        _firstTradeTime = tab.TimeServerCurrent;
                    }

                    _needToSave = true;
                }

                linesWithOrdersToOpenFact = GetLinesWithOpenOrdersFact();

                if (linesWithOrdersToOpenFact.Count >= MaxOpenOrdersInMarket)
                {
                    return;
                }
            }
        }

        private DateTime _lastCheckJournalTime = DateTime.MinValue;

        private void TryFreeJournal()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }

            if (AutoClearJournalIsOn == false)
            {
                return;
            }

            if (_lastCheckJournalTime.AddSeconds(10) > DateTime.Now)
            {
                return;
            }

            _lastCheckJournalTime = DateTime.Now;

            Position[] positions = tab.PositionsAll.ToArray();

            // 1 удаляем позиции с OpeningFail без всяких условий

            for (int i = 0; i < positions.Length; i++)
            {
                Position pos = positions[i];

                if (pos == null)
                {
                    continue;
                }

                if (pos.State == PositionStateType.OpeningFail
                    && pos.OpenVolume == 0
                    && pos.OpenActive == false
                    && pos.CloseActive == false)
                {
                    TryDeletePositionsFromJournal(pos);
                }
            }

            // 2 удаляем позиции со статусом Done, если пользователь это включил        

            int curDonePosInJournal = 0;

            for (int i = positions.Length - 1; i >= 0; i--)
            {
                Position pos = positions[i];

                if (pos == null)
                {
                    continue;
                }

                if (pos.State != PositionStateType.Done)
                {
                    continue;
                }

                if (pos.OpenVolume != 0)
                {
                    continue;
                }

                if (pos.OpenActive == true
                    || pos.CloseActive == true)
                {
                    continue;
                }

                curDonePosInJournal++;

                if (curDonePosInJournal > MaxClosePositionsInJournal)
                {
                    TryDeletePositionsFromJournal(pos);
                }
            }
        }

        private void TryDeletePositionsFromJournal(Position position)
        {
            TradeGridCreator gridCreator = GridCreator;
            BotTabSimple tab = Tab;
            if (gridCreator == null || tab == null || position == null || tab._journal == null)
            {
                return;
            }

            List<TradeGridLine> lines = gridCreator.Lines;
            if (lines == null || lines.Count == 0)
            {
                return;
            }

            bool isInGridNow = false;

            for (int i = 0; lines != null && i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                if (line.PositionNum == position.Number)
                {
                    isInGridNow = true;
                    break;
                }
            }

            if (isInGridNow == false)
            {
                tab._journal.DeletePosition(position);
            }
        }

        private void TryDeleteDonePositions()
        {
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return;
            }

            List<TradeGridLine> lines = gridCreator.Lines;

            if (lines == null)
            {
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                if (line.Position != null)
                {
                    // Позиция была закрыта
                    // Открывающий ордер был отозван
                    if (line.Position.State == PositionStateType.Done
                        ||
                        (line.Position.State == PositionStateType.OpeningFail
                        && line.Position.OpenActive == false))
                    {
                        line.Position = null;
                        line.PositionNum = -1;
                    }

                    else if (line.Position.State == PositionStateType.Deleted)
                    {
                        line.Position = null;
                        line.PositionNum = -1;
                    }
                }
            }
        }

        private void TryFindPositionsInJournalAfterReconnect()
        {
            TradeGridCreator gridCreator = GridCreator;
            BotTabSimple tab = Tab;
            if (gridCreator == null || tab == null)
            {
                return;
            }

            List<TradeGridLine> lines = gridCreator.Lines;
            List<Position> positions = tab.PositionsAll;

            if (lines == null || positions == null)
            {
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                // проблема 1. Номер позиции есть - самой позиции нет. 
                // произошёл перезапуск терминала. Ищем позу в журнале
                if (line.PositionNum != -1
                    && line.Position == null)
                {
                    bool isInArray = false;

                    for (int j = 0; j < positions.Count; j++)
                    {
                        Position journalPosition = positions[j];
                        if (journalPosition == null)
                        {
                            continue;
                        }

                        if (journalPosition.Number == line.PositionNum)
                        {
                            isInArray = true;
                            line.Position = journalPosition;
                            break;
                        }
                    }

                    if (isInArray == false)
                    {
                        line.Position = null;
                        line.PositionNum = -1;
                    }
                }
            }
        }

        #endregion

        #region Forced Close regime logic

        private void TryForcedCloseGrid()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }

            List<TradeGridLine> lines = GetLinesWithOpenPosition();

            bool havePositions = false;

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                if (line.Position == null
                    || line.Position.CloseActive == true)
                {
                    continue;
                }

                Position pos = line.Position;

                if (pos.State != PositionStateType.Done
                    || pos.OpenVolume >= 0)
                {
                    if (CheckMicroVolumes == true
                    && tab.CanTradeThisVolume(pos.OpenVolume) == false)
                    {
                        string message = "Micro volume detected. Position deleted \n";
                        message += "Position volume: " + pos.OpenVolume + "\n";
                        message += "Security name: " + pos.SecurityName;
                        SendNewLogMessage(message, LogMessageType.Signal);

                        line.Position = null;
                        line.PositionNum = -1;
                        continue;
                    }

                    tab.CloseAtMarket(pos, pos.OpenVolume);
                    havePositions = true;
                }
            }

            if (Regime == TradeGridRegime.CloseForced
                && havePositions == false)
            {
                string message = "Close Forced regime ended. No positions \n";
                message += "New regime: Off";
                SendNewLogMessage(message, LogMessageType.Signal);
                Regime = TradeGridRegime.Off;
                RePaintGrid();
                _needToSave = true;
            }
        }

        private static bool TryGetLastCandle(List<Candle> candles, out Candle candle)
        {
            candle = null;

            if (candles == null || candles.Count == 0)
            {
                return false;
            }

            candle = candles[candles.Count - 1];
            return candle != null;
        }

        #endregion

        #region Public interface

        public decimal FirstPriceReal
        {
            get
            {
                return _firstTradePrice;
            }
        }
        private decimal _firstTradePrice;

        public int OpenPositionsCount
        {
            get
            {

                return _openPositionsBySession;
            }
        }
        private int _openPositionsBySession;

        public decimal OpenVolumeByLines
        {
            get
            {
                // 1 берём позиции по сетке

                List<TradeGridLine> linesWithPositions = GetLinesWithOpenPosition();

                if (linesWithPositions == null
                    || linesWithPositions.Count == 0)
                {
                    return 0;
                }

                decimal result = 0;

                for (int i = 0; i < linesWithPositions.Count; i++)
                {
                    TradeGridLine line = linesWithPositions[i];
                    if (line == null || line.Position == null)
                    {
                        continue;
                    }

                    result += line.Position.OpenVolume;
                }

                return result;
            }
        }

        public decimal AllVolumeInLines
        {
            get
            {
                TradeGridCreator gridCreator = GridCreator;
                if (gridCreator == null)
                {
                    return 0;
                }

                List<TradeGridLine> lines = gridCreator.Lines;

                if (lines == null
                    || lines.Count == 0)
                {
                    return 0;
                }

                decimal result = 0;

                for (int i = 0; i < lines.Count; i++)
                {
                    TradeGridLine line = lines[i];
                    if (line == null)
                    {
                        continue;
                    }

                    result += line.Volume;
                }

                return result;
            }
        }

        public DateTime FirstTradeTime
        {
            get
            {
                return _firstTradeTime;
            }
        }
        private DateTime _firstTradeTime;

        public bool HaveOrdersWithNoMarketOrders()
        {
            // 1 берём все уровни с позициями
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return false;
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                TradeGridLine line = linesAll[i];
                if (line == null || line.Position == null)
                {
                    continue;
                }

                Position position = line.Position;

                if (position.OpenActive)
                {
                    if (TryGetLastOrder(position.OpenOrders, out Order openOrder) == false)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(openOrder.NumberMarket))
                    {
                        if (openOrder.State == OrderStateType.None
                            && _lastNoneOrderTime == DateTime.MinValue)
                        {
                            _lastNoneOrderTime = DateTime.Now;
                        }
                        else if (openOrder.State == OrderStateType.None
                            && _lastNoneOrderTime.AddMinutes(5) < DateTime.Now)
                        {// 5ть минут висит ордер со статусом NONE. Утерян
                            if (TryRemoveLastOrder(position.OpenOrders))
                            {
                                SendNewLogMessage("Remove NONE open order. Five minutes rule", LogMessageType.Signal);
                            }
                            return true;
                        }

                        return true;
                    }
                }

                if (position.CloseActive)
                {
                    if (TryGetLastOrder(position.CloseOrders, out Order closeOrder) == false)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(closeOrder.NumberMarket))
                    {
                        if (closeOrder.State == OrderStateType.None
                            && _lastNoneOrderTime == DateTime.MinValue)
                        {
                            _lastNoneOrderTime = DateTime.Now;
                        }
                        else if (closeOrder.State == OrderStateType.None
                            && _lastNoneOrderTime.AddMinutes(5) < DateTime.Now)
                        {// 5ть минут висит ордер со статусом NONE. Утерян
                            if (TryRemoveLastOrder(position.CloseOrders))
                            {
                                SendNewLogMessage("Remove NONE close order. Five minutes rule", LogMessageType.Signal);
                            }
                            return true;
                        }

                        return true;
                    }
                }
            }

            if (_lastNoneOrderTime != DateTime.MinValue)
            {
                _lastNoneOrderTime = DateTime.MinValue;
            }

            return false;
        }

        private DateTime _lastNoneOrderTime;

        public bool HaveOrdersTryToCancelLastSecond()
        {
            // возвращает true - если есть ордер который уже отослан на отзыв но всё ещё в статусе Active. За последние 3 секунды.
            // если true - значит последние операции ещё не завершены по снятию ордеров

            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return false;
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                TradeGridLine line = linesAll[i];
                if (line == null || line.Position == null)
                {
                    continue;
                }

                Position position = line.Position;

                if (position.OpenActive)
                {
                    if (TryGetLastOrder(position.OpenOrders, out Order openOrder) == false)
                    {
                        continue;
                    }

                    if (openOrder.State == OrderStateType.Active
                        && openOrder.IsSendToCancel == true)
                    {
                        if (openOrder.LastCancelTryLocalTime.AddSeconds(3) > DateTime.Now)
                        {
                            return true;
                        }
                    }
                }

                if (position.CloseActive)
                {
                    if (TryGetLastOrder(position.CloseOrders, out Order closeOrder) == false)
                    {
                        continue;
                    }

                    if (closeOrder.State == OrderStateType.Active
                        && closeOrder.IsSendToCancel == true)
                    {
                        if (closeOrder.LastCancelTryLocalTime.AddSeconds(3) > DateTime.Now)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool TryRemoveLastOrder(List<Order> orders)
        {
            if (orders == null || orders.Count == 0)
            {
                return false;
            }

            orders.RemoveAt(orders.Count - 1);
            return true;
        }

        public bool HaveCloseOrders
        {
            get
            {
                // 1 если уже есть позиции с ордерами на закрытие. Ничего не делаем

                List<TradeGridLine> linesWithOpenPositions = GetLinesWithOpenPosition();

                for (int i = 0; i < linesWithOpenPositions.Count; i++)
                {
                    TradeGridLine line = linesWithOpenPositions[i];
                    if (line == null)
                    {
                        continue;
                    }

                    Position pos = line.Position;

                    if (pos == null)
                    {
                        continue;
                    }

                    if (pos.CloseActive == true)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool HaveOpenPositionsByGrid
        {
            get
            {
                List<TradeGridLine> linesWithPositions = GetLinesWithOpenPosition();

                if (linesWithPositions != null &&
                    linesWithPositions.Count != 0)
                {
                    return true;
                }

                return false;
            }
        }

        public bool HaveOrdersInMarketInGrid
        {
            get
            {
                List<TradeGridLine> linesWithOpenOrders = GetLinesWithOpenOrdersFact();
                List<TradeGridLine> linesWithCloseOrders = GetLinesWithClosingOrdersFact();

                if (linesWithOpenOrders != null
                    && linesWithOpenOrders.Count > 0)
                {
                    return true;
                }
                if (linesWithCloseOrders != null
                  && linesWithCloseOrders.Count > 0)
                {
                    return true;
                }

                return false;
            }
        }

        public decimal MiddleEntryPrice
        {
            get
            {
                // 1 берём позиции по сетке

                List<Position> positions = GetPositionByGrid();

                if (positions == null
                    || positions.Count == 0)
                {
                    return 0;
                }

                // 2 берём из позиций все MyTrade по открывающим ордерам

                List<MyTrade> tradesOpenPos = new List<MyTrade>();

                for (int i = 0; i < positions.Count; i++)
                {
                    Position currentPos = positions[i];

                    if (currentPos == null)
                    {
                        continue;
                    }

                    List<Order> orders = currentPos.OpenOrders;

                    if (orders == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < orders.Count; j++)
                    {
                        Order currentOrder = orders[j];

                        if (currentOrder == null)
                        {
                            continue;
                        }

                        List<MyTrade> myTrades = currentOrder.MyTrades;

                        if (myTrades == null
                            || myTrades.Count == 0)
                        {
                            continue;
                        }

                        tradesOpenPos.AddRange(myTrades);
                    }
                }

                if (tradesOpenPos.Count == 0)
                {
                    return 0;
                }

                // 3 считаем среднюю цену входа

                decimal summ = 0;
                decimal volume = 0;

                for (int i = 0; i < tradesOpenPos.Count; i++)
                {
                    MyTrade trade = tradesOpenPos[i];

                    if (trade == null)
                    {
                        continue;
                    }

                    volume += trade.Volume;
                    summ += trade.Volume * trade.Price;
                }

                if (volume == 0)
                {
                    return 0;
                }

                decimal result = summ / volume;

                return result;
            }
        }

        public decimal MaxGridPrice
        {
            get
            {
                try
                {
                    TrailingUp trailingUp = TrailingUp;
                    if (trailingUp == null)
                    {
                        return 0;
                    }

                    return trailingUp.MaxGridPrice;
                }
                catch (Exception e)
                {
                    SendNewLogMessage(e.ToString(), LogMessageType.Error);
                    return 0;
                }
            }
        }

        public decimal MinGridPrice
        {
            get
            {
                try
                {
                    TrailingUp trailingUp = TrailingUp;
                    if (trailingUp == null)
                    {
                        return 0;
                    }

                    return trailingUp.MinGridPrice;
                }
                catch (Exception e)
                {
                    SendNewLogMessage(e.ToString(), LogMessageType.Error);
                    return 0;
                }
            }
        }

        public List<TradeGridLine> GetLinesWithOpenPosition()
        {
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return new List<TradeGridLine>();
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;
            if (linesAll == null || linesAll.Count == 0)
            {
                return new List<TradeGridLine>();
            }

            int expectedCapacity = linesAll.Count;
            int maxOpenOrdersInMarket = Math.Max(0, MaxOpenOrdersInMarket);
            if (maxOpenOrdersInMarket > 0)
            {
                expectedCapacity = Math.Min(expectedCapacity, Math.Max(maxOpenOrdersInMarket * 4, 8));
            }

            List<TradeGridLine> linesWithPositionFact = new List<TradeGridLine>(expectedCapacity);

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                TradeGridLine line = linesAll[i];
                if (line == null)
                {
                    continue;
                }

                if (line.Position != null
                    && line.Position.OpenVolume != 0)
                {
                    linesWithPositionFact.Add(line);
                }
            }
            return linesWithPositionFact;
        }

        public List<Position> GetPositionByGrid()
        {
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return new List<Position>();
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;

            if (linesAll == null ||
                linesAll.Count == 0)
            {
                return new List<Position>();
            }

            List<Position> positions = new List<Position>(linesAll.Count);

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                TradeGridLine line = linesAll[i];
                if (line == null)
                {
                    continue;
                }

                Position position = line.Position;

                if (position != null)
                {
                    positions.Add(position);
                }
            }
            return positions;
        }

        public List<TradeGridLine> GetLinesWithOpenOrdersNeed(decimal lastPrice)
        {
            TradeGridCreator gridCreator = GridCreator;
            BotTabSimple tab = Tab;

            if (gridCreator == null || tab == null)
            {
                return new List<TradeGridLine>();
            }
            Security security = tab.Security;
            if (security == null)
            {
                return new List<TradeGridLine>();
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;
            if (linesAll == null || linesAll.Count == 0)
            {
                return new List<TradeGridLine>();
            }

            int expectedCount = linesAll.Count;
            int maxOpenOrdersInMarket = Math.Max(0, MaxOpenOrdersInMarket);
            if (maxOpenOrdersInMarket > 0)
            {
                expectedCount = Math.Min(expectedCount, maxOpenOrdersInMarket);
            }

            List<TradeGridLine> linesWithOrdersToOpenNeed = new List<TradeGridLine>(expectedCount);

            decimal maxPriceUp = 0;
            decimal minPriceDown = 0;

            if (tab.StartProgram == StartProgram.IsOsTrader
                && MaxDistanceToOrdersPercent != 0)
            {
                maxPriceUp = lastPrice + lastPrice * (MaxDistanceToOrdersPercent / 100);
                minPriceDown = lastPrice - lastPrice * (MaxDistanceToOrdersPercent / 100);
            }

            if (gridCreator.GridSide == Side.Buy)
            {
                for (int i = 0; i < linesAll.Count; i++)
                {
                    TradeGridLine curLine = linesAll[i];
                    if (curLine == null)
                    {
                        continue;
                    }

                    Position position = curLine.Position;

                    if (position != null
                        && position.OpenVolume > 0
                        && position.OpenActive == false)
                    {
                        continue;
                    }

                    if (security.PriceLimitHigh != 0
                        && security.PriceLimitLow != 0)
                    {
                        if (OpenOrdersMakerOnly == true
                            &&
                            (curLine.PriceEnter > security.PriceLimitHigh
                            || curLine.PriceEnter < security.PriceLimitLow))
                        {
                            continue;
                        }

                        if (OpenOrdersMakerOnly == false
                            && curLine.Side == Side.Buy
                            && curLine.PriceEnter < security.PriceLimitLow)
                        {
                            continue;
                        }
                        if (OpenOrdersMakerOnly == false
                            && curLine.Side == Side.Sell
                            && curLine.PriceEnter > security.PriceLimitHigh)
                        {
                            continue;
                        }
                    }

                    if (maxPriceUp != 0
                        && minPriceDown != 0)
                    {
                        if (curLine.PriceEnter > maxPriceUp
                         || curLine.PriceEnter < minPriceDown)
                        {
                            continue;
                        }
                    }

                    if (OpenOrdersMakerOnly
                        && curLine.PriceEnter > lastPrice)
                    {
                        continue;
                    }

                    linesWithOrdersToOpenNeed.Add(curLine);

                    if (linesWithOrdersToOpenNeed.Count >= MaxOpenOrdersInMarket)
                    {
                        break;
                    }
                }
            }
            else if (gridCreator.GridSide == Side.Sell)
            {
                for (int i = 0; i < linesAll.Count; i++)
                {
                    TradeGridLine curLine = linesAll[i];
                    if (curLine == null)
                    {
                        continue;
                    }

                    Position position = curLine.Position;

                    if (position != null
                        && position.OpenVolume > 0
                        && position.OpenActive == false)
                    {
                        continue;
                    }

                    if (security.PriceLimitHigh != 0
                        && security.PriceLimitLow != 0)
                    {
                        if (curLine.PriceEnter > security.PriceLimitHigh
                            || curLine.PriceEnter < security.PriceLimitLow)
                        {
                            continue;
                        }
                    }

                    if (maxPriceUp != 0
                        && minPriceDown != 0)
                    {
                        if (curLine.PriceEnter > maxPriceUp
                         || curLine.PriceEnter < minPriceDown)
                        {
                            continue;
                        }
                    }

                    if (OpenOrdersMakerOnly
                        && curLine.PriceEnter < lastPrice)
                    {
                        continue;
                    }

                    linesWithOrdersToOpenNeed.Add(curLine);

                    if (linesWithOrdersToOpenNeed.Count >= MaxOpenOrdersInMarket)
                    {
                        break;
                    }
                }
            }
            return linesWithOrdersToOpenNeed;
        }

        public List<TradeGridLine> GetLinesWithOpenOrdersFact()
        {
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return new List<TradeGridLine>();
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;
            if (linesAll == null || linesAll.Count == 0)
            {
                return new List<TradeGridLine>();
            }

            int expectedCapacity = linesAll.Count;
            int maxOpenOrdersInMarket = Math.Max(0, MaxOpenOrdersInMarket);
            if (maxOpenOrdersInMarket > 0)
            {
                expectedCapacity = Math.Min(expectedCapacity, Math.Max(maxOpenOrdersInMarket * 2, 4));
            }

            List<TradeGridLine> linesWithOpenOrder = new List<TradeGridLine>(expectedCapacity);

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                TradeGridLine line = linesAll[i];
                if (line == null)
                {
                    continue;
                }

                if (line.Position != null
                    && line.Position.OpenActive)
                {
                    linesWithOpenOrder.Add(line);
                }
            }
            return linesWithOpenOrder;
        }

        public List<TradeGridLine> GetLinesWithClosingOrdersFact()
        {
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return new List<TradeGridLine>();
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;
            if (linesAll == null || linesAll.Count == 0)
            {
                return new List<TradeGridLine>();
            }

            int expectedCapacity = linesAll.Count;
            int maxCloseOrdersInMarket = Math.Max(0, MaxCloseOrdersInMarket);
            if (maxCloseOrdersInMarket > 0)
            {
                expectedCapacity = Math.Min(expectedCapacity, Math.Max(maxCloseOrdersInMarket * 3, 4));
            }
            else
            {
                int maxOpenOrdersInMarket = Math.Max(0, MaxOpenOrdersInMarket);
                if (maxOpenOrdersInMarket > 0)
                {
                    expectedCapacity = Math.Min(expectedCapacity, Math.Max(maxOpenOrdersInMarket * 2, 8));
                }
            }

            List<TradeGridLine> linesWithCloseOrder = new List<TradeGridLine>(expectedCapacity);

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                TradeGridLine line = linesAll[i];
                if (line == null)
                {
                    continue;
                }

                if (line.Position != null
                    && line.Position.CloseActive)
                {
                    linesWithCloseOrder.Add(line);
                }
            }
            return linesWithCloseOrder;
        }

        #endregion

        #region Log

        public void SendNewLogMessage(string message, LogMessageType type)
        {
            if (message == null)
            {
                message = string.Empty;
            }

            if (type == LogMessageType.Error)
            {
                BotTabSimple tab = Tab;
                string botName = tab?.NameStrategy ?? "unknown";
                string securityName = tab?.Connector?.SecurityName ?? "unknown";

                message = "Grid error. Bot: " + botName + "\n"
                + "Security name: " + securityName + "\n"
                + message;
            }

            LogMessageEvent?.Invoke(message, type);

            if (LogMessageEvent == null && type == LogMessageType.Error)
            {
                ServerMaster.SendNewLogMessage(message, type);
            }
        }

        public event Action<string, LogMessageType>? LogMessageEvent;

        #endregion
    }

    public enum TradeGridPrimeType
    {
        MarketMaking,
        OpenPosition
    }

    public enum TradeGridRegime
    {
        Off,
        OffAndCancelOrders,
        On,
        CloseOnly,
        CloseForced
    }

    public enum TradeGridLogicEntryRegime
    {
        OnTrade,
        OncePerSecond
    }

    public enum OnOffRegime
    {
        On,
        Off
    }

    public enum TradeGridValueType
    {
        Absolute,
        Percent,
    }

    public enum TradeGridVolumeType
    {
        Contracts,
        ContractCurrency,
        DepositPercent
    }
}

