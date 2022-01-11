using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VagabondK.App;
using VagaModbusAnalyzer.ViewModels;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace VagaModbusAnalyzer.Infrastructures
{
    [ServiceDescription(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class ThemeManager : NotifyPropertyChangeObject
    {
        public ThemeManager(IDialog dialog, ICrossThreadDispatcher dispatcher)
        {
            this.dialog = dialog;
            this.dispatcher = dispatcher;

            uiSettings = new UISettings();
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                ["SystemAccentColorLight3"] = uiSettings.GetColorValue(UIColorType.AccentLight3),
                ["SystemAccentColorLight2"] = uiSettings.GetColorValue(UIColorType.AccentLight2),
                ["SystemAccentColorLight1"] = uiSettings.GetColorValue(UIColorType.AccentLight1),
                ["SystemAccentColor"] = uiSettings.GetColorValue(UIColorType.Accent),
                ["SystemAccentColorDark1"] = uiSettings.GetColorValue(UIColorType.AccentDark1),
                ["SystemAccentColorDark2"] = uiSettings.GetColorValue(UIColorType.AccentDark2),
                ["SystemAccentColorDark3"] = uiSettings.GetColorValue(UIColorType.AccentDark3),
                ["SystemColorHighlightColor"] = uiSettings.GetColorValue(UIColorType.Accent),
            });

            uiSettings.ColorValuesChanged += UISettingsColorValuesChanged;

            ApplicationData.Current.DataChanged += ApplicationDataChanged;
        }

        private static readonly ElementTheme DefaultAppTheme = ElementTheme.Dark;
        private static readonly Color DefaultAccentColor = Colors.Orange;
        private static readonly bool DefaultUseWindowsDefaultAccentColor = false;

        private readonly UISettings uiSettings;
        private readonly IDialog dialog;
        private readonly ICrossThreadDispatcher dispatcher;

        public ElementTheme AppTheme
        {
            get => Get(() =>
            {
                if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey(nameof(AppTheme)))
                {
                    ApplicationData.Current.RoamingSettings.Values[nameof(AppTheme)] = DefaultAppTheme.ToString();
                }

                return Enum.Parse<ElementTheme>(ApplicationData.Current.RoamingSettings.Values[nameof(AppTheme)] as string);
            });
            set => Set(value);
        }

        public bool UseWindowsDefaultAccentColor
        {
            get => Get(() =>
            {
                if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey(nameof(UseWindowsDefaultAccentColor)))
                {
                    ApplicationData.Current.RoamingSettings.Values[nameof(UseWindowsDefaultAccentColor)] = DefaultUseWindowsDefaultAccentColor;
                }
                return (bool)ApplicationData.Current.RoamingSettings.Values[nameof(UseWindowsDefaultAccentColor)];
            });
            set => Set(value);
        }

        public Color AccentColor
        {
            get => Get(() =>
            {
                if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey(nameof(AccentColor)))
                {
                    ApplicationData.Current.RoamingSettings.Values[nameof(AccentColor)] = ColorToUInt32(DefaultAccentColor);
                }

                var colorCode = (uint)ApplicationData.Current.RoamingSettings.Values[nameof(AccentColor)];

                return Color.FromArgb(
                    (byte)((colorCode >> 24) & 0xff),
                    (byte)((colorCode >> 16) & 0xff),
                    (byte)((colorCode >> 8) & 0xff),
                    (byte)(colorCode & 0xff));
            });
            set => Set(value);
        }

        public ICommand ChooseCustomAccentColorCommand
        {
            get => GetCommand(async () =>
            {
                if (await dialog.ShowDialog<ThemeColorPickerPopup>(popup =>
                {
                    popup.Color = AccentColor;
                }, out var themeColorPickerPopup) == true)
                {
                    AccentColor = themeColorPickerPopup.Color;
                }
            });
        }

        public void InitTheme()
        {
            RefreshTheme();
            RefreshAccentColor();
        }

        private uint ColorToUInt32(Color color)
        {
            return ((uint)color.A << 24)
                | ((uint)color.R << 16)
                | ((uint)color.G << 8)
                | color.B;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(AppTheme):
                    if (AppTheme.ToString() != ApplicationData.Current.RoamingSettings.Values[nameof(AppTheme)] as string)
                        ApplicationData.Current.RoamingSettings.Values[nameof(AppTheme)] = AppTheme.ToString();
                    RefreshTheme();
                    break;
                case nameof(UseWindowsDefaultAccentColor):
                    if (UseWindowsDefaultAccentColor != (bool)ApplicationData.Current.RoamingSettings.Values[nameof(UseWindowsDefaultAccentColor)])
                        ApplicationData.Current.RoamingSettings.Values[nameof(UseWindowsDefaultAccentColor)] = UseWindowsDefaultAccentColor;
                    RefreshAccentColor();
                    break;
                case nameof(AccentColor):
                    if (ColorToUInt32(AccentColor) != (uint)ApplicationData.Current.RoamingSettings.Values[nameof(AccentColor)])
                        ApplicationData.Current.RoamingSettings.Values[nameof(AccentColor)] = ColorToUInt32(AccentColor);
                    RefreshAccentColor();
                    break;
            }
        }

        private void RefreshTheme()
        {
            if (Window.Current?.Content is FrameworkElement root)
            {
                root.RequestedTheme = AppTheme;
                var resources = Application.Current.Resources;
            }
        }

        private void RefreshAccentColor()
        {
            if (Window.Current?.Content is FrameworkElement root)
            {
                if (UseWindowsDefaultAccentColor)
                {
                    SetWindowsSettingAccentColors(uiSettings);
                }
                else
                {
                    Color[] accentColors = ColorAPI.GenerateAccentColors(AccentColor).ToArray();
                    var resources = Application.Current.Resources;
                    resources["SystemAccentColorLight3"] = accentColors[0];
                    resources["SystemAccentColorLight2"] = accentColors[1];
                    resources["SystemAccentColorLight1"] = accentColors[2];
                    resources["SystemAccentColor"] = accentColors[3];
                    resources["SystemAccentColorDark1"] = accentColors[4];
                    resources["SystemAccentColorDark2"] = accentColors[5];
                    resources["SystemAccentColorDark3"] = accentColors[6];
                    resources["SystemColorHighlightColor"] = resources["SystemAccentColorDark1"];

                    var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x40, 0x80, 0x80, 0x80);
                    titleBar.ButtonPressedBackgroundColor = Color.FromArgb(0x20, 0x80, 0x80, 0x80);
                }

                if (root.ActualTheme == ElementTheme.Dark)
                    root.RequestedTheme = ElementTheme.Light;
                else
                    root.RequestedTheme = ElementTheme.Dark;
                root.RequestedTheme = AppTheme;
            }
        }

        private void UISettingsColorValuesChanged(UISettings sender, object args)
        {
            if (UseWindowsDefaultAccentColor)
            {
                dispatcher?.Invoke(() =>
                {
                    SetWindowsSettingAccentColors(sender);
                });
            }
        }

        private void ApplicationDataChanged(ApplicationData sender, object args)
        {
            dispatcher?.Invoke(() =>
            {
                try
                {
                    AppTheme = Enum.Parse<ElementTheme>(ApplicationData.Current.RoamingSettings.Values[nameof(AppTheme)] as string);
                }
                catch { }

                try
                {
                    UseWindowsDefaultAccentColor = (bool)ApplicationData.Current.RoamingSettings.Values[nameof(UseWindowsDefaultAccentColor)];
                }
                catch { }

                try
                {
                    var colorCode = (uint)ApplicationData.Current.RoamingSettings.Values[nameof(AccentColor)];
                    AccentColor = Color.FromArgb(
                        (byte)((colorCode >> 24) & 0xff),
                        (byte)((colorCode >> 16) & 0xff),
                        (byte)((colorCode >> 8) & 0xff),
                        (byte)(colorCode & 0xff));
                }
                catch { }
            });
        }

        private static void SetWindowsSettingAccentColors(UISettings uiSettings)
        {
            var accentColor = uiSettings.GetColorValue(UIColorType.Accent);
            var resources = Application.Current.Resources;
            resources["SystemAccentColorLight3"] = uiSettings.GetColorValue(UIColorType.AccentLight3);
            resources["SystemAccentColorLight2"] = uiSettings.GetColorValue(UIColorType.AccentLight2);
            resources["SystemAccentColorLight1"] = uiSettings.GetColorValue(UIColorType.AccentLight1);
            resources["SystemAccentColor"] = accentColor;
            resources["SystemAccentColorDark1"] = uiSettings.GetColorValue(UIColorType.AccentDark1);
            resources["SystemAccentColorDark2"] = uiSettings.GetColorValue(UIColorType.AccentDark2);
            resources["SystemAccentColorDark3"] = uiSettings.GetColorValue(UIColorType.AccentDark3);
            resources["SystemColorHighlightColor"] = resources["SystemAccentColorDark1"];

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x40, 0x80, 0x80, 0x80);
            titleBar.ButtonPressedBackgroundColor = Color.FromArgb(0x20, 0x80, 0x80, 0x80);
        }

    }
}
