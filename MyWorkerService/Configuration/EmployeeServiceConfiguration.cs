using System;
using System.Collections.Generic;
using System.Text;
using DotNetCoreWCF.Client.Configuration.Settings;
using DotNetCoreWCF.Client.Model.Configuration;
using DotNetCoreWCF.Contracts.Model.Configuration;
using DotNetCoreWCF.Proxies.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetCoreWCF.Client.Configuration
{
	internal static class DemoConstants
	{
		public const string AutomaticConfigurationEmployeeClientDemo = "AutomaticConfigurationEmployeeClientDemo";
		public const string EmployeeClientDemo = "EmployeeClientDemo";
		public const string FactoryEmployeeClientDemo = "FactoryEmployeeClientDemo";
		public const string WcfGeneratedServiceDemo = "WcfGeneratedServiceDemo";
	} 

	public static class EmployeeServiceConfiguration
	{
		public static IServiceCollection RegisterEmployeeService(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<ServiceSettings>(Settings.ServiceSettingConstants.EmployeeService,
				configuration.GetSection("Services:NetTcp:EmployeeService"));

			services.Configure<ServiceSettings>(configuration.GetSection("Services:NetTcp:Default"));

			services.AddSingleton<IEmployeeClientProxyFactory>(provider =>
			{
				var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptionsSnapshot<ServiceSettings>>();
				var proxySettings = new ClientProxySettings(ServiceSettingConstants.EmployeeService, options);

				return new EmployeeClientProxyFactory(proxySettings);
			});

			services.AddTransient<EmployeeClientDemo>();
			services.AddTransient<Func<string, IEmployeeClientDemo>>(provider => key =>
			{
					return provider.GetService<EmployeeClientDemo>();
			});

			return services;
		}
	}
}
