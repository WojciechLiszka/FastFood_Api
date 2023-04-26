﻿using FastFood.Domain.Entities;
using FastFood.Domain.Interfaces;
using FastFood.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FastFoodDbContext _dbContext;

        public OrderRepository(FastFoodDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(Order order)
        {
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Order?> GetById(int id)
        {
            var order= await _dbContext.Orders
                .Include(o => o.OrderedDishes)
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();
            return order;
        }
    }
}