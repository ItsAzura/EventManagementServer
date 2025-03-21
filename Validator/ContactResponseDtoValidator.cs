using EventManagementServer.Dto;
using FluentValidation;

namespace EventManagementServer.Validator
{
    public class ContactResponseDtoValidator : AbstractValidator<ContactResponseDto>
    {
        public ContactResponseDtoValidator()
        {
            RuleFor(x => x.ResponseMessage)
                .NotEmpty().WithMessage("ContactName is required")
                .MaximumLength(100).WithMessage("ContactName must not exceed 100 characters");

            RuleFor(x => x.AdminEmail)
                .NotEmpty().WithMessage("ContactEmail is required")
                .EmailAddress().WithMessage("Invalid email address")
                .MaximumLength(100).WithMessage("ContactEmail must not exceed 100 characters");
  
        }
    }
}
