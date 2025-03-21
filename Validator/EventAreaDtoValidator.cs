using EventManagementServer.Dto;
using FluentValidation;

namespace EventManagementServer.Validator
{
    public class EventAreaDtoValidator : AbstractValidator<EventAreaDto>
    {
        public EventAreaDtoValidator()
        {
            RuleFor(x => x.EventID)
                .NotEmpty().WithMessage("EventID is required");

            RuleFor(x => x.AreaName)
                .NotEmpty().WithMessage("AreaName is required")
                .MaximumLength(100).WithMessage("AreaName must not exceed 100 characters");

            RuleFor(x => x.Capacity)
                .NotEmpty().WithMessage("AreaCapacity is required");
        }
    }
}
