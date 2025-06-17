using FluentValidation;
using Identity.Contracts.Roles;

namespace Identity.Api.Validators.Roles;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .Length(2, 50).WithMessage("Role name must be between 2 and 50 characters")
            .Matches(@"^[a-zA-Z0-9_\s-]+$").WithMessage("Role name can only contain letters, numbers, spaces, underscores, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Permissions)
            .Must(permissions => permissions == null || permissions.Count <= 50)
            .WithMessage("Cannot have more than 50 permissions per role")
            .When(x => x.Permissions != null);

        RuleForEach(x => x.Permissions)
            .NotEmpty().WithMessage("Permission cannot be empty")
            .MaximumLength(100).WithMessage("Permission cannot exceed 100 characters")
            .When(x => x.Permissions != null);
    }
}
