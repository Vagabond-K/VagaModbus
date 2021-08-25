using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Infrastructures
{
    public class ToBooleanExtension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<bool>();
        }
    }
    public class ToDoubleExtension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<double>();
        }
    }

    public class ToSingleExtension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<float>();
        }
    }

    public class ToSByteExtension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<sbyte>();
        }
    }

    public class ToByteExtension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<byte>();
        }
    }

    public class ToInt16Extension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<short>();
        }
    }


    public class ToUInt16Extension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<ushort>();
        }
    }


    public class ToInt32Extension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<int>();
        }
    }


    [MarkupExtensionReturnType(ReturnType = typeof(int))]
    public class ToUInt32Extension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<uint>();
        }
    }

    public class ToInt64Extension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<long>();
        }
    }


    public class ToUInt64Extension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<ulong>();
        }
    }

    public class ToDecimalExtension : MarkupExtension
    {
        public object Value { get; set; }

        protected override object ProvideValue()
        {
            return Value.To<decimal>();
        }
    }
}
