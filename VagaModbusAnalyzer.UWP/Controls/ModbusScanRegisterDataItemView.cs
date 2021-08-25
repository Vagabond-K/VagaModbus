using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
namespace VagaModbusAnalyzer.Controls
{
    public class ModbusScanRegisterDataItemView : Control
    {
        public ModbusScanRegisterDataItemView()
        {
            DefaultStyleKey = typeof(ModbusScanRegisterDataItemView);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            FirstByteView = GetTemplateChild("PART_FirstByteView") as ModbusScanByteDataItemView;
            SecondByteView = GetTemplateChild("PART_SecondByteView") as ModbusScanByteDataItemView;
            PART_Address = GetTemplateChild("PART_Address") as Border;
        }

        public ModbusScanByteDataItemView FirstByteView { get; private set; }
        public ModbusScanByteDataItemView SecondByteView { get; private set; }
        private Border PART_Address = null;

        private bool _IsPointerOver = false;
        public bool IsPointerOver
        {
            get => _IsPointerOver;
            private set
            {
                _IsPointerOver = value;
                if (_IsPointerOver) VisualStateManager.GoToState(this, "PointerOver", true);
                else VisualStateManager.GoToState(this, "Normal", true);
            }
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);

            var pointerPoint = e.GetCurrentPoint(null);
            Point point = pointerPoint.Position;
            point = new Point(Math.Round(point.X), Math.Round(point.Y));

            if (!VisualTreeHelper.FindElementsInHostCoordinates(point, this)
                .Any(u => u == PART_Address)
                && VisualTreeHelper.FindElementsInHostCoordinates(point, this)
                .Where(u => u == FirstByteView || u == SecondByteView).FirstOrDefault() is ModbusScanByteDataItemView pointedItem)
            {
                if (!pointedItem.IsSelected) IsPointerOver = true;
                else IsPointerOver = false;
            }
            else
            {
                IsPointerOver = true;
            }
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            IsPointerOver = false;
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            var pointerPoint = e.GetCurrentPoint(null);
            if (pointerPoint.Properties.IsLeftButtonPressed)
                IsPointerOver = false;
        }
    }
}
