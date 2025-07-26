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
            AppData = appData;
            this.dialog = dialog;
            this.stringLocalizer = stringLocalizer;
        }

        public AppData AppData { get; }
        private readonly IDialog dialog;
        private readonly IStringLocalizer stringLocalizer;


        public InstantCommand AddScanCommand { get => Get(() => new InstantCommand(AddScan)); }
        public InstantCommand<ModbusScan> EditScanCommand { get => Get(() => new InstantCommand<ModbusScan>(EditScan, CanEditScan)); }
        public InstantCommand<ModbusScan> DeleteScanCommand { get => Get(() => new InstantCommand<ModbusScan>(DeleteScan, CanDeleteScan)); }

        public InstantCommand<object> ModbusWriteCommand { get => Get(() => new InstantCommand<object>(ModbusWrite)); }

        private void ModbusWrite(object parameter)
        {
        }

        private async void AddScan()
        {
            if (await dialog.ShowDialog<EditModbusScan>(stringLocalizer["EditModbusScanView_AddScan/Title"], out var editModbusScan) == true)
            {
                AppData.SelectedChannel.ModbusScans.Add(new ModbusScan
                {
                    ObjectType = editModbusScan.ObjectType,
                    SlaveAddress = (byte)editModbusScan.SlaveAddress,
                    DetectSlaveAddress = editModbusScan.DetectSlaveAddress,
                    DetectSlaveAddrStart = (byte)editModbusScan.DetectSlaveAddrStart,
                    DetectSlaveAddrEnd = (byte)editModbusScan.DetectSlaveAddrEnd,
                    Address = (ushort)editModbusScan.Address,
                    Length = (ushort)editModbusScan.Length,
                    ResponseTimeout = editModbusScan.ResponseTimeout
                });
            }
        }

        private async void EditScan(ModbusScan modbusScan)
        {
            if (await dialog.ShowDialog<EditModbusScan>(stringLocalizer["EditModbusScanView_EditScan/Title"], viewModel =>
            {
                viewModel.SlaveAddress = modbusScan.SlaveAddress;
                viewModel.DetectSlaveAddress = modbusScan.DetectSlaveAddress;
                viewModel.DetectSlaveAddrStart = modbusScan.DetectSlaveAddrStart;
                viewModel.DetectSlaveAddrEnd = modbusScan.DetectSlaveAddrEnd;
                viewModel.ObjectType = modbusScan.ObjectType;
                viewModel.Address = modbusScan.Address;
                viewModel.Length = modbusScan.Length;
                viewModel.ResponseTimeout = modbusScan.ResponseTimeout;
            }, out var editModbusScan) == true)
            {
                bool updated = false;
                lock (modbusScan)
                {
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
                    if (modbusScan.DetectSlaveAddress != editModbusScan.DetectSlaveAddress)
                    {
                        modbusScan.DetectSlaveAddress = editModbusScan.DetectSlaveAddress;
                        updated = true;
                    }
                    if (modbusScan.DetectSlaveAddrStart != editModbusScan.DetectSlaveAddrStart)
                    {
                        modbusScan.DetectSlaveAddrStart = (byte)editModbusScan.DetectSlaveAddrStart;
                        updated = true;
                    }
                    if (modbusScan.DetectSlaveAddrEnd != editModbusScan.DetectSlaveAddrEnd)
                    {
                        modbusScan.DetectSlaveAddrEnd = (byte)editModbusScan.DetectSlaveAddrEnd;
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
                    {
                        modbusScan.CurrentSlaveAddress = null;
                        modbusScan.Status = null;
                    }
                }
                if (updated)
                    modbusScan.OnSettingChanged();
            }
        }

        private bool CanEditScan(ModbusScan modbusScan)
        {
            return modbusScan != null;
        }

        private void DeleteScan(ModbusScan modbusScan)
        {
            AppData.SelectedChannel.ModbusScans.Remove(modbusScan);
        }

        private bool CanDeleteScan(ModbusScan modbusScan)
        {
            return modbusScan != null;
        }
    }
}
