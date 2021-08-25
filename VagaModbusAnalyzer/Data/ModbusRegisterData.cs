using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusRegisterData<TValue> : ModbusData<TValue>
    {
        private bool isRegisterAddress;

        public bool IsRegisterAddress { get => isRegisterAddress; set => SetProperty(ref isRegisterAddress, value); }
    }
}
