﻿using FluentValidation;

namespace FastFood.Application.Dish
{
    public class DishDtoValidator : AbstractValidator<DishDto>
    {
        public DishDtoValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .NotNull()
                .MinimumLength(3)
                .MaximumLength(45);

            RuleFor(c => c.Description)
                .NotEmpty()
                .NotNull()
                .MinimumLength(3)
                .MaximumLength(500);

            RuleFor(c => c.BasePrize)
                .NotEmpty()
                .NotNull();
                

            RuleFor(c => c.BaseCaloricValue)
                .NotEmpty()
                .NotNull()
                .GreaterThanOrEqualTo(1);

            RuleFor(c => c.IsAvilable)
                .NotNull();

            RuleFor(c => c.AllowedCustomization)
                .NotNull();
        }
    }
}