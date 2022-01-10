using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VagabondK.App;
using VagaModbusAnalyzer.Controls;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace VagaModbusAnalyzer
{
    public class Shell : Shell<MainMenuItem>
    {
        public Shell(IServiceCollection services) : base(services)
        {
            ExecuteMainMenuCommand = new InstantCommand<MainMenuItem>(async mainMenuItem => await OpenPage(mainMenuItem), mainMenuItem => mainMenuItem?.CanExecute?.Invoke() ?? true);


            if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey(nameof(IsPaneOpen)))
            {
                ApplicationData.Current.RoamingSettings.Values[nameof(IsPaneOpen)] = true;
            }
            isPaneOpen = (bool)ApplicationData.Current.RoamingSettings.Values[nameof(IsPaneOpen)];
            ApplicationData.Current.DataChanged += OnApplicationDataChanged;

            Title = Package.Current.DisplayName;

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        private bool isPaneOpen;

        public bool IsPaneOpen
        {
            get => isPaneOpen;
            set
            {
                if (isPaneOpen != value)
                {
                    isPaneOpen = value;
                    ApplicationData.Current.RoamingSettings.Values[nameof(IsPaneOpen)] = isPaneOpen;
                }
            }
        }

        private MainPage mainPage;
        public MainPage MainPage
        {
            get
            {
                if (mainPage == null)
                {
                    mainPage = new MainPage();
                    mainPage.PageFrame.Navigating += OnNavigating;
                    mainPage.PageFrame.Navigated += OnNavigated;
                    mainPage.DataContext = this;
                }
                return mainPage;
            }
        }

        public ICommand ExecuteMainMenuCommand { get; }

        public IList<MainMenuItem> SplitViewMenuItems { get; } = new ObservableCollection<MainMenuItem>();
        public IList<MainMenuItem> SplitViewFooterMenuItems { get; } = new ObservableCollection<MainMenuItem>();

        private async void OnApplicationDataChanged(ApplicationData sender, object args)
        {
            await MainPage?.Dispatcher?.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                try
                {
                    IsPaneOpen = (bool)ApplicationData.Current.RoamingSettings.Values[nameof(IsPaneOpen)];
                }
                catch { }
            });
        }

        public override Task<PageContext<MainMenuItem>> OpenPage(MainMenuItem pageData)
        {
            if (!(SelectedPageContext is PageContext<MainMenuItem> oldPageContext)
                || oldPageContext.PageData != pageData)
            {
                var pageScope = ShellServiceProvider.CreatePageScope(pageData);
                var pageContext = pageScope.ServiceProvider.GetService<PageContext<MainMenuItem>>();

                MainPage.PageFrame.Navigate(typeof(ViewHostPage), pageContext, MainPage.PageFrame.BackStackDepth > 0
                    ? (NavigationTransitionInfo)new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft }
                    : new ContinuumNavigationTransitionInfo());

                (pageContext.ViewModel as INotifyLoaded)?.OnLoaded();
                pageContext.PageData.Select();

                foreach (var pageStackEntry in MainPage.PageFrame.BackStack)
                {
                    if (pageStackEntry.Parameter is PageContext<MainMenuItem> backPageContext)
                    {
                        if (backPageContext.PageData != pageData)
                            backPageContext.PageData?.Deselect();
                        else
                            (pageStackEntry.Parameter as IDisposable)?.Dispose();
                    }
                    else
                        (pageStackEntry.Parameter as IDisposable)?.Dispose();
                }
                MainPage.PageFrame.BackStack.Clear();
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

                return Task.FromResult(pageContext);
            }
            return Task.FromResult(SelectedPageContext as PageContext<MainMenuItem>);
        }

        public override Task<PageContext> OpenPage(Type viewModelType, Type viewType, string title)
        {
            var pageScope = (SelectedPageContext?.ServiceProvider ?? ShellServiceProvider).CreatePageScope(viewModelType, viewType, title);
            var pageContext = pageScope.ServiceProvider.GetService<PageContext>();

            MainPage.PageFrame.Navigate(typeof(ViewHostPage), pageContext, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

            (pageContext.ViewModel as INotifyLoaded)?.OnLoaded();

            return Task.FromResult(pageContext);
        }

        private void OnSelectedPageContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PageContext.Result))
                MainPage.PageFrame.GoBack(); 
        }



        private void OnNavigating(object sender, Windows.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            MainPage.PageFrame.Focus(FocusState.Programmatic);
        }

        private void OnNavigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e?.Content is ViewHostPage viewHostPage
                && e.Parameter is PageContext pageContext)
            {
                if (pageContext.View is DependencyObject dependencyObjectView
                    && ViewHostPage.GetViewHostPage(dependencyObjectView) is ViewHostPage oldViewHostPage)
                {
                    oldViewHostPage.Content = null;
                    pageContext.ReloadView();
                }

                viewHostPage.DataContext = pageContext;

                if (viewHostPage.Content != null)
                    ViewHostPage.SetViewHostPage(pageContext.View as DependencyObject, viewHostPage);

                MainPage.CommandBar.PrimaryCommands.Clear();
                MainPage.CommandBar.SecondaryCommands.Clear();

                if (pageContext.View is FrameworkElement frameworkElement)
                {
                    frameworkElement.SetBinding(FrameworkElement.DataContextProperty, new Binding() { Path = new PropertyPath(nameof(pageContext.ViewModel)) });

                    var commandBar = ViewHostPage.GetViewCommands(frameworkElement);
                    if (commandBar != null)
                    {
                        foreach (var command in commandBar.PrimaryCommands)
                        {
                            SetElementSourceBindings(command, frameworkElement);
                            MainPage.CommandBar.PrimaryCommands.Add(command);
                        }
                        foreach (var command in commandBar.SecondaryCommands)
                        {
                            SetElementSourceBindings(command, frameworkElement);
                            MainPage.CommandBar.SecondaryCommands.Add(command);
                        }
                    }
                    if (MainPage.CommandBar.OverflowButtonVisibility == CommandBarOverflowButtonVisibility.Auto)
                    {
                        MainPage.CommandBar.OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Visible;
                        MainPage.CommandBar.UpdateLayout();
                        MainPage.CommandBar.OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Auto;
                    }
                }

                if (SelectedPageContext != null)
                    (SelectedPageContext as PageContext<MainMenuItem>).PropertyChanged -= OnSelectedPageContextPropertyChanged;
                SelectedPageContext = pageContext;
                if (SelectedPageContext != null)
                    (SelectedPageContext as PageContext<MainMenuItem>).PropertyChanged += OnSelectedPageContextPropertyChanged;

                (pageContext.ViewModel as INotifyLoaded)?.OnLoaded();
            }

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        private void SetElementSourceBindings(ICommandBarElement command, FrameworkElement target)
        {
            if (command is FrameworkElement frameworkElement)
            {
                foreach (var property in command.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(p => p.PropertyType == typeof(DependencyProperty)))
                {
                    if (property.GetValue(null) is DependencyProperty dependencyProperty)
                    {
                        var binding = frameworkElement?.GetBindingExpression(dependencyProperty)?.ParentBinding;
                        if (!string.IsNullOrWhiteSpace(binding?.ElementName))
                        {
                            frameworkElement.SetBinding(dependencyProperty, new Binding
                            {
                                Path = binding.Path,
                                Converter = binding.Converter,
                                ConverterLanguage = binding.ConverterLanguage,
                                ConverterParameter = binding.ConverterParameter,
                                FallbackValue = binding.FallbackValue,
                                Mode = binding.Mode,
                                RelativeSource = binding.RelativeSource,
                                TargetNullValue = binding.TargetNullValue,
                                UpdateSourceTrigger = binding.UpdateSourceTrigger,
                                Source = target.FindName(binding.ElementName)
                            });
                        }
                    }
                }
            }
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (MainPage.PageFrame.CanGoBack)
            {
                e.Handled = true;
                MainPage.PageFrame.GoBack();
            }
        }

    }

    public class MainMenuItem : NotifyPropertyChangeObject, IPageData
    {
        public object Icon { get => Get<object>(); set => Set(value); }
        public string Title { get => Get<string>(); set => Set(value); }
        public string ViewModelTypeName { get => Get<string>(); set => Set(value); }
        public string ViewTypeName { get => Get<string>(); set => Set(value); }

        public bool IsSelected { get => Get(false); private set => Set(value); }

        public Func<bool> CanExecute { get => Get<Func<bool>>(); set => Set(value); }

        public void Select() => IsSelected = true;
        public void Deselect() => IsSelected = false;
    }
}
