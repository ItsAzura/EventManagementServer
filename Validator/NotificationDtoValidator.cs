using EventManagementServer.Dto;
using FluentValidation;

namespace EventManagementServer.Validator
{
    public class NotificationDtoValidator : AbstractValidator<NotificationDto>
    {
        public NotificationDtoValidator()
        {
            RuleFor(x => x.UserID)
                .NotEmpty().WithMessage("NotificationType is required");
                
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("NotificationMessage is required")
                .MaximumLength(100).WithMessage("NotificationMessage must not exceed 100 characters");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("NotificationDate is required")
                .MaximumLength(100).WithMessage("NotificationDate must not exceed 100 characters");
        }
    }
}
