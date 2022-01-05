using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VagaModbusAnalyzer.Views
{
    [View(typeof(IEditModbusHoldingRegisterWriterView))]
    public sealed partial class EditModbusHoldingRegisterWriterView : UserControl, IEditModbusHoldingRegisterWriterView
    {
        public EditModbusHoldingRegisterWriterView()
        {
            this.InitializeComponent();
        }
    }
}
