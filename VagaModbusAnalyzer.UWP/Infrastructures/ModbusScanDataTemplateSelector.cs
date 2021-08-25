using System.Collections.Generic;
using VagaModbusAnalyzer.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Infrastructures
{
    public class ModbusScanDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RegisterDataTemplate { get; set; }
        public DataTemplate BitDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is IEnumerable<ModbusRegister>)
            {
                return RegisterDataTemplate;
            }
            else if (item is IEnumerable<ModbusBoolean>)
            {
                return BitDataTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
