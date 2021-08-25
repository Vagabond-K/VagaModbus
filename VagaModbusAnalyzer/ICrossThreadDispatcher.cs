using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer
{
    public interface ICrossThreadDispatcher
    {
        void Invoke(Action callback);
    }
}
