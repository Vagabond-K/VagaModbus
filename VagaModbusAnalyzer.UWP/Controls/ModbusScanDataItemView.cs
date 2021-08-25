using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Controls
{
    public abstract class ModbusScanDataItemView : ContentControl
    {
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(ModbusScanDataItemView), new PropertyMetadata(false, SelectedChanged));



        private static void SelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ModbusScanDataItemView view
                && e.NewValue is bool isSelected)
            {
                if (isSelected) VisualStateManager.GoToState(view, "Selected", true);
                else VisualStateManager.GoToState(view, "Unselected", true);
            }
        }
    }
}
