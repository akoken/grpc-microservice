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

            Console.WriteLine("GetProductAsync started...");
            var response = await client.GetProductAsync(new GetProductRequest
            {
                ProductId = 1
            });

            Console.WriteLine("GetProductAsync response: " + response.ToString());

            Console.WriteLine("GetAllProducts started...");
            using (var clientData = client.GetAllProducts(new GetAllProductsRequest()))
            {
                while (await clientData.ResponseStream.MoveNext(new CancellationToken()))
                {
                    ProductModel currentProduct = clientData.ResponseStream.Current;
                    Console.WriteLine(currentProduct);
                }
            }

            Console.Read();
        }
    }
}
