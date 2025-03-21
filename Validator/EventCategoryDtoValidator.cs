using EventManagementServer.Dto;
using FluentValidation;

namespace EventManagementServer.Validator
{
    public class EventCategoryDtoValidator : AbstractValidator<EventCategoryDto>
    {
        public EventCategoryDtoValidator()
        {
            RuleFor(x => x.EventID)
                .NotEmpty().WithMessage("CategoryName is required");

            RuleFor(x => x.CategoryID)
                .NotEmpty().WithMessage("CategoryDescription is required");
        }
    }
}
