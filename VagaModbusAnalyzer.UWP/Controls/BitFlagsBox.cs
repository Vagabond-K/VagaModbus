using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Controls
{
    public class BitFlagsBox : Control
    {
        public BitFlagsBox()
        {
            DefaultStyleKey = typeof(BitFlagsBox);
        }




        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(BitFlagsBox), new PropertyMetadata((byte)0, OnValueChanged));


        private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is BitFlagsBox bitFlagsBox && e.NewValue is byte newValue)
            {
                bitFlagsBox.Bit0 = ((newValue >> 0) & 1) == 1;
                bitFlagsBox.Bit1 = ((newValue >> 1) & 1) == 1;
                bitFlagsBox.Bit2 = ((newValue >> 2) & 1) == 1;
                bitFlagsBox.Bit3 = ((newValue >> 3) & 1) == 1;
                bitFlagsBox.Bit4 = ((newValue >> 4) & 1) == 1;
                bitFlagsBox.Bit5 = ((newValue >> 5) & 1) == 1;
                bitFlagsBox.Bit6 = ((newValue >> 6) & 1) == 1;
                bitFlagsBox.Bit7 = ((newValue >> 7) & 1) == 1;
            }
        }



        public bool Bit0 { get { return (bool)GetValue(Bit0Property); } set { SetValue(Bit0Property, value); } }
        public static readonly DependencyProperty Bit0Property =
            DependencyProperty.Register("Bit0", typeof(bool), typeof(BitFlagsBox), new PropertyMetadata(false, (d, e) => UpdateValue(d, e, 0)));

        public bool Bit1 { get { return (bool)GetValue(Bit1Property); } set { SetValue(Bit1Property, value); } }
        public static readonly DependencyProperty Bit1Property =
            DependencyProperty.Register("Bit1", typeof(bool), typeof(BitFlagsBox), new PropertyMetadata(false, (d, e) => UpdateValue(d, e, 1)));

        public bool Bit2 { get { return (bool)GetValue(Bit2Property); } set { SetValue(Bit2Property, value); } }
        public static readonly DependencyProperty Bit2Property =
            DependencyProperty.Register("Bit2", typeof(bool), typeof(BitFlagsBox), new PropertyMetadata(false, (d, e) => UpdateValue(d, e, 2)));

        public bool Bit3 { get { return (bool)GetValue(Bit3Property); } set { SetValue(Bit3Property, value); } }
        public static readonly DependencyProperty Bit3Property =
            DependencyProperty.Register("Bit3", typeof(bool), typeof(BitFlagsBox), new PropertyMetadata(false, (d, e) => UpdateValue(d, e, 3)));

        public bool Bit4 { get { return (bool)GetValue(Bit4Property); } set { SetValue(Bit4Property, value); } }
        public static readonly DependencyProperty Bit4Property =
            DependencyProperty.Register("Bit4", typeof(bool), typeof(BitFlagsBox), new PropertyMetadata(false, (d, e) => UpdateValue(d, e, 4)));

        public bool Bit5 { get { return (bool)GetValue(Bit5Property); } set { SetValue(Bit5Property, value); } }
        public static readonly DependencyProperty Bit5Property =
            DependencyProperty.Register("Bit5", typeof(bool), typeof(BitFlagsBox), new PropertyMetadata(false, (d, e) => UpdateValue(d, e, 5)));

        public bool Bit6 { get { return (bool)GetValue(Bit6Property); } set { SetValue(Bit6Property, value); } }
        public static readonly DependencyProperty Bit6Property =
            DependencyProperty.Register("Bit6", typeof(bool), typeof(BitFlagsBox), new PropertyMetadata(false, (d, e) => UpdateValue(d, e, 6)));

        public bool Bit7 { get { return (bool)GetValue(Bit7Property); } set { SetValue(Bit7Property, value); } }
        public static readonly DependencyProperty Bit7Property =
            DependencyProperty.Register("Bit7", typeof(bool), typeof(BitFlagsBox), new PropertyMetadata(false, (d, e) => UpdateValue(d, e, 7)));


        private static void UpdateValue(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e, int index)
        {
            if (dependencyObject is BitFlagsBox bitFlagsBox && e.NewValue is bool newValue && bitFlagsBox.Value is byte oldValue)
            {
                if (newValue)
                    bitFlagsBox.Value = (byte)(oldValue | (1 << index));
                else
                    bitFlagsBox.Value = (byte)(oldValue ^ (1 << index));
            }
        }

        public object Description
        {
            get { return (object)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(object), typeof(BitFlagsBox), new PropertyMetadata(null));


        
    }
}
