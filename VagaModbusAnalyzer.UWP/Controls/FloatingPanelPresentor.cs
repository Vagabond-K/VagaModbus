using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace VagaModbusAnalyzer.Controls
{
    [ContentProperty(Name = "Child")]
    public sealed class FloatingPanelPresentor : Control
    {
        public FloatingPanelPresentor()
        {
            this.DefaultStyleKey = typeof(FloatingPanelPresentor);
        }

        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register("Child", typeof(UIElement), typeof(FloatingPanelPresentor), new PropertyMetadata(null));



        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_FloatingPanel") is Border border)
                _FloatingPanel = border;
        }

        private Border _FloatingPanel = null;

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            Show(e);
        }
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            Show(e);
        }
        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);
            Show(e);
        }
        protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            Show(e);
        }
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            Show(e);
        }

        private void Show(PointerRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                bool show = false;
                var point = e.GetCurrentPoint(null).Position;
                foreach (var element in VisualTreeHelper.FindElementsInHostCoordinates(new Point(Math.Round(point.X), Math.Round(point.Y)), this))
                {
                    if (element is FrameworkElement frameworkElement)
                    {
                        if (Show(frameworkElement, e))
                        {
                            show = true;
                            break;
                        }
                    }
                }
                if (!show)
                {
                    Hide();
                }
            }
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            if (!e.Handled)
                Hide();
        }

        private UIElement _CurrentView = null;
        private DataTemplate _CurrentDataTemplate = null;

        public bool Show(FrameworkElement target, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            if (_FloatingPanel != null)
            {
                var dataContext = target?.GetDataContext();

                if (target?.GetView() is UIElement view)
                {
                    _FloatingPanel.DataContext = dataContext ?? target.DataContext;
                    if (_CurrentView != view)
                        _FloatingPanel.Child = view;
                    _CurrentView = view;
                }
                else if (target?.GetViewTemplate() is DataTemplate dataTemplate)
                {
                    _FloatingPanel.DataContext = dataContext ?? target.DataContext;
                    if (_CurrentDataTemplate != dataTemplate)
                        _FloatingPanel.Child = dataTemplate.LoadContent() as UIElement;
                    _CurrentDataTemplate = dataTemplate;
                }
                else if (target?.GetViewTemplateSelector() is DataTemplateSelector dataTemplateSelector)
                {
                    _FloatingPanel.DataContext = dataContext ?? target.DataContext;
                    dataTemplate = dataTemplateSelector.SelectTemplate(_FloatingPanel.DataContext, _FloatingPanel);
                    if (dataTemplate == null) return false;
                    if (_CurrentDataTemplate != dataTemplate)
                        _FloatingPanel.Child = dataTemplate?.LoadContent() as UIElement;
                    _CurrentDataTemplate = dataTemplate;
                }
                else
                {
                    return false;
                }

                if (_FloatingPanel.Child?.Visibility == Visibility.Collapsed)
                    return false;

                var screenPoint = pointerRoutedEventArgs.GetCurrentPoint(this);
                var elementPoint = pointerRoutedEventArgs.GetCurrentPoint(target);

                Canvas.SetLeft(_FloatingPanel, ActualWidth < screenPoint.Position.X + _FloatingPanel.ActualWidth ? ActualWidth - _FloatingPanel.ActualWidth : screenPoint.Position.X);
                Canvas.SetTop(_FloatingPanel, ActualHeight < screenPoint.Position.Y + _FloatingPanel.ActualHeight ? ActualHeight - _FloatingPanel.ActualHeight : screenPoint.Position.Y);

                _FloatingPanel.IsHitTestVisible = false;
                _FloatingPanel.Visibility = Visibility.Visible;
                return true;
            }

            return false;
        }

        public void Hide()
        {
            if (_FloatingPanel != null && _FloatingPanel.Visibility == Visibility.Visible)
            {
                _FloatingPanel.Visibility = Visibility.Collapsed;
                _FloatingPanel.DataContext = null;

                _FloatingPanel.Child = null;
                _CurrentView = null;
                _CurrentDataTemplate = null;
            }
        }
    }

    public static class FloatingPanel
    {
        public static object GetDataContext(this FrameworkElement obj)
        {
            return (object)obj.GetValue(DataContextProperty);
        }

        public static void SetDataContext(this FrameworkElement obj, object value)
        {
            obj.SetValue(DataContextProperty, value);
        }

        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.RegisterAttached("DataContext", typeof(object), typeof(FloatingPanel), new PropertyMetadata(null));


        public static UIElement GetView(this FrameworkElement obj)
        {
            return (UIElement)obj.GetValue(ViewProperty);
        }

        public static void SetView(this FrameworkElement obj, UIElement value)
        {
            obj.SetValue(ViewProperty, value);
        }

        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.RegisterAttached("View", typeof(UIElement), typeof(FloatingPanel), new PropertyMetadata(null));



        public static DataTemplate GetViewTemplate(this FrameworkElement obj)
        {
            return (DataTemplate)obj.GetValue(ViewTemplateProperty);
        }

        public static void SetViewTemplate(this FrameworkElement obj, DataTemplate value)
        {
            obj.SetValue(ViewTemplateProperty, value);
        }

        public static readonly DependencyProperty ViewTemplateProperty =
            DependencyProperty.RegisterAttached("ViewTemplate", typeof(DataTemplate), typeof(FloatingPanel), new PropertyMetadata(null));




        public static DataTemplateSelector GetViewTemplateSelector(this FrameworkElement obj)
        {
            return (DataTemplateSelector)obj.GetValue(ViewTemplateSelectorProperty);
        }

        public static void SetViewTemplateSelector(this FrameworkElement obj, DataTemplateSelector value)
        {
            obj.SetValue(ViewTemplateSelectorProperty, value);
        }

        public static readonly DependencyProperty ViewTemplateSelectorProperty =
            DependencyProperty.RegisterAttached("ViewTemplateSelector", typeof(DataTemplateSelector), typeof(FloatingPanel), new PropertyMetadata(null));


    }

}
