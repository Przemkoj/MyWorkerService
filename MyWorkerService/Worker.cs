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

namespace MyWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HttpClient client;
        private TestServiceClient wcfClient;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            client = new HttpClient();
            wcfClient = new TestServiceClient();
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
                    _logger.LogInformation("Web service via rest api    : {0}", object1);                  
                }
                else
                {
                    _logger.LogError("The website is down");
                }
                var object2 = await wcfClient.GetDataAsync(urlParams);
                    _logger.LogInformation("Web service directly        : {0}", object2);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
