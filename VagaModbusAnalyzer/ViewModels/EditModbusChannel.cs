using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using VagabondK.App;
using VagaModbusAnalyzer;
using VagaModbusAnalyzer.ChannelSetting;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(DefaultViewType = typeof(Views.IEditModbusChannelView))]
    public class EditModbusChannel : NotifyPropertyChangeObject
    {
        public EditModbusChannel(PageContext pageContext, ModbusChannels channelMgt, ICrossThreadDispatcher dispatcher)
        {
            this.pageContext = pageContext;
            this.channelMgt = channelMgt;
            this.dispatcher = dispatcher;

            editingChannel = channelMgt.SelectedChannel;
            Channel = editingChannel.Copy();
        }

        private readonly PageContext pageContext;
        private readonly ModbusChannel editingChannel;
        private readonly ModbusChannels channelMgt;
        private readonly ICrossThreadDispatcher dispatcher;

        public ModbusChannel Channel { get; }
        public ICommand SaveCommand { get => GetCommand(Save); }

        private void Save()
        {
            editingChannel.StopScan();
            editingChannel.Name = Channel.Name;
            editingChannel.ModbusType = Channel.ModbusType;
            editingChannel.ChannelSetting = Channel.ChannelSetting;
            editingChannel.ScanInterval = Channel.ScanInterval;
            editingChannel.StartScan(channelMgt.ChannelFactory, dispatcher);

            pageContext.Result = true;
        }
    }
}
