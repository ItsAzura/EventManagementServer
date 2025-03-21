using EventManagementServer.Dto;
using FluentValidation;

namespace EventManagementServer.Validator
{
    public class RegistrationDetailDtoValidator : AbstractValidator<RegistrationDetailDto>
    {
        public RegistrationDetailDtoValidator()
        {
            RuleFor(x => x.TicketID)
                .NotEmpty().WithMessage("TicketID is required");

            RuleFor(x => x.Quantity)
                .NotEmpty().WithMessage("Quantity is required");
        }
    }
}
