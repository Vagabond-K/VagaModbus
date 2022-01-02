using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Controls
{
    public class TextBoxExtensions
    {
        public static bool GetSelectAllGotFocus(TextBox obj)
        {
            return (bool)obj.GetValue(SelectAllGotFocusProperty);
        }

        public static void SetSelectAllGotFocus(TextBox obj, bool value)
        {
            obj.SetValue(SelectAllGotFocusProperty, value);
        }

        public static readonly DependencyProperty SelectAllGotFocusProperty =
            DependencyProperty.RegisterAttached("SelectAllGotFocus", typeof(bool), typeof(TextBoxExtensions), new PropertyMetadata(false, OnSelectAllGotFocusChanged));

        private static void OnSelectAllGotFocusChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is TextBox textBox && e.NewValue is bool selectAllGotFocus)
            {
                if (selectAllGotFocus)
                    textBox.GotFocus += OnTextBoxGotFocus;
                else
                    textBox.GotFocus -= OnTextBoxGotFocus;
            }
        }

        private static void OnTextBoxGotFocus(object sender, RoutedEventArgs e) => (sender as TextBox)?.SelectAll();
    }
}
