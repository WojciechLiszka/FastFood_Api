﻿using FluentValidation;

namespace FastFood.Application.Ingredient.Command
{
    public class IngredientDtoValidator : AbstractValidator<IngredientDto>
    {
        public IngredientDtoValidator()
        {
            RuleFor(c => c.Name)
               .NotNull()
               .NotEmpty()
               .MinimumLength(3)
               .MaximumLength(45);

            RuleFor(c => c.Description)
                .NotEmpty()
                .NotNull()
                .MinimumLength(3)
                .MaximumLength(500);

            RuleFor(c => c.Prize)
                .NotEmpty()
                .NotNull();
                
        }
    }
}