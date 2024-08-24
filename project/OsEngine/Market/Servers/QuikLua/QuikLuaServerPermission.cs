﻿namespace OsEngine.Market.Servers.QuikLua
{
    internal class QuikLuaServerPermission : IServerPermission
    {
        public ServerType ServerType
        {
            get { return ServerType.QuikLua; }
        }

        #region DataFeedPermissions

        public bool DataFeedTf1SecondCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf2SecondCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf5SecondCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf10SecondCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf15SecondCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf20SecondCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf30SecondCanLoad
        {
            get { return false; }
        }
        public bool DataFeedTf1MinuteCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf2MinuteCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf5MinuteCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf10MinuteCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf15MinuteCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf30MinuteCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf1HourCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf2HourCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTf4HourCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTfDayCanLoad
        {
            get { return false; }
        }
        public bool DataFeedTfTickCanLoad
        {
            get { return false; }
        }

        public bool DataFeedTfMarketDepthCanLoad
        {
            get { return false; }
        }

        #endregion

        #region Trade permission

        public bool MarketOrdersIsSupport
        {
            get { return false; }
        }

        public bool IsCanChangeOrderPrice
        {
            get { return false; }
        }

        public bool IsUseLotToCalculateProfit
        {
            get { return true; }
        }

        public TimeFramePermission TradeTimeFramePermission
        {
            get { return _tradeTimeFramePermission; }
        }
        private TimeFramePermission _tradeTimeFramePermission
    = new TimeFramePermission()
    {
        TimeFrameSec1IsOn = false,
        TimeFrameSec2IsOn = false,
        TimeFrameSec5IsOn = false,
        TimeFrameSec10IsOn = false,
        TimeFrameSec15IsOn = false,
        TimeFrameSec20IsOn = false,
        TimeFrameSec30IsOn = false,
        TimeFrameMin1IsOn = true,
        TimeFrameMin2IsOn = true,
        TimeFrameMin3IsOn = false,
        TimeFrameMin5IsOn = true,
        TimeFrameMin10IsOn = true,
        TimeFrameMin15IsOn = true,
        TimeFrameMin20IsOn = true,
        TimeFrameMin30IsOn = true,
        TimeFrameMin45IsOn = false,
        TimeFrameHour1IsOn = true,
        TimeFrameHour2IsOn = true,
        TimeFrameHour4IsOn = true,
        TimeFrameDayIsOn = true
    };

        public int WaitTimeSecondsAfterFirstStartToSendOrders
        {
            get { return 20; }
        }

        public bool UseStandartCandlesStarter
        {
            get { return true; }
        }

        public bool ManuallyClosePositionOnBoard_IsOn
        {
            get { return false; }
        }

        public string[] ManuallyClosePositionOnBoard_ValuesForTrimmingName
        {
            get { return null; }
        }

        public string[] ManuallyClosePositionOnBoard_ExceptionPositionNames
        {
            get { return null; }
        }

        public bool CanQueryOrdersAfterReconnect
        {
            get { return false; }
        }

        public bool CanQueryOrderStatus
        {
            get { return false; }
        }

        #endregion
    }
}