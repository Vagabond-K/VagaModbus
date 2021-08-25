using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using VagabondK.App;
using VagabondK.Protocols.Channels;
using VagaModbusAnalyzer.ChannelSetting;

namespace VagaModbusAnalyzer
{
    public class AppData : NotifyPropertyChangeObject
    {
        public AppData()
        {
            Channels = new ObservableCollection<ModbusChannel>();
        }

        public ObservableCollection<ModbusChannel> Channels { get => Get<ObservableCollection<ModbusChannel>>(); set => Set(value); }

        [JsonIgnore]
        public Shell Shell { get => Get<Shell>(); private set => Set(value); }

        public event EventHandler ChannelsCollectionChanged;

        protected override bool OnPropertyChanging(QueryPropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(Channels))
            {
                if (Channels != null)
                    Channels.CollectionChanged -= OnChannelsCollectionChanged;
            }
            
            return base.OnPropertyChanging(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(Channels))
            {
                if (Channels != null)
                    Channels.CollectionChanged += OnChannelsCollectionChanged;
            }
        }

        private void OnChannelsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var channel in e.OldItems)
                    (channel as ModbusChannel)?.Dispose();

            ChannelsCollectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
