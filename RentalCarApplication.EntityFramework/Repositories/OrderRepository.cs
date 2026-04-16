using RentalCarApplication.Core.Repositories;
using RentalCarApplication.Core.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace RentalCarApplication.EntityFramework.Repositories
{
    internal sealed class OrderRepository : IOrderRepository
    {
        private readonly ApplicationContext _context;
        public OrderRepository(ApplicationContext context)
        {
            _context = context;
        }

        public Order Create(Order entity)
        {
            var order = _context.Set<Order>().Add(entity).Entity;
            return order;
        }

        public bool Delete(int id)
        {
            var entity = _context.Set<Order>().Find(id);
            if (entity != null)
            {
                _context.Set<Order>().Remove(entity);
                return true;
            }
            else
                return false;
        }
        public bool CheckUserOrders(string email)
        {
            if (_context.Set<Order>().FirstOrDefault(e => e.Email == email && e.Status==null) != null)
                return false;
            else
                return true;
        }

        public bool HasOverlappingOrder(int carId, DateTime rentDate, DateTime returnDate, int? excludeOrderId = null)
        {
            return _context.Set<Order>().Any(e =>
                e.CarId == carId &&
                e.Status != false &&
                (!excludeOrderId.HasValue || e.OrderId != excludeOrderId.Value) &&
                rentDate < e.ReturnDate &&
                returnDate > e.RentDate);
        }

        public Order Find(int id)
        {
            var entity = _context.Set<Order>().Find(id);
            return entity;
        }

        public IEnumerable<Order> FindAll()
        {
            IEnumerable<Order> entities = _context.Set<Order>().ToList();
            return entities;
        }

        public async Task<IEnumerable<Order>> FindAllAsync()
        {
            IEnumerable<Order> entities = await _context.Set<Order>().ToListAsync();
            return entities;
        }

        public async Task<Order> FindAsync(int id)
        {
            var entity = await _context.Set<Order>().FirstOrDefaultAsync((e) => e.OrderId == id);
            return entity;
        }

        public Order Update(int id, Order entity)
        {
            var entry = _context.Set<Order>().First(e => e.OrderId == entity.OrderId);
            _context.Entry(entry).CurrentValues.SetValues(entity);

            return entry;
        }

        
    }
}
