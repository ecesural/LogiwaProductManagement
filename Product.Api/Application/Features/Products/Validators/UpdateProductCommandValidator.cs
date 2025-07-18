using FluentValidation;
using Product.Api.Application.Features.Products.Commands;

namespace Product.Api.Application.Features.Products.Validators;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");
        When(x => x.Title.IsSet, () =>
        {
            RuleFor(x => x.Title)
                .Must(x => !string.IsNullOrWhiteSpace(x.Value))
                .WithMessage("Title cannot be empty.")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Title.Value)
                        .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
                });
        });
        When(x => x.StockQuantity.IsSet, () =>
        {
            RuleFor(x => x.StockQuantity.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
        });
    }
}
