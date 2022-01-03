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
    public class WriteData : NotifyPropertyChangeObject
    {
        public WriteData(AppData appData, IDialog dialog, IStringLocalizer stringLocalizer, ICrossThreadDispatcher dispatcher)
        {
            AppData = appData;
            this.dialog = dialog;
            this.stringLocalizer = stringLocalizer;
            this.dispatcher = dispatcher;
        }

        public AppData AppData { get; }
        private readonly IDialog dialog;
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICrossThreadDispatcher dispatcher;

        public InstantCommand AddHoldingRegisterWriterCommand { get => Get(() => new InstantCommand(AddHoldingRegisterWriter)); }
        public InstantCommand AddCoilWriterCommand { get => Get(() => new InstantCommand(AddCoilWriter)); }
        public InstantCommand<ModbusWriter> EditWriterCommand { get => Get(() => new InstantCommand<ModbusWriter>(EditWriter, CanExecute)); }
        public InstantCommand<ModbusWriter> DeleteWriterCommand { get => Get(() => new InstantCommand<ModbusWriter>(DeleteWriter, CanExecute)); }
        public InstantCommand<ModbusWriter> ModbusWriteCommand { get => Get(() => new InstantCommand<ModbusWriter>(ModbusWrite, CanExecute)); }


        private void AddHoldingRegisterWriter()
            => AppData.SelectedChannel.ModbusWriters.Add(new ModbusHoldingRegisterWriter());

        private async void AddCoilWriter()
        {
            if (await dialog.ShowDialog<EditModbusCoilWriter>(stringLocalizer["EditModbusCoilWriterView_AddWriter/Title"], out var editCoilWriter) == true)
            {
                AppData.SelectedChannel.ModbusWriters.Add(new ModbusCoilWriter
                {
                    SlaveAddress = (byte)editCoilWriter.SlaveAddress,
                    Address = (ushort)editCoilWriter.Address,
                    ResponseTimeout = editCoilWriter.ResponseTimeout,
                    WriteSettings = new System.Collections.ObjectModel.ObservableCollection<ModbusWriteCoilSetting>(
                        Enumerable.Range(0, editCoilWriter.Length).Select(i => new ModbusWriteCoilSetting()))
                });
            }
        }

        private async void EditWriter(ModbusWriter modbusWriter)
        {
            if (modbusWriter is ModbusCoilWriter coilWriter
                && await dialog.ShowDialog<EditModbusCoilWriter>(stringLocalizer["EditModbusCoilWriterView_EditWriter/Title"], viewModel =>
            {
                viewModel.SlaveAddress = coilWriter.SlaveAddress;
                viewModel.Address = coilWriter.Address;
                viewModel.ResponseTimeout = coilWriter.ResponseTimeout;
                viewModel.Length = coilWriter.WriteSettings.Count;
            }, out var editCoilWriter) == true)
            {
                lock (coilWriter)
                {
                    coilWriter.SlaveAddress = (byte)editCoilWriter.SlaveAddress;
                    coilWriter.Address = (ushort)editCoilWriter.Address;
                    coilWriter.ResponseTimeout = editCoilWriter.ResponseTimeout;

                    while (coilWriter.WriteSettings.Count < editCoilWriter.Length)
                        coilWriter.WriteSettings.Add(new ModbusWriteCoilSetting());
                    while (coilWriter.WriteSettings.Count > editCoilWriter.Length)
                        coilWriter.WriteSettings.RemoveAt(coilWriter.WriteSettings.Count - 1);
                }
            }
        }

        private void DeleteWriter(ModbusWriter modbusWriter)
            => AppData.SelectedChannel.ModbusWriters.Remove(modbusWriter);

        private async void ModbusWrite(ModbusWriter parameter)
            => await AppData.SelectedChannel.Write(parameter, dispatcher);

        private bool CanExecute(ModbusWriter modbusWriter) => modbusWriter != null;
    }
}
