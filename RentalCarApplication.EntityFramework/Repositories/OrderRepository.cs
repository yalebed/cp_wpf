using Microsoft.EntityFrameworkCore;
using RentalCarApplication.Core.Model;
using RentalCarApplication.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
            ExecuteOrderCommand(command =>
            {
                bool statusIsString = IsStatusStringColumn(command.Connection);
                command.CommandText = statusIsString
                    ? @"
                        INSERT INTO Orders
                            (RentDate, ReturnDate, Status, Price, FrontPhotoPath, RearPhotoPath, SidePhotoPath, CompletedAt, CarId, Email)
                        VALUES
                            (@RentDate, @ReturnDate, @StatusText, @Price, @FrontPhotoPath, @RearPhotoPath, @SidePhotoPath, @CompletedAt, @CarId, @Email)"
                                            : @"
                        INSERT INTO Orders
                            (RentDate, ReturnDate, Status, Price, FrontPhotoPath, RearPhotoPath, SidePhotoPath, CompletedAt, CarId, Email)
                        VALUES
                            (@RentDate, @ReturnDate, @StatusBit, @Price, @FrontPhotoPath, @RearPhotoPath, @SidePhotoPath, @CompletedAt, @CarId, @Email)";

                AddCommonOrderParameters(command, entity, statusIsString);
                command.ExecuteNonQuery();
            });

            return entity;
        }

        public bool Delete(int id)
        {
            return ExecuteOrderCommand(command =>
            {
                command.CommandText = "DELETE FROM Orders WHERE OrderId = @OrderId";
                AddParameter(command, "@OrderId", id);
                return command.ExecuteNonQuery() > 0;
            });
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
            ExecuteOrderCommand(command =>
            {
                bool statusIsString = IsStatusStringColumn(command.Connection);
                command.CommandText = statusIsString
                    ? @"
UPDATE Orders SET
    RentDate = @RentDate,
    ReturnDate = @ReturnDate,
    Status = @StatusText,
    Price = @Price,
    FrontPhotoPath = @FrontPhotoPath,
    RearPhotoPath = @RearPhotoPath,
    SidePhotoPath = @SidePhotoPath,
    CompletedAt = @CompletedAt,
    CarId = @CarId,
    Email = @Email
WHERE OrderId = @OrderId"
                    : @"
UPDATE Orders SET
    RentDate = @RentDate,
    ReturnDate = @ReturnDate,
    Status = @StatusBit,
    Price = @Price,
    FrontPhotoPath = @FrontPhotoPath,
    RearPhotoPath = @RearPhotoPath,
    SidePhotoPath = @SidePhotoPath,
    CompletedAt = @CompletedAt,
    CarId = @CarId,
    Email = @Email
WHERE OrderId = @OrderId";

                AddParameter(command, "@OrderId", id);
                AddCommonOrderParameters(command, entity, statusIsString);
                command.ExecuteNonQuery();
            });

            return entity;
        }

        private void AddCommonOrderParameters(DbCommand command, Order entity, bool statusIsString)
        {
            AddParameter(command, "@RentDate", entity.RentDate);
            AddParameter(command, "@ReturnDate", entity.ReturnDate);
            if (statusIsString)
            {
                AddParameter(command, "@StatusText", ConvertStatusToString(entity.Status));
            }
            else
            {
                AddParameter(command, "@StatusBit", entity.Status.HasValue ? entity.Status.Value : DBNull.Value);
            }
            AddParameter(command, "@Price", entity.Price);
            AddParameter(command, "@FrontPhotoPath", string.IsNullOrWhiteSpace(entity.FrontPhotoPath) ? DBNull.Value : entity.FrontPhotoPath);
            AddParameter(command, "@RearPhotoPath", string.IsNullOrWhiteSpace(entity.RearPhotoPath) ? DBNull.Value : entity.RearPhotoPath);
            AddParameter(command, "@SidePhotoPath", string.IsNullOrWhiteSpace(entity.SidePhotoPath) ? DBNull.Value : entity.SidePhotoPath);
            AddParameter(command, "@CompletedAt", entity.CompletedAt.HasValue ? entity.CompletedAt.Value : DBNull.Value);
            AddParameter(command, "@CarId", entity.CarId);
            AddParameter(command, "@Email", string.IsNullOrWhiteSpace(entity.Email) ? DBNull.Value : entity.Email);
        }

        private static string ConvertStatusToString(bool? status)
        {
            return status switch
            {
                true => "Approved",
                false => "Canceled",
                null => "Pending"
            };
        }

        private static bool IsStatusStringColumn(DbConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT TOP 1 t.name
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('Orders')
  AND c.name = 'Status'";

            var result = command.ExecuteScalar()?.ToString();
            return result == "nvarchar" || result == "varchar" || result == "nchar" || result == "char";
        }

        private static void AddParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        private void ExecuteOrderCommand(Action<DbCommand> action)
        {
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            try
            {
                if (shouldClose)
                {
                    connection.Open();
                }

                using var command = connection.CreateCommand();
                action(command);
            }
            finally
            {
                if (shouldClose && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private T ExecuteOrderCommand<T>(Func<DbCommand, T> action)
        {
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            try
            {
                if (shouldClose)
                {
                    connection.Open();
                }

                using var command = connection.CreateCommand();
                return action(command);
            }
            finally
            {
                if (shouldClose && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
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
