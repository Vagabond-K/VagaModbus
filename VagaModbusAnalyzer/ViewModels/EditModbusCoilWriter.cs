using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(DefaultViewType = typeof(Views.IEditModbusCoilWriterView))]
    public class EditModbusCoilWriter : EditModbusWriter
    {
        public int Length { get => Get(1); set => Set(value); }
    }
}
