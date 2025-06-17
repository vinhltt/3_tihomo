using FluentValidation;
using Identity.Contracts.ApiKeys;

namespace Identity.Api.Validators.ApiKeys;

public class CreateApiKeyRequestValidator : AbstractValidator<CreateApiKeyRequest>
{
    public CreateApiKeyRequestValidator()
    {        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("API key name is required")
            .Length(3, 100).WithMessage("API key name must be between 3 and 100 characters");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x.Scopes)
            .NotNull().WithMessage("Scopes list cannot be null")
            .Must(scopes => scopes == null || scopes.Count <= 20)
            .WithMessage("Cannot have more than 20 scopes per API key");

        RuleForEach(x => x.Scopes)
            .NotEmpty().WithMessage("Scope cannot be empty")
            .MaximumLength(50).WithMessage("Scope cannot exceed 50 characters")
            .When(x => x.Scopes != null);
    }
}
