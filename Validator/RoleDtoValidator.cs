using EventManagementServer.Dto;
using FluentValidation;

namespace EventManagementServer.Validator
{
    public class RoleDtoValidator : AbstractValidator<RoleDto>
    {
        public RoleDtoValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("RoleName is required")
                .MaximumLength(100).WithMessage("RoleName must not exceed 100 characters");

            RuleFor(x => x.RoleDescription)
                .NotEmpty().WithMessage("RoleDescription is required")
                .MaximumLength(100).WithMessage("RoleDescription must not exceed 100 characters");
        }
    }
}
