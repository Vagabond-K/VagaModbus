using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VagabondK.App;
using VagaModbusAnalyzer.Infrastructures;
using VagaModbusAnalyzer.Views;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(ServiceLifetime.Singleton)]
    public class Settings : NotifyPropertyChangeObject
    {
        public Settings(ThemeManager themeManager, IDialog dialog)
        {
            ThemeManager = themeManager;
        }

        public ThemeManager ThemeManager { get; }

        public string AppName { get => Package.Current.DisplayName; }

        public string Version
        {
            get
            {
                var version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }

    }
}
