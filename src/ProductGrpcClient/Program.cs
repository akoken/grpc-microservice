using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProductGrpc.Protos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductGrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Waiting for server is running.");
            Thread.Sleep(3000);

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new ProductProtoService.ProductProtoServiceClient(channel);

            await GetProductAsync(client);
            await GetAllProducts(client);
            await AddProductAsync(client);

            await UpdateProductAsync(client);
            await DeleteProductAsync(client);
            Console.Read();
        }

        private static async Task GetAllProducts(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("GetAllProducts started...");
            using var clientData = client.GetAllProducts(new GetAllProductsRequest());
            await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(responseData);
            }
        }

        private static async Task GetProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("GetProductAsync started...");
            var response = await client.GetProductAsync(new GetProductRequest
            {
                ProductId = 1
            });

            Console.WriteLine("GetProductAsync response: " + response.ToString());
        }

        private static async Task AddProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("AddProductAsync started...");
            var response = await client.AddProductAsync(new AddProductRequest
            {
                Product = new ProductModel
                {
                    Name = "Red",
                    Description = "New Red Phone Mi10T",
                    Price = 699,
                    Status = ProductStatus.InStock,
                    CreatedDate = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            });

            Console.WriteLine("AddProductAsync response: " + response.ToString());
        }

        private static async Task UpdateProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("UpdateProductAsync started...");

            var response = await client.UpdateProductAsync(new UpdateProductRequest
            {
                Product = new ProductModel
                {
                    ProductId = 1,
                    Name = "Red",
                    Description = "New Red Phone Mi10T",
                    Price = 699,
                    Status = ProductStatus.InStock,
                    CreatedDate = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            });

            Console.WriteLine("UpdateProductAsync response: " + response.ToString());
        }

        private static async Task DeleteProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("DeleteProductAsync started...");
            var response = await client.DeleteProductAsync(new DeleteProductRequest
            {
                ProductId = 3
            });

            Console.WriteLine("DeleteProductAsync response: " + response.ToString());
        }
    }
}
