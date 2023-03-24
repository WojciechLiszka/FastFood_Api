﻿using FastFood.Application.Restaurant.Commands.CreateRestaurant;
using FastFood.Application.Restaurant.Commands.DeleteRestaurant;
using FastFood.Application.Restaurant.Commands.UpdateRestaurant;
using FastFood.Application.Restaurant.Queries;
using FastFood.Application.Restaurant.Queries.GetRestaurantById;
using FastFood.Application.Restaurant.Queries.GetRestaurants;
using FastFood.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastFood.Api.Controllers
{
    [ApiController]
    [Route("/api/restaurant")]
    [Authorize]
    public class RestaurantController : Controller
    {
        private readonly IMediator _mediator;

        public RestaurantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<string>> Create([FromBody] CreateRestaurantCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var id = await _mediator.Send(command);
            return Created($"api/restaurant/{id}", null);
        }

        [HttpGet]
        [Route("/api/restaurant/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<GetRestaurantDto>> GetById([FromRoute] int id)
        {
            var request = new GetRestaurantByIdQuery()
            {
                Id = id
            };
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<GetRestaurantDto>>> Get([FromQuery] GetRestaurantsQuery query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpDelete]
        [Route("/api/restaurant/{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var request = new DeleteRestaurantCommand()
            {
                Id = id
            };

            await _mediator.Send(request);

            return NoContent();
        }

        [HttpPut]
        [Route("/api/restaurant/{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateRestaurantDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var request = new UpdateRestaurantCommand()
            {
                Id = id,
                Description = dto.Description,
                ContactNumber = dto.ContactNumber,
                Email = dto.Email,
                Country = dto.Country,
                City = dto.City,
                Street = dto.Street,
                ApartmentNumber = dto.ApartmentNumber
            };

            await _mediator.Send(request);

            return Ok();
        }
    }
}