using System;
using DotNetCoreWCF.Proxies.Factories;
using Microsoft.Extensions.Logging;

namespace DotNetCoreWCF.Client
{
	public class EmployeeClientDemo : IEmployeeClientDemo
	{
		private readonly IEmployeeClientProxyFactory _clientFactory;

		public EmployeeClientDemo(IEmployeeClientProxyFactory clientFactory)
		{
			_clientFactory = clientFactory;
		}

		public int Run()
		{
			Contracts.Model.Employees.EmployeeResponse response = null;

			try
			{
				using (var client = _clientFactory.GetClient(ClientType.ChannelFactory))
				{
					response = client.Get(new Contracts.Model.Employees.EmployeeRequest { ActiveOnly = true });
				}
				return response.Employees.Length;
			}
			catch (Exception ex)
			{
			}
			return 0;
		}
	}
}