using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using VagabondK.App;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(DefaultViewType = typeof(Views.IEditModbusHoldingRegisterWriterView))]
    public class AddModbusHoldingRegisterWriter : NotifyPropertyChangeObject
    {
        public AddModbusHoldingRegisterWriter(AppData appData, PageContext pageContext)
        {
            AppData = appData;
            this.pageContext = pageContext;

            ModbusWriter = new ModbusWriter()
            {
                ObjectType = VagabondK.Protocols.Modbus.ModbusObjectType.HoldingRegister
            };
            ModbusWriter.WriteValues.CollectionChanged += OnWriteValuesCollectionChanged;
            ModbusWriter.WriteValues.Add(new ModbusWriteValue());
        }

        private void OnWriteValuesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveCommand.RaiseCanExecuteChanged();
        }

        private readonly PageContext pageContext;

        public AppData AppData { get; }
        public ModbusWriter ModbusWriter { get; }

        public ICommand AddValueCommand { get => GetCommand(() => ModbusWriter.WriteValues.Add(new ModbusWriteValue())); }
        public ICommand DeleteValueCommand { get => GetCommand<ModbusWriteValue>(Delete); }

        public IInstantCommand SaveCommand { get => GetCommand(Save, CanSave); }

        private void Delete(ModbusWriteValue writeValue)
        {
            if (writeValue != null)
                ModbusWriter.WriteValues.Remove(writeValue);
        }

        private void Save()
        {
            ModbusWriter.WriteValues.CollectionChanged -= OnWriteValuesCollectionChanged;
            AppData.SelectedChannel.ModbusWriters.Add(ModbusWriter);
            pageContext.Result = true;
        }

        private bool CanSave() => ModbusWriter.WriteValues.Count > 0;
    }
}
