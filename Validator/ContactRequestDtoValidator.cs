using EventManagementServer.Dto;
using FluentValidation;

namespace EventManagementServer.Validator
{
    public class ContactRequestDtoValidator : AbstractValidator<ContactRequestDto>
    {
        public ContactRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required")
                .MaximumLength(100).WithMessage("Message must not exceed 100 characters");
        }
    }
}
