using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Models;
using ProductGrpc.Protos;
using System;
using System.Threading.Tasks;

namespace ProductGrpc.Services
{
    public class ProductService : ProductProtoService.ProductProtoServiceBase
    {
        private readonly ProductContext _productContext;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ProductContext productContext, ILogger<ProductService> logger)
        {
            _productContext = productContext;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<Empty> Test(Empty request, ServerCallContext context)
        {
            return base.Test(request, context);
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            var product = await _productContext.Product.FindAsync(request.ProductId);
            if (product == null)
            {
                //throw grpc exception
            }

            var productModel = new ProductModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = (float)product.Price,
                Status = Protos.ProductStatus.InStock,
                CreatedDate = Timestamp.FromDateTime(product.CreatedDate)
            };

            return productModel;
        }

        public override async Task GetAllProducts(GetAllProductsRequest request, IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            var productList = await _productContext.Product.ToListAsync();

            foreach (var product in productList)        
            {
                var productModel = new ProductModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = (float)product.Price,
                    Status = Protos.ProductStatus.InStock,
                    CreatedDate = Timestamp.FromDateTime(product.CreatedDate)
                };

                await responseStream.WriteAsync(productModel);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var product = new Product
            {
                ProductId = request.Product.ProductId,
                Name = request.Product.Name,
                Description = request.Product.Description,
                Price = (decimal)request.Product.Price,
                Status = (Models.ProductStatus)request.Product.Status,
                CreatedDate = request.Product.CreatedDate.ToDateTime()
            };

            await _productContext.Product.AddAsync(product);
            await _productContext.SaveChangesAsync();

            var productModel = new ProductModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = (float)product.Price,
                Status = (Protos.ProductStatus)product.Status,
                CreatedDate = Timestamp.FromDateTime(product.CreatedDate)
            };

            return productModel;
        }
    }
}
