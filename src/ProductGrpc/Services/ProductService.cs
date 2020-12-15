﻿using AutoMapper;
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
        private readonly IMapper _mapper;

        public ProductService(ProductContext productContext, ILogger<ProductService> logger, IMapper mapper)
        {
            _productContext = productContext;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

            return _mapper.Map<ProductModel>(product);            
        }

        public override async Task GetAllProducts(GetAllProductsRequest request, IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            var productList = await _productContext.Product.ToListAsync();

            foreach (var product in productList)        
            {
                var productModel = _mapper.Map<ProductModel>(product);
                await responseStream.WriteAsync(productModel);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);

            await _productContext.Product.AddAsync(product);
            await _productContext.SaveChangesAsync();

            return _mapper.Map<ProductModel>(product);            
        }
    }
}
