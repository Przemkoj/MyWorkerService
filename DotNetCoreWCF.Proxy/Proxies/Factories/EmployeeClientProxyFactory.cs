using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using DotNetCoreWCF.Contracts.Interfaces;
using DotNetCoreWCF.Contracts.Model.Configuration;
using Microsoft.Extensions.Logging;

namespace DotNetCoreWCF.Proxies.Factories
{
	public interface IEmployeeClientProxyFactory
	{
		IEmployeeClient GetClient(ClientType? clientType = null);
	}

	public enum ClientType
	{
		Default,
		ChannelFactory,
		ManualBindings
	}

	public class EmployeeClientProxyFactory : IEmployeeClientProxyFactory
	{
		private readonly IClientProxySettings _proxySettings;
		private Lazy<ChannelFactory<IEmployeeService>> _channelFactory;

		public EmployeeClientProxyFactory(IClientProxySettings proxySettings)
		{
			_proxySettings = proxySettings;
			_channelFactory = new Lazy<System.ServiceModel.ChannelFactory<IEmployeeService>>(() =>
			{
				var binding = _proxySettings.GetNetTcpBinding();
				var endpointAddress = _proxySettings.GetEndpointAddress();

				var factory = new System.ServiceModel.ChannelFactory<IEmployeeService>(binding, endpointAddress);
				factory.Endpoint.TrySetMaxItemsInObjectGraph(_proxySettings);

				return factory;
			});
		}

		public IEmployeeClient GetClient(ClientType? clientType = null)
		{
			bool isProxyEnabled = _proxySettings?.Enabled ?? false;

			if (isProxyEnabled && clientType == ClientType.ManualBindings)
			{
				var binding = _proxySettings.GetNetTcpBinding();
				var endpointAddress = _proxySettings.GetEndpointAddress();

				var client = new EmployeeClient(binding, endpointAddress);
				client.TrySetMaxItemsInObjectGraph(_proxySettings);

				return client;
			}

			if (isProxyEnabled && clientType == ClientType.ChannelFactory)
				return new ManuallyConfiguredEmployeeClient(_channelFactory.Value);

			// this does not work in netcore mode since this relies on automatic configuration
			return new EmployeeClient();
		}
	}
}