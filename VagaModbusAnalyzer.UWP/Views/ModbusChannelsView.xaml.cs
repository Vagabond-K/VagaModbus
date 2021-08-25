using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VagaModbusAnalyzer.Infrastructures;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VagaModbusAnalyzer.Views
{
    [View]
    public sealed partial class ModbusChannelsView : UserControl
    {
        public ModbusChannelsView()
        {
            this.InitializeComponent();
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
            itemWidthCalculator = Resources["ItemWidthCalculator"] as ItemWidthCalculator;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PART_GridView.ScrollIntoView(PART_GridView.SelectedItem);
        }

        private ItemWidthCalculator itemWidthCalculator = null;


        private void OnSizeChanged(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                int desiredWidth = 350;
                int itemMargin = 18;
                int clientMargin = 30;

                int columnCount = (int)((ActualWidth - clientMargin) / (desiredWidth + itemMargin));
                if (columnCount == 0)
                {
                    columnCount += 1;
                }
                itemWidthCalculator.ItemWidth = Math.Floor((ActualWidth - clientMargin) / columnCount - itemMargin);
            }
        }
    }

}
