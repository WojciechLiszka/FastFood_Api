﻿using MediatR;

namespace OrderEat.Application.Allergen.Commands.DeleteAllergen
{
    public class DeleteAllergenCommand : IRequest
    {
        public int Id { get; set; }
    }
}