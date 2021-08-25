using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Infrastructures
{
    [ServiceDescription(typeof(VagabondK.App.ViewProvider))]
    public class ViewProvider : VagabondK.App.ViewProvider
    {
        public ViewProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override bool CanReload(Type viewType) => viewType != null 
            && !typeof(ContentDialog).IsAssignableFrom(viewType);
    }
}
