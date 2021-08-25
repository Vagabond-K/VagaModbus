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
    public class AddModbusChannel : NotifyPropertyChangeObject
    {
        public AddModbusChannel(PageContext pageContext, AppData appData, IStringLocalizer stringLocalizer, ModbusChannels channelMgt, ICrossThreadDispatcher dispatcher)
        {
            this.pageContext = pageContext;
            this.appData = appData;
            this.channelMgt = channelMgt;
            this.dispatcher = dispatcher;

            var newChannel = new ModbusChannel();
            string defaultNewChannelName = stringLocalizer["NewChannelName"];
            string newChannelName = defaultNewChannelName;
            int nameExistCount = 1;

            while (this.appData.Channels.Where(c => c.Name == newChannelName).Count() > 0)
            {
                nameExistCount++;
                newChannelName = $"{defaultNewChannelName}({nameExistCount})";
            }

            newChannel.Name = newChannelName;

            Channel = newChannel;
        }

        private readonly PageContext pageContext;
        private readonly AppData appData;
        private readonly ModbusChannels channelMgt;
        private readonly ICrossThreadDispatcher dispatcher;

        public ModbusChannel Channel { get; }
        public ICommand SaveCommand { get => GetCommand(Save); }

        private void Save()
        {
            appData.Channels.Add(Channel);
            channelMgt.SelectedChannel = Channel;
            Channel.StartScan(channelMgt.ChannelFactory, dispatcher);

            pageContext.Result = true;
        }
    }
}
