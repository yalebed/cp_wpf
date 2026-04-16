using Microsoft.EntityFrameworkCore;
using RentalCarApplication.Core.Model;
using RentalCarApplication.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentalCarApplication.EntityFramework.Repositories
{
    internal sealed class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationContext _context;

        public ReviewRepository(ApplicationContext context)
        {
            _context = context;
        }

        public Review Create(Review entity)
        {
            return _context.Set<Review>().Add(entity).Entity;
        }

        public bool Delete(int id)
        {
            var entity = _context.Set<Review>().Find(id);
            if (entity == null)
            {
                return false;
            }

            _context.Set<Review>().Remove(entity);
            return true;
        }

        public bool ExistsForOrder(int orderId)
        {
            return _context.Set<Review>().Any(x => x.OrderId == orderId);
        }

        public Review Find(int id)
        {
            return _context.Set<Review>().Find(id);
        }

        public IEnumerable<Review> FindAll()
        {
            return _context.Set<Review>()
                .Include(x => x.User)
                .Include(x => x.Car)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public async Task<IEnumerable<Review>> FindAllAsync()
        {
            return await _context.Set<Review>()
                .Include(x => x.User)
                .Include(x => x.Car)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review> FindAsync(int id)
        {
            return await _context.Set<Review>()
                .Include(x => x.User)
                .Include(x => x.Car)
                .FirstOrDefaultAsync(x => x.ReviewId == id);
        }

        public IEnumerable<Review> FindByCarId(int carId)
        {
            return _context.Set<Review>()
                .Include(x => x.User)
                .Where(x => x.CarId == carId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public Review Update(int id, Review entity)
        {
            var entry = _context.Set<Review>().First(x => x.ReviewId == entity.ReviewId);
            _context.Entry(entry).CurrentValues.SetValues(entity);
            return entry;
        }
    }
}
