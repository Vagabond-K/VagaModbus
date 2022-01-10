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

        public ModbusWriter EditingModbusWriter { get; private set; }

        private async void AddHoldingRegisterWriter()
        {
            EditingModbusWriter = null;
            await shell.OpenPage<EditModbusHoldingRegisterWriter>($"{stringLocalizer["MasterDetailMenuButton_WriteData/Content"]} > {stringLocalizer["WriteDataView_AppBarButton_AddHoldingRegisterWriter/Label"]}");
        }

        private async void AddCoilWriter()
        {
            EditingModbusWriter = null;
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
            EditingModbusWriter = modbusWriter;
            switch (modbusWriter.ObjectType)
            {
                case VagabondK.Protocols.Modbus.ModbusObjectType.Coil:
                    if (await dialog.ShowDialog<EditModbusCoilWriter>(stringLocalizer["EditModbusCoilWriterView_EditWriter/Title"], viewModel =>
                    {
                        viewModel.SlaveAddress = modbusWriter.SlaveAddress;
                        viewModel.Address = modbusWriter.Address;
                        viewModel.ResponseTimeout = modbusWriter.ResponseTimeout;
                        viewModel.Length = modbusWriter.WriteValues.Count;
                    }, out var editCoilWriter) == true)
                    {
                        lock (modbusWriter)
                        {
                            modbusWriter.SlaveAddress = (byte)editCoilWriter.SlaveAddress;
                            modbusWriter.Address = (ushort)editCoilWriter.Address;
                            modbusWriter.ResponseTimeout = editCoilWriter.ResponseTimeout;

                            while (modbusWriter.WriteValues.Count < editCoilWriter.Length)
                                modbusWriter.WriteValues.Add(new ModbusWriteValue() { Type = TypeCode.Boolean });
                            while (modbusWriter.WriteValues.Count > editCoilWriter.Length)
                                modbusWriter.WriteValues.RemoveAt(modbusWriter.WriteValues.Count - 1);
                        }
                    }
                    break;
                case VagabondK.Protocols.Modbus.ModbusObjectType.HoldingRegister:
                    await shell.OpenPage<EditModbusHoldingRegisterWriter>($"{stringLocalizer["MasterDetailMenuButton_WriteData/Content"]} > {stringLocalizer["WriteDataView_AppBarButton_EditHoldingRegisterWriter/Label"]}");
                    break;
            }
        }

        private void DeleteWriter(ModbusWriter modbusWriter)
            => AppData.SelectedChannel.ModbusWriters.Remove(modbusWriter);

        private async void ModbusWrite(ModbusWriter parameter)
            => await AppData.SelectedChannel.Write(parameter, dispatcher);

        private bool CanExecute(ModbusWriter modbusWriter) => modbusWriter != null;
    }
}
