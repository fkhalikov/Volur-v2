using FluentValidation;
using Volur.Application.UseCases.GetSymbols;

namespace Volur.Application.Validators;

/// <summary>
/// Validator for GetSymbolsQuery.
/// </summary>
public sealed class GetSymbolsQueryValidator : AbstractValidator<GetSymbolsQuery>
{
    public GetSymbolsQueryValidator()
    {
        RuleFor(x => x.ExchangeCode)
            .NotEmpty()
            .WithMessage("Exchange code is required.")
            .MaximumLength(10)
            .WithMessage("Exchange code cannot exceed 10 characters.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(500)
            .WithMessage("Page size cannot exceed 500.");

        RuleFor(x => x.SearchQuery)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchQuery))
            .WithMessage("Search query cannot exceed 100 characters.");
    }
}

