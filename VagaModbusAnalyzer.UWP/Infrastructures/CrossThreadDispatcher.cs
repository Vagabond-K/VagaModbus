using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace VagaModbusAnalyzer.Infrastructures
{
    [ServiceDescription(typeof(ICrossThreadDispatcher), ServiceLifetime.Singleton)]
    public class CrossThreadDispatcher : ICrossThreadDispatcher
    {
        public CrossThreadDispatcher(Shell mainViewModel)
        {
            this.mainViewModel = mainViewModel;
        }

        private readonly Shell mainViewModel;

        public void Invoke(Action callback)
        {
            mainViewModel?.MainPage?.Dispatcher?.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => callback?.Invoke());
        }
    }
}
