using BusinessLogiclayer.DTO;
using BusinessLogiclayer.ServiceContracts;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OrdersMicroservice.API.ApiControllers;
[Route("api/[controller]")]
[ApiController]
public class OrdersController(IOrdersService ordersService) : ControllerBase
{
    private readonly IOrdersService _ordersService = ordersService;

    [HttpGet]
    public async Task<IEnumerable<OrderResponse?>> GetOrders()
    {
        List<OrderResponse?> orders = await _ordersService.GetOrders();
        return orders;
    }

    [HttpGet("/search/orderId/{orderId}")]
    public async Task<IActionResult> GetOrderByOrderId(Guid orderId)
    {
        FilterDefinition<Order> filterDefinition = Builders<Order>.Filter.Eq(x => x.OrderID, orderId);

        OrderResponse? order = await _ordersService.GetOrderByCondition(filterDefinition);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    [HttpGet("/search/productId/{productId}")]
    public async Task<IActionResult> GetOrdersByProductId(Guid productId)
    {
        FilterDefinition<Order> filterDefinition = Builders<Order>.Filter.ElemMatch(x => x.OrderItems,
            Builders<OrderItem>.Filter.Eq(tempProduct => tempProduct.ProductID, productId));

        List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filterDefinition);
        if (orders == null)
        {
            return NotFound();
        }
        return Ok(orders);
    }

    [HttpGet("/search/orderDate/{orderDate}")]
    public async Task<IActionResult> GetOrdersByOrderDate(DateTime orderDate)
    {
        FilterDefinition<Order> filterDefinition = Builders<Order>.Filter.Eq(x => x.OrderDate.ToString("yyy-MM-dd"), orderDate.ToString("yyy-MM-dd"));

        List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filterDefinition);
        if (orders == null)
        {
            return NotFound();
        }
        return Ok(orders);
    }

    [HttpGet("/search/userId/{userId}")]
    public async Task<IActionResult> GetOrdersByUserId(Guid userId)
    {
        FilterDefinition<Order> filterDefinition = Builders<Order>.Filter.Eq(x => x.UserID, userId);

        List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filterDefinition);
        if (orders == null)
        {
            return NotFound();
        }
        return Ok(orders);
    }



    [HttpPost("add-order")]
    public async Task<IActionResult> AddOrder([FromBody] OrderAddRequest orderAddRequest)
    {
        if (orderAddRequest == null) { return BadRequest(); }

        OrderResponse? addedOrder = await _ordersService.AddOrder(orderAddRequest);
        if (addedOrder == null)
        {
            return Problem("Error adding product");
        }
        return Created($"api/Orders/search/orderId,{addedOrder?.OrderID}", addedOrder);
    }

    [HttpPut("{oderId}")]
    public async Task<IActionResult> UpdateOrder([FromBody] OrderUpdateRequest orderUpdateRequest, [FromRoute] Guid oderId)
    {
        if (orderUpdateRequest == null) { return BadRequest(); }
        if (orderUpdateRequest.OrderID != oderId) { return BadRequest(); }

        OrderResponse? updatedOrder = await _ordersService.UpdateOrder(orderUpdateRequest);
        if (updatedOrder == null)
        {
            return Problem("Error updating product");
        }
        return Ok(updatedOrder);
    }

    [HttpDelete("{orderId}")]
    public async Task<IActionResult> DeleteOrder(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return BadRequest();
        }
        bool isDeleted = await _ordersService.DeleteOrder(orderId);
        if (!isDeleted)
        {
            return Problem("Error deleting product");
        }
        return Ok();
    }
}
