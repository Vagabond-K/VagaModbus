using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace VagaModbusAnalyzer.Controls
{
    public sealed class NumericBox : TextBox
    {
        public NumericBox()
        {
            RegisterPropertyChangedCallback(TextProperty, TextPropertyChangedCallback);
            TextChanging += NumericBox_TextChanging;
            SelectionChanged += NumericBox_SelectionChanged;

            SetBinding(TextProperty, new Binding { Path = new PropertyPath(nameof(Value)), Source = this, Mode = BindingMode.OneWay });
        }

        public double? MaxValue
        {
            get { return (double?)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double?), typeof(NumericBox), new PropertyMetadata(null));

        public double? MinValue
        {
            get { return (double?)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double?), typeof(NumericBox), new PropertyMetadata(null));



        public bool AllowNullInput
        {
            get { return (bool)GetValue(AllowNullInputProperty); }
            set { SetValue(AllowNullInputProperty, value); }
        }

        public static readonly DependencyProperty AllowNullInputProperty =
            DependencyProperty.Register("AllowNullInput", typeof(bool), typeof(NumericBox), new PropertyMetadata(false));


        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(NumericBox), new PropertyMetadata(null));


        public TypeCode ValueTypeCode
        {
            get { return (TypeCode)GetValue(ValueTypeCodeProperty); }
            set { SetValue(ValueTypeCodeProperty, value); }
        }

        public static readonly DependencyProperty ValueTypeCodeProperty =
            DependencyProperty.Register("ValueTypeCode", typeof(TypeCode), typeof(NumericBox), new PropertyMetadata(TypeCode.Double));


        private string oldText = "0";
        private int oldSelectionStart = 0;

        private void TextPropertyChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            oldText = Text ?? (AllowNullInput ? "" : "0");
        }

        private void NumericBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            oldSelectionStart = SelectionStart;
        }

        private void NumericBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            //if (string.IsNullOrWhiteSpace(Text))
            //{
            //    Text = AllowNullInput ? "" : "0";
            //    SelectionStart = 1;
            //    if (!AllowNullInput) return;
            //}
            //else if (Text.Last() == '.')
            //{

            //}

            string newText = Text ?? (AllowNullInput ? "" : "0");

            if (!string.Equals(oldText, newText))
            {
                if (double.TryParse(newText, out var numeric))
                {
                    var limitedNumeric = numeric;
                    var maxValue = MaxValue ?? GetMaxValue(ValueTypeCode);
                    var minValue = MinValue ?? GetMinValue(ValueTypeCode);

                    if (limitedNumeric > maxValue) limitedNumeric = maxValue;
                    if (limitedNumeric < minValue) limitedNumeric = minValue;

                    Value = Convert.ChangeType(limitedNumeric, ValueTypeCode);

                    if (limitedNumeric != numeric)
                        Text = Value.ToString();
                }
                else if (string.IsNullOrEmpty(newText))
                {
                    Value = AllowNullInput ? null : (MinValue == null || MinValue.Value <= 0 ? 0d : MinValue);
                }
                else
                {
                    Text = oldText;
                    SelectionStart = oldSelectionStart;
                }

                var text = Text;
                while (text.Length > 1 && text[0] == '0' && text[1] != '.')
                    text = text.Remove(0, 1);
                if (Text != text)
                {
                    Text = text;
                    SelectionStart = oldSelectionStart;
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (Value == null)
                Value = AllowNullInput ? null : (MinValue == null || MinValue.Value <= 0 ? 0d : MinValue);

            Text = Value?.ToString() ?? string.Empty;
        }

        static Type ToType(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Boolean:
                    return typeof(bool);
                case TypeCode.Byte:
                    return typeof(byte);
                case TypeCode.Char:
                    return typeof(char);
                case TypeCode.DateTime:
                    return typeof(DateTime);
                case TypeCode.DBNull:
                    return typeof(DBNull);
                case TypeCode.Decimal:
                    return typeof(decimal);
                case TypeCode.Double:
                    return typeof(double);
                case TypeCode.Int16:
                    return typeof(short);
                case TypeCode.Int32:
                    return typeof(int);
                case TypeCode.Int64:
                    return typeof(long);
                case TypeCode.Object:
                    return typeof(object);
                case TypeCode.SByte:
                    return typeof(sbyte);
                case TypeCode.Single:
                    return typeof(float);
                case TypeCode.String:
                    return typeof(string);
                case TypeCode.UInt16:
                    return typeof(ushort);
                case TypeCode.UInt32:
                    return typeof(uint);
                case TypeCode.UInt64:
                    return typeof(ulong);
            }
            return null;
        }

        static double GetMaxValue(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Byte:
                    return byte.MaxValue;
                case TypeCode.Char:
                    return char.MaxValue;
                case TypeCode.Double:
                    return double.MaxValue;
                case TypeCode.Int16:
                    return short.MaxValue;
                case TypeCode.Int32:
                    return int.MaxValue;
                case TypeCode.Int64:
                    return long.MaxValue;
                case TypeCode.SByte:
                    return sbyte.MaxValue;
                case TypeCode.Single:
                    return float.MaxValue;
                case TypeCode.UInt16:
                    return ushort.MaxValue;
                case TypeCode.UInt32:
                    return uint.MaxValue;
                case TypeCode.UInt64:
                    return ulong.MaxValue;
            }
            return double.MaxValue;
        }

        static double GetMinValue(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Byte:
                    return byte.MinValue;
                case TypeCode.Char:
                    return char.MinValue;
                case TypeCode.Double:
                    return double.MinValue;
                case TypeCode.Int16:
                    return short.MinValue;
                case TypeCode.Int32:
                    return int.MinValue;
                case TypeCode.Int64:
                    return long.MinValue;
                case TypeCode.SByte:
                    return sbyte.MinValue;
                case TypeCode.Single:
                    return float.MinValue;
                case TypeCode.UInt16:
                    return ushort.MinValue;
                case TypeCode.UInt32:
                    return uint.MinValue;
                case TypeCode.UInt64:
                    return ulong.MinValue;
            }
            return double.MinValue;
        }

    }
}
