#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace OsEngine.MachineLearning.Onnx
{
    /// <summary>
    /// Materialized outputs returned from a single ONNX inference run.
    /// </summary>
    public sealed class OnnxInferenceResult
    {
        private readonly Dictionary<string, OnnxTensorOutput> _outputs;

        internal OnnxInferenceResult(Dictionary<string, OnnxTensorOutput> outputs)
        {
            _outputs = outputs;
        }

        public IReadOnlyCollection<string> OutputNames => _outputs.Keys.ToArray();

        public OnnxTensorOutput GetOutput(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Output name is required.", nameof(name));
            }

            if (_outputs.TryGetValue(name, out OnnxTensorOutput output))
            {
                return output;
            }

            throw new KeyNotFoundException($"ONNX output '{name}' was not returned by the model.");
        }

        public bool TryGetOutput(string name, out OnnxTensorOutput output)
        {
            output = null;

            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return _outputs.TryGetValue(name, out output);
        }
    }
}
