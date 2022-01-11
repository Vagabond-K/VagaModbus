using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VagaModbusAnalyzer.Infrastructures
{
    public class PropertySetCommand : DependencyObject, System.Windows.Input.ICommand
    {
        public object PropertyBinding
        {
            get { return (object)GetValue(PropertyBindingProperty); }
            set { SetValue(PropertyBindingProperty, value); }
        }

        public static readonly DependencyProperty PropertyBindingProperty =
            DependencyProperty.Register("PropertyBinding", typeof(object), typeof(PropertySetCommand), new PropertyMetadata(null));

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            PropertyBinding = parameter;
        }
    }
}
