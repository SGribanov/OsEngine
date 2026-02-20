/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Threading;

#nullable enable

namespace OsEngine.Entity
{
    public class AwaitObject
    {
        public AwaitObject(string label, decimal maxValue, decimal curValue, bool needStartFakeValueMove)
        {
            _label = label;
            _valueMaximum = maxValue;
            _valueCurrent = curValue;

            if(needStartFakeValueMove)
            {
                Thread worker = new Thread(FakeMoveValueTreadArea);
                worker.IsBackground = true;
                worker.Start();
            }
        }

        public void Dispose()
        {
            _isDisposed = true;

            DisposedEvent?.Invoke();
        }

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }
        private bool _isDisposed;

        private void FakeMoveValueTreadArea()
        {
            while(true)
            {
                Thread.Sleep(500);

                if(_isDisposed)
                {
                    return;
                }

                decimal val = ValueCurrent;

                val += val + _valueMaximum / 7;

                if(val > _valueMaximum)
                {
                    val = 0;
                }

                ValueCurrent = val;
            }
        }

        public string Label
        {
            get { return _label; }
            set
            {
                if(_label == value)
                {
                    return;
                }
                _label = value;

                LabelChangedEvent?.Invoke(_label);
            }
        }
        private string _label;

        public decimal ValueCurrent
        {
            get { return _valueCurrent; }
            set
            {
                if(value == _valueCurrent)
                {
                    return;
                }

                _valueCurrent = value;

                ValueCurrentChangedEvent?.Invoke(_valueCurrent);
            }
        }
        private decimal _valueCurrent;

        public decimal ValueMaximum
        {
            get { return _valueMaximum; }
            set
            {
                if(value == _valueMaximum)
                {
                    return;
                }

                _valueMaximum = value;

                ValueMaximumChangedEvent?.Invoke(_valueMaximum);
            }
        }

        private decimal _valueMaximum;

        public event Action<string>? LabelChangedEvent;

        public event Action<decimal>? ValueCurrentChangedEvent;

        public event Action<decimal>? ValueMaximumChangedEvent;

        public event Action? DisposedEvent;
    }
}
