using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer
{
    class ServiceProviderContractResolver : DefaultContractResolver
    {
        private readonly IServiceProvider serviceProvider;

        public ServiceProviderContractResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            JsonObjectContract contract = base.CreateObjectContract(objectType);
            contract.DefaultCreator = () => serviceProvider.GetService(objectType);

            return contract;
        }
    }
}
