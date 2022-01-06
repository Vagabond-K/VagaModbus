using System.Collections.Generic;
using VagaModbusAnalyzer.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Infrastructures
{
    public class ModbusWriterDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HoldingRegisterWriterTemplate { get; set; }
        public DataTemplate CoilWriterTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ModbusWriter modbusWriter)
            {
                switch (modbusWriter.ObjectType)
                {
                    case VagabondK.Protocols.Modbus.ModbusObjectType.HoldingRegister:
                        return HoldingRegisterWriterTemplate;
                    case VagabondK.Protocols.Modbus.ModbusObjectType.Coil:
                        return CoilWriterTemplate;
                }
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
