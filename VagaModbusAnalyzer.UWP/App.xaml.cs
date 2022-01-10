using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VagaModbusAnalyzer.Infrastructures;
using VagaModbusAnalyzer.ViewModels;
using VagaModbusAnalyzer.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace VagaModbusAnalyzer
{
    sealed partial class App : Application
    {
        public App()
        {
            new UISettings().ColorValuesChanged += UISettings_ColorValuesChanged;

            this.InitializeComponent();

            Suspending += OnSuspending;
            UnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            throw e.Exception;
        }

        static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.All,
            Converters = new JsonConverter[] { new ModbusEndianJsonConverter() }
        };

        private Shell mainViewModel;
        private ApplicationView applicationView = null;

        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            MainPage mainPage = Window.Current.Content as MainPage;
            if (mainPage == null)
            {
                AppData appData = await LoadAppData();

                ServiceCollection services = new ServiceCollection();
                services.AddSingleton(appData);
                services.AddServices(typeof(AppData).Assembly);
                services.AddServices(typeof(App).Assembly);

                mainViewModel = new Shell(services);

                var themeManager = mainViewModel.ShellServiceProvider.GetService<ThemeManager>();

                mainPage = mainViewModel.MainPage;
                mainPage.PageFrame.NavigationFailed += OnNavigationFailed;


                var localizer = mainViewModel.ShellServiceProvider.GetService<IStringLocalizer>();

                foreach (var item in new MainMenuItem[]
                {
                    new MainMenuItem()
                    {
                        Icon = 0xEDA3, 
                        Title = localizer["MasterDetailMenuButton_ModbusChannels/Content"],
                        ViewModelTypeName = typeof(ModbusChannels).AssemblyQualifiedName,
                        ViewTypeName = typeof(ModbusChannelsView).AssemblyQualifiedName
                    },
                    new MainMenuItem()
                    {
                        Icon = 0xF19D,
                        Title = localizer["MasterDetailMenuButton_ReadData/Content"],
                        ViewModelTypeName = typeof(ReadData).AssemblyQualifiedName,
                        ViewTypeName = typeof(ReadDataView).AssemblyQualifiedName,
                        CanExecute = () => appData?.Channels?.Count > 0
                    },
                    new MainMenuItem()
                    {
                        Icon = 0xE929,
                        Title = localizer["MasterDetailMenuButton_WriteData/Content"],
                        ViewModelTypeName = typeof(WriteData).AssemblyQualifiedName,
                        ViewTypeName = typeof(WriteDataView).AssemblyQualifiedName,
                        CanExecute = () => appData?.Channels?.Count > 0
                    },
                    new MainMenuItem()
                    {
                        Icon = 0xE8FD,
                        Title = localizer["MasterDetailMenuButton_Logs/Content"],
                        ViewModelTypeName = typeof(Logs).AssemblyQualifiedName,
                        ViewTypeName = typeof(LogsView).AssemblyQualifiedName,
                        CanExecute = () => appData?.Channels?.Count > 0
                    },
                })
                {
                    mainViewModel.SplitViewMenuItems.Add(item);
                }

                foreach (var item in new MainMenuItem[]
                {
                    new MainMenuItem() { Icon = 0xE115, Title = localizer["MasterDetailMenuButton_Settings/Content"], ViewModelTypeName = typeof(Settings).AssemblyQualifiedName, ViewTypeName = typeof(SettingsView).AssemblyQualifiedName },
                })
                {
                    mainViewModel.SplitViewFooterMenuItems.Add(item);
                }


                appData.ChannelsCollectionChanged += AppData_ChannelsCollectionChanged;
                AppData_ChannelsCollectionChanged(appData, EventArgs.Empty);

                Window.Current.Content = mainPage;

                themeManager.InitTheme();
            }
            base.OnLaunched(e);


            foreach (var channel in mainViewModel.ShellServiceProvider.GetService<AppData>().Channels)
            {
                channel.StartScan(mainViewModel.ShellServiceProvider.GetService<IChannelFactory>(), mainViewModel.ShellServiceProvider.GetService<ICrossThreadDispatcher>());
            }

            mainViewModel = mainPage.DataContext as Shell;

            if (e.PrelaunchActivated == false)
            {
                var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.LayoutMetricsChanged += (sender, _) =>
                {
                    mainPage.OnInitCustomTitleBar(sender);
                };
                coreTitleBar.ExtendViewIntoTitleBar = true;

                applicationView = ApplicationView.GetForCurrentView();
                var titleBar = applicationView.TitleBar;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                if (mainPage.PageFrame.Content == null)
                {
                    await mainViewModel.OpenPage(mainViewModel.SplitViewMenuItems.FirstOrDefault());
                }
                Window.Current.Activate();
            }
        }

        private void UISettings_ColorValuesChanged(UISettings sender, object args)
        {
            var DefaultTheme = new UISettings();
            var uiTheme = DefaultTheme.GetColorValue(UIColorType.Foreground);
            var titleBar = applicationView.TitleBar;
            titleBar.ButtonForegroundColor = uiTheme;
        }

        private void AppData_ChannelsCollectionChanged(object sender, EventArgs e)
        {
            (mainViewModel.ExecuteMainMenuCommand as IInstantCommand)?.RaiseCanExecuteChanged();
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SaveAppData();

            deferral.Complete();
        }

        private async Task<AppData> LoadAppData()
        {
            AppData appData = null;

            try
            {
                var dataFileInfo = await ApplicationData.Current.LocalFolder.TryGetItemAsync("data");

                if (dataFileInfo != null)
                {
                    var dataFile = await ApplicationData.Current.LocalFolder.GetFileAsync("data");
                    using (var stream = await dataFile.OpenStreamForReadAsync())
                    using (var streamReader = new StreamReader(stream))
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        appData = JsonSerializer.Create(jsonSerializerSettings).Deserialize<AppData>(jsonReader);
                    }
                }
            }
            catch
            {

            }

            if (appData == null)
                appData = new AppData();

            return appData;
        }

        private async Task SaveAppData()
        {
            try
            {
                var appData = mainViewModel.ShellServiceProvider.GetService<AppData>();
                if (appData != null)
                {
                    var dataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("data", CreationCollisionOption.ReplaceExisting);
                    using (var stream = await dataFile.OpenStreamForWriteAsync())
                    using (var streamWriter = new StreamWriter(stream))
                    {
                        JsonSerializer.Create(jsonSerializerSettings).Serialize(streamWriter, appData);
                    }
                }
            }
            catch
            {

            }
        }
    }
}
