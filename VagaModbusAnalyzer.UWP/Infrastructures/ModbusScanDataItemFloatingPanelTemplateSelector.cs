using VagaModbusAnalyzer.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Infrastructures
{
    public class ModbusScanDataItemFloatingPanelTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ByteDataSummaryTemplate { get; set; }
        public DataTemplate BitDataSummaryTemplate { get; set; }
        public DataTemplate ByteDataTemplate { get; set; }
        public DataTemplate BitDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ModbusScanRegisterDataItemView registerDataItemView)
            {
                return ByteDataTemplate;
            }
            else if (item is ModbusScanDataItemView itemView)
            {
                switch (item?.GetType()?.Name)
                {
                    case nameof(ModbusScanByteDataItemView):
                        return itemView.IsSelected ? ByteDataSummaryTemplate : ByteDataTemplate;
                    case nameof(ModbusScanBitDataItemView):
                        return itemView.IsSelected ? BitDataSummaryTemplate : BitDataTemplate;
                }
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}
