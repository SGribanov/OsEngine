#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Linq;

namespace OsEngine.MachineLearning.Onnx
{
    /// <summary>
    /// Materialized tensor output returned by ONNX runtime.
    /// </summary>
    public sealed class OnnxTensorOutput
    {
        private readonly Array _values;
        private readonly int[] _dimensions;

        internal OnnxTensorOutput(string name, Type elementType, Array values, int[] dimensions)
        {
            Name = name;
            ElementType = elementType;
            _values = values;
            _dimensions = dimensions;
        }

        public string Name { get; }

        public Type ElementType { get; }

        public int[] Dimensions => (int[])_dimensions.Clone();

        public T[] GetTensorData<T>()
            where T : unmanaged
        {
            if (_values is not T[] typedValues)
            {
                throw new InvalidOperationException(
                    $"Tensor output '{Name}' contains '{ElementType.Name}', but '{typeof(T).Name}' was requested.");
            }

            return (T[])typedValues.Clone();
        }

        public T GetScalar<T>()
            where T : unmanaged
        {
            T[] values = GetTensorData<T>();

            if (values.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Tensor output '{Name}' is not a scalar. Shape: [{string.Join(", ", _dimensions)}], values: {values.Length}.");
            }

            return values[0];
        }

        public override string ToString()
        {
            return $"{Name} ({ElementType.Name}) [{string.Join(", ", _dimensions)}]";
        }
    }
}
