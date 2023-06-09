﻿using OrderEat.Domain.Entities;
using OrderEat.Domain.Interfaces;
using OrderEat.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace OrderEat.Infrastructure.Repositories
{
    public class AllergenRepository : IAllergenRepository
    {
        private readonly OrderEatDbContext _dbContext;

        public AllergenRepository(OrderEatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Commit()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> Create(Allergen allergen)
        {
            _dbContext.Allergens.Add(allergen);
            await _dbContext.SaveChangesAsync();
            return allergen.Id;
        }

        public async Task Delete(Allergen allergen)
        {
            _dbContext.Allergens.Remove(allergen);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<Allergen?> GetById(int id)
        {
            var result = await _dbContext.Allergens.FirstOrDefaultAsync(x => x.Id == id);

            return result;
        }

        public async Task<Allergen?> GetByName(string name)
        {
            var result = await _dbContext.Allergens.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());

            return result;
        }
        public IQueryable<Allergen> Search(string? phrase)
        {
            var allergens = _dbContext
                .Allergens
                .Where(b => phrase == null
                || b.Name.ToLower().Contains(phrase.ToLower())
                || b.Description.ToLower().Contains(phrase.ToLower()));
            return allergens;
        }
    }
}