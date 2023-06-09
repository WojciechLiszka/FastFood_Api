﻿using AutoMapper;
using OrderEat.Domain.Interfaces;
using MediatR;

namespace OrderEat.Application.Allergen.Commands.CreateAllergen
{
    public class CreateAllergenCommandHandler : IRequestHandler<CreateAllergenCommand, int>
    {
        private readonly IMapper _mapper;
        private readonly IAllergenRepository _repository;

        public CreateAllergenCommandHandler(IMapper mapper, IAllergenRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<int> Handle(CreateAllergenCommand request, CancellationToken cancellationToken)
        {
            var allergen = _mapper.Map<Domain.Entities.Allergen>(request);
            var id = await _repository.Create(allergen);
            return id;
        }
    }
}