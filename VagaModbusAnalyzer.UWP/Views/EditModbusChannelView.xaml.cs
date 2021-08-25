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
    [View(typeof(IEditModbusChannelView))]
    public sealed partial class EditModbusChannelView : UserControl, IEditModbusChannelView
    {
        public EditModbusChannelView()
        {
            this.InitializeComponent();
            Loaded += EditModbusChannelView_Loaded;
        }

        private void EditModbusChannelView_Loaded(object sender, RoutedEventArgs e)
        {
            PART_TextBox_Name.Focus(FocusState.Keyboard);
            PART_TextBox_Name.SelectAll();
        }

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null) return;

            List<string> portNames = new List<string>();

            string serialSelector = SerialDevice.GetDeviceSelector();
            DeviceInformationCollection infoList = await DeviceInformation.FindAllAsync(serialSelector);
            foreach (var info in infoList)
            {
                SerialDevice serialDevice = await SerialDevice.FromIdAsync(info.Id);
                if (serialDevice != null)
                {
                    portNames.Add(serialDevice.PortName);
                    serialDevice.Dispose();
                }
            }

            if (portNames.Count == 0)
            {
                portNames = null;
            }
            else
            {
                sender.ItemsSource = portNames;
                sender.IsSuggestionListOpen = true;
            }
        }
    }
}
