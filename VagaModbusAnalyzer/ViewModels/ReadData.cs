using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VagabondK.App;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(ServiceLifetime.Singleton)]
    public class ReadData : NotifyPropertyChangeObject
    {
        public ReadData(AppData appData, IDialog dialog, IStringLocalizer stringLocalizer)
        {
            this.appData = appData;
            this.dialog = dialog;
            this.stringLocalizer = stringLocalizer;
        }

        private readonly AppData appData;
        private readonly IDialog dialog;
        private readonly IStringLocalizer stringLocalizer;


        public InstantCommand AddScanCommand { get => Get(() => new InstantCommand(AddScan)); }
        public InstantCommand<ModbusScan> EditScanCommand { get => Get(() => new InstantCommand<ModbusScan>(EditScan, CanEditScan)); }
        public InstantCommand<ModbusScan> DeleteScanCommand { get => Get(() => new InstantCommand<ModbusScan>(DeleteScan, CanDeleteScan)); }

        public InstantCommand<object> ModbusWriteCommand { get => Get(() => new InstantCommand<object>(ModbusWrite)); }

        private void ModbusWrite(object parameter)
        {
        }

        public IReadOnlyList<ModbusChannel> Channels => appData.Channels;
        public ModbusChannel SelectedChannel { get { return Get(() => appData?.Channels?.FirstOrDefault()); } set { Set(value); } }

        private async void AddScan()
        {
            if (await dialog.ShowDialog<EditModbusScan>(stringLocalizer["EditModbusScanView_AddScan/Title"], out var editModbusScan) == true)
            {
                SelectedChannel.ModbusScans.Add(ModbusScan.CreateScan(
                    SelectedChannel,
                    editModbusScan.ObjectType,
                    (byte)editModbusScan.SlaveAddress,
                    (ushort)editModbusScan.Address,
                    (ushort)editModbusScan.Length,
                    editModbusScan.ResponseTimeout
                ));
            }
        }

        private async void EditScan(ModbusScan modbusScan)
        {
            if (await dialog.ShowDialog<EditModbusScan>(stringLocalizer["EditModbusScanView_EditScan/Title"], viewModel =>
            {
                viewModel.SlaveAddress = modbusScan.SlaveAddress;
                viewModel.ObjectType = modbusScan.ObjectType;
                viewModel.Address = modbusScan.Address;
                viewModel.Length = modbusScan.Length;
                viewModel.ResponseTimeout = modbusScan.ResponseTimeout;
            }, out var editModbusScan) == true)
            {
                lock (modbusScan)
                {
                    bool updated = false;
                    if (modbusScan.ObjectType != editModbusScan.ObjectType)
                    {
                        modbusScan.ObjectType = editModbusScan.ObjectType;
                        updated = true;
                    }
                    if (modbusScan.SlaveAddress != editModbusScan.SlaveAddress)
                    {
                        modbusScan.SlaveAddress = (byte)editModbusScan.SlaveAddress;
                        updated = true;
                    }
                    if (modbusScan.Address != editModbusScan.Address)
                    {
                        modbusScan.Address = (ushort)editModbusScan.Address;
                        updated = true;
                    }
                    if (modbusScan.Length != editModbusScan.Length)
                    {
                        modbusScan.Length = (ushort)editModbusScan.Length;
                        updated = true;
                    }

                    modbusScan.ResponseTimeout = editModbusScan.ResponseTimeout;

                    if (updated)
                        modbusScan.OnSettingChanged();
                }
            }
        }

        private bool CanEditScan(ModbusScan modbusScan)
        {
            return modbusScan != null;
        }

        private void DeleteScan(ModbusScan modbusScan)
        {
            SelectedChannel.ModbusScans.Remove(modbusScan);
        }

        private bool CanDeleteScan(ModbusScan modbusScan)
        {
            return modbusScan != null;
        }
    }
}
