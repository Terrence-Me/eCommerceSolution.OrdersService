using BusinessLogiclayer.DTO;
using FluentValidation;

namespace BusinessLogiclayer.Validators;
public class OrderUpdateRequestValidator : AbstractValidator<OrderUpdateRequest>
{
    public OrderUpdateRequestValidator(IValidator<OrderItemUpdateRequest> orderItemUpdateRequest)
    {
        RuleFor(x => x.OrderID).NotEmpty().WithErrorCode("Order Id can't be blank");
        RuleFor(x => x.UserID).NotEmpty().WithErrorCode("User Id can't be blank");
        RuleFor(x => x.OrderDate).NotEmpty().WithErrorCode("Order Date can't be blank");
        RuleFor(x => x.OrderItems).NotEmpty().WithErrorCode("Order Items can't be blank");
        RuleForEach(x => x.OrderItems).SetValidator(orderItemUpdateRequest);

    }
}
