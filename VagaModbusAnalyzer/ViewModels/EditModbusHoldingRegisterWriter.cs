using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(DefaultViewType = typeof(Views.IEditModbusHoldingRegisterWriterView))]
    public class EditModbusHoldingRegisterWriter : NotifyPropertyChangeObject
    {
    }
}
