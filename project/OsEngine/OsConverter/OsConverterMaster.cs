#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Logging;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;

namespace OsEngine.OsConverter
{
    public class OsConverterMaster
    {

        public OsConverterMaster(TextBox textBoxSourceFile, TextBox textBoxExitFile, ComboBox comboBoxTimeFrame,
            WindowsFormsHost logFormsHost)
        {
            _textBoxSourceFile = textBoxSourceFile;
            _textBoxExitFile = textBoxExitFile;
            _comboBoxTimeFrame = comboBoxTimeFrame;
            TimeFrame = TimeFrame.Sec1;

            Load();

            _comboBoxTimeFrame.Items.Add(TimeFrame.Sec1);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Sec2);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Sec5);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Sec10);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Sec15);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Sec20);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Sec30);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Min1);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Min2);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Min5);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Min10);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Min15);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Min20);
            _comboBoxTimeFrame.Items.Add(TimeFrame.Min30);


            _comboBoxTimeFrame.SelectedItem = TimeFrame;

            _comboBoxTimeFrame.SelectionChanged += _comboBoxTimeFrame_SelectionChanged;

            _textBoxSourceFile.Text = _sourceFile;
            _textBoxExitFile.Text = _exitFile;

            Log log = new Log("OsDataMaster", StartProgram.IsOsData);
            log.StartPaint(logFormsHost);
            log.Listen(this);
        }

        public void Load()
        {
            try
            {
                OsConverterSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                TimeFrame = settings.TimeFrame;
                _sourceFile = settings.SourceFile;
                _exitFile = settings.ExitFile;
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        public void Save()
        {
            try
            {
                SettingsManager.Save(
                    GetSettingsPath(),
                    new OsConverterSettingsDto
                    {
                        TimeFrame = TimeFrame,
                        SourceFile = _sourceFile,
                        ExitFile = _exitFile
                    });
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        private static string GetSettingsPath()
        {
            return "Engine\\Converter.txt";
        }

        private static OsConverterSettingsDto ParseLegacySettings(string content)
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

            TimeFrame timeFrame = TimeFrame.Sec1;
            if (lines.Length > 0)
            {
                Enum.TryParse(lines[0], out timeFrame);
            }

            return new OsConverterSettingsDto
            {
                TimeFrame = timeFrame,
                SourceFile = lines.Length > 1 ? lines[1] : null,
                ExitFile = lines.Length > 2 ? lines[2] : null
            };
        }

        private sealed class OsConverterSettingsDto
        {
            public TimeFrame TimeFrame { get; set; }

            public string SourceFile { get; set; }

            public string ExitFile { get; set; }
        }

        private string _sourceFile;

        private string _exitFile;

        public TimeFrame TimeFrame;

        private TextBox _textBoxSourceFile;

        private TextBox _textBoxExitFile;

        private ComboBox _comboBoxTimeFrame;

        private void _comboBoxTimeFrame_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Enum.TryParse(_comboBoxTimeFrame.SelectedItem.ToString(), out TimeFrame);
            Save();
        }

        public void SelectSourceFile()
        {
            FileDialog myDialog = new OpenFileDialog();

            if (string.IsNullOrWhiteSpace(_sourceFile))
            {
                myDialog.FileName = _sourceFile;
            }

            myDialog.ShowDialog();

            if (myDialog.FileName != "") // if anything is selected
            {
                _sourceFile = myDialog.FileName;
                Save();
            }

            _textBoxSourceFile.Text = _sourceFile;
        }

        public void CreateExitFile()
        {
            FileDialog myDialog = new SaveFileDialog();

            if (string.IsNullOrWhiteSpace(_exitFile))
            {
                myDialog.FileName = _exitFile;
            }

            myDialog.ShowDialog();

            if (myDialog.FileName != "") //  if anything is selected
            {
                if (!myDialog.FileName.Contains(".txt"))
                {
                    myDialog.FileName += ".txt";
                }

                _exitFile = myDialog.FileName;
                Save();
            }

            _textBoxExitFile.Text = _exitFile;
        }

        public void StartConvert()
        {
            if (_worker != null &&
                _worker.IsCompleted)
            {
                SendNewLogMessage(OsLocalization.Converter.Message1, LogMessageType.System);
                return;
            }

            _worker = new Task(WorkerSpaceStreaming); // Use the streaming worker
            _worker.Start();
        }

        private void WorkerSpaceStreaming()
        {
            string tempOutputPath = _exitFile + ".tmp";

            try
            {
                string fullExitPath = Path.GetFullPath(_exitFile);
                string? exitDirectory = Path.GetDirectoryName(fullExitPath);

                if (!string.IsNullOrWhiteSpace(exitDirectory))
                {
                    Directory.CreateDirectory(exitDirectory);
                }

                using (StreamReader reader = new StreamReader(_sourceFile))
                using (StreamWriter writer = new StreamWriter(tempOutputPath, false))
                {
                    SendNewLogMessage(OsLocalization.Converter.Message4, LogMessageType.System);
                    SendNewLogMessage(OsLocalization.Converter.Message5, LogMessageType.System);

                    List<Trade> trades = new List<Trade>();
                    DateTime currentDay = DateTime.MinValue;

                    while (!reader.EndOfStream)
                    {
                        Trade trade = new Trade();
                        trade.SetTradeFromString(reader.ReadLine());

                        if (currentDay == DateTime.MinValue)
                        {
                            currentDay = trade.Time.Date;
                        }

                        if (trade.Time.Date != currentDay || reader.EndOfStream)
                        {
                            ProcessTradesAndWriteCandles(trades, writer);
                            trades = new List<Trade>(); // Clear trades for the next day
                            currentDay = trade.Time.Date;
                        }

                        trades.Add(trade);
                    }

                    // Process any remaining trades
                    if (trades.Count > 0)
                    {
                        ProcessTradesAndWriteCandles(trades, writer);
                    }

                    writer.Flush();
                    if (writer.BaseStream is FileStream fileStream)
                    {
                        fileStream.Flush(true);
                    }
                    else
                    {
                        writer.BaseStream.Flush();
                    }
                    SendNewLogMessage(OsLocalization.Converter.Message9, LogMessageType.System);
                }

                if (File.Exists(_exitFile))
                {
                    File.Replace(tempOutputPath, _exitFile, _exitFile + ".bak", true);
                }
                else
                {
                    File.Move(tempOutputPath, _exitFile);
                }
            }
            catch (Exception ex)
            {
                SendNewLogMessage(ex.ToString(), LogMessageType.Error);
            }
            finally
            {
                if (File.Exists(tempOutputPath))
                {
                    File.Delete(tempOutputPath);
                }
            }

            _worker = null;
        }

        private void ProcessTradesAndWriteCandles(List<Trade> trades, StreamWriter writer)
        {
            TimeFrameBuilder timeFrameBuilder = new TimeFrameBuilder(StartProgram.IsOsData);
            timeFrameBuilder.TimeFrame = TimeFrame;

            CandleSeries series = new CandleSeries(timeFrameBuilder, new Security() { Name = "Unknown" },
                StartProgram.IsOsConverter);
            series.IsStarted = true;

            for(int i = 0;i < trades.Count;i++)
            {
                series.SetNewTicks(trades[i]);
            }

            List<Candle> candles = series.CandlesAll;

            if (candles != null)
            {
                for (int i = 0; i < candles.Count; i++)
                {
                    writer.WriteLine(candles[i].StringToSave);
                }
            }

            series.Clear(); // Clear the candle series
        }

        private Task _worker;

        public void SendNewLogMessage(string message, LogMessageType type)
        {
            if (LogMessageEvent != null)
            {
                LogMessageEvent(message, type);
            }
        }

        public event Action<string, LogMessageType> LogMessageEvent;
    }
}

