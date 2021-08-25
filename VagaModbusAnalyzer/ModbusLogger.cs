using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using VagabondK.Protocols.Logging;

namespace VagaModbusAnalyzer
{
    public class ModbusLogger : NotifyPropertyChangeObject, IChannelLogger
    {
        public ModbusLogger(ModbusChannel modbusChannel)
        {
            this.modbusChannel = modbusChannel;
        }

        private readonly ModbusChannel modbusChannel;
        internal ICrossThreadDispatcher dispatcher;

        public void Log(ChannelLog log)
        {
            if (IsOn)
            {
                dispatcher?.Invoke(() =>
                {
                    itemsSource.Add(log);
                    if (SelectedItem == null)
                        SelectedItem = log;

                    if (itemsSource.Count > 10000)
                        itemsSource.RemoveAt(0);
                });
            }
        }

        public ICommand ClearCommand { get => GetCommand(Clear); }

        public void Clear()
        {
            itemsSource.Clear();
        }

        public bool IsOn { get => Get(false); set => Set(value); }

        private ObservableCollection<ChannelLog> itemsSource = new ObservableCollection<ChannelLog>();

        public ReadOnlyObservableCollection<ChannelLog> ItemsSource { get => Get(() => new ReadOnlyObservableCollection<ChannelLog>(itemsSource)); }
        public ChannelLog SelectedItem
        {
            get => Get<ChannelLog>();
            set
            {
                var oldValue = SelectedItem;
                Set(value);
                if (value == null && ItemsSource.Count > 0)
                    Set(oldValue);
            }
        }

    }
}
