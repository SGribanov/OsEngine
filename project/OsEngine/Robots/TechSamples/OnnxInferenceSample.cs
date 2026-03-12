#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.IO;
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.MachineLearning.Onnx;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Attributes;
using OsEngine.OsTrader.Panels.Tab;

/* Description
TechSample robot for OsEngine

Builds a fixed candle-return feature vector, runs local ONNX inference,
and converts the score into a simple long/short decision.
 */

namespace OsEngine.Robots.TechSamples
{
    [Bot("OnnxInferenceSample")]
    public class OnnxInferenceSample : BotPanel
    {
        private readonly BotTabSimple _tab;
        private readonly OnnxModelRuntime _modelRuntime;

        private readonly StrategyParameterString _regime;
        private readonly StrategyParameterDecimal _volume;
        private readonly StrategyParameterString _modelPath;
        private readonly StrategyParameterString _inputName;
        private readonly StrategyParameterString _outputName;
        private readonly StrategyParameterDecimal _signalThreshold;

        private string _loadedModelPath = string.Empty;
        private string _lastFailedModelPath = string.Empty;

        public OnnxInferenceSample(string name, StartProgram startProgram)
            : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            _regime = CreateParameter("Regime", "Off", new[] { "Off", "On", "OnlyLong", "OnlyShort" }, "Base");
            _volume = CreateParameter("Volume", 1m, 1m, 100m, 1m, "Base");
            _signalThreshold = CreateParameter("SignalThreshold", 0.05m, 0m, 10m, 0.01m, "Base");

            _modelPath = CreateParameter("ModelPath", string.Empty, "Model");
            _inputName = CreateParameter("InputTensorName", "features", "Model");
            _outputName = CreateParameter("OutputTensorName", "score", "Model");

            _modelRuntime = CreateOnnxModelRuntime();

            _tab.CandleFinishedEvent += OnCandleFinished;
            ParametrsChangeByUser += OnnxInferenceSample_ParametrsChangeByUser;

            Description = "Runs a local ONNX model on the last four candle returns and converts the score into simple market orders.";
        }

        public override string GetNameStrategyType()
        {
            return "OnnxInferenceSample";
        }

        public override void ShowIndividualSettingsDialog()
        {
        }

        private void OnnxInferenceSample_ParametrsChangeByUser()
        {
            _lastFailedModelPath = string.Empty;
            TryEnsureModelLoaded(forceReload: true);
        }

        private void OnCandleFinished(List<Candle> candles)
        {
            if (_regime.ValueString == "Off")
            {
                return;
            }

            if (candles == null || candles.Count < 5)
            {
                return;
            }

            if (TryEnsureModelLoaded(forceReload: false) == false)
            {
                return;
            }

            float[] features = BuildFeatures(candles);
            float signal;

            try
            {
                OnnxInferenceResult result = _modelRuntime.Run(
                    OnnxTensorInput.Create(_inputName.ValueString, features, 1, features.Length));

                signal = result.GetOutput(_outputName.ValueString).GetScalar<float>();
            }
            catch (Exception error)
            {
                SendNewLogMessage($"ONNX inference failed: {error.Message}", LogMessageType.Error);
                return;
            }

            ApplySignal(signal);
        }

        private bool TryEnsureModelLoaded(bool forceReload)
        {
            try
            {
                string modelPath = _modelPath.ValueString?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(modelPath))
                {
                    return false;
                }

                string fullPath = Path.GetFullPath(modelPath);

                if (forceReload == false &&
                    _modelRuntime.IsLoaded &&
                    string.Equals(_loadedModelPath, fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (forceReload == false &&
                    _modelRuntime.IsLoaded == false &&
                    string.Equals(_lastFailedModelPath, fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                _modelRuntime.Load(fullPath);
                _loadedModelPath = fullPath;
                _lastFailedModelPath = string.Empty;

                return true;
            }
            catch (Exception error)
            {
                _loadedModelPath = string.Empty;
                _lastFailedModelPath = _modelPath.ValueString?.Trim() ?? string.Empty;
                SendNewLogMessage($"Unable to load ONNX model: {error.Message}", LogMessageType.Error);
                return false;
            }
        }

        private float[] BuildFeatures(List<Candle> candles)
        {
            int startIndex = candles.Count - 5;
            float[] features = new float[4];

            for (int i = 0; i < features.Length; i++)
            {
                decimal currentClose = candles[startIndex + i + 1].Close;
                decimal previousClose = candles[startIndex + i].Close;
                features[i] = (float)(currentClose - previousClose);
            }

            return features;
        }

        private void ApplySignal(float signal)
        {
            decimal threshold = _signalThreshold.ValueDecimal;
            List<Position> positions = _tab.PositionsOpenAll ?? new List<Position>();

            if (positions.Count == 0)
            {
                if (signal > (float)threshold && _regime.ValueString != "OnlyShort")
                {
                    _tab.BuyAtMarket(_volume.ValueDecimal);
                }
                else if (signal < (float)(-threshold) && _regime.ValueString != "OnlyLong")
                {
                    _tab.SellAtMarket(_volume.ValueDecimal);
                }

                return;
            }

            Position position = positions[0];

            if (position.State != PositionStateType.Open)
            {
                return;
            }

            if (position.Direction == Side.Buy && signal < (float)(-threshold))
            {
                _tab.CloseAtMarket(position, position.OpenVolume);
            }
            else if (position.Direction == Side.Sell && signal > (float)threshold)
            {
                _tab.CloseAtMarket(position, position.OpenVolume);
            }
        }
    }
}
