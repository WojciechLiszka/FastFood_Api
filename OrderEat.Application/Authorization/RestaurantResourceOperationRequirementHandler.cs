﻿using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OrderEat.Application.Authorization
{
    public class RestaurantResourceOperationRequirementHandler : AuthorizationHandler<RestaurantResourceOperationRequirement, Domain.Entities.Restaurant>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RestaurantResourceOperationRequirement requirement, Domain.Entities.Restaurant resource)
        {
            if (requirement.ResourceOperation is ResourceOperation.Create or ResourceOperation.Read)
            {
                context.Succeed(requirement);
            }

            var userId = int.Parse(context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var userRole = context.User.FindFirst(c => c.Type == ClaimTypes.Role).Value;

            if (resource.CreatedById == userId || userRole == "Admin")
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}