using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace VagaModbusAnalyzer.Controls
{
    public class ModbusScanBitDataItemView : ModbusScanDataItemView
    {
        public ModbusScanBitDataItemView()
        {
            DefaultStyleKey = typeof(ModbusScanBitDataItemView);
        }

        public bool? Value
        {
            get { return (bool?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(bool?), typeof(ModbusScanBitDataItemView), new PropertyMetadata(null));

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_SelectedBackround = GetTemplateChild("PART_SelectedBackround") as Rectangle;
        }

        private Rectangle PART_SelectedBackround = null;

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
            base.OnPointerEntered(e);

            var pointerPoint = e.GetCurrentPoint(null);
            Point point = pointerPoint.Position;
            point = new Point(Math.Round(point.X), Math.Round(point.Y));

            if (VisualTreeHelper.FindElementsInHostCoordinates(point, this)
                .Where(u => u == PART_SelectedBackround).FirstOrDefault() != null)
            {
                if (!IsSelected) IsPointerOver = true;
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
