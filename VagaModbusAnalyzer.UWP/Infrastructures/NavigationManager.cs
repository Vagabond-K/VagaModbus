using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagabondK.App;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Infrastructures
{
    [ServiceDescription]
    public class NavigationManager : INavigationManager
    {
        public NavigationManager(PageContext pageContext)
        {
            this.pageContext = pageContext;
        }

        private readonly PageContext pageContext;

        public bool CanGoBack => throw new NotImplementedException();

        public void ClearBackStack()
        {
            throw new NotImplementedException();
        }

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public Task<PageContext> Navigate(Type viewModelType, Type viewType, string title, Action<object, object> initializer, out object viewModel)
        {
            throw new NotImplementedException();
        }

        //private Frame GetNavigationFrame()
        //{
        //    if (this.pageContext?.View == null)
        //    {

        //    }
        //}


    }
}
