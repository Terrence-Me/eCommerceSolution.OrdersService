using AutoMapper;
using BusinessLogiclayer.DTO;
using BusinessLogiclayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;


namespace BusinessLogiclayer.Services;
public class OrdersService(IOrdersRepository orderRepository, IMapper mapper, IValidator<OrderAddRequest> orderAddRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator) : IOrdersService
{
    //private readonly IOrdersRepository _orderRepository = orderRepository;
    //private readonly IMapper _mapper;
    //private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    //private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    //private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    //private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;

    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        ArgumentNullException.ThrowIfNull(orderAddRequest);


        ValidationResult orderAddRequestValidationResult = await orderAddRequestValidator.ValidateAsync(orderAddRequest);
        if (!orderAddRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(x => x.ErrorMessage));
            throw new ArgumentException(errors);
        }

        // TODO: Add Logic for checking if UserId exist in Users MicroService

        Order orderInput = mapper.Map<Order>(orderAddRequest);

        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        orderInput.TotalBill = orderInput.OrderItems.Sum(x => x.TotalPrice);

        Order? addedOrder = await orderRepository.AddOrder(orderInput);
        if (addedOrder == null)
        {
            return null;
        }

        OrderResponse orderResponse = mapper.Map<OrderResponse>(addedOrder);

        return orderResponse;

    }

    public async Task<bool> DeleteOrder(Guid orderId)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderID, orderId);
        Order? exisitingOrder = await orderRepository.GetOrderByCondition(filter);

        if (exisitingOrder == null)
        {
            return false;
        }

        bool isDeleted = await orderRepository.DeleteOrder(orderId);

        return isDeleted;

    }

    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        Order? order = await orderRepository.GetOrderByCondition(filter);
        if (order == null)
        {
            return null;
        }

        return mapper.Map<OrderResponse>(order);
    }

    public async Task<List<OrderResponse?>> GetOrders()
    {
        IEnumerable<Order?> allOrders = await orderRepository.GetOrders();

        IEnumerable<OrderResponse?> orderResponses = mapper.Map<List<OrderResponse>>(allOrders);

        return orderResponses.ToList();
    }

    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await orderRepository.GetOrdersByCondition(filter);

        IEnumerable<OrderResponse?> orderResponses = mapper.Map<List<OrderResponse>>(orders);

        return orderResponses.ToList();
    }

    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        ArgumentNullException.ThrowIfNull(orderUpdateRequest);


        ValidationResult orderUpdateRequestValidationResult = await orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(x => x.ErrorMessage));
            throw new ArgumentException(errors);
        }

        // TODO: Add Logic for checking if UserId exist in Users MicroService

        Order orderInput = mapper.Map<Order>(orderUpdateRequest);

        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        orderInput.TotalBill = orderInput.OrderItems.Sum(x => x.TotalPrice);

        Order? updatedOrder = await orderRepository.UpdateOrder(orderInput);
        if (updatedOrder == null)
        {
            return null;
        }

        OrderResponse updatedOrderResponse = mapper.Map<OrderResponse>(updatedOrder);

        return updatedOrderResponse;
    }
}
