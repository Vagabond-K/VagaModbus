using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace VagaModbusAnalyzer.Controls
{
    public class ModbusScanView : Windows.UI.Xaml.Controls.Primitives.SelectorItem
    {
        public ModbusScanView()
        {
            DefaultStyleKey = typeof(ModbusScanView);
            RegisterPropertyChangedCallback(IsSelectedProperty, new DependencyPropertyChangedCallback(SelectedChanged));
        }

        private static void SelectedChanged(DependencyObject sender, DependencyProperty e)
        {
            if (sender is ModbusScanView view)
            {
                if (view.IsSelected)
                {
                    VisualStateManager.GoToState(view, "Selected", true);

                    DependencyObject parent = view;
                    while (parent != null && !(parent is ModbusChannelScanPivot))
                    {
                        parent = VisualTreeHelper.GetParent(parent);

                        if (parent is ModbusChannelScanPivot modbusChannelScanPivot)
                        {
                            modbusChannelScanPivot.SelectedScanView = view;
                            break;
                        }
                    }
                }
                else VisualStateManager.GoToState(view, "Unselected", true);
            }
        }


        public ICommand EditCommand
        {
            get { return (ICommand)GetValue(EditCommandProperty); }
            set { SetValue(EditCommandProperty, value); }
        }

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register("EditCommand", typeof(ICommand), typeof(ModbusScanView), new PropertyMetadata(null));


        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(ModbusScanView), new PropertyMetadata(null));



    }
}
