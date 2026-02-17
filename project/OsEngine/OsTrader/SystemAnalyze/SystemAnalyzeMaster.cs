/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Market;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace OsEngine.OsTrader.SystemAnalyze
{
    public class SystemUsageAnalyzeMaster
    {
        #region Service

        public static void Activate()
        {
            if (_worker == null)
            {
                _ramMemoryUsageAnalyze = new RamMemoryUsageAnalyze();
                _ramMemoryUsageAnalyze.RamUsageCollectionChange += _ramMemoryUsageAnalyze_RamUsageCollectionChange;

                _cpuUsageAnalyze = new CpuUsageAnalyze();
                _cpuUsageAnalyze.CpuUsageCollectionChange += _cpuUsageAnalyze_CpuUsageCollectionChange;

                _ecqUsageAnalyze = new EcqUsageAnalyze();
                _ecqUsageAnalyze.EcqUsageCollectionChange += _ecqUsageAnalyze_EcqUsageCollectionChange;

                _moqUsageAnalyze = new MoqUsageAnalyze();
                _moqUsageAnalyze.MoqUsageCollectionChange += _moqUsageAnalyze_MoqUsageCollectionChange;

                _worker = new Thread(WorkMethod);
                _worker.IsBackground = true;
                _worker.Start();
            }
        }

        private static void _ui_Closed(object sender, EventArgs e)
        {
            _ui = null;
        }

        private static RamMemoryUsageAnalyze _ramMemoryUsageAnalyze;

        private static CpuUsageAnalyze _cpuUsageAnalyze;

        private static EcqUsageAnalyze _ecqUsageAnalyze;

        private static MoqUsageAnalyze _moqUsageAnalyze;

        #endregion

        #region Settings

        private static SystemAnalyzeUi _ui;

        public static void ShowDialog()
        {
            try
            {
                if (_ui == null)
                {
                    _ui = new SystemAnalyzeUi();
                    _ui.Closed += _ui_Closed;
                    _ui.Show();
                }
                else
                {
                    if (_ui.WindowState == System.Windows.WindowState.Minimized)
                    {
                        _ui.WindowState = System.Windows.WindowState.Normal;
                    }

                    _ui.Activate();
                }
            }
            catch (Exception ex)
            {
                ServerMaster.SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error);
            }
        }

        public static bool RamCollectDataIsOn
        {
            get
            {
                return _ramMemoryUsageAnalyze.RamCollectDataIsOn;
            }
            set
            {
                _ramMemoryUsageAnalyze.RamCollectDataIsOn = value;
            }
        }

        public static bool CpuCollectDataIsOn
        {
            get
            {
                return _cpuUsageAnalyze.CpuCollectDataIsOn;
            }
            set
            {
                _cpuUsageAnalyze.CpuCollectDataIsOn = value;
            }
        }

        public static bool EcqCollectDataIsOn
        {
            get
            {
                return _ecqUsageAnalyze.EcqCollectDataIsOn;
            }
            set
            {
                _ecqUsageAnalyze.EcqCollectDataIsOn = value;
            }
        }

        public static bool MoqCollectDataIsOn
        {
            get
            {
                return _moqUsageAnalyze.MoqCollectDataIsOn;
            }
            set
            {
                _moqUsageAnalyze.MoqCollectDataIsOn = value;
            }
        }

        public static SavePointPeriod RamPeriodSavePoint
        {
            get
            {
                return _ramMemoryUsageAnalyze.RamPeriodSavePoint;
            }
            set
            {
                _ramMemoryUsageAnalyze.RamPeriodSavePoint = value;
            }
        }

        public static SavePointPeriod CpuPeriodSavePoint
        {
            get
            {
                return _cpuUsageAnalyze.CpuPeriodSavePoint;
            }
            set
            {
                _cpuUsageAnalyze.CpuPeriodSavePoint = value;
            }
        }

        public static SavePointPeriod EcqPeriodSavePoint
        {
            get
            {
                return _ecqUsageAnalyze.EcqPeriodSavePoint;
            }
            set
            {
                _ecqUsageAnalyze.EcqPeriodSavePoint = value;
            }
        }

        public static SavePointPeriod MoqPeriodSavePoint
        {
            get
            {
                return _moqUsageAnalyze.MoqPeriodSavePoint;
            }
            set
            {
                _moqUsageAnalyze.MoqPeriodSavePoint = value;
            }
        }

        public static int RamPointsMax
        {
            get
            {
                return _ramMemoryUsageAnalyze.RamPointsMax;
            }
            set
            {
                _ramMemoryUsageAnalyze.RamPointsMax = value;
            }
        }

        public static int CpuPointsMax
        {
            get
            {
                return _cpuUsageAnalyze.CpuPointsMax;
            }
            set
            {
                _cpuUsageAnalyze.CpuPointsMax = value;
            }
        }

        public static int EcqPointsMax
        {
            get
            {
                return _ecqUsageAnalyze.EcqPointsMax;
            }
            set
            {
                _ecqUsageAnalyze.EcqPointsMax = value;
            }
        }

        public static int MoqPointsMax
        {
            get
            {
                return _moqUsageAnalyze.MoqPointsMax;
            }
            set
            {
                _moqUsageAnalyze.MoqPointsMax = value;
            }
        }

        #endregion

        #region Data

        public static List<SystemUsagePointRam> ValuesRam
        {
            get
            {
                return _ramMemoryUsageAnalyze.Values;
            }
        }

        public static List<SystemUsagePointCpu> ValuesCpu
        {
            get
            {
                return _cpuUsageAnalyze.Values;
            }
        }

        public static List<SystemUsagePointEcq> ValuesEcq
        {
            get
            {
                return _ecqUsageAnalyze.Values;
            }
        }

        public static List<SystemUsagePointMoq> ValuesMoq
        {
            get
            {
                return _moqUsageAnalyze.Values;
            }
        }

        public static SystemUsagePointRam LastValueRam
        {
            get
            {
                List < SystemUsagePointRam > values = _ramMemoryUsageAnalyze.Values;

                if(values != null 
                    && values.Count > 0)
                {
                    return values[^1];
                }
                else
                {
                    return null;
                }
            }
        }

        public static SystemUsagePointCpu LastValueCpu
        {
            get
            {
                List<SystemUsagePointCpu> values = _cpuUsageAnalyze.Values;

                if (values != null
                    && values.Count > 0)
                {
                    return values[^1];
                }
                else
                {
                    return null;
                }
            }
        }

        public static SystemUsagePointEcq LastValueEcq
        {
            get
            {
                List<SystemUsagePointEcq> values = _ecqUsageAnalyze.Values;

                if (values != null
                    && values.Count > 0)
                {
                    return values[^1];
                }
                else
                {
                    return null;
                }
            }
        }

        public static SystemUsagePointMoq LastValueMoq
        {
            get
            {
                List<SystemUsagePointMoq> values = _moqUsageAnalyze.Values;

                if (values != null
                    && values.Count > 0)
                {
                    return values[^1];
                }
                else
                {
                    return null;
                }
            }
        }

        public static int MarketDepthClearingCount
        {
            get
            {
                return _ecqUsageAnalyze.MarketDepthClearingCount;
            }
            set
            {
                _ecqUsageAnalyze.MarketDepthClearingCount = value;
            }
        }

        public static int BidAskClearingCount
        {
            get
            {
                return _ecqUsageAnalyze.BidAskClearingCount;
            }
            set
            {
                _ecqUsageAnalyze.BidAskClearingCount = value;
            }
        }

        public static int OrdersInQueue
        {
            get
            {
                return _moqUsageAnalyze.MaxOrdersInQueue;
            }
            set
            {
                _moqUsageAnalyze.MaxOrdersInQueue = value;
            }
        }

        #endregion

        #region Work thread

        private static Thread _worker;

        private static void WorkMethod()
        {
            while(true)
            {
                try
                {
                    if (MainWindow.ProccesIsWorked == false)
                    {
                        return;
                    }

                    Thread.Sleep(1000);

                    _ramMemoryUsageAnalyze.CalculateData();
                    _cpuUsageAnalyze.CalculateData();
                    _ecqUsageAnalyze.CalculateData();
                    _moqUsageAnalyze.CalculateData();
                }
                catch(Exception ex)
                {
                    ServerMaster.SendNewLogMessage(ex.ToString(),Logging.LogMessageType.Error);
                }
            }
        }

        #endregion

        #region Events

        private static void _ramMemoryUsageAnalyze_RamUsageCollectionChange(List<SystemUsagePointRam> values)
        {
            try
            {
                if(RamUsageCollectionChange != null)
                {
                    RamUsageCollectionChange(values);
                }
            }
            catch(Exception ex) 
            {
                ServerMaster.SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error);
            }
        }

        private static void _cpuUsageAnalyze_CpuUsageCollectionChange(List<SystemUsagePointCpu> values)
        {
            try
            {
                if (CpuUsageCollectionChange != null)
                {
                    CpuUsageCollectionChange(values);
                }
            }
            catch (Exception ex)
            {
                ServerMaster.SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error);
            }
        }

        private static void _ecqUsageAnalyze_EcqUsageCollectionChange(List<SystemUsagePointEcq> values)
        {
            try
            {
                if (EcqUsageCollectionChange != null)
                {
                    EcqUsageCollectionChange(values);
                }
            }
            catch (Exception ex)
            {
                ServerMaster.SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error);
            }
        }

        private static void _moqUsageAnalyze_MoqUsageCollectionChange(List<SystemUsagePointMoq> values)
        {
            if (MoqUsageCollectionChange != null)
            {
                MoqUsageCollectionChange(values);
            }
        }

        public static event Action<List<SystemUsagePointRam>> RamUsageCollectionChange;

        public static event Action<List<SystemUsagePointCpu>> CpuUsageCollectionChange;

        public static event Action<List<SystemUsagePointEcq>> EcqUsageCollectionChange;

        public static event Action<List<SystemUsagePointMoq>> MoqUsageCollectionChange;

        #endregion

    }

    internal static class SystemUsageAnalyzePaths
    {
        private const string SettingsDirectoryPath = @"Engine\SystemStress\";

        public static string GetSettingsPath(string fileName)
        {
            return SettingsDirectoryPath + fileName;
        }
    }

    public class RamMemoryUsageAnalyze
    {
        public List<SystemUsagePointRam> Values = new List<SystemUsagePointRam>();

        public RamMemoryUsageAnalyze()
        {
            Load();
        }

        private void Load()
        {
            try
            {
                RamMemoryUsageSettingsDto settings = OsEngine.Entity.SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                _ramCollectDataIsOn = settings.RamCollectDataIsOn;
                _ramPeriodSavePoint = settings.RamPeriodSavePoint;
                _ramPointsMax = settings.RamPointsMax;
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void Save()
        {
            try
            {
                string settingsDirectoryPath = Path.GetDirectoryName(GetSettingsPath());
                if (string.IsNullOrEmpty(settingsDirectoryPath) == false
                    && Directory.Exists(settingsDirectoryPath) == false)
                {
                    Directory.CreateDirectory(settingsDirectoryPath);
                }

                OsEngine.Entity.SettingsManager.Save(
                    GetSettingsPath(),
                    new RamMemoryUsageSettingsDto
                    {
                        RamCollectDataIsOn = _ramCollectDataIsOn,
                        RamPeriodSavePoint = _ramPeriodSavePoint,
                        RamPointsMax = _ramPointsMax
                    });
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private static string GetSettingsPath()
        {
            return SystemUsageAnalyzePaths.GetSettingsPath("RamMemorySettings.txt");
        }

        private static RamMemoryUsageSettingsDto ParseLegacySettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string normalized = content.Replace("\r", string.Empty);
            string[] lines = normalized.Split('\n');

            if (lines.Length > 0 && lines[lines.Length - 1] == string.Empty)
            {
                Array.Resize(ref lines, lines.Length - 1);
            }

            bool collectDataIsOn = false;
            if (lines.Length > 0)
            {
                bool.TryParse(lines[0], out collectDataIsOn);
            }

            SavePointPeriod period = SavePointPeriod.OneSecond;
            if (lines.Length > 1)
            {
                Enum.TryParse(lines[1], out period);
            }

            int pointsMax = 100;
            if (lines.Length > 2 && int.TryParse(lines[2], out int parsedPointsMax))
            {
                pointsMax = parsedPointsMax;
            }

            return new RamMemoryUsageSettingsDto
            {
                RamCollectDataIsOn = collectDataIsOn,
                RamPeriodSavePoint = period,
                RamPointsMax = pointsMax
            };
        }

        private sealed class RamMemoryUsageSettingsDto
        {
            public bool RamCollectDataIsOn { get; set; }

            public SavePointPeriod RamPeriodSavePoint { get; set; }

            public int RamPointsMax { get; set; }
        }

        public bool RamCollectDataIsOn
        {
            get
            {
                return _ramCollectDataIsOn;
            }
            set
            {
                if(_ramCollectDataIsOn == value)
                {
                    return;
                }

                _ramCollectDataIsOn = value;
                Save();
            }
        }
        private bool _ramCollectDataIsOn;

        public SavePointPeriod RamPeriodSavePoint
        {
            get
            {
                return _ramPeriodSavePoint;
            }
            set
            {
                if (_ramPeriodSavePoint == value)
                {
                    return;
                }

                _ramPeriodSavePoint = value;
                Save();
                _nextCalculateTime = DateTime.MinValue;
            }
        }
        private SavePointPeriod _ramPeriodSavePoint;

        public int RamPointsMax
        {
            get
            {
                return _ramPointsMax;
            }
            set
            {
                if (_ramPointsMax == value)
                {
                    return;
                }

                _ramPointsMax = value;
                Save();
            }
        }
        private int _ramPointsMax = 100;

        private DateTime _nextCalculateTime;

        public void CalculateData()
        {
            if(_ramCollectDataIsOn == false)
            {
                return;
            }

            if(_nextCalculateTime != DateTime.MinValue
                && _nextCalculateTime > DateTime.Now)
            {
                return;
            }

            if(_ramPeriodSavePoint == SavePointPeriod.OneSecond)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(1);
            }
            else if (_ramPeriodSavePoint == SavePointPeriod.TenSeconds)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(10);
            }
            else //if (_ramPeriodSavePoint == SavePointPeriod.Minute)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(60);
            }

            // 1 текущий размер программы в оперативной памяти
            Process proc = Process.GetCurrentProcess();
            long memoryMyProcess = proc.PrivateMemorySize64;
            int myMegaBytes = Convert.ToInt32(memoryMyProcess / 1024);

            // 2 общий размер оперативной памяти

            var info = new Microsoft.VisualBasic.Devices.ComputerInfo();
            ulong maxRam = info.TotalPhysicalMemory;
            int maxMegabytes = Convert.ToInt32(maxRam / 1024);

            // 3 свободный размер оперативной памяти

            ulong freeRam = info.AvailablePhysicalMemory;
            int freeMegabytes = Convert.ToInt32(freeRam / 1024);

            decimal osEngineOccupiedPercent = Math.Round(Convert.ToDecimal(Convert.ToDecimal(myMegaBytes) / (maxMegabytes / 100)), 2);
            decimal totalOccupiedPercent = Math.Round(Convert.ToDecimal((Convert.ToDecimal(maxMegabytes) - freeMegabytes) / (maxMegabytes / 100)), 2);

            SystemUsagePointRam newPoint = new SystemUsagePointRam();
            newPoint.Time = DateTime.Now;
            newPoint.ProgramUsedPercent = osEngineOccupiedPercent;
            newPoint.SystemUsedPercent = totalOccupiedPercent;

            SaveNewPoint(newPoint);
        }

        private void SaveNewPoint(SystemUsagePointRam point)
        {
            Values.Add(point);

            if(Values.Count > _ramPointsMax)
            {
                Values.RemoveAt(0);
            }

            if (RamUsageCollectionChange != null)
            {
                RamUsageCollectionChange(Values);
            }
        }

        public event Action<List<SystemUsagePointRam>> RamUsageCollectionChange;
    }

    public class CpuUsageAnalyze
    {
        public List<SystemUsagePointCpu> Values = new List<SystemUsagePointCpu>();

        public CpuUsageAnalyze()
        {
            Load();
        }

        private void Load()
        {
            try
            {
                CpuUsageSettingsDto settings = OsEngine.Entity.SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                _cpuCollectDataIsOn = settings.CpuCollectDataIsOn;
                _cpuPeriodSavePoint = settings.CpuPeriodSavePoint;
                _cpuPointsMax = settings.CpuPointsMax;
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void Save()
        {
            try
            {
                string settingsDirectoryPath = Path.GetDirectoryName(GetSettingsPath());
                if (string.IsNullOrEmpty(settingsDirectoryPath) == false
                    && Directory.Exists(settingsDirectoryPath) == false)
                {
                    Directory.CreateDirectory(settingsDirectoryPath);
                }

                OsEngine.Entity.SettingsManager.Save(
                    GetSettingsPath(),
                    new CpuUsageSettingsDto
                    {
                        CpuCollectDataIsOn = _cpuCollectDataIsOn,
                        CpuPeriodSavePoint = _cpuPeriodSavePoint,
                        CpuPointsMax = _cpuPointsMax
                    });
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private static string GetSettingsPath()
        {
            return SystemUsageAnalyzePaths.GetSettingsPath("CpuMemorySettings.txt");
        }

        private static CpuUsageSettingsDto ParseLegacySettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string normalized = content.Replace("\r", string.Empty);
            string[] lines = normalized.Split('\n');

            if (lines.Length > 0 && lines[lines.Length - 1] == string.Empty)
            {
                Array.Resize(ref lines, lines.Length - 1);
            }

            bool collectDataIsOn = false;
            if (lines.Length > 0)
            {
                bool.TryParse(lines[0], out collectDataIsOn);
            }

            SavePointPeriod period = SavePointPeriod.OneSecond;
            if (lines.Length > 1)
            {
                Enum.TryParse(lines[1], out period);
            }

            int pointsMax = 100;
            if (lines.Length > 2 && int.TryParse(lines[2], out int parsedPointsMax))
            {
                pointsMax = parsedPointsMax;
            }

            return new CpuUsageSettingsDto
            {
                CpuCollectDataIsOn = collectDataIsOn,
                CpuPeriodSavePoint = period,
                CpuPointsMax = pointsMax
            };
        }

        private sealed class CpuUsageSettingsDto
        {
            public bool CpuCollectDataIsOn { get; set; }

            public SavePointPeriod CpuPeriodSavePoint { get; set; }

            public int CpuPointsMax { get; set; }
        }

        public bool CpuCollectDataIsOn
        {
            get
            {
                return _cpuCollectDataIsOn;
            }
            set
            {
                if (_cpuCollectDataIsOn == value)
                {
                    return;
                }

                _cpuCollectDataIsOn = value;
                Save();
            }
        }
        private bool _cpuCollectDataIsOn;

        public SavePointPeriod CpuPeriodSavePoint
        {
            get
            {
                return _cpuPeriodSavePoint;
            }
            set
            {
                if (_cpuPeriodSavePoint == value)
                {
                    return;
                }

                _cpuPeriodSavePoint = value;
                Save();
                _nextCalculateTime = DateTime.MinValue;
            }
        }
        private SavePointPeriod _cpuPeriodSavePoint;

        public int CpuPointsMax
        {
            get
            {
                return _cpuPointsMax;
            }
            set
            {
                if (_cpuPointsMax == value)
                {
                    return;
                }

                _cpuPointsMax = value;
                Save();
            }
        }
        private int _cpuPointsMax = 100;

        private DateTime _nextCalculateTime;

        public void CalculateData()
        {
            if (_cpuCollectDataIsOn == false)
            {
                return;
            }

            if (_nextCalculateTime != DateTime.MinValue
                && _nextCalculateTime > DateTime.Now)
            {
                return;
            }

            if (_cpuPeriodSavePoint == SavePointPeriod.OneSecond)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(1);
            }
            else if (_cpuPeriodSavePoint == SavePointPeriod.TenSeconds)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(10);
            }
            else //if (_cpuPeriodSavePoint == SavePointPeriod.Minute)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(60);
            }

            if(_cpuCounterTotal == null)
            {
                try
                {
                    _cpuCounterTotal = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    _cpuCounterOsEngine = new PerformanceCounter("Process", "% Processor Time", "OsEngine");
                }
                catch
                {
                    _cpuCollectDataIsOn = false;
                    ServerMaster.SendNewLogMessage("Can run processor data collection on this PC.", Logging.LogMessageType.Error);
                    return;
                }
            }
           
            SystemUsagePointCpu newPoint = new SystemUsagePointCpu();
            newPoint.Time = DateTime.Now;
            newPoint.TotalOccupiedPercent = Math.Round(Convert.ToDecimal(_cpuCounterTotal.NextValue()),3);
            newPoint.ProgramOccupiedPercent = Math.Round(Convert.ToDecimal(_cpuCounterOsEngine.NextValue() / Environment.ProcessorCount), 3);

            SaveNewPoint(newPoint);
        }

        private PerformanceCounter _cpuCounterTotal;

        private PerformanceCounter _cpuCounterOsEngine;

        private void SaveNewPoint(SystemUsagePointCpu point)
        {
            Values.Add(point);

            if (Values.Count > _cpuPointsMax)
            {
                Values.RemoveAt(0);
            }

            if (CpuUsageCollectionChange != null)
            {
                CpuUsageCollectionChange(Values);
            }
        }

        public event Action<List<SystemUsagePointCpu>> CpuUsageCollectionChange;
    }

    public class EcqUsageAnalyze
    {
        public List<SystemUsagePointEcq> Values = new List<SystemUsagePointEcq>();

        public EcqUsageAnalyze()
        {
            Load();
        }

        private void Load()
        {
            try
            {
                EcqUsageSettingsDto settings = OsEngine.Entity.SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                _ecqCollectDataIsOn = settings.EcqCollectDataIsOn;
                _ecqPeriodSavePoint = settings.EcqPeriodSavePoint;
                _ecqPointsMax = settings.EcqPointsMax;
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void Save()
        {
            try
            {
                string settingsDirectoryPath = Path.GetDirectoryName(GetSettingsPath());
                if (string.IsNullOrEmpty(settingsDirectoryPath) == false
                    && Directory.Exists(settingsDirectoryPath) == false)
                {
                    Directory.CreateDirectory(settingsDirectoryPath);
                }

                OsEngine.Entity.SettingsManager.Save(
                    GetSettingsPath(),
                    new EcqUsageSettingsDto
                    {
                        EcqCollectDataIsOn = _ecqCollectDataIsOn,
                        EcqPeriodSavePoint = _ecqPeriodSavePoint,
                        EcqPointsMax = _ecqPointsMax
                    });
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private static string GetSettingsPath()
        {
            return SystemUsageAnalyzePaths.GetSettingsPath("EcqMemorySettings.txt");
        }

        private static EcqUsageSettingsDto ParseLegacySettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string normalized = content.Replace("\r", string.Empty);
            string[] lines = normalized.Split('\n');

            if (lines.Length > 0 && lines[lines.Length - 1] == string.Empty)
            {
                Array.Resize(ref lines, lines.Length - 1);
            }

            bool collectDataIsOn = false;
            if (lines.Length > 0)
            {
                bool.TryParse(lines[0], out collectDataIsOn);
            }

            SavePointPeriod period = SavePointPeriod.OneSecond;
            if (lines.Length > 1)
            {
                Enum.TryParse(lines[1], out period);
            }

            int pointsMax = 100;
            if (lines.Length > 2 && int.TryParse(lines[2], out int parsedPointsMax))
            {
                pointsMax = parsedPointsMax;
            }

            return new EcqUsageSettingsDto
            {
                EcqCollectDataIsOn = collectDataIsOn,
                EcqPeriodSavePoint = period,
                EcqPointsMax = pointsMax
            };
        }

        private sealed class EcqUsageSettingsDto
        {
            public bool EcqCollectDataIsOn { get; set; }

            public SavePointPeriod EcqPeriodSavePoint { get; set; }

            public int EcqPointsMax { get; set; }
        }

        public bool EcqCollectDataIsOn
        {
            get
            {
                return _ecqCollectDataIsOn;
            }
            set
            {
                if (_ecqCollectDataIsOn == value)
                {
                    return;
                }

                _ecqCollectDataIsOn = value;
                Save();
            }
        }
        private bool _ecqCollectDataIsOn;

        public SavePointPeriod EcqPeriodSavePoint
        {
            get
            {
                return _ecqPeriodSavePoint;
            }
            set
            {
                if (_ecqPeriodSavePoint == value)
                {
                    return;
                }

                _ecqPeriodSavePoint = value;
                Save();
                _nextCalculateTime = DateTime.MinValue;
            }
        }
        private SavePointPeriod _ecqPeriodSavePoint;

        public int EcqPointsMax
        {
            get
            {
                return _ecqPointsMax;
            }
            set
            {
                if (_ecqPointsMax == value)
                {
                    return;
                }

                _ecqPointsMax = value;
                Save();
            }
        }
        private int _ecqPointsMax = 100;

        public int MarketDepthClearingCount
        {
            get
            {
                return _marketDepthClearingCount;
            }
            set
            {
                _marketDepthClearingCount = value;
            }
        }
        private int _marketDepthClearingCount;

        public int BidAskClearingCount
        {
            get
            {
                return _bidAskClearingCount;
            }
            set
            {
                _bidAskClearingCount = value;
            }
        }
        private int _bidAskClearingCount;

        private DateTime _nextCalculateTime;

        public void CalculateData()
        {
            if (_ecqCollectDataIsOn == false)
            {
                return;
            }

            if (_nextCalculateTime != DateTime.MinValue
                && _nextCalculateTime > DateTime.Now)
            {
                return;
            }

            if (_ecqPeriodSavePoint == SavePointPeriod.OneSecond)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(1);
            }
            else if (_ecqPeriodSavePoint == SavePointPeriod.TenSeconds)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(10);
            }
            else //if (_cpuPeriodSavePoint == SavePointPeriod.Minute)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(60);
            }

            SystemUsagePointEcq newPoint = new SystemUsagePointEcq();
            newPoint.Time = DateTime.Now;
            newPoint.MarketDepthClearingCount = _marketDepthClearingCount;
            newPoint.BidAskClearingCount = _bidAskClearingCount;

            _marketDepthClearingCount = 0;
            _bidAskClearingCount = 0;

            SaveNewPoint(newPoint);
        }

        private void SaveNewPoint(SystemUsagePointEcq point)
        {
            Values.Add(point);

            if (Values.Count > _ecqPointsMax)
            {
                Values.RemoveAt(0);
            }

            if (EcqUsageCollectionChange != null)
            {
                EcqUsageCollectionChange(Values);
            }
        }

        public event Action<List<SystemUsagePointEcq>> EcqUsageCollectionChange;
    }

    public class MoqUsageAnalyze
    {
        public List<SystemUsagePointMoq> Values = new List<SystemUsagePointMoq>();

        public MoqUsageAnalyze()
        {
            Load();
        }

        private void Load()
        {
            try
            {
                MoqUsageSettingsDto settings = OsEngine.Entity.SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                _moqCollectDataIsOn = settings.MoqCollectDataIsOn;
                _moqPeriodSavePoint = settings.MoqPeriodSavePoint;
                _moqPointsMax = settings.MoqPointsMax;
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void Save()
        {
            try
            {
                string settingsDirectoryPath = Path.GetDirectoryName(GetSettingsPath());
                if (string.IsNullOrEmpty(settingsDirectoryPath) == false
                    && Directory.Exists(settingsDirectoryPath) == false)
                {
                    Directory.CreateDirectory(settingsDirectoryPath);
                }

                OsEngine.Entity.SettingsManager.Save(
                    GetSettingsPath(),
                    new MoqUsageSettingsDto
                    {
                        MoqCollectDataIsOn = _moqCollectDataIsOn,
                        MoqPeriodSavePoint = _moqPeriodSavePoint,
                        MoqPointsMax = _moqPointsMax
                    });
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private static string GetSettingsPath()
        {
            return SystemUsageAnalyzePaths.GetSettingsPath("MoqMemorySettings.txt");
        }

        private static MoqUsageSettingsDto ParseLegacySettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string normalized = content.Replace("\r", string.Empty);
            string[] lines = normalized.Split('\n');

            if (lines.Length > 0 && lines[lines.Length - 1] == string.Empty)
            {
                Array.Resize(ref lines, lines.Length - 1);
            }

            bool collectDataIsOn = false;
            if (lines.Length > 0)
            {
                bool.TryParse(lines[0], out collectDataIsOn);
            }

            SavePointPeriod period = SavePointPeriod.OneSecond;
            if (lines.Length > 1)
            {
                Enum.TryParse(lines[1], out period);
            }

            int pointsMax = 100;
            if (lines.Length > 2 && int.TryParse(lines[2], out int parsedPointsMax))
            {
                pointsMax = parsedPointsMax;
            }

            return new MoqUsageSettingsDto
            {
                MoqCollectDataIsOn = collectDataIsOn,
                MoqPeriodSavePoint = period,
                MoqPointsMax = pointsMax
            };
        }

        private sealed class MoqUsageSettingsDto
        {
            public bool MoqCollectDataIsOn { get; set; }

            public SavePointPeriod MoqPeriodSavePoint { get; set; }

            public int MoqPointsMax { get; set; }
        }

        public bool MoqCollectDataIsOn
        {
            get
            {
                return _moqCollectDataIsOn;
            }
            set
            {
                if (_moqCollectDataIsOn == value)
                {
                    return;
                }

                _moqCollectDataIsOn = value;
                Save();
            }
        }
        private bool _moqCollectDataIsOn;

        public SavePointPeriod MoqPeriodSavePoint
        {
            get
            {
                return _moqPeriodSavePoint;
            }
            set
            {
                if (_moqPeriodSavePoint == value)
                {
                    return;
                }

                _moqPeriodSavePoint = value;
                Save();
                _nextCalculateTime = DateTime.MinValue;
            }
        }
        private SavePointPeriod _moqPeriodSavePoint;

        public int MoqPointsMax
        {
            get
            {
                return _moqPointsMax;
            }
            set
            {
                if (_moqPointsMax == value)
                {
                    return;
                }

                _moqPointsMax = value;
                Save();
            }
        }
        private int _moqPointsMax = 100;

        public int MaxOrdersInQueue
        {
            get
            {
                return _maxOrdersInQueue;
            }
            set
            {
                if(value > _maxOrdersInQueue)
                {
                    _maxOrdersInQueue = value;
                }
            }
        }
        private int _maxOrdersInQueue;

        private DateTime _nextCalculateTime;

        public void CalculateData()
        {
            if (_moqCollectDataIsOn == false)
            {
                return;
            }

            if (_nextCalculateTime != DateTime.MinValue
                && _nextCalculateTime > DateTime.Now)
            {
                return;
            }

            if (_moqPeriodSavePoint == SavePointPeriod.OneSecond)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(1);
            }
            else if (_moqPeriodSavePoint == SavePointPeriod.TenSeconds)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(10);
            }
            else //if (_cpuPeriodSavePoint == SavePointPeriod.Minute)
            {
                _nextCalculateTime = DateTime.Now.AddSeconds(60);
            }

            SystemUsagePointMoq newPoint = new SystemUsagePointMoq();
            newPoint.Time = DateTime.Now;
            newPoint.MaxOrdersInQueue = _maxOrdersInQueue;

            _maxOrdersInQueue = 0;

            SaveNewPoint(newPoint);
        }

        private void SaveNewPoint(SystemUsagePointMoq point)
        {
            Values.Add(point);

            if (Values.Count > _moqPointsMax)
            {
                Values.RemoveAt(0);
            }

            if (MoqUsageCollectionChange != null)
            {
                MoqUsageCollectionChange (Values);
            }
        }

        public event Action<List<SystemUsagePointMoq>> MoqUsageCollectionChange;
    }


    public class SystemUsagePointRam
    {
        public DateTime Time;

        public decimal ProgramUsedPercent;

        public decimal SystemUsedPercent;
    }

    public class SystemUsagePointCpu
    {
        public DateTime Time;

        public decimal ProgramOccupiedPercent;

        public decimal TotalOccupiedPercent;
    }

    public class SystemUsagePointEcq
    {
        public DateTime Time;

        public decimal MarketDepthClearingCount;

        public decimal BidAskClearingCount;
    }

    public class SystemUsagePointMoq
    {
        public DateTime Time;

        public decimal MaxOrdersInQueue;

    }

    public enum SavePointPeriod
    {
        OneSecond,
        TenSeconds,
        Minute
    }
}
