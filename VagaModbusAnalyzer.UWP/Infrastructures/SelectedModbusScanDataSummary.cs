using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagaModbusAnalyzer.Data;

namespace VagaModbusAnalyzer.Infrastructures
{
    public class SelectedModbusScanDataSummary : NotifyPropertyChangeObject
    {
        public ObservableCollection<ModbusData> SelectedItems
        {
            get => Get(() =>
            {
                var result = new ObservableCollection<ModbusData>();
                result.CollectionChanged += SelectedItemsCollectionChanged;
                return result;
            });
        }

        private void SelectedItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var oldItem in e.OldItems.Cast<ModbusData>())
                    if (oldItem is INotifyPropertyChanged notifyPropertyChanged)
                        notifyPropertyChanged.PropertyChanged -= OnModbusDataPropertyChanged;
            if (e.NewItems != null)
                foreach (var newItem in e.NewItems.Cast<ModbusData>())
                    if (newItem is INotifyPropertyChanged notifyPropertyChanged)
                        notifyPropertyChanged.PropertyChanged += OnModbusDataPropertyChanged;
            UpdateSummary();
        }

        private void OnModbusDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModbusData<object>.Value))
            {
                UpdateSummary();
            }
        }

        public void UpdateSummary()
        {
            lock (this)
            {
                var values = SelectedItems?.ToArray();
                HasValues = values != null && values.Length > 1
                    && !(values.Where(v => v is ModbusRegisterByte).Count() > 0
                    && values.Where(v => v is ModbusBoolean).Count() > 0);

                if (!HasValues)
                {
                    IsSelectedRegisterUnit = false;

                    StartAddressText = null;
                    EndAddressText = null;

                    SignedIntegerValueABCD = null;
                    SignedIntegerValueBADC = null;
                    SignedIntegerValueCDAB = null;
                    SignedIntegerValueDCBA = null;

                    UnsignedIntegerValueABCD = null;
                    UnsignedIntegerValueBADC = null;
                    UnsignedIntegerValueCDAB = null;
                    UnsignedIntegerValueDCBA = null;

                    SingleValueABCD = null;
                    SingleValueBADC = null;
                    SingleValueCDAB = null;
                    SingleValueDCBA = null;

                    DoubleValueABCD = null;
                    DoubleValueBADC = null;
                    DoubleValueCDAB = null;
                    DoubleValueDCBA = null;
                }
                else
                {
                    if (values[0] is ModbusBoolean)
                    {
                        IsSelectedRegisterUnit = false;
                        var bitDataItems = values.Cast<ModbusBoolean>().OrderBy(v => v.Address).ToArray();

                        StartAddressText = bitDataItems.First().Address.ToString();
                        EndAddressText = bitDataItems.Last().Address.ToString();

                        if (bitDataItems.Any(b => b.Value == null))
                        {
                            SignedIntegerValueABCD = null;
                            UnsignedIntegerValueABCD = null;
                        }
                        else
                        {
                            long signedIntegerValue = 0;
                            ulong unsignedIntegerValue = 0;
                            for (int i = 0; i < bitDataItems.Length; i++)
                            {
                                signedIntegerValue = signedIntegerValue | ((bitDataItems[bitDataItems.Length - 1 - i].Value.Value ? 1L : 0L) << i);
                                unsignedIntegerValue = unsignedIntegerValue | ((bitDataItems[bitDataItems.Length - 1 - i].Value.Value ? 1UL : 0UL) << i);
                            }

                            SignedIntegerValueABCD = signedIntegerValue;
                            UnsignedIntegerValueABCD = unsignedIntegerValue;
                        }

                        SignedIntegerValueBADC = null;
                        SignedIntegerValueCDAB = null;
                        SignedIntegerValueDCBA = null;

                        UnsignedIntegerValueBADC = null;
                        UnsignedIntegerValueCDAB = null;
                        UnsignedIntegerValueDCBA = null;

                        SingleValueABCD = null;
                        SingleValueBADC = null;
                        SingleValueCDAB = null;
                        SingleValueDCBA = null;

                        DoubleValueABCD = null;
                        DoubleValueBADC = null;
                        DoubleValueCDAB = null;
                        DoubleValueDCBA = null;
                    }
                    else if (values[0] is ModbusRegisterByte)
                    {
                        var byteDataItems = values.Cast<ModbusRegisterByte>().OrderBy(v => v.Address).ThenBy(v => v.Register.FirstByte == v ? 0 : 1).ToArray();

                        var first = byteDataItems.First();
                        IsSelectedRegisterUnit = first.Register.FirstByte == first && byteDataItems.Length % 2 == 0;

                        StartAddressText = ToByteAddress(byteDataItems.First());
                        EndAddressText = ToByteAddress(byteDataItems.Last());

                        if (byteDataItems.Any(b => b.Value == null))
                        {
                            SignedIntegerValueABCD = null;
                            SignedIntegerValueBADC = null;
                            SignedIntegerValueCDAB = null;
                            SignedIntegerValueDCBA = null;

                            UnsignedIntegerValueABCD = null;
                            UnsignedIntegerValueBADC = null;
                            UnsignedIntegerValueCDAB = null;
                            UnsignedIntegerValueDCBA = null;

                            SingleValueABCD = null;
                            SingleValueBADC = null;
                            SingleValueCDAB = null;
                            SingleValueDCBA = null;

                            DoubleValueABCD = null;
                            DoubleValueBADC = null;
                            DoubleValueCDAB = null;
                            DoubleValueDCBA = null;
                        }
                        else
                        {
                            if (byteDataItems.Length <= 8)  //길이가 8바이트 이하일 경우
                            {
                                SignedIntegerValueABCD = ToInt64(byteDataItems.Select(b => b.Value.Value).ToArray());
                                UnsignedIntegerValueABCD = ToUInt64(byteDataItems.Select(b => b.Value.Value).ToArray());
                                SignedIntegerValueDCBA = ToInt64(byteDataItems.Reverse().Select(b => b.Value.Value).ToArray());
                                UnsignedIntegerValueDCBA = ToUInt64(byteDataItems.Reverse().Select(b => b.Value.Value).ToArray());

                                if (IsSelectedRegisterUnit)
                                {
                                    if (byteDataItems.Length == 2)
                                    {
                                        HasIntegerValueRegisterUnit = false;
                                        HasIntegerValueByteUnit = true;
                                    }
                                    else
                                    {
                                        HasIntegerValueRegisterUnit = true;
                                        HasIntegerValueByteUnit = false;
                                    }
                                    SignedIntegerValueBADC = ToInt64(ToBADC(byteDataItems).Select(b => b.Value.Value).ToArray());
                                    SignedIntegerValueCDAB = ToInt64(ToCDAB(byteDataItems).Select(b => b.Value.Value).ToArray());
                                    UnsignedIntegerValueBADC = ToUInt64(ToBADC(byteDataItems).Select(b => b.Value.Value).ToArray());
                                    UnsignedIntegerValueCDAB = ToUInt64(ToCDAB(byteDataItems).Select(b => b.Value.Value).ToArray());
                                }
                                else
                                {
                                    HasIntegerValueRegisterUnit = false;
                                    HasIntegerValueByteUnit = true;
                                    SignedIntegerValueBADC = null;
                                    SignedIntegerValueCDAB = null;
                                    UnsignedIntegerValueBADC = null;
                                    UnsignedIntegerValueCDAB = null;
                                }
                            }
                            else  //길이가 8바이트를 초과할 경우
                            {
                                HasIntegerValueRegisterUnit = false;
                                HasIntegerValueByteUnit = false;

                                SignedIntegerValueABCD = null;
                                SignedIntegerValueBADC = null;
                                SignedIntegerValueCDAB = null;
                                SignedIntegerValueDCBA = null;

                                UnsignedIntegerValueABCD = null;
                                UnsignedIntegerValueBADC = null;
                                UnsignedIntegerValueCDAB = null;
                                UnsignedIntegerValueDCBA = null;
                            }

                            if (byteDataItems.Length == 4)
                            {
                                SingleValueABCD = ToSingle(byteDataItems.Select(b => b.Value.Value).ToArray());
                                SingleValueDCBA = ToSingle(byteDataItems.Reverse().Select(b => b.Value.Value).ToArray());

                                if (IsSelectedRegisterUnit)
                                {
                                    HasSingleValueRegisterUnit = true;
                                    HasSingleValueByteUnit = false;
                                    SingleValueBADC = ToSingle(ToBADC(byteDataItems).Select(b => b.Value.Value).ToArray());
                                    SingleValueCDAB = ToSingle(ToCDAB(byteDataItems).Select(b => b.Value.Value).ToArray());
                                }
                                else
                                {
                                    HasSingleValueRegisterUnit = false;
                                    HasSingleValueByteUnit = true;
                                    SingleValueBADC = null;
                                    SingleValueCDAB = null;
                                }

                                HasDoubleValueRegisterUnit = false;
                                HasDoubleValueByteUnit = false;
                                DoubleValueABCD = null;
                                DoubleValueBADC = null;
                                DoubleValueCDAB = null;
                                DoubleValueDCBA = null;
                            }
                            else if (byteDataItems.Length == 8)
                            {
                                DoubleValueABCD = ToDouble(byteDataItems.Select(b => b.Value.Value).ToArray());
                                DoubleValueDCBA = ToDouble(byteDataItems.Reverse().Select(b => b.Value.Value).ToArray());

                                if (IsSelectedRegisterUnit)
                                {
                                    HasDoubleValueRegisterUnit = true;
                                    HasDoubleValueByteUnit = false;
                                    DoubleValueBADC = ToDouble(ToBADC(byteDataItems).Select(b => b.Value.Value).ToArray());
                                    DoubleValueCDAB = ToDouble(ToCDAB(byteDataItems).Select(b => b.Value.Value).ToArray());
                                }
                                else
                                {
                                    HasDoubleValueRegisterUnit = false;
                                    HasDoubleValueByteUnit = true;
                                    DoubleValueBADC = null;
                                    DoubleValueCDAB = null;
                                }

                                HasSingleValueRegisterUnit = false;
                                HasSingleValueByteUnit = false;
                                SingleValueABCD = null;
                                SingleValueBADC = null;
                                SingleValueCDAB = null;
                                SingleValueDCBA = null;
                            }
                            else
                            {
                                HasSingleValueRegisterUnit = false;
                                HasSingleValueByteUnit = false;
                                SingleValueABCD = null;
                                SingleValueBADC = null;
                                SingleValueCDAB = null;
                                SingleValueDCBA = null;
                                HasDoubleValueRegisterUnit = false;
                                HasDoubleValueByteUnit = false;
                                DoubleValueABCD = null;
                                DoubleValueBADC = null;
                                DoubleValueCDAB = null;
                                DoubleValueDCBA = null;
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<ModbusRegisterByte> ToBADC(ModbusRegisterByte[] items)
        {
            for (int i = 0; i < items.Length / 2; i++)
                for (int j = 0; j < 2; j++)
                    yield return items[i * 2 + (1 - j)];
        }
        private static IEnumerable<ModbusRegisterByte> ToCDAB(ModbusRegisterByte[] items)
        {
            for (int i = 0; i < items.Length / 2; i++)
                for (int j = 0; j < 2; j++)
                    yield return items[(items.Length / 2 - 1 - i) * 2 + j];
        }

        private static float ToSingle(byte[] bytes)
            => BitConverter.IsLittleEndian ? BitConverter.ToSingle(bytes.Reverse().ToArray(), 0) : BitConverter.ToSingle(bytes, 0);

        private static double ToDouble(byte[] bytes)
            => BitConverter.IsLittleEndian ? BitConverter.ToDouble(bytes.Reverse().ToArray(), 0) : BitConverter.ToDouble(bytes, 0);

        private static long ToInt64(byte[] bytes)
        {
            byte[] empty = new byte[8 - bytes.Length];
            Array.Fill(empty, bytes[0] >= 128 ? (byte)0xff : (byte)0x00);

            return BitConverter.IsLittleEndian ? BitConverter.ToInt64(empty.Concat(bytes).Reverse().ToArray(), 0) : BitConverter.ToInt64(empty.Concat(bytes).ToArray(), 0);
        }

        private static ulong ToUInt64(byte[] bytes)
            => BitConverter.IsLittleEndian ? BitConverter.ToUInt64(new byte[8 - bytes.Length].Concat(bytes).Reverse().ToArray(), 0) : BitConverter.ToUInt64(new byte[8 - bytes.Length].Concat(bytes).ToArray(), 0);

        private static string ToByteAddress(ModbusRegisterByte byteDataItem)
            => $"{byteDataItem.Address}.{(byteDataItem.Register.FirstByte == byteDataItem ? 'A' : 'B')}";

        public bool HasValues { get => Get(false); private set => Set(value); }
        public bool IsSelectedRegisterUnit { get => Get(false); private set => Set(value); }

        public string StartAddressText { get => Get<string>(); private set => Set(value); }
        public string EndAddressText { get => Get<string>(); private set => Set(value); }

        public bool HasIntegerValueRegisterUnit { get => Get(false); private set => Set(value); }
        public bool HasIntegerValueByteUnit { get => Get(false); private set => Set(value); }

        public long? SignedIntegerValueABCD { get => Get<long?>(); private set => Set(value); }
        public long? SignedIntegerValueBADC { get => Get<long?>(); private set => Set(value); }
        public long? SignedIntegerValueCDAB { get => Get<long?>(); private set => Set(value); }
        public long? SignedIntegerValueDCBA { get => Get<long?>(); private set => Set(value); }

        public ulong? UnsignedIntegerValueABCD { get => Get<ulong?>(); private set => Set(value); }
        public ulong? UnsignedIntegerValueBADC { get => Get<ulong?>(); private set => Set(value); }
        public ulong? UnsignedIntegerValueCDAB { get => Get<ulong?>(); private set => Set(value); }
        public ulong? UnsignedIntegerValueDCBA { get => Get<ulong?>(); private set => Set(value); }

        public bool HasSingleValueRegisterUnit { get => Get(false); private set => Set(value); }
        public bool HasSingleValueByteUnit { get => Get(false); private set => Set(value); }
        public float? SingleValueABCD { get => Get<float?>(); private set => Set(value); }
        public float? SingleValueBADC { get => Get<float?>(); private set => Set(value); }
        public float? SingleValueCDAB { get => Get<float?>(); private set => Set(value); }
        public float? SingleValueDCBA { get => Get<float?>(); private set => Set(value); }

        public bool HasDoubleValueRegisterUnit { get => Get(false); private set => Set(value); }
        public bool HasDoubleValueByteUnit { get => Get(false); private set => Set(value); }
        public double? DoubleValueABCD { get => Get<double?>(); private set => Set(value); }
        public double? DoubleValueBADC { get => Get<double?>(); private set => Set(value); }
        public double? DoubleValueCDAB { get => Get<double?>(); private set => Set(value); }
        public double? DoubleValueDCBA { get => Get<double?>(); private set => Set(value); }

    }
}
