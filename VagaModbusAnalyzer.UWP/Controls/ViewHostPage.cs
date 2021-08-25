using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagabondK.App;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace VagaModbusAnalyzer.Controls
{
    public class ViewHostPage : Page
    {
        public ViewHostPage()
        {
            SetBinding(ContentProperty, new Binding { Path = new PropertyPath(nameof(PageContext.View)) });
        }

        

        //protected override void OnContentChanged(object oldContent, object newContent)
        //{
        //    base.OnContentChanged(oldContent, newContent);

        //    if (newContent is FrameworkElement frameworkElement)
        //        frameworkElement.SetBinding(DataContextProperty, new Binding { Path = new PropertyPath(nameof(PageContext.ViewModel)) });
        //}


        public static ViewCommands GetViewCommands(DependencyObject obj)
        {
            return (ViewCommands)obj.GetValue(ViewCommandsProperty);
        }

        public static void SetViewCommands(DependencyObject obj, ViewCommands value)
        {
            obj.SetValue(ViewCommandsProperty, value);
        }

        public static readonly DependencyProperty ViewCommandsProperty =
            DependencyProperty.RegisterAttached("ViewCommands", typeof(ViewCommands), typeof(ViewHostPage), new PropertyMetadata(null));



        public static ViewHostPage GetViewHostPage(DependencyObject obj)
        {
            return (ViewHostPage)obj.GetValue(ViewHostPageProperty);
        }

        public static void SetViewHostPage(DependencyObject obj, ViewHostPage value)
        {
            obj.SetValue(ViewHostPageProperty, value);
        }

        public static readonly DependencyProperty ViewHostPageProperty =
            DependencyProperty.RegisterAttached("ViewHostPage", typeof(ViewHostPage), typeof(ViewHostPage), new PropertyMetadata(null));


    }

    public class ViewCommands : DependencyObject
    {
        public ViewCommands()
        {
            PrimaryCommands = new ObservableCollection<ICommandBarElement>();
            SecondaryCommands = new ObservableCollection<ICommandBarElement>();
        }

        public ObservableCollection<ICommandBarElement> PrimaryCommands
        {
            get { return (ObservableCollection<ICommandBarElement>)GetValue(PrimaryCommandsProperty); }
            private set { SetValue(PrimaryCommandsProperty, value); }
        }

        public static readonly DependencyProperty PrimaryCommandsProperty =
            DependencyProperty.Register("PrimaryCommands", typeof(ObservableCollection<ICommandBarElement>), typeof(ViewCommands), null);


        public ObservableCollection<ICommandBarElement> SecondaryCommands
        {
            get { return (ObservableCollection<ICommandBarElement>)GetValue(SecondaryCommandsProperty); }
            private set { SetValue(SecondaryCommandsProperty, value); }
        }

        public static readonly DependencyProperty SecondaryCommandsProperty =
            DependencyProperty.Register("SecondaryCommands", typeof(ObservableCollection<ICommandBarElement>), typeof(ViewCommands), null);
    }

}
