using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
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
                Status = ProductStatus.InStock,
                CreatedDate = Timestamp.FromDateTime(product.CreatedDate)
            };

            return productModel;
        }
    }
}
