using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using ServiceReference;
using DotNetCoreWCF.Proxies.Factories;
using DotNetCoreWCF.Client;
using DotNetCoreWCF.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MyWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HttpClient client;
        private TestServiceClient wcfClient;
        private IEmployeeClientDemo wcfEmployeeClient;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(env))
            {
                env = "Development";
            }

            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables();
            IConfigurationRoot configuration = builder.Build();
            var services = new ServiceCollection();
            services.AddTransient<EmployeeClientDemo>();
            services.RegisterEmployeeService(configuration);
            var provider = services.BuildServiceProvider();

            client = new HttpClient();
            wcfClient = new TestServiceClient();
            wcfEmployeeClient = provider.GetService<EmployeeClientDemo>();
            return base.StartAsync(cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            client.Dispose();
            wcfClient.Abort();
            _logger.LogInformation("The service has been stopped.");
            return base.StopAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            client.BaseAddress = new Uri("https://localhost:44313/api/values/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            while (!stoppingToken.IsCancellationRequested)
            {
                var urlParams = DateTimeOffset.Now.Second;
                HttpResponseMessage response = client.GetAsync(urlParams.ToString()).Result;
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                if (response.IsSuccessStatusCode)
                {
                    var object1 = response.Content.ReadAsStringAsync(stoppingToken).Result.Trim('"');
                    _logger.LogInformation("Web service via rest api        : {0}", object1);
                }
                else
                {
                    _logger.LogError("The website is down");
                }
                var object2 = await wcfClient.GetDataAsync(urlParams);
                _logger.LogInformation("Web service directly            : {0}", object2);

                var object3 = wcfEmployeeClient.Run();
                _logger.LogInformation("Employee Web service directly   : {0}", object3);
                _logger.LogInformation("*******************************");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
