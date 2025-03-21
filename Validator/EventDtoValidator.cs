using EventManagementServer.Dto;
using FluentValidation;

namespace EventManagementServer.Validator
{
    public class EventDtoValidator : AbstractValidator<EventDto>
    {
        public EventDtoValidator()
        {
            RuleFor(x => x.EventName)
                .NotEmpty().WithMessage("EventName is required")
                .MaximumLength(100).WithMessage("EventName must not exceed 100 characters");

            RuleFor(x => x.EventDescription)
                .NotEmpty().WithMessage("EventDescription is required")
                .MaximumLength(100).WithMessage("EventDescription must not exceed 100 characters");

            RuleFor(x => x.EventDate)
                .NotEmpty().WithMessage("EventDate is required");

            RuleFor(x => x.EventLocation)
                .NotEmpty().WithMessage("EventLocation is required")
                .MaximumLength(100).WithMessage("EventLocation must not exceed 100 characters");

            RuleFor(x => x.CreatedBy)
                .NotEmpty().WithMessage("EventOrganizer is required");   

            RuleFor(x => x.EventStatus)
                .NotEmpty().WithMessage("EventStatus is required");
        }
    }
}
