﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    [View(typeof(IEditModbusCoilWriterView))]
    public sealed partial class EditModbusCoilWriterView : ContentDialog, IEditModbusCoilWriterView
    {
        public EditModbusCoilWriterView()
        {
            this.InitializeComponent();
        }
    }
}
