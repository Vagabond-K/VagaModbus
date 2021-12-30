using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(ServiceLifetime.Singleton)]
    public class Logs : NotifyPropertyChangeObject
    {
        public Logs(AppData appData)
        {
            AppData = appData;
        }

        public AppData AppData { get; }

        //public IReadOnlyList<ModbusChannel> Channels => AppData.Channels;
        //public ModbusChannel SelectedChannel { get => AppData?.SelectedChannel; set => AppData.SelectedChannel = value; }

        public bool IsAutoScroll { get => Get(false); set => Set(value); }
    }
}
