using BusinessLogiclayer.DTO;
using FluentValidation;

namespace BusinessLogiclayer.Validators;
public class OrderAddRequestValidator : AbstractValidator<OrderAddRequest>
{
    public OrderAddRequestValidator(IValidator<OrderItemAddRequest> orderItemAddRequestValidator)
    {
        RuleFor(x => x.UserID).NotEmpty().WithErrorCode("User ID cna't be blank");
        RuleFor(x => x.OrderDate).NotEmpty().WithErrorCode("OrderDate is required");
        RuleFor(x => x.OrderItems).NotEmpty().WithErrorCode("OrderItems is required");
        RuleForEach(x => x.OrderItems).SetValidator(orderItemAddRequestValidator);
    }
}
