using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(DefaultViewType = typeof(Views.IEditModbusCoilWriterView))]
    public class EditModbusCoilWriter : NotifyPropertyChangeObject
    {
        public int SlaveAddress { get => Get(1); set => Set(value); }
        public int Address { get => Get(0); set => Set(value); }
        public int ResponseTimeout { get => Get(5000); set => Set(value); }
        public int Length { get => Get(1); set => Set(value); }
    }
}
