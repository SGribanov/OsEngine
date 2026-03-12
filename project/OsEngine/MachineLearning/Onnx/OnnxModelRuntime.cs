#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.ML.OnnxRuntime;

namespace OsEngine.MachineLearning.Onnx
{
    /// <summary>
    /// Deterministic ONNX runtime wrapper for robot code.
    /// </summary>
    public sealed class OnnxModelRuntime : IDisposable
    {
        private readonly Lock _sync = new();
        private InferenceSession _session;
        private string _modelPath;
        private string[] _inputNames = Array.Empty<string>();
        private string[] _outputNames = Array.Empty<string>();
        private Dictionary<string, NodeMetadata> _outputMetadata = new(StringComparer.Ordinal);
        private bool _isDisposed;

        public bool IsLoaded
        {
            get
            {
                lock (_sync)
                {
                    return _session != null;
                }
            }
        }

        public string ModelPath
        {
            get
            {
                lock (_sync)
                {
                    ThrowIfDisposed();
                    return _modelPath;
                }
            }
        }

        public string[] InputNames
        {
            get
            {
                lock (_sync)
                {
                    ThrowIfDisposed();
                    return (string[])_inputNames.Clone();
                }
            }
        }

        public string[] OutputNames
        {
            get
            {
                lock (_sync)
                {
                    ThrowIfDisposed();
                    return (string[])_outputNames.Clone();
                }
            }
        }

        public void Load(string modelPath)
        {
            if (string.IsNullOrWhiteSpace(modelPath))
            {
                throw new ArgumentException("Model path is required.", nameof(modelPath));
            }

            string fullPath = Path.GetFullPath(modelPath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"ONNX model file was not found: {fullPath}", fullPath);
            }

            InferenceSession newSession = new(fullPath);

            lock (_sync)
            {
                ThrowIfDisposed();

                DisposeSessionNoLock();

                _session = newSession;
                _modelPath = fullPath;
                _inputNames = _session.InputMetadata.Keys.ToArray();
                _outputNames = _session.OutputMetadata.Keys.ToArray();
                _outputMetadata = _session.OutputMetadata.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            }
        }

        public void Reload()
        {
            string currentPath;

            lock (_sync)
            {
                ThrowIfDisposed();

                if (string.IsNullOrWhiteSpace(_modelPath))
                {
                    throw new InvalidOperationException("Cannot reload ONNX model because no model has been loaded yet.");
                }

                currentPath = _modelPath;
            }

            Load(currentPath);
        }

        public OnnxInferenceResult Run(params OnnxTensorInput[] inputs)
        {
            return Run((IEnumerable<OnnxTensorInput>)inputs);
        }

        public OnnxInferenceResult Run(IEnumerable<OnnxTensorInput> inputs)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException(nameof(inputs));
            }

            OnnxTensorInput[] inputArray = inputs.ToArray();

            if (inputArray.Length == 0)
            {
                throw new ArgumentException("At least one tensor input is required.", nameof(inputs));
            }

            lock (_sync)
            {
                ThrowIfDisposed();

                if (_session == null)
                {
                    throw new InvalidOperationException("Cannot run ONNX inference because no model has been loaded yet.");
                }

                List<NamedOnnxValue> namedInputs = inputArray.Select(static input => input.CreateNamedValue()).ToList();

                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> rawOutputs = _session.Run(namedInputs);
                Dictionary<string, OnnxTensorOutput> outputs = new(StringComparer.Ordinal);

                foreach (DisposableNamedOnnxValue output in rawOutputs)
                {
                    outputs[output.Name] = MaterializeOutput(output);
                }

                return new OnnxInferenceResult(outputs);
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_isDisposed)
                {
                    return;
                }

                DisposeSessionNoLock();
                _outputMetadata.Clear();
                _inputNames = Array.Empty<string>();
                _outputNames = Array.Empty<string>();
                _modelPath = null;
                _isDisposed = true;
            }
        }

        private OnnxTensorOutput MaterializeOutput(DisposableNamedOnnxValue output)
        {
            if (!_outputMetadata.TryGetValue(output.Name, out NodeMetadata metadata))
            {
                throw new InvalidOperationException($"ONNX runtime metadata for output '{output.Name}' was not found.");
            }

            Type elementType = metadata.ElementType
                ?? throw new NotSupportedException($"ONNX output '{output.Name}' does not expose a tensor element type.");

            if (elementType == typeof(float))
            {
                var tensor = output.AsTensor<float>();
                return new OnnxTensorOutput(output.Name, typeof(float), tensor.ToArray(), tensor.Dimensions.ToArray());
            }

            if (elementType == typeof(double))
            {
                var tensor = output.AsTensor<double>();
                return new OnnxTensorOutput(output.Name, typeof(double), tensor.ToArray(), tensor.Dimensions.ToArray());
            }

            if (elementType == typeof(int))
            {
                var tensor = output.AsTensor<int>();
                return new OnnxTensorOutput(output.Name, typeof(int), tensor.ToArray(), tensor.Dimensions.ToArray());
            }

            if (elementType == typeof(long))
            {
                var tensor = output.AsTensor<long>();
                return new OnnxTensorOutput(output.Name, typeof(long), tensor.ToArray(), tensor.Dimensions.ToArray());
            }

            if (elementType == typeof(bool))
            {
                var tensor = output.AsTensor<bool>();
                return new OnnxTensorOutput(output.Name, typeof(bool), tensor.ToArray(), tensor.Dimensions.ToArray());
            }

            throw new NotSupportedException(
                $"ONNX output '{output.Name}' uses unsupported tensor element type '{elementType.FullName}'.");
        }

        private void DisposeSessionNoLock()
        {
            _session?.Dispose();
            _session = null;
            _modelPath = null;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(OnnxModelRuntime));
            }
        }
    }
}
