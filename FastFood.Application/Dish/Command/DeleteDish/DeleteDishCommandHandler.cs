﻿using Domain.Domain.Exceptions;
using FastFood.Application.Authorization;
using FastFood.Domain.Exceptions;
using FastFood.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace FastFood.Application.Dish.Command.DeleteDish
{
    public class DeleteDishCommandHandler : IRequestHandler<DeleteDishCommand>
    {
        private readonly IDishRepository _repository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContext;

        public DeleteDishCommandHandler(IDishRepository repository, IAuthorizationService authorizationService, IUserContextService userContext)
        {
            _repository = repository;
            _authorizationService = authorizationService;
            _userContext = userContext;
        }

        public async Task Handle(DeleteDishCommand request, CancellationToken cancellationToken)
        {
            var dish = await _repository.GetById(request.id);

            if (dish == null)
            {
                throw new NotFoundException("Dish not found");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(_userContext.User, dish.Restaurant, new ResourceOperationRequirement(ResourceOperation.Delete));

            if (!authorizationResult.Succeeded)
            {
                throw new ForbiddenException();
            }

            await _repository.Delete(dish);
        }
    }
}