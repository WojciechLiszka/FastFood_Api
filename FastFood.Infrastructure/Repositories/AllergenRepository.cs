﻿using FastFood.Domain.Entities;
using FastFood.Domain.Interfaces;
using FastFood.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Infrastructure.Repositories
{
    public class AllergenRepository : IAllergenRepository
    {
        private readonly FastFoodDbContext _dbContext;

        public AllergenRepository(FastFoodDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(Allergen allergen)
        {
            _dbContext.Allergens.Add(allergen);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Allergen?> GetById(int id)
        {
            var result = await _dbContext.Allergens.FirstOrDefaultAsync(x => x.Id == id);

            return result;
        }

        
    }
}