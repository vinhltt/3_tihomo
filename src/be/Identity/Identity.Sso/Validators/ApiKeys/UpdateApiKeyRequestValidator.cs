using FluentValidation;
using Identity.Contracts.ApiKeys;

namespace Identity.Api.Validators.ApiKeys;

public class UpdateApiKeyRequestValidator : AbstractValidator<UpdateApiKeyRequest>
{
    public UpdateApiKeyRequestValidator()
    {        RuleFor(x => x.Name)
            .Length(3, 100).WithMessage("API key name must be between 3 and 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x.Scopes)
            .Must(scopes => scopes == null || scopes.Count <= 20)
            .WithMessage("Cannot have more than 20 scopes per API key")
            .When(x => x.Scopes != null);

        RuleForEach(x => x.Scopes)
            .NotEmpty().WithMessage("Scope cannot be empty")
            .MaximumLength(50).WithMessage("Scope cannot exceed 50 characters")
            .When(x => x.Scopes != null);
    }
}
