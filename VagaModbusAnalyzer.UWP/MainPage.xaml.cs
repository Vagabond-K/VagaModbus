using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VagaModbusAnalyzer
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            PART_SplitView.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, IsPaneOpenPropertyChanged);
            SizeChanged += ShellSizeChanged;
            Loaded += OnLoaded;

            ActualThemeChanged += OnActualThemeChanged;
        }

        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            switch (RequestedTheme)
            {
                case ElementTheme.Dark:
                    titleBar.ButtonForegroundColor = Colors.White;
                    break;
                case ElementTheme.Light:
                    titleBar.ButtonForegroundColor = Colors.Black;
                    break;
                default:
                    var DefaultTheme = new UISettings();
                    var uiTheme = DefaultTheme.GetColorValue(UIColorType.Foreground);
                    titleBar.ButtonForegroundColor = uiTheme;
                    break;
            }
        }

        public Frame PageFrame { get => PART_AppFrame; }
        public CommandBar CommandBar { get => PART_CommandBar; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (PART_SplitView.DisplayMode == SplitViewDisplayMode.CompactInline && DataContext is MainViewModel mainViewModel)
            {
                PART_SplitView.IsPaneOpen = mainViewModel.IsPaneOpen;
            }
        }

        private void OpenSplitMenu()
        {
            switch (PART_SplitView.DisplayMode)
            {
                case SplitViewDisplayMode.CompactOverlay:
                    PART_SplitView.IsPaneOpen = true;
                    break;
                case SplitViewDisplayMode.CompactInline:
                    if (DataContext is MainViewModel mainViewModel)
                        mainViewModel.IsPaneOpen = PART_SplitView.IsPaneOpen = !PART_SplitView.IsPaneOpen;
                    break;
            }
        }

        private void ShellSizeChanged(object sender, SizeChangedEventArgs e)
        {
            PART_CommandBarContent.MaxWidth = ActualWidth;
        }


        private void IsPaneOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (PART_SplitView.IsPaneOpen && PART_SplitViewPane.ActualWidth > 0)
            {
                PART_SplitView.OpenPaneLength = PART_SplitViewPane.ActualWidth;
            }
        }

        private void PART_SplitViewPane_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            IsPaneOpenPropertyChanged(PART_SplitView, SplitView.IsPaneOpenProperty);
        }

        public void OnInitCustomTitleBar(CoreApplicationViewTitleBar coreApplicationViewTitleBar)
        {
            PART_TitleBar.Height = coreApplicationViewTitleBar.Height;
            PART_TitleBar_LeftPaddingColumn.Width = new GridLength(coreApplicationViewTitleBar.SystemOverlayLeftInset);
            PART_TitleBar_RightPaddingColumn.Width = new GridLength(coreApplicationViewTitleBar.SystemOverlayRightInset);
            Window.Current.SetTitleBar(PART_TitleBar);
        }

    }
}
