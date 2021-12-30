using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VagabondK.App;
using VagaModbusAnalyzer;
using VagaModbusAnalyzer.ChannelSetting;
using VagaModbusAnalyzer.Views;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(ServiceLifetime.Singleton)]
    public class ModbusChannels : NotifyPropertyChangeObject
    {
        public ModbusChannels(AppData appData, Shell shell, IStringLocalizer stringLocalizer, IChannelFactory channelFactory)
        {
            AppData = appData;
            this.shell = shell;
            this.stringLocalizer = stringLocalizer;
            ChannelFactory = channelFactory;
        }

        public AppData AppData { get; }
        private readonly Shell shell;
        private readonly IStringLocalizer stringLocalizer;

        public IChannelFactory ChannelFactory { get; }
        public InstantCommand AddChannelCommand => GetCommand(AddChannel);
        public InstantCommand<ModbusChannel> EditChannelCommand => GetCommand<ModbusChannel>(EditChannel, channel => channel != null);
        public InstantCommand<ModbusChannel> DeleteChannelCommand => GetCommand<ModbusChannel>(DeleteChannel, channel => channel != null);

        public async void AddChannel()
        {
            await shell.OpenPage<AddModbusChannel>($"{stringLocalizer["MasterDetailMenuButton_ModbusChannels/Content"]} > {stringLocalizer["EditModbusChannelView_AddChannel/Title"]}");
        }

        public async void EditChannel(ModbusChannel channel)
        {
            AppData.SelectedChannel = channel;
            await shell.OpenPage<EditModbusChannel, IEditModbusChannelView>($"{stringLocalizer["MasterDetailMenuButton_ModbusChannels/Content"]} > {stringLocalizer["EditModbusChannelView_EditChannel/Title"]}");
        }

        public void DeleteChannel(ModbusChannel channel)
        {
            if (channel == null) return;

            channel.StopScan();

            int index = AppData.Channels.IndexOf(channel);
            AppData.Channels.Remove(channel);

            if (index < AppData.Channels.Count)
                AppData.SelectedChannel = AppData.Channels[index];
            else if (AppData.Channels.Count > 0)
                AppData.SelectedChannel = AppData.Channels.Last();
            else
                AppData.SelectedChannel = null;
        }
    }
}
