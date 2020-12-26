using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Waiting for server is running.");
            Thread.Sleep(3000);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using var channel = GrpcChannel.ForAddress(_config.GetValue<string>("WorkerService:ServiceUrl"));
                var client = new ProductProtoService.ProductProtoServiceClient(channel);

                Console.WriteLine("AddProductAsync started...");
                var response = await client.AddProductAsync(new AddProductRequest
                {
                    Product = new ProductModel
                    {
                        Name = _config.GetValue<string>("WorkerService:ProductName") + DateTime.Now,
                        Description = "New Red Phone Mi10T",
                        Price = 699,
                        Status = ProductStatus.InStock,
                        CreatedDate = Timestamp.FromDateTime(DateTime.UtcNow)
                    }
                });

                Console.WriteLine("AddProductAsync response: " + response.ToString());

                await Task.Delay(_config.GetValue<int>("WorkerService:TaskInterval"), stoppingToken);
            }
        }
    }
}
