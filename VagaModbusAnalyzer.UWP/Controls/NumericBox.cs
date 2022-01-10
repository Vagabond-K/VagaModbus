using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace VagaModbusAnalyzer.Controls
{
    public sealed class NumericBox : TextBox
    {
        public NumericBox()
        {
            RegisterPropertyChangedCallback(TextProperty, OnTextPropertyChangedCallback);
            BeforeTextChanging += OnBeforeTextChanging;
            TextChanging += OnTextChanging;

            InputScope scope = new InputScope();
            InputScopeName scopeName = new InputScopeName();
            scopeName.NameValue = InputScopeNameValue.Number;
            scope.Names.Add(scopeName);
            InputScope = scope;
        }

        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(NumericBox), new PropertyMetadata(double.NaN));

        public double MinValue
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(NumericBox), new PropertyMetadata(double.NaN));


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
            DependencyProperty.Register("Value", typeof(object), typeof(NumericBox), new PropertyMetadata(null, OnValueChanged));


        public TypeCode ValueType
        {
            get { return (TypeCode)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        public static readonly DependencyProperty ValueTypeProperty =
            DependencyProperty.Register("ValueType", typeof(TypeCode), typeof(NumericBox), new PropertyMetadata(TypeCode.Decimal));




        public TypeCode? EditType
        {
            get { return (TypeCode?)GetValue(EditTypeProperty); }
            set { SetValue(EditTypeProperty, value); }
        }

        public static readonly DependencyProperty EditTypeProperty =
            DependencyProperty.Register("EditType", typeof(TypeCode?), typeof(NumericBox), new PropertyMetadata(null));

        private TypeCode ActualEditType => EditType ?? ValueType;

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is NumericBox numericBox
                && FocusManager.GetFocusedElement() != numericBox)
            {
                numericBox.SetValueToText();
            }
        }

        private void OnTextPropertyChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            oldText = Text;

            try
            {
                var text = Text;

                while (text.Length > 1 && text[0] == '0' && text[1] != '.')
                    text = text.Remove(0, 1);
                if (text.Contains('.'))
                    while (text.Length > 0 && text[text.Length - 1] == '0')
                        text = text.Remove(text.Length - 1);
                if (text.Length > 1 && text[text.Length - 1] == '.')
                    text = text.Remove(text.Length - 1);

                if (ActualEditType == TypeCode.Double && double.TryParse(text, out var doubleNewValue))
                {
                    var newValue = doubleNewValue;

                    double maxValue = double.MaxValue;
                    double minValue = double.MinValue;

                    if (!double.IsNaN(MaxValue)) maxValue = Math.Min(maxValue, MaxValue);
                    if (!double.IsNaN(MinValue)) minValue = Math.Max(minValue, MinValue);

                    if (newValue > maxValue) newValue = maxValue;
                    if (newValue < minValue) newValue = minValue;

                    var typedNewValue = ChangeType(newValue, ValueType);

                    if (!Equals(Value, typedNewValue))
                        Value = typedNewValue;
                }
                else if (ActualEditType == TypeCode.Single && float.TryParse(text, out var floatNewValue))
                {
                    var newValue = floatNewValue;

                    double maxValue = float.MaxValue;
                    double minValue = float.MinValue;

                    if (!double.IsNaN(MaxValue)) maxValue = Math.Min(maxValue, MaxValue);
                    if (!double.IsNaN(MinValue)) minValue = Math.Max(minValue, MinValue);

                    if (newValue > maxValue) newValue = (float)maxValue;
                    if (newValue < minValue) newValue = (float)minValue;

                    var typedNewValue = ChangeType(newValue, ValueType);

                    if (!Equals(Value, typedNewValue))
                        Value = typedNewValue;
                }
                else if (decimal.TryParse(text, out var newDecimalValue))
                {
                    var newValue = newDecimalValue;

                    var maxValue = GetMaxValue(ActualEditType);
                    var minValue = GetMinValue(ActualEditType);

                    if (!double.IsNaN(MaxValue)) maxValue = Math.Min(maxValue, DoubleToDecimal(MaxValue));
                    if (!double.IsNaN(MinValue)) minValue = Math.Max(minValue, DoubleToDecimal(MinValue));

                    if (newValue > maxValue) newValue = maxValue;
                    if (newValue < minValue) newValue = minValue;

                    var typedNewValue = ActualEditType == ValueType
                        ? ChangeType(newValue, ValueType)
                        : ChangeType(ChangeType(newValue, ActualEditType), ValueType);

                    if (!Equals(Value, typedNewValue))
                        Value = typedNewValue;
                }
                else
                {
                    if (AllowNullInput)
                        Value = null;
                    else if (double.IsNaN(MinValue) || MinValue <= 0)
                        Value = ChangeType(0, ValueType);
                    else
                        Value = ChangeType(MinValue, ValueType);
                }
            }
            catch
            {
            }
        }

        private string oldText = null;

        private bool IsValidText(string text)
            => (ActualEditType == TypeCode.Double && double.TryParse(text, out _)
                || ActualEditType == TypeCode.Single && float.TryParse(text, out _)
                || decimal.TryParse(text, out _)
                || text == "-."
                || text == "-"
                || text == ".")
                && !text.Contains(',')
                && (text.Length == 1 || text.Length > 1 && text[text.Length - 1] != '-' && text[text.Length - 1] != '+');

        private void OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs e)
        {
            var text = Text;

            if (string.IsNullOrEmpty(text)) return;

            if (!IsValidText(text))
            {
                if (string.IsNullOrEmpty(oldText))
                {
                    SelectionStart = 0;
                    SelectionLength = 0;
                }
                var oldSelectionStart = SelectionStart;
                var oldSelectionLength = SelectionLength;
                Text = oldText;
                SelectionStart = oldSelectionStart;
                SelectionLength = oldSelectionLength;
                return;
            }

            if (string.IsNullOrEmpty(oldText) && text.Length == 1)
            {
                SelectionStart = 1;
                SelectionLength = 0;
            }
        }

        private void OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs e)
        {
            var text = e.NewText;

            if (string.IsNullOrEmpty(text)) return;

            if (!IsValidText(text))
            {
                if (string.IsNullOrEmpty(oldText))
                {
                    SelectionStart = 0;
                    SelectionLength = 0;
                }
                e.Cancel = true;
                return;
            }

            if (string.IsNullOrEmpty(oldText) && text.Length == 1)
            {
                SelectionStart = 1;
                SelectionLength = 0;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            SetValueToText();

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            SetValueToText();

            base.OnLostFocus(e);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                GetBindingExpression(ValueProperty)?.UpdateSource();
                SetValueToText();
            }
        }

        private void SetValueToText()
        {
            string text;

            if (Value is float floatValue)
                text = floatValue.ToString("R");
            else if (Value is double doubleValue)
                text = doubleValue.ToString("R");
            else
                text = Value?.ToString() ?? string.Empty;

            while (text.Length > 1 && text[0] == '0' && text[1] != '.')
                text = text.Remove(0, 1);
            if (text.Contains('.'))
                while (text.Length > 0 && text[text.Length - 1] == '0')
                    text = text.Remove(text.Length - 1);
            if (text.Length > 1 && text[text.Length - 1] == '.')
                text = text.Remove(text.Length - 1);

            var oldSelectionStart = SelectionStart;
            var oldSelectionLength = SelectionLength;
            Text = text;
            SelectionStart = oldSelectionStart;
            SelectionLength = oldSelectionLength;

            try
            {
                GetBindingExpression(ValueProperty)?.UpdateSource();
            }
            catch { }
        }

        static decimal DoubleToDecimal(double doubleValue) => doubleValue > (double)decimal.MaxValue ? decimal.MaxValue : (decimal)doubleValue;

        static object ChangeType(object value, TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Decimal:
                    if (value is float floatValue)
                        return floatValue > (double)decimal.MaxValue ? decimal.MaxValue : (decimal)floatValue;
                    else if (value is double doubleValue)
                        return DoubleToDecimal(doubleValue);
                    break;
            }
            return Convert.ChangeType(value, code);
        }

        static decimal GetMaxValue(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Byte:
                    return byte.MaxValue;
                case TypeCode.Char:
                    return char.MaxValue;
                case TypeCode.Double:
                    return decimal.MaxValue;
                case TypeCode.Int16:
                    return short.MaxValue;
                case TypeCode.Int32:
                    return int.MaxValue;
                case TypeCode.Int64:
                    return long.MaxValue;
                case TypeCode.SByte:
                    return sbyte.MaxValue;
                case TypeCode.Single:
                    return decimal.MaxValue;
                case TypeCode.UInt16:
                    return ushort.MaxValue;
                case TypeCode.UInt32:
                    return uint.MaxValue;
                case TypeCode.UInt64:
                    return ulong.MaxValue;
            }
            return ulong.MaxValue;
        }

        static decimal GetMinValue(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Byte:
                    return byte.MinValue;
                case TypeCode.Char:
                    return char.MinValue;
                case TypeCode.Double:
                    return decimal.MinValue;
                case TypeCode.Int16:
                    return short.MinValue;
                case TypeCode.Int32:
                    return int.MinValue;
                case TypeCode.Int64:
                    return long.MinValue;
                case TypeCode.SByte:
                    return sbyte.MinValue;
                case TypeCode.Single:
                    return decimal.MinValue;
                case TypeCode.UInt16:
                    return ushort.MinValue;
                case TypeCode.UInt32:
                    return uint.MinValue;
                case TypeCode.UInt64:
                    return ulong.MinValue;
            }
            return long.MinValue;
        }

    }
}
