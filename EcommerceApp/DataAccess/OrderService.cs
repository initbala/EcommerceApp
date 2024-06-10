namespace dapp1.DataAccess
{
    using Dapper;
    using ECommerceApp.Models;
    using Microsoft.Data.SqlClient;
    using System.Data;

    public interface IOrderService
    {
        Task<ApiResponse> GetLatestOrderAsync(string email, string customerId);
    }

    public class OrderService : IOrderService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public OrderService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }


        //---------------------Logics---------------------------------------
        public async Task<ApiResponse> GetLatestOrderAsync(string email, string customerId)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var customerQuery = @"SELECT * FROM Customers WHERE CustomerId = @CustomerId AND Email = @Email";
                    var customer = await db.QueryFirstOrDefaultAsync<Customer>(customerQuery, new { CustomerId = customerId, Email = email });

                    if (customer == null)
                    {
                        return null;
                    }

                    //--------------------Getting Customer Address---------------------------------------------
                    var address = $"{customer.HouseNo} {customer.Street}, {customer.Town}, {customer.PostCode}";


                    //----------------------Getting Order Id----------------------------------------------
                    var orderQuery = @"SELECT TOP 1 * FROM Orders WHERE CustomerId = @CustomerId ORDER BY OrderDate DESC";
                    var order = await db.QueryFirstOrDefaultAsync<Order>(orderQuery, new { CustomerId = customerId });

                    if (order == null)
                    {
                        return new ApiResponse { Customer = customer, Order = null };
                    }

                    //------------------------Getting All Order Items---------------------------------------------
                    var orderItemsQuery = @"
                SELECT 
                    oi.Quantity, 
                    oi.Price AS PriceEach, 
                    CASE WHEN o.ContainsGift = 1 THEN 'Gift' ELSE p.ProductName END AS Product
                FROM OrderItems oi
                JOIN Products p ON oi.ProductId = p.ProductId
                JOIN Orders o ON oi.OrderId = o.OrderId
                WHERE oi.OrderId = @OrderId";

                    var orderItems = (await db.QueryAsync<OrderItem>(orderItemsQuery, new { OrderId = order.OrderId })).ToList();


                    //----------------------------Preparing Response to send------------------------------------
                    return new ApiResponse
                    {
                        Customer = customer,
                        Order = new Order
                        {
                            OrderId = order.OrderId,
                            OrderDate = order.OrderDate,
                            DeliveryAddress = address,
                            OrderItems = orderItems,
                            DeliveryExpected = order.DeliveryExpected
                        }
                    };
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

}
