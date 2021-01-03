using System;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Protos;

namespace ShoppingCartGrpc.Services
{
    public class ShoppingCartService : ShoppingCartProtoService.ShoppingCartProtoServiceBase
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly IMapper _mapper;

        public ShoppingCartService(ShoppingCartContext shoppingCartContext, ILogger<ShoppingCartService> logger, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    }
}
