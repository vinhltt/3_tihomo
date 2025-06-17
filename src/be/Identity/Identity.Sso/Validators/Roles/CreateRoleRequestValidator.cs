using FluentValidation;
using Identity.Contracts.Roles;

namespace Identity.Api.Validators.Roles;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .Length(2, 50).WithMessage("Role name must be between 2 and 50 characters")
            .Matches(@"^[a-zA-Z0-9_\s-]+$").WithMessage("Role name can only contain letters, numbers, spaces, underscores, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("Permissions list cannot be null")
            .Must(permissions => permissions == null || permissions.Count <= 50)
            .WithMessage("Cannot have more than 50 permissions per role");

        RuleForEach(x => x.Permissions)
            .NotEmpty().WithMessage("Permission cannot be empty")
            .MaximumLength(100).WithMessage("Permission cannot exceed 100 characters")
            .When(x => x.Permissions != null);
    }
}
