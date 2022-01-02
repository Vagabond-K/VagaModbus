using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;
using VagabondK.App;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(ServiceLifetime.Singleton)]
    public class WriteData : NotifyPropertyChangeObject
    {
        public WriteData(AppData appData, ICrossThreadDispatcher dispatcher)
        {
            AppData = appData;
            this.dispatcher = dispatcher;
        }

        public AppData AppData { get; }
        private readonly ICrossThreadDispatcher dispatcher;

        public InstantCommand AddHoldingRegisterWriterCommand { get => Get(() => new InstantCommand(AddHoldingRegisterWriter)); }
        public InstantCommand AddCoilWriterCommand { get => Get(() => new InstantCommand(AddCoilWriter)); }
        public InstantCommand<ModbusWriter> EditWriterCommand { get => Get(() => new InstantCommand<ModbusWriter>(EditWriter, CanExecute)); }
        public InstantCommand<ModbusWriter> DeleteWriterCommand { get => Get(() => new InstantCommand<ModbusWriter>(DeleteWriter, CanExecute)); }
        public InstantCommand<ModbusWriter> ModbusWriteCommand { get => Get(() => new InstantCommand<ModbusWriter>(ModbusWrite, CanExecute)); }


        private void AddHoldingRegisterWriter()
            => AppData.SelectedChannel.ModbusWriters.Add(new ModbusHoldingRegisterWriter());

        private void AddCoilWriter()
            => AppData.SelectedChannel.ModbusWriters.Add(new ModbusCoilWriter());

        private void EditWriter(ModbusWriter modbusWriter)
        {
        }

        private void DeleteWriter(ModbusWriter modbusWriter)
            => AppData.SelectedChannel.ModbusWriters.Remove(modbusWriter);

        private async void ModbusWrite(ModbusWriter parameter)
            => await AppData.SelectedChannel.Write(parameter, dispatcher);

        private bool CanExecute(ModbusWriter modbusWriter) => modbusWriter != null;
    }
}
