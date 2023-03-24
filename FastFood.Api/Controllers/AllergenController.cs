﻿using FastFood.Application.Allergen.Commands.CreateAllergen;
using FastFood.Application.Allergen.Commands.DeleteAllergen;
using FastFood.Application.Allergen.Commands.UpdateAllergen;
using FastFood.Application.Allergen.Queries.GetAllergenById;
using FastFood.Application.Allergen.Queries.GetAllergens;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastFood.Api.Controllers
{
    [ApiController]
    [Route("api/allergen")]
    [Authorize]
    public class AllergenController : Controller
    {
        private readonly IMediator _mediator;

        public AllergenController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult<string>> Create([FromBody] CreateAllergenCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var id = await _mediator.Send(command);

            return Created($"api/allergen/{id}", null);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<AllergenDto>>> Get([FromQuery] GetAllergensQuery query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{id}")]
        public async Task<ActionResult<AllergenDto>> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetAllergenByIdQuery()
            {
                Id = id
            });

            return Ok(result);
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateAllergenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var Request = new UpdateAllergenCommand()
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description
            };

            await _mediator.Send(Request);

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize (Roles = "Admin")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            await _mediator.Send(new DeleteAllergenCommand()
            {
                Id = id
            });

            return NoContent();
        }
    }
}