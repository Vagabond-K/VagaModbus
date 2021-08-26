using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagabondK.App;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace VagaModbusAnalyzer.Infrastructures
{
    [ServiceDescription(typeof(IDialog))]
    public class Dialog : IDialog
    {
        public Dialog(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider serviceProvider;


        private static ContentDialogResult? GetResult(ContentDialog obj)
        {
            return (ContentDialogResult?)obj.GetValue(ResultProperty);
        }

        private static void SetResult(ContentDialog obj, ContentDialogResult? value)
        {
            obj.SetValue(ResultProperty, value);
        }

        private static readonly DependencyProperty ResultProperty =
            DependencyProperty.RegisterAttached("Result", typeof(ContentDialogResult?), typeof(Dialog), new PropertyMetadata(null));


        public Task<bool?> ShowDialog(Type viewModelType, Type viewType, string title, Action<object, object> initializer, out object viewModel)
        {
            using (var pageScope = serviceProvider.CreatePageScope(viewModelType, viewType, title))
            {
                var pageContext = pageScope.ServiceProvider.GetRequiredService<PageContext>();
                
                initializer?.Invoke(pageContext.ViewModel, pageContext.View);

                if (!(pageContext.View is ContentDialog dialog))
                {
                    dialog = new ContentDialog
                    {
                    };

                    dialog.SetBinding(ContentDialog.TitleProperty, new Binding() { Path = new PropertyPath(nameof(pageContext.Title)) });
                    dialog.SetBinding(ContentControl.ContentProperty, new Binding() { Path = new PropertyPath(nameof(pageContext.View)) });
                    (pageContext.View as FrameworkElement)?.SetBinding(FrameworkElement.DataContextProperty, new Binding() { Path = new PropertyPath(nameof(pageContext.ViewModel)) });
                    dialog.DataContext = pageContext;
                }
                else
                {
                    dialog.DataContext = pageContext.ViewModel;
                    dialog.SetBinding(ContentDialog.TitleProperty, new Binding() { Path = new PropertyPath(nameof(pageContext.Title)), Source = pageContext });
                }

                var themeManager = pageScope.ServiceProvider.GetService<ThemeManager>();
                if (themeManager != null)
                    dialog.RequestedTheme = themeManager.AppTheme;

                pageContext.Result = null;

                void OnPageContextPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
                {
                    switch (e.PropertyName)
                    {
                        case nameof(PageContext.Result):
                            if (pageContext.Result != null)
                            {
                                SetResult(dialog, pageContext.Result == true ? ContentDialogResult.Primary : ContentDialogResult.Secondary);
                                dialog.Hide();
                                //dialog.DialogResult = pageContext.Result;
                            }
                            break;
                        case nameof(PageContext.View):
                            (pageContext.View as FrameworkElement)?.SetBinding(FrameworkElement.DataContextProperty, new Binding() { Path = new PropertyPath(nameof(pageContext.ViewModel)) });
                            break;
                    }
                }

                void OnLoaded(object sender, RoutedEventArgs e)
                {
                    (pageContext.ViewModel as INotifyLoaded)?.OnLoaded();
                }

                async void Closing(object sender, ContentDialogClosingEventArgs e)
                {
                    if (pageContext.ViewModel is IQueryClosing queryClosing)
                        e.Cancel = await queryClosing.QueryClosing(ContentDialogResultToBoolean(e.Result) ?? false) == false;
                }

                void OnClosed(object sender, ContentDialogClosedEventArgs e)
                {
                    pageContext.PropertyChanged -= OnPageContextPropertyChanged;
                    dialog.Loaded -= OnLoaded;
                    dialog.Closed -= OnClosed;
                    dialog.Closing -= Closing;

                    if (pageContext.Result == null)
                        pageContext.Result = ContentDialogResultToBoolean(e.Result);
                }

                pageContext.PropertyChanged += OnPageContextPropertyChanged;
                dialog.Loaded += OnLoaded;
                dialog.Closing += Closing;
                dialog.Closed += OnClosed;

                viewModel = pageContext.ViewModel;

                return dialog.ShowAsync().AsTask().ContinueWith(contentDialogResult => ContentDialogResultToBoolean(contentDialogResult.Result));
            }
        }

        private static bool? ContentDialogResultToBoolean(ContentDialogResult? result)
            => (result ?? ContentDialogResult.None) == ContentDialogResult.None ? (bool?)null : result == ContentDialogResult.Primary;
    }
}
