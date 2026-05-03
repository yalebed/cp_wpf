using Microsoft.EntityFrameworkCore;
using RentalCarApplication.Core.Model;
using RentalCarApplication.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

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

            return false;
        }

        public bool CheckUserOrders(string email)
        {
            return !FindAll().Any(e => e.Email == email && e.Status == null);
        }

        public bool HasOverlappingOrder(int carId, DateTime rentDate, DateTime returnDate, int? excludeOrderId = null)
        {
            return FindAll().Any(e =>
                e.CarId == carId &&
                e.Status != false &&
                e.CompletedAt == null &&
                (!excludeOrderId.HasValue || e.OrderId != excludeOrderId.Value) &&
                rentDate < GetBusyUntil(e) &&
                returnDate > e.RentDate);
        }

        public Order Find(int id)
        {
            return FindAll().FirstOrDefault(x => x.OrderId == id);
        }

        public IEnumerable<Order> FindAll()
        {
            var result = new List<Order>();
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            try
            {
                if (shouldClose)
                {
                    connection.Open();
                }

                using var command = connection.CreateCommand();
                command.CommandText = @"
SELECT
    OrderId,
    City,
    RentDate,
    ReturnDate,
    Status,
    Price,
    CarId,
    Email,
    FrontPhotoPath,
    RearPhotoPath,
    SidePhotoPath,
    CompletedAt
FROM Orders";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new Order
                    {
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        City = reader.IsDBNull(reader.GetOrdinal("City")) ? string.Empty : reader.GetString(reader.GetOrdinal("City")),
                        RentDate = reader.GetDateTime(reader.GetOrdinal("RentDate")),
                        ReturnDate = reader.GetDateTime(reader.GetOrdinal("ReturnDate")),
                        Status = ConvertLegacyStatus(reader["Status"]),
                        Price = reader.GetDouble(reader.GetOrdinal("Price")),
                        CarId = reader.GetInt32(reader.GetOrdinal("CarId")),
                        Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                        FrontPhotoPath = reader.IsDBNull(reader.GetOrdinal("FrontPhotoPath")) ? null : reader.GetString(reader.GetOrdinal("FrontPhotoPath")),
                        RearPhotoPath = reader.IsDBNull(reader.GetOrdinal("RearPhotoPath")) ? null : reader.GetString(reader.GetOrdinal("RearPhotoPath")),
                        SidePhotoPath = reader.IsDBNull(reader.GetOrdinal("SidePhotoPath")) ? null : reader.GetString(reader.GetOrdinal("SidePhotoPath")),
                        CompletedAt = reader.IsDBNull(reader.GetOrdinal("CompletedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CompletedAt"))
                    });
                }
            }
            finally
            {
                if (shouldClose && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return result;
        }

        public Task<IEnumerable<Order>> FindAllAsync()
        {
            return Task.FromResult(FindAll());
        }

        public Task<Order> FindAsync(int id)
        {
            return Task.FromResult(Find(id));
        }

        public Order Update(int id, Order entity)
        {
            _context.Set<Order>().Update(entity);
            return entity;
        }

        private static bool? ConvertLegacyStatus(object statusValue)
        {
            if (statusValue == null || statusValue == DBNull.Value)
            {
                return null;
            }

            if (statusValue is bool boolValue)
            {
                return boolValue;
            }

            var stringValue = statusValue.ToString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            return stringValue switch
            {
                "1" => true,
                "0" => false,
                "True" => true,
                "False" => false,
                "Approved" => true,
                "Completed" => true,
                "Canceled" => false,
                "Pending" => null,
                _ => null
            };
        }

        private static DateTime GetBusyUntil(Order order)
        {
            return order.CompletedAt ?? order.ReturnDate;
        }
    }
}
