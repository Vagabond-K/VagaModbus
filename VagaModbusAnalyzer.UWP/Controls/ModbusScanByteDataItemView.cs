using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace VagaModbusAnalyzer.Controls
{
    public class ModbusScanByteDataItemView : ModbusScanDataItemView
    {
        public ModbusScanByteDataItemView()
        {
            DefaultStyleKey = typeof(ModbusScanByteDataItemView);
        }




        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(ModbusScanByteDataItemView), new PropertyMetadata(null));




        public CornerRadius Corner
        {
            get { return (CornerRadius)GetValue(CornerProperty); }
            set { SetValue(CornerProperty, value); }
        }

        public static readonly DependencyProperty CornerProperty =
            DependencyProperty.Register("Corner", typeof(CornerRadius), typeof(ModbusScanByteDataItemView), new PropertyMetadata(new CornerRadius()));




        public int IndexInRegister
        {
            get { return (int)GetValue(IndexInRegisterProperty); }
            set { SetValue(IndexInRegisterProperty, value); }
        }

        public static readonly DependencyProperty IndexInRegisterProperty =
            DependencyProperty.Register("IndexInRegister", typeof(int), typeof(ModbusScanByteDataItemView), new PropertyMetadata(0));



    }
}
