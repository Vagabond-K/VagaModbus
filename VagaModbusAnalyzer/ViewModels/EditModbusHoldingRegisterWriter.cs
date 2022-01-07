using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using VagabondK.App;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(DefaultViewType = typeof(Views.IEditModbusHoldingRegisterWriterView))]
    public class EditModbusHoldingRegisterWriter : NotifyPropertyChangeObject
    {
        public EditModbusHoldingRegisterWriter(AppData appData, PageContext pageContext)
        {
            AppData = appData;
            this.pageContext = pageContext;
        }

        private void OnWriteValuesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveCommand.RaiseCanExecuteChanged();
        }

        private readonly PageContext pageContext;
        private ModbusWriter modbusWriter;

        public AppData AppData { get; }

        public virtual ModbusWriter ModbusWriter
        {
            get
            {
                if (modbusWriter == null)
                {
                    modbusWriter = new ModbusWriter()
                    {
                        ObjectType = VagabondK.Protocols.Modbus.ModbusObjectType.HoldingRegister
                    };
                    modbusWriter.WriteValues.CollectionChanged += OnWriteValuesCollectionChanged;
                    
                    var editingModbusWriter = (pageContext?.Owner?.ViewModel as WriteData)?.EditingModbusWriter;
                    if (editingModbusWriter != null)
                    {
                        editingModbusWriter.CopyTo(ModbusWriter);
                    }
                    else
                    {
                        modbusWriter.WriteValues.Add(new ModbusWriteValue());
                    }
                }
                return modbusWriter;
            }
        }

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

            var editingModbusWriter = (pageContext?.Owner?.ViewModel as WriteData)?.EditingModbusWriter;
            if (editingModbusWriter != null)
            {
                ModbusWriter.CopyTo(editingModbusWriter);
            }
            else
            {
                AppData.SelectedChannel.ModbusWriters.Add(ModbusWriter);
            }
            pageContext.Result = true;
        }

        private bool CanSave() => ModbusWriter.WriteValues.Count > 0;
    }
}
