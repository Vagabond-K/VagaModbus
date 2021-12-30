using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagaModbusAnalyzer.Data;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

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




        public static IList<ModbusData> GetSelectedItems(UIElement obj)
        {
            return (IList<ModbusData>)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(UIElement obj, IList<ModbusData> value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(IList<ModbusData>), typeof(ModbusScanDataItemView), new PropertyMetadata(null, SelectedItemsPropertyChanged));

        private static void SelectedItemsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs de)
        {
            if (dependencyObject is UIElement root)
            {
                var viewDictionary = new Dictionary<ModbusData, ModbusScanDataItemView>();
                var selectedItems = de.NewValue as IList<ModbusData>;

                void onPointerPressed(object sender, PointerRoutedEventArgs e)
                {
                    if (e.Handled) return;

                    var pointerPoint = e.GetCurrentPoint(null);

                    Point pressedPosition = pointerPoint.Position;
                    pressedPosition = new Point(Math.Round(pressedPosition.X), Math.Round(pressedPosition.Y));

                    if (VisualTreeHelper.FindElementsInHostCoordinates(pressedPosition, root).Where(u => u is Windows.UI.Xaml.Controls.Primitives.SelectorItem element).FirstOrDefault() is Windows.UI.Xaml.Controls.Primitives.SelectorItem selectorItem)
                    {
                        selectorItem.IsSelected = true;
                    }

                    if (pointerPoint.Properties.IsLeftButtonPressed
                        && VisualTreeHelper.FindElementsInHostCoordinates(pressedPosition, root).Where(u => u is ItemsRepeater element
                        && (element.DataContext is IEnumerable<ModbusRegister>
                        || element.DataContext is IEnumerable<ModbusBoolean>)).FirstOrDefault() is ItemsRepeater itemsRepeater
                        && VisualTreeHelper.FindElementsInHostCoordinates(pressedPosition, itemsRepeater).Where(u => u is ModbusScanDataItemView).FirstOrDefault() is ModbusScanDataItemView view)
                    {
                        DeselectAll(selectedItems, viewDictionary);

                        int pressedItemIndex = -1;

                        //view.IsSelected = true;
                        //if (SelectedDataItems?.Contains(view.DataContext) == false)
                        //    SelectedDataItems?.Add(view.DataContext as ModbusScanDataItem);

                        root.CapturePointer(e.Pointer);

                        if (view is ModbusScanByteDataItemView byteView)
                        {
                            var parent = VisualTreeHelper.GetParent(view);
                            while (parent != null && !(parent is ModbusScanRegisterDataItemView))
                                parent = VisualTreeHelper.GetParent(parent);

                            if (parent is ModbusScanRegisterDataItemView registerDataItemView)
                            {
                                pressedItemIndex = itemsRepeater.GetElementIndex(registerDataItemView) * 2
                                    + byteView.IndexInRegister;
                                SetByteViewSelect(selectedItems, itemsRepeater, viewDictionary, pressedItemIndex, true);
                            }
                        }
                        else if (view.DataContext is ModbusBoolean bitDataItem)
                        {
                            pressedItemIndex = itemsRepeater.GetElementIndex(view);
                            SetBitViewSelect(selectedItems, itemsRepeater, viewDictionary, pressedItemIndex, true);
                        }

                        var selectStartIndex = pressedItemIndex;
                        var selectEndIndex = pressedItemIndex;

                        void onPointerMoved(object s, PointerRoutedEventArgs eventArgs)
                        {
                            if (eventArgs.Handled) return;

                            var movePoint = eventArgs.GetCurrentPoint(null);
                            if (!movePoint.Properties.IsLeftButtonPressed) return;

                            Point movePosition = movePoint.Position;
                            movePosition = new Point(Math.Round(movePosition.X), Math.Round(movePosition.Y));

                            var selectedItem = VisualTreeHelper.FindElementsInHostCoordinates(movePosition, itemsRepeater)
                                                .Where(u => u is ModbusScanDataItemView).FirstOrDefault();
                            if (!(selectedItem is ModbusScanDataItemView))
                            {
                                var rect = new Rect(pressedPosition, movePosition);

                                var list = VisualTreeHelper.FindElementsInHostCoordinates(new Rect(0, rect.Y, rect.X + rect.Width, rect.Height), itemsRepeater).ToArray();

                                selectedItem = VisualTreeHelper.FindElementsInHostCoordinates(new Rect(0, rect.Y, rect.X + rect.Width, rect.Height), itemsRepeater)
                                                .Where(u => u is ModbusScanDataItemView)
                                                .Select(u => new { ModbusScanDataItemView = u as ModbusScanDataItemView, eventArgs.GetCurrentPoint(u).Position })
                                                .GroupBy(u => (int)Math.Abs(u.Position.Y))
                                                .OrderBy(g => g.Key)
                                                .FirstOrDefault()?
                                                .OrderBy(u => Math.Abs(u.Position.X))
                                                .FirstOrDefault()?.ModbusScanDataItemView;
                            }

                            if (selectedItem is ModbusScanDataItemView movingView)
                            {
                                if (movingView is ModbusScanByteDataItemView movingByteView)
                                {
                                    var parent = VisualTreeHelper.GetParent(movingView);
                                    while (parent != null && !(parent is ModbusScanRegisterDataItemView))
                                        parent = VisualTreeHelper.GetParent(parent);

                                    if (parent is ModbusScanRegisterDataItemView registerView)
                                    {
                                        int startIndex = pressedItemIndex;
                                        int endIndex = itemsRepeater.GetElementIndex(registerView) * 2
                                            + movingByteView.IndexInRegister;

                                        if (startIndex > endIndex)
                                        {
                                            var temp = startIndex;
                                            startIndex = endIndex;
                                            endIndex = temp;
                                        }

                                        for (int i = selectStartIndex - 1; i >= startIndex; i--)
                                        {
                                            SetByteViewSelect(selectedItems, itemsRepeater, viewDictionary, i, true, true);
                                        }
                                        for (int i = selectEndIndex + 1; i <= endIndex; i++)
                                        {
                                            SetByteViewSelect(selectedItems, itemsRepeater, viewDictionary, i, true);
                                        }

                                        for (int i = selectStartIndex; i < startIndex; i++)
                                        {
                                            SetByteViewSelect(selectedItems, itemsRepeater, viewDictionary, i, false);
                                        }
                                        for (int i = endIndex + 1; i <= selectEndIndex; i++)
                                        {
                                            SetByteViewSelect(selectedItems, itemsRepeater, viewDictionary, i, false);
                                        }

                                        selectStartIndex = startIndex;
                                        selectEndIndex = endIndex;
                                    }
                                }
                                else if (movingView.DataContext is ModbusBoolean bitDataItem)
                                {
                                    int startIndex = pressedItemIndex;
                                    int endIndex = itemsRepeater.GetElementIndex(movingView);

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

                                    for (int i = selectStartIndex - 1; i >= startIndex; i--)
                                    {
                                        SetBitViewSelect(selectedItems, itemsRepeater, viewDictionary, i, true, true);
                                    }
                                    for (int i = selectEndIndex + 1; i <= endIndex; i++)
                                    {
                                        SetBitViewSelect(selectedItems, itemsRepeater, viewDictionary, i, true);
                                    }

                                    for (int i = selectStartIndex; i < startIndex; i++)
                                    {
                                        SetBitViewSelect(selectedItems, itemsRepeater, viewDictionary, i, false);
                                    }
                                    for (int i = endIndex + 1; i <= selectEndIndex; i++)
                                    {
                                        SetBitViewSelect(selectedItems, itemsRepeater, viewDictionary, i, false);
                                    }

                                    selectStartIndex = startIndex;
                                    selectEndIndex = endIndex;
                                }
                            }
                        }

                        void onEndSelection(object s, PointerRoutedEventArgs eventArgs)
                        {
                            if (e.Handled) return;
                            root.PointerMoved -= onPointerMoved;
                            root.PointerReleased -= onEndSelection;
                            root.PointerCaptureLost -= onEndSelection;
                            if (selectStartIndex == selectEndIndex)
                                DeselectAll(selectedItems, viewDictionary);
                        }

                        root.PointerMoved += onPointerMoved;
                        root.PointerReleased += onEndSelection;
                        root.PointerCaptureLost += onEndSelection;
                    }
                    else
                    {
                        DeselectAll(selectedItems, viewDictionary);
                    }
                }

                if (selectedItems != null)
                {
                    root.PointerPressed += onPointerPressed;
                }
                else
                {
                    root.PointerPressed -= onPointerPressed;
                }

                if (dependencyObject is Pivot pivot)
                {
                    void pivotSelectionChanged(object sender, SelectionChangedEventArgs e)
                    {
                        DeselectAll(selectedItems, viewDictionary);
                    }

                    if (selectedItems != null)
                    {
                        pivot.SelectionChanged += pivotSelectionChanged;
                    }
                    else
                    {
                        pivot.SelectionChanged -= pivotSelectionChanged;
                    }
                }
            }
        }

        private static void DeselectAll(IList<ModbusData> selectedItems, Dictionary<ModbusData, ModbusScanDataItemView> viewDictionary)
        {
            if (selectedItems != null)
                foreach (var item in selectedItems)
                {
                    if (viewDictionary.TryGetValue(item, out var view))
                    {
                        view.IsSelected = false;
                        viewDictionary.Remove(item);
                    }
                }

            selectedItems?.Clear();
        }

        private static void SetByteViewSelect(IList<ModbusData> selectedItems, ItemsRepeater itemsRepeater, Dictionary<ModbusData, ModbusScanDataItemView> viewDictionary, int i, bool select, bool isInsertFirst = false)
        {
            if (itemsRepeater.TryGetElement(i / 2) is ModbusScanRegisterDataItemView registerView
                && registerView.DataContext is ModbusRegister modbusScanRegisterDataItem)
            {
                var view = i % 2 == 0 ? registerView.FirstByteView : registerView.SecondByteView;
                var item = i % 2 == 0 ? modbusScanRegisterDataItem.FirstByte : modbusScanRegisterDataItem.SecondByte;

                view.IsSelected = select;
                if (select)
                {
                    if (selectedItems?.Contains(item) == false)
                    {
                        if (isInsertFirst) selectedItems?.Insert(0, item);
                        else selectedItems?.Add(item);
                        viewDictionary[item] = view;
                    }
                }
                else
                {
                    selectedItems?.Remove(item);
                    viewDictionary.Remove(item);
                }
            }
        }

        private static void SetBitViewSelect(IList<ModbusData> selectedItems, ItemsRepeater itemsRepeater, Dictionary<ModbusData, ModbusScanDataItemView> viewDictionary, int i, bool select, bool isInsertFirst = false)
        {
            if (itemsRepeater.TryGetElement(i) is ModbusScanBitDataItemView view
                && view.DataContext is ModbusBoolean item)
            {
                view.IsSelected = select;
                if (select)
                {
                    if (selectedItems?.Contains(item) == false)
                    {
                        if (isInsertFirst) selectedItems?.Insert(0, item);
                        else selectedItems?.Add(item);
                        viewDictionary[item] = view;
                    }
                }
                else
                {
                    selectedItems?.Remove(item);
                    viewDictionary.Remove(item);
                }
            }
        }

    }
}
