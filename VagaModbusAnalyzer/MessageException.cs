using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer
{
    public class MessageException : Exception
    {
        public MessageException(string exception) : base(exception) { }
    }
}
