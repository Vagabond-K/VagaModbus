using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Converters
{
    public class BinaryConverter : IValueConverter
    {
        public int Length { get; set; } = 8;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;

            switch (value.GetType().Name)
            {
                case nameof(Byte):
                    return System.Convert.ToString(value.To<byte>(), 2).PadLeft(Length, '0');
                case nameof(UInt16):
                    return System.Convert.ToString(value.To<ushort>(), 2).PadLeft(Length, '0');
                case nameof(UInt32):
                    return System.Convert.ToString(value.To<uint>(), 2).PadLeft(Length, '0');
                //case nameof(UInt64):
                //    return System.Convert.ToString(value.To<ulong>(), 2).PadLeft(Length, '0');
                case nameof(SByte):
                    return System.Convert.ToString(value.To<sbyte>(), 2).PadLeft(Length, '0');
                case nameof(Int16):
                    return System.Convert.ToString(value.To<short>(), 2).PadLeft(Length, '0');
                case nameof(Int32):
                    return System.Convert.ToString(value.To<int>(), 2).PadLeft(Length, '0');
                case nameof(Int64):
                    return System.Convert.ToString(value.To<long>(), 2).PadLeft(Length, '0');
                    //case nameof(Single):
                    //    return System.Convert.ToString(value.ToSingle(), 2).PadLeft(Length, '0');
                    //case nameof(Double):
                    //    return System.Convert.ToString(value.ToDouble(), 2).PadLeft(Length, '0');
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BinaryConverterExtension : MarkupExtension
    {
        public int Length { get; set; } = 1;

        protected override object ProvideValue() => new BinaryConverter { Length = Length };
    }
}
