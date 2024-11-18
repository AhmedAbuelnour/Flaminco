using ErrorOr;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.Validation.Abstractions
{
    public interface IValidator<TInput> where TInput : class
    {
        ValueTask<ErrorOr<Success>> Validate(AbstractValidator<TInput> builder);
    }


    public class Announcement
    {
        public string ProfileId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }



    public class AnnouncementValidation([FromKeyedServices(nameof(StartsWithRule))] IValidationRule<Announcement, string> rule) : AbstractValidator<Announcement>
    {
        public async ValueTask<ErrorOr<Success>> Validate(AbstractValidator<Announcement> builder)
        {
            await builder.NotNull(x => x.ProfileId, Error.Validation("ProfileId cannot be null."))
                         .NotEmpty(x => x.Type, Error.Validation("Type cannot be empty."))
                         .Length(x => x.Type, 1, 15, Error.Validation("Type must be between 1 and 15 characters."))
                         .Length(x => x.Value, 0, 100, Error.Validation("Value cannot exceed 100 characters."))
                         .AddRuleAsync(rule, x => x.ProfileId);

            return builder.Validate();
        }
    }
}
