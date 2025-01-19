﻿using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories;
public class OrdersRepository : IOrdersRepository
{
    private readonly IMongoCollection<Order> _orders;
    private readonly string collectionName = "orders";

    public OrdersRepository(IMongoDatabase mongoDatabase)
    {
        _orders = mongoDatabase.GetCollection<Order>(collectionName);
    }

    public async Task<Order?> AddOrder(Order order)
    {
        order.OrderID = Guid.NewGuid();
        order._id = order.OrderID;

        foreach (OrderItem orderItem in order.OrderItems)
        {

            orderItem._id = Guid.NewGuid();
        }
        await _orders.InsertOneAsync(order);

        return order;
    }

    public async Task<bool> DeleteOrder(Guid orderId)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderID, orderId);
        Order? existingOrder = (await _orders.FindAsync(filter)).FirstOrDefault();

        if (existingOrder == null)
        {
            return false;
        }
        DeleteResult deleteResult = await _orders.DeleteOneAsync(filter);
        return deleteResult.DeletedCount > 0;


    }

    public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        return (await _orders.FindAsync(filter)).FirstOrDefault();
    }

    public async Task<IEnumerable<Order>> GetOrders()
    {
        return (await _orders.FindAsync(Builders<Order>.Filter.Empty)).ToList();
    }

    public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        return (await _orders.FindAsync(filter)).ToList();
    }

    public async Task<Order?> UpdateOrder(Order order)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderID, order.OrderID);
        Order? existingOrder = (await _orders.FindAsync(filter)).FirstOrDefault();

        if (existingOrder == null)
        {
            return null;
        }

        ReplaceOneResult replaceOneResult = _orders.ReplaceOne(filter, order);

        return order;
    }
}
