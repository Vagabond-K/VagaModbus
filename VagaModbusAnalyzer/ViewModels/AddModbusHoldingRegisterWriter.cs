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

            ModbusWriter = new ModbusHoldingRegisterWriter();
            ModbusWriter.WriteValues.CollectionChanged += OnWriteValuesCollectionChanged;
        }

        private void OnWriteValuesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveCommand.RaiseCanExecuteChanged();
        }

        private readonly PageContext pageContext;

        public AppData AppData { get; }
        public ModbusHoldingRegisterWriter ModbusWriter { get; }

        public ICommand AddValueCommand { get => GetCommand(() => ModbusWriter.WriteValues.Add(new ModbusWriteHoldingRegister())); }
        public ICommand DeleteValueCommand { get => GetCommand<ModbusWriteHoldingRegister>(Delete); }

        public IInstantCommand SaveCommand { get => GetCommand(Save, CanSave); }

        private void Delete(ModbusWriteHoldingRegister writeValue)
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
