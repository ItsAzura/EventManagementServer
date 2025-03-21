using EventManagementServer.Dto;
using FluentValidation;

namespace EventManagementServer.Validator
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("UserName is required")
                .MaximumLength(50).WithMessage("UserName must not exceed 50 characters");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters");
            RuleFor(x => x.PasswordHash)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
                .MaximumLength(255).WithMessage("Password must not exceed 255 characters");
        }
    }
}
