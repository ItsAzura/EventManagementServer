using EventManagementServer.Dto;
using FluentValidation;

namespace EventManagementServer.Validator
{
    public class TicketDtoValidator : AbstractValidator<TicketDto>
    {
        public TicketDtoValidator()
        {
            RuleFor(x => x.EventAreaID)
                .NotEmpty().WithMessage("EventId is required");

            RuleFor(x => x.TicketName)
                .NotEmpty().WithMessage("TicketName is required")
                .MaximumLength(100).WithMessage("TicketName must not exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(100).WithMessage("Description must not exceed 100 characters");

            RuleFor(x => x.Quantity)
                .NotEmpty().WithMessage("Quantity is required");

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("Price is required");

        }
    }
}
