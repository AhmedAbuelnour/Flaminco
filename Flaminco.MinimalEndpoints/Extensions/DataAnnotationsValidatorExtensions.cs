using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Flaminco.MinimalEndpoints.Extensions
{
    public static partial class DataAnnotationsValidatorExtensions
    {
        public static void DataAnnotations<T, TProperty>(this IRuleBuilderInitial<T, TProperty> ruleBuilder)
        {
            // Retrieve the validation attributes and map them during the setup phase.
            List<Action<TProperty, ValidationContext<T>>> propertyRules = [];

            foreach (PropertyInfo property in typeof(TProperty).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                foreach (var attribute in property.GetCustomAttributes<ValidationAttribute>(true))
                {
                    propertyRules.Add((instance, context) => MapAttributeToRule(attribute, property, instance, context));
                }
            }

            // Add a single FluentValidation custom rule to execute all mapped rules.
            ruleBuilder.Custom((instance, context) =>
            {
                if (instance == null) return; // Skip validation if the instance is null

                foreach (Action<TProperty, ValidationContext<T>> rule in propertyRules)
                {
                    rule(instance, context);
                }
            });
        }

        private static void MapAttributeToRule<T, TProperty>(ValidationAttribute attribute, PropertyInfo property, TProperty instance, ValidationContext<T> context)
        {
            object? propertyValue = property.GetValue(instance);

            switch (attribute)
            {
                case RequiredAttribute required:
                    if (propertyValue == null)
                    {
                        context.AddFailure(new ValidationFailure
                        {
                            AttemptedValue = propertyValue,
                            ErrorMessage = required.ErrorMessage ?? $"{property.Name} is required.",
                            PropertyName = property.Name,
                            ErrorCode = $"REQ_{property.Name.ToUpperInvariant()}"
                        });
                    }
                    break;

                case StringLengthAttribute stringLength:
                    if (propertyValue is string stringValue &&
                        (stringValue.Length < stringLength.MinimumLength || stringValue.Length > stringLength.MaximumLength))
                    {
                        context.AddFailure(new ValidationFailure
                        {
                            AttemptedValue = stringValue,
                            ErrorMessage = stringLength.ErrorMessage ?? $"{property.Name} must be between {stringLength.MinimumLength} and {stringLength.MaximumLength} characters.",
                            PropertyName = property.Name,
                            ErrorCode = $"STRLEN_{property.Name.ToUpperInvariant()}"
                        });
                    }
                    break;

                case MinLengthAttribute minLength:
                    if (propertyValue is string minStringValue && minStringValue.Length < minLength.Length)
                    {
                        context.AddFailure(new ValidationFailure
                        {
                            AttemptedValue = minStringValue,
                            ErrorMessage = minLength.ErrorMessage ?? $"{property.Name} must be at least {minLength.Length} characters.",
                            PropertyName = property.Name,
                            ErrorCode = $"MINLEN_{property.Name.ToUpperInvariant()}"
                        });
                    }
                    break;

                case MaxLengthAttribute maxLength:
                    if (propertyValue is string maxStringValue && maxStringValue.Length > maxLength.Length)
                    {
                        context.AddFailure(new ValidationFailure
                        {
                            AttemptedValue = maxStringValue,
                            ErrorMessage = maxLength.ErrorMessage ?? $"{property.Name} must not exceed {maxLength.Length} characters.",
                            PropertyName = property.Name,
                            ErrorCode = $"MAXLEN_{property.Name.ToUpperInvariant()}"
                        });
                    }
                    break;

                case Base64StringAttribute base64String:
                    if (propertyValue is string base64Value)
                    {
                        Span<byte> buffer = new(new byte[base64Value.Length]);
                        if (!Convert.TryFromBase64String(base64Value, buffer, out _))
                        {
                            context.AddFailure(new ValidationFailure
                            {
                                AttemptedValue = base64Value,
                                ErrorMessage = base64String.ErrorMessage ?? $"{property.Name} must be a valid Base64 encoded string.",
                                PropertyName = property.Name,
                                ErrorCode = $"BASE64_{property.Name.ToUpperInvariant()}"
                            });
                        }
                    }
                    break;

                case RangeAttribute range:
                    if (propertyValue is IComparable comparableValue)
                    {
                        var min = Convert.ChangeType(range.Minimum, comparableValue.GetType());
                        var max = Convert.ChangeType(range.Maximum, comparableValue.GetType());
                        if (comparableValue.CompareTo(min) < 0 || comparableValue.CompareTo(max) > 0)
                        {
                            context.AddFailure(new ValidationFailure
                            {
                                AttemptedValue = propertyValue,
                                ErrorMessage = range.ErrorMessage ?? $"{property.Name} must be between {range.Minimum} and {range.Maximum}.",
                                PropertyName = property.Name,
                                ErrorCode = $"RNG_{property.Name.ToUpperInvariant()}"
                            });
                        }
                    }
                    break;

                case RegularExpressionAttribute regex:
                    if (propertyValue is string regexValue && !Regex.IsMatch(regexValue, regex.Pattern))
                    {
                        context.AddFailure(new ValidationFailure
                        {
                            AttemptedValue = regexValue,
                            ErrorMessage = regex.ErrorMessage ?? $"{property.Name} format is invalid.",
                            PropertyName = property.Name,
                            ErrorCode = $"REGEX_{property.Name.ToUpperInvariant()}"
                        });
                    }
                    break;

                case PhoneAttribute phone:
                    if (propertyValue is string phoneValue && !PhoneNumberValidatorRegex().IsMatch(phoneValue))
                    {
                        context.AddFailure(new ValidationFailure
                        {
                            AttemptedValue = phoneValue,
                            ErrorMessage = phone.ErrorMessage ?? $"{property.Name} must be a valid phone number.",
                            PropertyName = property.Name,
                            ErrorCode = $"PHONE_{property.Name.ToUpperInvariant()}"
                        });
                    }
                    break;

                case UrlAttribute url:
                    if (propertyValue is string urlValue && !Uri.TryCreate(urlValue, UriKind.Absolute, out _))
                    {
                        context.AddFailure(new ValidationFailure
                        {
                            AttemptedValue = urlValue,
                            ErrorMessage = url.ErrorMessage ?? $"{property.Name} must be a valid URL.",
                            PropertyName = property.Name,
                            ErrorCode = $"URL_{property.Name.ToUpperInvariant()}"
                        });
                    }
                    break;

                case CompareAttribute compare:
                    if (propertyValue is { } compareValue)
                    {
                        var otherProperty = typeof(TProperty).GetProperty(compare.OtherProperty);
                        if (otherProperty == null || !Equals(compareValue, otherProperty.GetValue(instance)))
                        {
                            context.AddFailure(new ValidationFailure
                            {
                                AttemptedValue = compareValue,
                                ErrorMessage = compare.ErrorMessage ?? $"{property.Name} must match {compare.OtherProperty}.",
                                PropertyName = property.Name,
                                ErrorCode = $"COMPARE_{property.Name.ToUpperInvariant()}"
                            });
                        }
                    }
                    break;

                case FileExtensionsAttribute fileExtensions:
                    if (propertyValue is string fileValue &&
                        !fileExtensions.Extensions.Split(',').Select(e => e.Trim())
                            .Any(ext => fileValue.EndsWith($".{ext}", StringComparison.OrdinalIgnoreCase)))
                    {
                        context.AddFailure(new ValidationFailure
                        {
                            AttemptedValue = fileValue,
                            ErrorMessage = fileExtensions.ErrorMessage ?? $"{property.Name} must have one of the following extensions: {fileExtensions.Extensions}.",
                            PropertyName = property.Name,
                            ErrorCode = $"FILEEXT_{property.Name.ToUpperInvariant()}"
                        });
                    }
                    break;

                case CreditCardAttribute creditCard:
                    if (propertyValue is string creditCardValue && !new CreditCardValidator<T>().IsValid(null, creditCardValue))
                    {
                        context.AddFailure(new ValidationFailure
                        {
                            AttemptedValue = creditCardValue,
                            ErrorMessage = creditCard.ErrorMessage ?? $"{property.Name} must be a valid credit card number.",
                            PropertyName = property.Name,
                            ErrorCode = $"CREDITCARD_{property.Name.ToUpperInvariant()}"
                        });
                    }
                    break;

                case EmailAddressAttribute email:
                    if (propertyValue is string emailValue && !EmailValidatorRegex().IsMatch(emailValue))
                    {
                        context.AddFailure(new ValidationFailure
                        {
                            AttemptedValue = emailValue,
                            ErrorMessage = email.ErrorMessage ?? $"{property.Name} must be a valid email address.",
                            PropertyName = property.Name,
                            ErrorCode = $"EMAIL_{property.Name.ToUpperInvariant()}"
                        });
                    }
                    break;
            }
        }

        [GeneratedRegex(@"^\+?[1-9]\d{1,14}$")]
        private static partial Regex PhoneNumberValidatorRegex();
        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailValidatorRegex();
    }
}
