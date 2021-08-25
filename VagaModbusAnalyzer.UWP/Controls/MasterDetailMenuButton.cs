using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Controls
{
    public class MasterDetailMenuButton : Button
    {
        public MasterDetailMenuButton()
        {
            DefaultStyleKey = typeof(MasterDetailMenuButton);
        }

        public object IconContent
        {
            get { return (object)GetValue(IconContentProperty); }
            set { SetValue(IconContentProperty, value); }
        }

        public static readonly DependencyProperty IconContentProperty =
            DependencyProperty.Register("IconContent", typeof(object), typeof(MasterDetailMenuButton), new PropertyMetadata(null));



        public double IconPanelWidth
        {
            get { return (double)GetValue(IconPanelWidthProperty); }
            set { SetValue(IconPanelWidthProperty, value); }
        }

        public static readonly DependencyProperty IconPanelWidthProperty =
            DependencyProperty.Register("IconPanelWidth", typeof(double), typeof(MasterDetailMenuButton), new PropertyMetadata(42d));



        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(MasterDetailMenuButton), new PropertyMetadata(false));

    }
}
