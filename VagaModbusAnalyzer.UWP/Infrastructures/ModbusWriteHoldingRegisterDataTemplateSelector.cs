using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Infrastructures
{
    public class ModbusWriteHoldingRegisterDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NumericTemplate { get; set; }
        public DataTemplate BitFlagsTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ModbusWriteValue writeValue)
            {
                switch (writeValue.Type)
                {
                    case TypeCode.Boolean:
                        return BitFlagsTemplate;
                    default:
                        return NumericTemplate;
                }
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
