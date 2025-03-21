using EventManagementServer.Dto;
using FluentValidation;

namespace EventManagementServer.Validator
{
    public class RegistrationDtoValidator : AbstractValidator<RegistrationDto>
    {
        public RegistrationDtoValidator()
        {
            RuleFor(x => x.UserID)
                .NotEmpty().WithMessage("UserID is required");

            RuleFor(x => x.RegistrationDate)
                .NotEmpty().WithMessage("RegistrationDate is required");

            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("PaymentDate is required");

            RuleFor(x => x.RegistrationDetails)
                .NotEmpty().WithMessage("RegistrationDetails is required");
        }
    }
}
