using FluentValidation;
using PlatformApp.Application.Catalog.Commands;

namespace PlatformApp.Application.Catalog.Validators;

public sealed class CreateCatalogItemValidator : AbstractValidator<CreateCatalogItemCommand>
{
    public CreateCatalogItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Price).GreaterThan(0).PrecisionScale(18, 2, false);
        RuleFor(x => x.CatalogBrandId).NotEmpty();
        RuleFor(x => x.CatalogTypeId).NotEmpty();
        RuleFor(x => x.AvailableStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PictureUri).MaximumLength(500);
    }
}

public sealed class UpdateCatalogItemValidator : AbstractValidator<UpdateCatalogItemCommand>
{
    public UpdateCatalogItemValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Price).GreaterThan(0).PrecisionScale(18, 2, false);
        RuleFor(x => x.CatalogBrandId).NotEmpty();
        RuleFor(x => x.CatalogTypeId).NotEmpty();
        RuleFor(x => x.AvailableStock).GreaterThanOrEqualTo(0);
    }
}
