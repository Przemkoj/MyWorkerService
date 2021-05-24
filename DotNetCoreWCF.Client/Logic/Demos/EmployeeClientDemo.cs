using System;
using DotNetCoreWCF.Proxies.Factories;
using Microsoft.Extensions.Logging;

namespace DotNetCoreWCF.Client
{
	public class EmployeeClientDemo : IEmployeeClientDemo
	{
		private readonly ILogger _logger;
		private readonly IEmployeeClientProxyFactory _clientFactory;

		public EmployeeClientDemo(IEmployeeClientProxyFactory clientFactory, ILogger logger)
		{
			_clientFactory = clientFactory;
			_logger = logger;
		}

		public void Run()
		{
            DotNetCoreWCF.Contracts.Model.Employees.EmployeeResponse response = null;

            try
            {
                using (var client = _clientFactory.GetClient(ClientType.ChannelFactory))
                {
                    response = client.Get(new DotNetCoreWCF.Contracts.Model.Employees.EmployeeRequest { ActiveOnly = true });
                }

                _logger.LogInformation($"Employee web service directly  : {response?.Employees.Length}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oh noes!");
            }
        }
	}
}