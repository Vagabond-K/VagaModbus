using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public abstract class ModbusData : INotifyPropertyChanged
    {
        private ushort address;

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<TProperty>(ref TProperty target, TProperty value, [CallerMemberName] string propertyName = null)
            => this.Set(ref target, value, PropertyChanged, propertyName);

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ushort Address { get => address; set => SetProperty(ref address, value); }
    }

    public abstract class ModbusData<TValue> : ModbusData, INotifyPropertyChanged
    {
        private TValue value;

        [JsonIgnore]
        public TValue Value { get => value; set => SetProperty(ref this.value, value); }
    }
}
