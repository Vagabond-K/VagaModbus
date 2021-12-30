using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VagaModbusAnalyzer.Data;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace VagaModbusAnalyzer.Controls
{
    public class ModbusChannelScanPivot : Pivot
    {
        public ModbusChannelScanPivot()
        {
            DefaultStyleKey = typeof(ModbusChannelScanPivot);
            SelectionChanged += PivotSelectionChanged;
        }

        private void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeselectAll();
            SelectedView = null;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public IList<ModbusData> SelectedDataItems
        {
            get { return (IList<ModbusData>)GetValue(SelectedDataItemsProperty); }
            set { SetValue(SelectedDataItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedDataItemsProperty =
            DependencyProperty.Register("SelectedDataItems", typeof(IList<ModbusData>), typeof(ModbusChannelScanPivot), new PropertyMetadata(null));



        public Windows.UI.Xaml.Controls.Primitives.SelectorItem SelectedView
        {
            get { return (Windows.UI.Xaml.Controls.Primitives.SelectorItem)GetValue(SelectedViewProperty); }
            set { SetValue(SelectedViewProperty, value); }
        }

        public static readonly DependencyProperty SelectedViewProperty =
            DependencyProperty.Register("SelectedView", typeof(Windows.UI.Xaml.Controls.Primitives.SelectorItem), typeof(ModbusChannelScanPivot), new PropertyMetadata(null, (d, e) =>
            {
                if (d is ModbusChannelScanPivot modbusChannelScanPivot
                    && e.OldValue is Windows.UI.Xaml.Controls.Primitives.SelectorItem oldSelectedView)
                {
                    oldSelectedView.IsSelected = false;
                    modbusChannelScanPivot.DeselectAll();
                }
            }));





        public ICommand ModbusWriteCommand
        {
            get { return (ICommand)GetValue(ModbusWriteCommandProperty); }
            set { SetValue(ModbusWriteCommandProperty, value); }
        }

        public static readonly DependencyProperty ModbusWriteCommandProperty =
            DependencyProperty.Register("ModbusWriteCommand", typeof(ICommand), typeof(ModbusChannelScanPivot), new PropertyMetadata(null));






        private ItemsRepeater _SelectStartItemsRepeater = null;
        private Point? _PointerPressedItemPoint = null;
        private int _PointerPressedItemIndex = -1;

        private int? _SelectStartIndex = null;
        private int? _SelectEndIndex = null;
        private Dictionary<ModbusData, ModbusScanDataItemView> _SelectedViewDictionary = new Dictionary<ModbusData, ModbusScanDataItemView>();


        private void DeselectAll()
        {
            //if (SelectedDataItems != null)
            //    foreach (var item in SelectedDataItems)
            //        item.IsSelected = false;

            if (SelectedDataItems != null)
                foreach (var item in SelectedDataItems)
                {
                    if (_SelectedViewDictionary.TryGetValue(item, out var view))
                    {
                        view.IsSelected = false;
                        _SelectedViewDictionary.Remove(item);
                    }
                }
            

            //if (_SelectStartIndex != null && _SelectEndIndex != null && _SelectStartItemsRepeater != null)
            //{
            //    for (int i = _SelectStartIndex.Value; i <= _SelectEndIndex.Value; i++)
            //    {
            //        SetByteViewSelect(i, false);
            //        SetBitViewSelect(i, false);
            //    }
            //}

            SelectedDataItems?.Clear();

            _SelectStartIndex = null;
            _SelectEndIndex = null;
        }

        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);

            var point = e.GetPosition(null);
            point = new Point(Math.Round(point.X), Math.Round(point.Y));
            if (VisualTreeHelper.FindElementsInHostCoordinates(point, this).Where(u => u is ItemsRepeater element
                && (element.DataContext is IEnumerable<ModbusRegister>
                || element.DataContext is IEnumerable<ModbusBoolean>)).FirstOrDefault() is ItemsRepeater itemsRepeater)
            {
                _SelectStartItemsRepeater = itemsRepeater;

                if (itemsRepeater.DataContext is IEnumerable<ModbusRegister>
                    && VisualTreeHelper.FindElementsInHostCoordinates(point, itemsRepeater).Where(u => u is ModbusScanByteDataItemView).FirstOrDefault() is ModbusScanByteDataItemView byteDataItemView
                    && VisualTreeHelper.FindElementsInHostCoordinates(point, itemsRepeater).Where(u => u is ModbusScanRegisterDataItemView).FirstOrDefault() is ModbusScanRegisterDataItemView registerDataItemView)
                {
                    if (!registerDataItemView.IsPointerOver && byteDataItemView.IsSelected)
                    {
                        ModbusWriteCommand?.Execute(null);
                    }
                    else
                    {
                        ModbusWriteCommand?.Execute(null);
                    }
                }
                else if (itemsRepeater.DataContext is IEnumerable<ModbusBoolean>
                    && VisualTreeHelper.FindElementsInHostCoordinates(point, itemsRepeater).Where(u => u is ModbusScanBitDataItemView).FirstOrDefault() is ModbusScanBitDataItemView bitDataItemView)
                {
                    if (!bitDataItemView.IsPointerOver && bitDataItemView.IsSelected)
                    {
                        ModbusWriteCommand?.Execute(null);
                    }
                    else
                    {
                        ModbusWriteCommand?.Execute(null);
                    }
                }
            }
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.Handled) return;

            var pointerPoint = e.GetCurrentPoint(null);

            Point point = pointerPoint.Position;
            point = new Point(Math.Round(point.X), Math.Round(point.Y));

            if (VisualTreeHelper.FindElementsInHostCoordinates(point, this).Where(u => u is Windows.UI.Xaml.Controls.Primitives.SelectorItem element).FirstOrDefault() is Windows.UI.Xaml.Controls.Primitives.SelectorItem selectorItem)
            {
                selectorItem.IsSelected = true;
            }

            if (!pointerPoint.Properties.IsLeftButtonPressed) return;

            if (VisualTreeHelper.FindElementsInHostCoordinates(point, this).Where(u => u is ItemsRepeater element
                && (element.DataContext is IEnumerable<ModbusRegister>
                || element.DataContext is IEnumerable<ModbusBoolean>)).FirstOrDefault() is ItemsRepeater itemsRepeater)
            {
                _SelectStartItemsRepeater = itemsRepeater;

                if (VisualTreeHelper.FindElementsInHostCoordinates(point, itemsRepeater).Where(u => u is ModbusScanDataItemView).FirstOrDefault() is ModbusScanDataItemView view)
                {
                    DeselectAll();


                    //view.IsSelected = true;
                    //if (SelectedDataItems?.Contains(view.DataContext) == false)
                    //    SelectedDataItems?.Add(view.DataContext as ModbusScanDataItem);

                    CapturePointer(e.Pointer);
                    _PointerPressedItemPoint = point;

                    if (view is ModbusScanByteDataItemView modbusScanByteDataItemView)
                    {
                        var parent = VisualTreeHelper.GetParent(view);
                        while (parent != null && !(parent is ModbusScanRegisterDataItemView))
                            parent = VisualTreeHelper.GetParent(parent);

                        if (parent is ModbusScanRegisterDataItemView registerDataItemView)
                        {
                            _PointerPressedItemIndex = _SelectStartItemsRepeater.GetElementIndex(registerDataItemView) * 2
                                + modbusScanByteDataItemView.IndexInRegister;
                            SetByteViewSelect(_PointerPressedItemIndex, true);
                        }
                    }
                    else if (view.DataContext is ModbusBoolean bitDataItem)
                    {
                        _PointerPressedItemIndex = _SelectStartItemsRepeater.GetElementIndex(view);
                        SetBitViewSelect(_PointerPressedItemIndex, true);
                    }
                    _SelectStartIndex = _PointerPressedItemIndex;
                    _SelectEndIndex = _PointerPressedItemIndex;
                }
            }
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);
            if (e.Handled) return;

            if (_PointerPressedItemPoint != null)
            {
                var pointerPoint = e.GetCurrentPoint(null);
                if (!pointerPoint.Properties.IsLeftButtonPressed) return;

                Point point = pointerPoint.Position;
                point = new Point(Math.Round(point.X), Math.Round(point.Y));

                var selectedItem = VisualTreeHelper.FindElementsInHostCoordinates(point, _SelectStartItemsRepeater)
                                    .Where(u => u is ModbusScanDataItemView).FirstOrDefault();
                if (!(selectedItem is ModbusScanDataItemView))
                {
                    var rect = new Rect(_PointerPressedItemPoint.Value, point);

                    var list = VisualTreeHelper.FindElementsInHostCoordinates(new Rect(0, rect.Y, rect.X + rect.Width, rect.Height), _SelectStartItemsRepeater).ToArray();

                    selectedItem = VisualTreeHelper.FindElementsInHostCoordinates(new Rect(0, rect.Y, rect.X + rect.Width, rect.Height), _SelectStartItemsRepeater)
                                    .Where(u => u is ModbusScanDataItemView)
                                    .Select(u => new { ModbusScanDataItemView = u as ModbusScanDataItemView, e.GetCurrentPoint(u).Position })
                                    .GroupBy(u => (int)Math.Abs(u.Position.Y))
                                    .OrderBy(g => g.Key)
                                    .FirstOrDefault()?
                                    .OrderBy(u => Math.Abs(u.Position.X))
                                    .FirstOrDefault()?.ModbusScanDataItemView;
                }

                if (selectedItem is ModbusScanDataItemView view)
                {
                    if (view is ModbusScanByteDataItemView modbusScanByteDataItemView)
                    {
                        var parent = VisualTreeHelper.GetParent(view);
                        while (parent != null && !(parent is ModbusScanRegisterDataItemView))
                            parent = VisualTreeHelper.GetParent(parent);

                        if (parent is ModbusScanRegisterDataItemView registerDataItemView)
                        {
                            int startIndex = _PointerPressedItemIndex;
                            int endIndex = _SelectStartItemsRepeater.GetElementIndex(registerDataItemView) * 2
                                + modbusScanByteDataItemView.IndexInRegister;

                            if (startIndex > endIndex)
                            {
                                var temp = startIndex;
                                startIndex = endIndex;
                                endIndex = temp;
                            }

                            for (int i = _SelectStartIndex.Value - 1; i >= startIndex; i--)
                            {
                                SetByteViewSelect(i, true, true);
                            }
                            for (int i = _SelectEndIndex.Value + 1; i <= endIndex; i++)
                            {
                                SetByteViewSelect(i, true);
                            }

                            for (int i = _SelectStartIndex.Value; i < startIndex; i++)
                            {
                                SetByteViewSelect(i, false);
                            }
                            for (int i = endIndex + 1; i <= _SelectEndIndex.Value; i++)
                            {
                                SetByteViewSelect(i, false);
                            }

                            _SelectStartIndex = startIndex;
                            _SelectEndIndex = endIndex;
                        }
                    }
                    else if (view.DataContext is ModbusBoolean bitDataItem)
                    {
                        int startIndex = _PointerPressedItemIndex;
                        int endIndex = _SelectStartItemsRepeater.GetElementIndex(view);

                        int maxCount = 64;
                        if (Math.Abs(endIndex - startIndex) >= maxCount)
                        {
                            if (startIndex > endIndex)
                                endIndex = startIndex - (maxCount - 1);
                            else
                                endIndex = startIndex + (maxCount - 1);
                        }

                        if (startIndex > endIndex)
                        {
                            var temp = startIndex;
                            startIndex = endIndex;
                            endIndex = temp;
                        }

                        for (int i = _SelectStartIndex.Value - 1; i >= startIndex; i--)
                        {
                            SetBitViewSelect(i, true, true);
                        }
                        for (int i = _SelectEndIndex.Value + 1; i <= endIndex; i++)
                        {
                            SetBitViewSelect(i, true);
                        }

                        for (int i = _SelectStartIndex.Value; i < startIndex; i++)
                        {
                            SetBitViewSelect(i, false);
                        }
                        for (int i = endIndex + 1; i <= _SelectEndIndex.Value; i++)
                        {
                            SetBitViewSelect(i, false);
                        }

                        _SelectStartIndex = startIndex;
                        _SelectEndIndex = endIndex;
                    }
                }
            }
        }

        private void SetByteViewSelect(int i, bool select, bool isInsertFirst = false)
        {
            if (_SelectStartItemsRepeater.TryGetElement(i / 2) is ModbusScanRegisterDataItemView registerView
                && registerView.DataContext is ModbusRegister modbusScanRegisterDataItem)
            {
                var view = i % 2 == 0 ? registerView.FirstByteView : registerView.SecondByteView;
                var item = i % 2 == 0 ? modbusScanRegisterDataItem.FirstByte : modbusScanRegisterDataItem.SecondByte;

                view.IsSelected = select;
                if (select)
                {
                    if (SelectedDataItems?.Contains(item) == false)
                    {
                        if (isInsertFirst) SelectedDataItems?.Insert(0, item);
                        else SelectedDataItems?.Add(item);
                        _SelectedViewDictionary[item] = view;
                    }
                }
                else
                {
                    SelectedDataItems?.Remove(item);
                    _SelectedViewDictionary.Remove(item);
                }
            }
        }

        private void SetBitViewSelect(int i, bool select, bool isInsertFirst = false)
        {
            if (_SelectStartItemsRepeater.TryGetElement(i) is ModbusScanBitDataItemView view
                && view.DataContext is ModbusBoolean item)
            {
                view.IsSelected = select;
                if (select)
                {
                    if (SelectedDataItems?.Contains(item) == false)
                    {
                        if (isInsertFirst) SelectedDataItems?.Insert(0, item);
                        else SelectedDataItems?.Add(item);
                        _SelectedViewDictionary[item] = view;
                    }
                }
                else
                {
                    SelectedDataItems?.Remove(item);
                    _SelectedViewDictionary.Remove(item);
                }
            }
        }


        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (e.Handled) return;
            EndSelect();
        }

        protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            if (e.Handled) return;
            EndSelect();
        }

        private void EndSelect()
        {
            _PointerPressedItemPoint = null;

            if (_SelectStartIndex != null && _SelectEndIndex != null
                && _SelectStartIndex.Value == _SelectEndIndex.Value)
            {
                DeselectAll();
            }
        }
    }
}
