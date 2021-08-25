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
            this.appData = appData;
        }

        private readonly AppData appData;

        public IReadOnlyList<ModbusChannel> Channels => appData.Channels;
        public ModbusChannel SelectedChannel { get => Get(() => appData?.Channels?.FirstOrDefault()); set => Set(value); }

        public bool IsAutoScroll { get => Get(false); set => Set(value); }
    }
}
