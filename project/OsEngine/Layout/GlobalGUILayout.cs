#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OsEngine.Entity;
using System.IO;
using System.Globalization;

namespace OsEngine.Layout
{
    public class GlobalGUILayout
    {
        private static bool _isFirstTime = true;

        public static void Listen(System.Windows.Window ui, string name)
        {
            lock (_lockerArrayWithWindows)
            {
                if (_isFirstTime == true)
                {
                    _isFirstTime = false;
                    Load();

                    for (int i = 0; i < UiOpenWindows.Count; i++)
                    {
                        UiOpenWindows[i].UiLocationChangeEvent += UiLocationChangeEvent;
                    }

                    if(ScreenSettingsIsAllRight() == false)
                    {
                        UiOpenWindows = new List<OpenWindow>();
                    }

                    Thread worker = new Thread(SaveWorkerPlace);
                    worker.IsBackground = true;
                    worker.Start();
                }
            }

            lock (_lockerArrayWithWindows)
            {
                for (int i = 0; i < UiOpenWindows.Count; i++)
                {
                    if (UiOpenWindows[i].Name == name)
                    {
                        SetLayoutInWindow(ui, UiOpenWindows[i].Layout);
                        UiOpenWindows[i].WindowCreateTime = DateTime.Now;
                        UiOpenWindows[i].IsActivate = false;
                        UiOpenWindows[i].Ui = ui;
                        return;
                    }
                }
            }

            OpenWindow window = new OpenWindow();
            window.Name = name;
            window.WindowCreateTime = DateTime.Now;
            window.Layout = new OpenWindowLayout();
            window.Ui = ui;
            window.UiLocationChangeEvent += UiLocationChangeEvent;

            SetLayoutFromWindow(ui, window);

            lock(_lockerArrayWithWindows)
            {
                UiOpenWindows.Add(window);
                _needToSave = true;
            }
      
        }

        private static readonly Lock _lockerArrayWithWindows = new();

        private static void UiLocationChangeEvent(System.Windows.Window ui, string name)
        {
            lock (_lockerArrayWithWindows)
            {
                for (int i = 0; i < UiOpenWindows.Count; i++)
                {
                    if (UiOpenWindows[i].Name == name)
                    {
                        if (UiOpenWindows[i].IsActivate == false)
                        {
                            if (UiOpenWindows[i].WindowCreateTime.AddSeconds(1) > DateTime.Now)
                            {
                                SetLayoutInWindow(ui, UiOpenWindows[i].Layout);
                            }
                            else
                            {
                                UiOpenWindows[i].IsActivate = true;
                            }

                            return;
                        }
                        
                        if (UiOpenWindows[i].WindowUpdateTime.AddMilliseconds(300) > DateTime.Now)
                        {
                            return;
                        }

                        SetLayoutFromWindow(ui, UiOpenWindows[i]);
                        UiOpenWindows[i].WindowUpdateTime = DateTime.Now;

                        break;
                    }
                }
                _needToSave = true;
            }
        }

        private static void SetLayoutFromWindow(System.Windows.Window ui, OpenWindow windowLayout)
        {

            if (double.IsNaN(ui.ActualHeight) == false)
            {
                windowLayout.Layout.Height = Convert.ToDecimal(ui.ActualHeight);
            }

            if (double.IsNaN(ui.ActualWidth) == false)
            {
                windowLayout.Layout.Widht = Convert.ToDecimal(ui.ActualWidth);
            }

            if (double.IsNaN(ui.Left) == false)
            {
                windowLayout.Layout.Left = Convert.ToDecimal(ui.Left);
            }

            if (double.IsNaN(ui.Top) == false)
            {
                windowLayout.Layout.Top = Convert.ToDecimal(ui.Top);
            }

            if (ui.WindowState == System.Windows.WindowState.Maximized)
            {
                windowLayout.Layout.IsExpand = true;
            }
            else
            {
                windowLayout.Layout.IsExpand = false;
            }			
        }

        private static void SetLayoutInWindow(System.Windows.Window ui, OpenWindowLayout layout)
        {
            if(layout.Height == 0 ||
                layout.Widht == 0 ||
                layout.Left == 0 ||
                layout.Top == 0)
            {
                return;
            }

            if (layout.Left == -32000 ||
               layout.Top == -32000)
            {
                return;
            }

            if (layout.Left < -50 ||
              layout.Top < -50 ||
              layout.Height < 0 ||
              layout.Widht < 0)
            {
                return;
            }

            if (layout.Height < 0 || layout.Widht < 0)
            {
                return;
            }           

            if (layout.Height > int.MaxValue ||
                layout.Widht > int.MaxValue ||
                layout.Left > int.MaxValue ||
                layout.Top > int.MaxValue)
            {
                return;
            }

            if (layout.IsExpand == true)
            {
                ui.Left = Convert.ToDouble(layout.Left);
                ui.Top = Convert.ToDouble(layout.Top);
                return;
            }

            ui.Height = Convert.ToDouble(layout.Height);
            ui.Width = Convert.ToDouble(layout.Widht);
            ui.Left = Convert.ToDouble(layout.Left);
            ui.Top = Convert.ToDouble(layout.Top);

            if (ui.Top < 0)
            {
                ui.Top = 0;
            }
        }

        public static List<OpenWindow> UiOpenWindows = new List<OpenWindow>();

        private static bool _needToSave;

        public static bool IsClosed;

        private static void SaveWorkerPlace()
        {
            while(true)
            {
                Thread.Sleep(1000);

                bool needToSave;
                lock (_lockerArrayWithWindows)
                {
                    needToSave = _needToSave;
                }

                if (needToSave == false)
                {
                    continue;
                }

                if(IsClosed)
                {
                    return;
                }

                Save();
            }
        }

        private static void Save()
        {
            try
            {
                List<string> windowsToSave = new List<string>();

                lock (_lockerArrayWithWindows)
                {
                    for(int i = 0;i < UiOpenWindows.Count;i++)
                    {
                        if (UiOpenWindows[i].Layout.Height == 0 ||
                            UiOpenWindows[i].Layout.Widht == 0 ||
                            UiOpenWindows[i].Layout.Left == 0 ||
                            UiOpenWindows[i].Layout.Top == 0)
                        {
                            continue;
                        }

                        if (UiOpenWindows[i].Layout.Left == -32000 ||
                            UiOpenWindows[i].Layout.Top == -32000)
                        {//свернутое значение окна пропускаем при сохранение
                            continue;
                        }


                        if (UiOpenWindows[i].Layout.Height < 0 || UiOpenWindows[i].Layout.Widht < 0)
                        {
                            continue;
                        }

                        windowsToSave.Add(UiOpenWindows[i].GetSaveString());
                    }
                }

                SettingsManager.Save(
                    GetLayoutSettingsPath(),
                    new GlobalLayoutSettingsDto
                    {
                        Windows = windowsToSave.ToArray()
                    });
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }
        }

        private static void Load()
        {
            if (!SettingsManager.Exists(GetLayoutSettingsPath()))
            {
                return;
            }

            try
            {
                GlobalLayoutSettingsDto settings = SettingsManager.Load(
                    GetLayoutSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacyLayoutSettings);

                if (settings == null || settings.Windows == null)
                {
                    return;
                }

                for (int i = 0; i < settings.Windows.Length; i++)
                {
                    string res = settings.Windows[i];

                    if(string.IsNullOrEmpty(res))
                    {
                        return;
                    }

                    OpenWindow window = new OpenWindow();
                    window.LoadFromString(res);
                    UiOpenWindows.Add(window);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }
        }

        private static string GetLayoutSettingsPath()
        {
            return GetLayoutFilePath("LayoutGui.toml");
        }

        private static string GetLayoutFilePath(string fileName)
        {
            return @"Engine\" + fileName;
        }

        private static GlobalLayoutSettingsDto ParseLegacyLayoutSettings(string content)
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

            return new GlobalLayoutSettingsDto
            {
                Windows = lines
            };
        }

        private sealed class GlobalLayoutSettingsDto
        {
            public string[] Windows { get; set; }
        }

        // Проверка размера экрана

        private static bool ScreenSettingsIsAllRight()
        {
            int widthCur = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width;
            int heightCur = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Height;
            int monitorCountCur = System.Windows.Forms.Screen.AllScreens.Length;

            if (!SettingsManager.Exists(GetScreenResolutionPath()))
            {
                SaveResolution(widthCur, heightCur, monitorCountCur);
                return true;
            }

            ScreenResolutionSettingsDto settings = SettingsManager.Load(
                GetScreenResolutionPath(),
                defaultValue: new ScreenResolutionSettingsDto(),
                legacyLoader: ParseLegacyScreenResolutionSettings);

            int widthOld = settings.Width;
            int heightOld = settings.Height;
            int monitorCountOld = settings.MonitorCount;

            if(widthCur != widthOld ||
                heightCur != heightOld ||
                monitorCountCur != monitorCountOld)
            {
                SaveResolution(widthCur, heightCur, monitorCountCur);
                return false;
            }

            SaveResolution(widthCur, heightCur, monitorCountCur);
            return true;
        }

        private static void SaveResolution(int widthCur, int heightCur, int monitorCountCur)
        {
            try
            {
                SettingsManager.Save(
                    GetScreenResolutionPath(),
                    new ScreenResolutionSettingsDto
                    {
                        Width = widthCur,
                        Height = heightCur,
                        MonitorCount = monitorCountCur
                    });
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }
        }

        private static string GetScreenResolutionPath()
        {
            return GetLayoutFilePath("ScreenResolution.toml");
        }

        private static ScreenResolutionSettingsDto ParseLegacyScreenResolutionSettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new ScreenResolutionSettingsDto();
            }

            string normalized = content.Replace("\r", string.Empty);
            string[] lines = normalized.Split('\n');

            if (lines.Length > 0 && lines[lines.Length - 1] == string.Empty)
            {
                Array.Resize(ref lines, lines.Length - 1);
            }

            int width = 0;
            if (lines.Length > 0)
            {
                int.TryParse(lines[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out width);
            }

            int height = 0;
            if (lines.Length > 1)
            {
                int.TryParse(lines[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out height);
            }

            int monitorCount = 0;
            if (lines.Length > 2)
            {
                int.TryParse(lines[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out monitorCount);
            }

            return new ScreenResolutionSettingsDto
            {
                Width = width,
                Height = height,
                MonitorCount = monitorCount
            };
        }

        private sealed class ScreenResolutionSettingsDto
        {
            public int Width { get; set; }

            public int Height { get; set; }

            public int MonitorCount { get; set; }
        }
    }


    public class OpenWindow
    {
        public System.Windows.Window Ui
        {
            get
            {
                return _ui;
            }
            set
            {
                if(_ui != null)
                {
                    _ui.LocationChanged -= _ui_LocationChanged;
                    _ui.SizeChanged -= _ui_SizeChanged;
                    _ui.Closed -= _ui_Closed;
                }

                _ui = value;

                if(_ui != null)
                {
                    _ui.LocationChanged += _ui_LocationChanged;
                    _ui.SizeChanged += _ui_SizeChanged;
                    _ui.Closed += _ui_Closed;
                }
            }
        }

        private void _ui_Closed(object sender, EventArgs e)
        {
            _ui.LocationChanged -= _ui_LocationChanged;
            _ui.SizeChanged -= _ui_SizeChanged;
            _ui.Closed -= _ui_Closed;
            _ui = null;
        }

        private void _ui_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            UiLocationChangeEvent(_ui, Name);
        }

        private void _ui_LocationChanged(object sender, EventArgs e)
        {
            UiLocationChangeEvent(_ui, Name);
        }

        private System.Windows.Window _ui;

        public OpenWindowLayout Layout;

        public string Name;

        public DateTime WindowCreateTime;

        public bool IsActivate = false;

        public DateTime WindowUpdateTime;

        public string GetSaveString()
        {
            string res = "";

            res += Name + "#";
            res += Layout.Height.ToString(CultureInfo.InvariantCulture)
                + "$" + Layout.Left.ToString(CultureInfo.InvariantCulture)
                + "$" + Layout.Top.ToString(CultureInfo.InvariantCulture)
                + "$" + Layout.Widht.ToString(CultureInfo.InvariantCulture)
                + "$" + Layout.IsExpand;

            return res;
        }

        public void LoadFromString(string str)
        {
            Layout = new OpenWindowLayout();

            if (string.IsNullOrWhiteSpace(str))
            {
                Name = string.Empty;
                return;
            }

            string[] save = str.Split('#');
            Name = save[0];

            if (save.Length < 2)
            {
                return;
            }

            string[] strLayout = save[1].Split('$');

            if (strLayout.Length < 5)
            {
                return;
            }

            Layout.Height = ParseLayoutDecimal(strLayout[0]);
            Layout.Left = ParseLayoutDecimal(strLayout[1]);
            Layout.Top = ParseLayoutDecimal(strLayout[2]);
            Layout.Widht = ParseLayoutDecimal(strLayout[3]);

            if (bool.TryParse(strLayout[4], out bool isExpand))
            {
                Layout.IsExpand = isExpand;
            }
        }

        private static decimal ParseLayoutDecimal(string value)
        {
            if (TryParseLayoutDecimal(value, out decimal parsed))
            {
                return parsed;
            }

            return 0;
        }

        private static bool TryParseLayoutDecimal(string value, out decimal parsed)
        {
            const NumberStyles parseStyle = NumberStyles.Float | NumberStyles.AllowLeadingSign;

            if (decimal.TryParse(value, parseStyle, CultureInfo.CurrentCulture, out parsed))
            {
                return true;
            }

            if (decimal.TryParse(value, parseStyle, CultureInfo.InvariantCulture, out parsed))
            {
                return true;
            }

            return decimal.TryParse(value, parseStyle, CultureInfo.GetCultureInfo("ru-RU"), out parsed);
        }

        public event Action<System.Windows.Window, string> UiLocationChangeEvent;
    }

    public class OpenWindowLayout
    {
        public decimal Top;

        public decimal Left;

        public decimal Widht;

        public decimal Height;
		
        public bool IsExpand;    // является ли окно развернутым		
    }
}

