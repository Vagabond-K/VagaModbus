using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace VagaModbusAnalyzer.Controls
{
    public class SelectorItemButton : Button
    {
        public SelectorItemButton()
        {
            DefaultStyleKey = typeof(SelectorItemButton);
            Click += SelectorItemButton_Click;
        }

        private void SelectorItemButton_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = this;

            do
            {
                parent = VisualTreeHelper.GetParent(parent);

                if (parent is Windows.UI.Xaml.Controls.Primitives.SelectorItem selectorItem)
                {
                    selectorItem.IsSelected = true;
                }
            } while (parent != null);
        }
    }

    public static class SelectorItem
    {
        public static bool GetIsAutoSelectOnClick(FrameworkElement obj)
        {
            return (bool)obj.GetValue(IsAutoSelectOnClickProperty);
        }

        public static void SetIsAutoSelectOnClick(FrameworkElement obj, bool value)
        {
            obj.SetValue(IsAutoSelectOnClickProperty, value);
        }

        public static readonly DependencyProperty IsAutoSelectOnClickProperty =
            DependencyProperty.RegisterAttached("IsAutoSelectOnClick", typeof(bool), typeof(SelectorItem), new PropertyMetadata(false, (d, e) =>
            {
                if (e.NewValue is bool isAutoSelectOnClick)
                {
                    if (d is FrameworkElement frameworkElement)
                    {
                        if (isAutoSelectOnClick)
                            frameworkElement.AddHandler(UIElement.PointerPressedEvent, _SelectParentSelectorItem, true);
                        else
                            frameworkElement.RemoveHandler(UIElement.PointerPressedEvent, _SelectParentSelectorItem);
                    }
                }
            }));

        private static readonly PointerEventHandler _SelectParentSelectorItem = new PointerEventHandler(FrameworkElement_PointerPressed);

        private static void FrameworkElement_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SelectParentSelectorItem(sender);
        }

        private static void SelectParentSelectorItem(object element)
        {
            DependencyObject parent = element as DependencyObject;

            do
            {
                parent = VisualTreeHelper.GetParent(parent);

                if (parent is Windows.UI.Xaml.Controls.Primitives.SelectorItem selectorItem)
                {
                    selectorItem.IsSelected = true;
                }
            } while (parent != null);
        }
    }
}
