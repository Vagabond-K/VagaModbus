using System;
using System.Collections.Generic;
using System.Text;
using VagabondK.Protocols.Modbus;
using VagaModbusAnalyzer.Views;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(DefaultViewType = typeof(IEditModbusScanView))]
    public class EditModbusScan : NotifyPropertyChangeObject
    {
        public ModbusObjectType ObjectType { get => Get(ModbusObjectType.InputRegister); set => Set(value); }
        public int SlaveAddress { get => Get(1); set => Set(value); }
        public bool DetectSlaveAddress { get => Get(false); set => Set(value); }
        public int DetectSlaveAddrStart { get => Get(0); set => Set(value); }
        public int DetectSlaveAddrEnd { get => Get(255); set => Set(value); }
        public int Address { get => Get(0); set => Set(value); }
        public int Length { get => Get(1); set => Set(value); }
        public int ResponseTimeout { get => Get(1000); set => Set(value); }
    }
}
