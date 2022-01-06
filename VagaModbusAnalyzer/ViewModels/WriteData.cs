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
        public WriteData(AppData appData, Shell shell, IDialog dialog, IStringLocalizer stringLocalizer, ICrossThreadDispatcher dispatcher)
        {
            AppData = appData;
            this.shell = shell;
            this.dialog = dialog;
            this.stringLocalizer = stringLocalizer;
            this.dispatcher = dispatcher;
        }

        public AppData AppData { get; }
        private readonly Shell shell;
        private readonly IDialog dialog;
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICrossThreadDispatcher dispatcher;

        public InstantCommand AddHoldingRegisterWriterCommand { get => Get(() => new InstantCommand(AddHoldingRegisterWriter)); }
        public InstantCommand AddCoilWriterCommand { get => Get(() => new InstantCommand(AddCoilWriter)); }
        public InstantCommand<ModbusWriter> EditWriterCommand { get => Get(() => new InstantCommand<ModbusWriter>(EditWriter, CanExecute)); }
        public InstantCommand<ModbusWriter> DeleteWriterCommand { get => Get(() => new InstantCommand<ModbusWriter>(DeleteWriter, CanExecute)); }
        public InstantCommand<ModbusWriter> ModbusWriteCommand { get => Get(() => new InstantCommand<ModbusWriter>(ModbusWrite, CanExecute)); }


        private async void AddHoldingRegisterWriter()
        {
            await shell.OpenPage<AddModbusHoldingRegisterWriter>($"{AppData.SelectedChannel.Name} > {stringLocalizer["WriteDataView_AppBarButton_AddHoldingRegisterWriter/Label"]}");
        }

        private async void AddCoilWriter()
        {
            if (await dialog.ShowDialog<EditModbusCoilWriter>(stringLocalizer["EditModbusCoilWriterView_AddWriter/Title"], out var editCoilWriter) == true)
            {
                AppData.SelectedChannel.ModbusWriters.Add(new ModbusWriter
                {
                    SlaveAddress = (byte)editCoilWriter.SlaveAddress,
                    Address = (ushort)editCoilWriter.Address,
                    ResponseTimeout = editCoilWriter.ResponseTimeout,
                    ObjectType = VagabondK.Protocols.Modbus.ModbusObjectType.Coil,
                    WriteValues = new System.Collections.ObjectModel.ObservableCollection<ModbusWriteValue>(
                        Enumerable.Range(0, editCoilWriter.Length).Select(i => new ModbusWriteValue() { Type = TypeCode.Boolean }))
                });
            }
        }

        private async void EditWriter(ModbusWriter modbusWriter)
        {
            if (modbusWriter is ModbusWriter coilWriter
                && await dialog.ShowDialog<EditModbusCoilWriter>(stringLocalizer["EditModbusCoilWriterView_EditWriter/Title"], viewModel =>
            {
                viewModel.SlaveAddress = coilWriter.SlaveAddress;
                viewModel.Address = coilWriter.Address;
                viewModel.ResponseTimeout = coilWriter.ResponseTimeout;
                viewModel.Length = coilWriter.WriteValues.Count;
            }, out var editCoilWriter) == true)
            {
                lock (coilWriter)
                {
                    coilWriter.SlaveAddress = (byte)editCoilWriter.SlaveAddress;
                    coilWriter.Address = (ushort)editCoilWriter.Address;
                    coilWriter.ResponseTimeout = editCoilWriter.ResponseTimeout;

                    while (coilWriter.WriteValues.Count < editCoilWriter.Length)
                        coilWriter.WriteValues.Add(new ModbusWriteValue() { Type = TypeCode.Boolean });
                    while (coilWriter.WriteValues.Count > editCoilWriter.Length)
                        coilWriter.WriteValues.RemoveAt(coilWriter.WriteValues.Count - 1);
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
