using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace VagaModbusAnalyzer.Infrastructures
{
    [ServiceDescription(typeof(IStringLocalizer), ServiceLifetime.Singleton)]
    public class StringLocalizer : IStringLocalizer
    {
        public StringLocalizer()
        {
            resourceLoader = ResourceLoader.GetForCurrentView();
        }

        private readonly ResourceLoader resourceLoader = null;

        //private static Lazy<StringLocalizer> instance = new Lazy<StringLocalizer>();
        //public static StringLocalizer Instance
        //{
        //    get
        //    {
        //        return instance.Value;
        //    }
        //}


        public LocalizedString this[string name]
        {
            get
            {
                var result = resourceLoader.GetString(name);
                if (string.IsNullOrEmpty(result))
                    result = name;
                return new LocalizedString(name, result);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var result = resourceLoader.GetString(name);
                if (string.IsNullOrEmpty(result))
                    result = name;
                else
                    result = string.Format(result, arguments);
                return new LocalizedString(name, result);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        public static string GetString(string resource)
        {
            var result = ResourceLoader.GetForCurrentView().GetString(resource);
            if (string.IsNullOrEmpty(result))
                result = resource;
            return result;
        }
    }
}
