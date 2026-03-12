#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace OsEngine.MachineLearning.Onnx
{
    /// <summary>
    /// Managed tensor input definition for ONNX inference.
    /// </summary>
    public sealed class OnnxTensorInput
    {
        private readonly Func<NamedOnnxValue> _factory;
        private readonly int[] _dimensions;

        private OnnxTensorInput(string name, Type elementType, int[] dimensions, Func<NamedOnnxValue> factory)
        {
            Name = name;
            ElementType = elementType;
            _dimensions = dimensions;
            _factory = factory;
        }

        public string Name { get; }

        public Type ElementType { get; }

        public int[] Dimensions => (int[])_dimensions.Clone();

        public static OnnxTensorInput Create<T>(string name, T[] values, params int[] dimensions)
            where T : unmanaged
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Tensor input name is required.", nameof(name));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (dimensions == null || dimensions.Length == 0)
            {
                throw new ArgumentException("Tensor input dimensions are required.", nameof(dimensions));
            }

            int[] dimsCopy = (int[])dimensions.Clone();

            if (dimsCopy.Any(dim => dim <= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(dimensions), "Tensor dimensions must be greater than zero.");
            }

            int expectedLength = 1;

            for (int i = 0; i < dimsCopy.Length; i++)
            {
                checked
                {
                    expectedLength *= dimsCopy[i];
                }
            }

            if (expectedLength != values.Length)
            {
                throw new ArgumentException(
                    $"Tensor input '{name}' expects {expectedLength} values for shape [{string.Join(", ", dimsCopy)}], but received {values.Length}.",
                    nameof(values));
            }

            T[] valuesCopy = (T[])values.Clone();

            return new OnnxTensorInput(
                name,
                typeof(T),
                dimsCopy,
                () => NamedOnnxValue.CreateFromTensor(name, new DenseTensor<T>(valuesCopy, dimsCopy)));
        }

        internal NamedOnnxValue CreateNamedValue()
        {
            return _factory();
        }
    }
}
