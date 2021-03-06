﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Models;
using ShoppingCartGrpc.Protos;

namespace ShoppingCartGrpc.Services
{
    [Authorize]
    public class ShoppingCartService : ShoppingCartProtoService.ShoppingCartProtoServiceBase
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly DiscountService _discountService;
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly IMapper _mapper;

        public ShoppingCartService(ShoppingCartContext shoppingCartContext, DiscountService discountService, ILogger<ShoppingCartService> logger, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
            _shoppingCartContext = shoppingCartContext ?? throw new ArgumentNullException(nameof(shoppingCartContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public override async Task<ShoppingCartModel> GetShoppingCart(GetShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartContext.ShoppingCart.FirstOrDefaultAsync(p => p.UserName == request.Username);
            if(shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with UserName={request.Username}"));
            }

            return _mapper.Map<ShoppingCartModel>(shoppingCart);
        }

        public override async Task<ShoppingCartModel> CreateShoppingCart(ShoppingCartModel request, ServerCallContext context)
        {
            var shoppingCart = _mapper.Map<ShoppingCart>(request);
            bool isExist = await _shoppingCartContext.ShoppingCart.AnyAsync(s => s.UserName == shoppingCart.UserName);
            if(isExist)
            {
                _logger.LogError("Invalid username for ShoppingCart creation. Username: {userName}", shoppingCart.UserName);
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with username : {request.Username} is already exist."));
            }

            _shoppingCartContext.ShoppingCart.Add(shoppingCart);

            await _shoppingCartContext.SaveChangesAsync();

            _logger.LogInformation("ShoppingCart is successfully created. Username: {userName}", shoppingCart.UserName);

            return _mapper.Map<ShoppingCartModel>(shoppingCart);
        }

        [AllowAnonymous]
        public override async Task<RemoveItemFromShoppingCartResponse> RemoveItemFromShoppingCart(RemoveItemFromShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartContext.ShoppingCart.FirstOrDefaultAsync(p => p.UserName == request.Username);
            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with UserName={request.Username}"));
            }

            ShoppingCartItem removeCartItem = shoppingCart.Items.FirstOrDefault(p => p.ProductId == request.RemoveCartItem.ProductId);
            if(removeCartItem == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"CartItem with ProductId={request.RemoveCartItem.ProductId}"));
            }

            shoppingCart.Items.Remove(removeCartItem);

            int removeCount = await _shoppingCartContext.SaveChangesAsync();

            return new RemoveItemFromShoppingCartResponse { Success = removeCount > 0 };
        }

        [AllowAnonymous]
        public override async Task<AddItemToShoppingCartResponse> AddItemToShoppingCart(IAsyncStreamReader<AddItemToShoppingCartRequest> requestStream, ServerCallContext context)
        {

            while(await requestStream.MoveNext())
            {
                var shoppingCart = await _shoppingCartContext.ShoppingCart.FirstOrDefaultAsync(p => p.UserName == requestStream.Current.Username);
                if (shoppingCart == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with UserName={requestStream.Current.Username}"));
                }

                var newAddedCartItem = _mapper.Map<ShoppingCartItem>(requestStream.Current.NewCartItem);
                var cartItem = shoppingCart.Items.FirstOrDefault(p => p.ProductId == newAddedCartItem.ProductId);
                if(cartItem != null)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    var discount = await _discountService.GetDiscount(requestStream.Current.DiscountCode);
                    newAddedCartItem.Price -= discount.Amount;
                    shoppingCart.Items.Add(newAddedCartItem);
                }
            }

            var insertCount = await _shoppingCartContext.SaveChangesAsync();
            var response = new AddItemToShoppingCartResponse
            {
                InsertCount = insertCount,
                Success = insertCount > 0
            };

            return response;
        }
    }
}
