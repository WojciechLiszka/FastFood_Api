﻿using FastFood.Application.Dish;
using FastFood.Application.Dish.Command.CreateDish;
using FastFood.Application.Dish.Command.DeleteDish;
using FastFood.Application.Dish.Command.UpdateDish;
using FastFood.Application.Dish.Queries;
using FastFood.Application.Dish.Queries.GedDishesFromRestaurant;
using FastFood.Application.Dish.Queries.GetDishById;
using FastFood.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastFood.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class DishController : Controller
    {
        private readonly IMediator _mediator;

        public DishController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("restaurant/{restaurantid}/dish")]
        [Authorize(Roles ="Admin,Owner")]
        
        public async Task<ActionResult<string>> Create([FromRoute] int restaurantid, [FromBody] DishDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var request = new CreateDishCommand()
            {
                RestaurantId = restaurantid,
                Name = dto.Name,
                Description = dto.Description,

                BasePrize = dto.BasePrize,
                BaseCaloricValue = dto.BaseCaloricValue,

                AllowedCustomization = dto.AllowedCustomization,
                IsAvilable = dto.IsAvilable,
            };

            var id = await _mediator.Send(request);

            return Created($"/api/dish/{id}", null);
        }

        [HttpGet]
        [Route("dish/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<GetDishDto>> GetById([FromRoute] int id)
        {
            var request = new GetDishByIdQuery()
            {
                DishId = id
            };

            var dto = await _mediator.Send(request);

            return Ok(dto);
        }

        [HttpPut]
        [Route("dish/{id}")]
        [Authorize (Roles = "Admin,Owner")]
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] DishDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var request = new UpdateDishCommand()
            {
                DishId = id,
                Name = dto.Name,
                Description = dto.Description,

                BasePrize = dto.BasePrize,
                BaseCaloricValue = dto.BaseCaloricValue,

                AllowedCustomization = dto.AllowedCustomization,
                IsAvilable = dto.IsAvilable
            };

            await _mediator.Send(request);

            return Ok();
        }

        [HttpGet]
        [Route("restaurant/{restaurantid}/dish")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<GetDishDto>>> GetFromRestaurant([FromRoute] int restaurantid, [FromQuery] PagedResultDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var request = new GetDishesFromRestaurantQuery()
            {
                RestaurantId = restaurantid,
                SearchPhrase = dto.SearchPhrase,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                SortBy = dto.SortBy,
                SortDirection = dto.SortDirection
            };

            var result = await _mediator.Send(request);

            return Ok(result);
        }

        [HttpDelete]
        [Route("dish/{id}")]
        [Authorize(Roles ="Admin,Owner")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var request = new DeleteDishCommand()
            {
                id = id
            };

            await _mediator.Send(request);

            return NoContent();
        }
    }
}