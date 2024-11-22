using Flaminco.ManualMapper.Abstractions;
using Flaminco.MinimalMediatR.Abstractions;
using Flaminco.MinimalMediatR.Extensions;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WebApplication1.Validations
{
    public class Person
    {
        [MinLength(3, ErrorMessage = "something to show here ")]
        public string Name { get; set; }
        [Range(18, 55, ErrorMessage = "something to show here ")]
        public int Age { get; set; }

    }


    public class AddPersonCommand : IEndPoint
    {
        [FromBody] public Person Person { get; set; }
    }

    public class AddPersonCommandHandler(IEntityDtoAdapter<Entity, Dto> _entityDtoAdapter) : IEndPointHandler<AddPersonCommand>
    {
        public async Task<IResult> Handle(AddPersonCommand request, CancellationToken cancellationToken)
        {
            Dto dto = new Dto
            {
                Id = 1,
                FullName = "Ahmed Abuelnour"
            };

            Entity entity = _entityDtoAdapter.ToEntity(dto);

            return Results.Ok();
        }
    }

    public class MyModelValidator : AbstractValidator<AddPersonCommand>
    {
        public MyModelValidator()
        {
            RuleFor(a => a.Person).SetValidator(new PersonValidator()).WithErrorCode("less_than_2");
        }
    }


    public class TestModule : IModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MediatePost<AddPersonCommand>("/person/add")
                .WithName("add-person");
        }
    }


    public class PersonValidator : AbstractValidator<Person>
    {
        public PersonValidator()
        {
            this.ApplyDataAnnotations();
        }
    }

    public static partial class DataAnnotationsValidatorExtensions
    {
        public static void ApplyDataAnnotations<T>(this AbstractValidator<T> validator)
        {
            foreach (PropertyInfo property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                IEnumerable<ValidationAttribute> attributes = property.GetCustomAttributes<ValidationAttribute>(true);

                if (!attributes.Any()) continue;

                var parameter = Expression.Parameter(typeof(T), "x");
                var propertyAccess = Expression.Property(parameter, property);
                var lambda = Expression.Lambda<Func<T, object>>(
                    Expression.Convert(propertyAccess, typeof(object)),
                    parameter
                );

                IRuleBuilderInitial<T, object> rule = validator.RuleFor(lambda);

                foreach (ValidationAttribute attribute in attributes)
                {
                    MapAttributeToRule(rule, attribute, property);
                }
            }
        }

        private static void MapAttributeToRule<T>(IRuleBuilderInitial<T, object> rule, ValidationAttribute attribute, PropertyInfo property)
        {
            switch (attribute)
            {
                case RequiredAttribute required:
                    rule.NotNull().WithMessage(required.ErrorMessage ?? $"{property.Name} is required.");
                    break;

                case StringLengthAttribute stringLength:
                    rule.Must(value => value is string stringValue &&
                               stringValue.Length >= stringLength.MinimumLength &&
                               stringValue.Length <= stringLength.MaximumLength).WithMessage(stringLength.ErrorMessage ?? $"{property.Name} must be between {stringLength.MinimumLength} and {stringLength.MaximumLength} characters.");
                    break;

                case MinLengthAttribute minLength:
                    rule.Must(value => value is string stringValue && stringValue.Length >= minLength.Length).WithMessage(minLength.ErrorMessage ?? $"{property.Name} must be at least {minLength.Length} characters.");
                    break;

                case MaxLengthAttribute maxLength:
                    rule.Must(value => value is string stringValue && stringValue.Length <= maxLength.Length).WithMessage(maxLength.ErrorMessage ?? $"{property.Name} must not exceed {maxLength.Length} characters.");
                    break;

                case Base64StringAttribute base64String:
                    rule.Must(value =>
                    {
                        if (value is string stringValue)
                        {
                            // Check if the string is a valid Base64 string
                            Span<byte> buffer = new(new byte[stringValue.Length]);
                            return Convert.TryFromBase64String(stringValue, buffer, out _);
                        }
                        return false;
                    }).WithMessage(base64String.ErrorMessage ?? $"{property.Name} must be a valid Base64 encoded string.");
                    break;

                case RangeAttribute range:
                    rule.Must(value =>
                    {
                        if (value is IComparable comparableValue)
                        {
                            var min = Convert.ChangeType(range.Minimum, comparableValue.GetType());
                            var max = Convert.ChangeType(range.Maximum, comparableValue.GetType());
                            return comparableValue.CompareTo(min) >= 0 && comparableValue.CompareTo(max) <= 0;
                        }
                        return false;
                    }).WithMessage(range.ErrorMessage ?? $"{property.Name} must be between {range.Minimum} and {range.Maximum}.");
                    break;

                case RegularExpressionAttribute regex:
                    rule.Must(value => value is string stringValue && System.Text.RegularExpressions.Regex.IsMatch(stringValue, regex.Pattern)).WithMessage(regex.ErrorMessage ?? $"{property.Name} format is invalid.");
                    break;

                case PhoneAttribute phone:
                    rule.Must(value => value is string stringValue && PhoneNumberValidatorRegex().IsMatch(stringValue)).WithMessage(phone.ErrorMessage ?? $"{property.Name} must be a valid phone number.");
                    break;

                case UrlAttribute url:
                    rule.Must(value => value is string stringValue && Uri.TryCreate(stringValue, UriKind.Absolute, out _)).WithMessage(url.ErrorMessage ?? $"{property.Name} must be a valid URL.");
                    break;

                case CreditCardAttribute creditCard:
                    rule.Must(value => value is string stringValue && new CreditCardValidator<T>().IsValid(null, stringValue)).WithMessage($"{property.Name} must be a valid credit card number.");
                    break;

                case EmailAddressAttribute email:
                    rule.Must(value => value is string stringValue && EmailValidatorRegex().IsMatch(stringValue)).WithMessage(email.ErrorMessage ?? $"{property.Name} must be a valid email address.");
                    break;

                case CompareAttribute compare:
                    rule.Must((obj, value, context) =>
                    {
                        Type type = typeof(T);
                        PropertyInfo? otherProperty = type.GetProperty(compare.OtherProperty);
                        if (otherProperty == null)
                        {
                            context.MessageFormatter.AppendArgument("OtherProperty", compare.OtherProperty);
                            return false;
                        }
                        var otherValue = otherProperty.GetValue(obj);
                        return Equals(value, otherValue);
                    }).WithMessage(compare.ErrorMessage ?? $"{property.Name} must match {compare.OtherProperty}.");
                    break;

                case FileExtensionsAttribute fileExtensions:
                    rule.Must(value => value is string stringValue && fileExtensions.Extensions.Split(',').Select(e => e.Trim()).Any(ext => stringValue.EndsWith($".{ext}", StringComparison.OrdinalIgnoreCase)))
                        .WithMessage(fileExtensions.ErrorMessage ?? $"{property.Name} must have one of the following extensions: {fileExtensions.Extensions}.");
                    break;
            }
        }

        [GeneratedRegex(@"^\+?[1-9]\d{1,14}$")]
        private static partial Regex PhoneNumberValidatorRegex();
        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailValidatorRegex();
    }

}
