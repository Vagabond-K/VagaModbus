using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Converters
{
    public class HexConverter : IValueConverter
    {
        public int Length { get; set; } = 1;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;

            if (value is IEnumerable<byte> bytes)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var byteValue in bytes)
                {
                    if (stringBuilder.Length > 0)
                        stringBuilder.Append(' ');
                    stringBuilder.Append(byteValue.ToString("X2"));
                }
                return stringBuilder.ToString();
            }

            switch (value.GetType().Name)
            {
                case nameof(Byte):
                    return value.To<byte>().ToString("x").ToUpper().PadLeft(Length, '0');
                case nameof(UInt16):
                    return value.To<ushort>().ToString("x").ToUpper().PadLeft(Length, '0');
                case nameof(UInt32):
                    return value.To<uint>().ToString("x").ToUpper().PadLeft(Length, '0');
                case nameof(UInt64):
                    return value.To<ulong>().ToString("x").ToUpper().PadLeft(Length, '0');
                case nameof(SByte):
                    return value.To<sbyte>().ToString("x").ToUpper().PadLeft(Length, '0');
                case nameof(Int16):
                    return value.To<short>().ToString("x").ToUpper().PadLeft(Length, '0');
                case nameof(Int32):
                    return value.To<int>().ToString("x").ToUpper().PadLeft(Length, '0');
                case nameof(Int64):
                    return value.To<long>().ToString("x").ToUpper().PadLeft(Length, '0');
                case nameof(Single):
                    return value.To<float>().ToString("x").ToUpper().PadLeft(Length, '0');
                case nameof(Double):
                    return value.To<double>().ToString("x").ToUpper().PadLeft(Length, '0');
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class HexConverterExtension : MarkupExtension
    {
        public int Length { get; set; } = 1;

        protected override object ProvideValue() => new HexConverter { Length = Length };
    }
}
