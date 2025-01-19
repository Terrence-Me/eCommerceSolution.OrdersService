using BusinessLogiclayer.DTO;
using FluentValidation;

namespace BusinessLogiclayer.Validators;
public class OrderItemUpdateRequestValidator : AbstractValidator<OrderItemUpdateRequest>
{
    public OrderItemUpdateRequestValidator()
    {
        RuleFor(x => x.ProductID).NotEmpty().WithErrorCode("Product Id can't be blank");
        RuleFor(x => x.UnitPrice).NotEmpty().WithErrorCode("Unit Price can't be blank").GreaterThan(0).WithErrorCode("Unit Price can't be less than or equal to zero");
        RuleFor(x => x.Quantity).NotEmpty().WithErrorCode("Quantity can't be balnk").GreaterThan(0).WithErrorCode("Quantity can't be less than or equal to zero");
    }


}