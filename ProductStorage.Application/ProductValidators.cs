using FluentValidation;

namespace ProductStorage.Application;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Product name must less than 200 characters and not empty.");
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Weight).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum().NotEqual(Domain.ProductType.Unknown);
        RuleFor(x => x.WarehouseNumber).GreaterThan(0);
    }
}

public class ProductFilterValidator : AbstractValidator<ProductFilter>
{
    public ProductFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
        RuleFor(x => x.Type).IsInEnum().When(x => x.Type.HasValue);
        RuleFor(x => x.WarehouseNumber).GreaterThan(0).When(x => x.WarehouseNumber.HasValue);
        RuleFor(x => x).Must(x => !x.CreatedFrom.HasValue || !x.CreatedTo.HasValue ||
                                   x.CreatedFrom <= x.CreatedTo)
            .WithMessage("CreatedFrom must be earlier than or equal to CreatedTo.");
    }
}
