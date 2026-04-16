using RentalCarApplication.Core.Model;
using System.Collections.Generic;

namespace RentalCarApplication.Core.Repositories
{
    public interface IReviewRepository : IRepository<Review, int>
    {
        IEnumerable<Review> FindByCarId(int carId);
        bool ExistsForOrder(int orderId);
    }
}
