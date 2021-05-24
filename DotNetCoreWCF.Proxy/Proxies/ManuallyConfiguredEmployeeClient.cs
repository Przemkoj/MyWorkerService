using System;
using System.Collections.Generic;
using System.Text;
using DotNetCoreWCF.Contracts.Interfaces;
using DotNetCoreWCF.Contracts.Model.Employees;
using Microsoft.Extensions.Logging;

namespace DotNetCoreWCF.Proxies
{
	public class ManuallyConfiguredEmployeeClient : IEmployeeClient, IDisposable
	{
		public ManuallyConfiguredEmployeeClient(System.ServiceModel.ChannelFactory<IEmployeeService> channelFactory)
		{
			Channel = channelFactory.CreateChannel();
			// https://blogs.msdn.microsoft.com/wenlong/2007/10/25/best-practice-always-open-wcf-client-proxy-explicitly-when-it-is-shared/
			((System.ServiceModel.IClientChannel)Channel).Open();
		}

		public IEmployeeService Channel { get; }

		public DeleteEmployeeResponse Delete(DeleteEmployeeRequest request)
		{
			return Channel.Delete(request);
		}

		public EmployeeResponse Get(EmployeeRequest request)
		{
			return Channel.Get(request);
		}

		public Employee UpdateEmployee(Employee employee)
		{
			return Channel.UpdateEmployee(employee);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					var client = ((System.ServiceModel.IClientChannel)Channel);
					try
					{
						client.Close();
					}
					catch (System.ServiceModel.CommunicationException e)
					{
						client.Abort();
					}
					catch (TimeoutException e)
					{
						client.Abort();
					}
					catch (Exception e) { client.Abort(); throw; }

				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
