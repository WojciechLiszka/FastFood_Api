﻿using Domain.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OrderEat.Application.Authorization;
using OrderEat.Domain.Exceptions;
using OrderEat.Domain.Interfaces;

namespace OrderEat.Application.Order.Command.RealizeOrder
{
    public class RealizeOrderCommandHandler : IRequestHandler<RealizeOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserContextService _userContextService;
        private readonly IAuthorizationService _authorizationService;

        public RealizeOrderCommandHandler(IOrderRepository orderRepository, IUserContextService userContextService, IAuthorizationService authorizationService)
        {
            _orderRepository = orderRepository;
            _userContextService = userContextService;
            _authorizationService = authorizationService;
        }

        public async Task Handle(RealizeOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetById(request.Orderid);

            if (order == null)
            {
                throw new NotFoundException("Order not found");
            }
            var authorizationResult = await _authorizationService.AuthorizeAsync(_userContextService.User, order, new OrderRsourceOperationRequirement(ResourceOperation.Update));
            if (order.OrderedDishes.IsNullOrEmpty())
            {
                throw new BadRequestException("You need to add dishes to order");
            }
            if (order.Status == Domain.Models.OrderStatus.Ordered)
            {
                throw new BadRequestException("This order is already realized");
            }
            if (order.Status == Domain.Models.OrderStatus.Realized)
            {
                throw new BadRequestException("This order is finished");
            }
            order.OrderDate = DateTime.Now;
            order.Status = Domain.Models.OrderStatus.Ordered;
            await _orderRepository.Commit();
        }
    }
}