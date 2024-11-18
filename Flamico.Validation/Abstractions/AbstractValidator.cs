using ErrorOr;
using System.Text.RegularExpressions;

namespace Flaminco.Validation.Abstractions
{
    public class ValidationRuleAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    public interface IValidationRule<TModel, TProperty>
    {
        ValueTask<ErrorOr<Success>> ValidateAsync(TModel model, Func<TModel, TProperty> propertySelector);
    }

    [ValidationRuleAttribute(nameof(StartsWithRule))]
    public class StartsWithRule : IValidationRule<Announcement, string>
    {
        public async ValueTask<ErrorOr<Success>> ValidateAsync(Announcement model, Func<Announcement, string> propertySelector)
        {
            var value = propertySelector(model);

            if (string.IsNullOrEmpty(value) || !value.StartsWith("Ahhahhh"))
            {
                return Error.Validation("StartsWithRule", "_errorMessage");
            }

            return Result.Success;
        }
    }


    public abstract class AbstractValidator<TModel> where TModel : class
    {
        private readonly List<Error> _errors = [];
        private TModel? _model;
        public ErrorOr<Success> ValidateAgainst(TModel model)
        {
            _model = model;


        }


        public async ValueTask<AbstractValidator<TModel>> AddRuleAsync<TProperty>(IValidationRule<TModel, TProperty> rule, Func<TModel, TProperty> propertySelector)
        {
            if (await rule.ValidateAsync(_model, propertySelector) is ErrorOr<Success> errorOrSuccess)
            {
                if (errorOrSuccess.IsError)
                {
                    _errors.AddRange(errorOrSuccess.Errors);
                }
            }
            return this;
        }

        // Common Validations
        public AbstractValidator<TModel> NotNull<TProperty>(Func<TModel, TProperty> propertySelector, Error error)
        {
            if (propertySelector(_model) == null)
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> NotEmpty(Func<TModel, string> propertySelector, Error error)
        {
            if (string.IsNullOrWhiteSpace(propertySelector(_model)))
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> EqualTo<TProperty>(Func<TModel, TProperty> propertySelector, TProperty targetValue, Error error) where TProperty : IComparable
        {
            if (!propertySelector(_model).Equals(targetValue))
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> NotEqualTo<TProperty>(Func<TModel, TProperty> propertySelector, TProperty targetValue, Error error) where TProperty : IComparable
        {
            if (propertySelector(_model).Equals(targetValue))
            {
                _errors.Add(error);
            }

            return this;
        }

        // String Validations
        public AbstractValidator<TModel> Length(Func<TModel, string> propertySelector, int min, int max, Error error)
        {
            string value = propertySelector(_model);

            if (value == null || value.Length < min || value.Length > max)
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> MinimumLength(Func<TModel, string> propertySelector, int minLength, Error error)
        {
            string value = propertySelector(_model);

            if (value == null || value.Length < minLength)
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> MaximumLength(Func<TModel, string> propertySelector, int maxLength, Error error)
        {
            string value = propertySelector(_model);

            if (value != null && value.Length > maxLength)
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> Matches(Func<TModel, string> propertySelector, string pattern, Error error)
        {
            string value = propertySelector(_model);

            if (value == null || !Regex.IsMatch(value, pattern))
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> EmailAddress(Func<TModel, string> propertySelector, Error error)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            return Matches(propertySelector, emailPattern, error);
        }

        public AbstractValidator<TModel> CreditCard(Func<TModel, string> propertySelector, Error error)
        {
            string creditCardPattern = @"^\d{4}-?\d{4}-?\d{4}-?\d{4}$";

            return Matches(propertySelector, creditCardPattern, error);
        }

        public AbstractValidator<TModel> Url(Func<TModel, string> propertySelector, Error error)
        {
            string urlPattern = @"^(http|https)://[^\s/$.?#].[^\s]*$";

            return Matches(propertySelector, urlPattern, error);
        }

        public AbstractValidator<TModel> Contains(Func<TModel, string> propertySelector, string substring, Error error)
        {
            var value = propertySelector(_model);

            if (value == null || !value.Contains(substring))
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> StartsWith(Func<TModel, string> propertySelector, string prefix, Error error)
        {
            string value = propertySelector(_model);

            if (value == null || !value.StartsWith(prefix))
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> EndsWith(Func<TModel, string> propertySelector, string suffix, Error error)
        {
            string value = propertySelector(_model);

            if (value == null || !value.EndsWith(suffix))
            {
                _errors.Add(error);
            }
            return this;
        }

        // Numeric Validations
        public AbstractValidator<TModel> InclusiveBetween<TProperty>(Func<TModel, TProperty> propertySelector, TProperty min, TProperty max, Error error) where TProperty : IComparable
        {
            var value = propertySelector(_model);
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> ExclusiveBetween<TProperty>(Func<TModel, TProperty> propertySelector, TProperty min, TProperty max, Error error) where TProperty : IComparable
        {
            var value = propertySelector(_model);
            if (value.CompareTo(min) <= 0 || value.CompareTo(max) >= 0)
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> GreaterThan<TProperty>(Func<TModel, TProperty> propertySelector, TProperty minValue, Error error) where TProperty : IComparable
        {
            if (propertySelector(_model).CompareTo(minValue) <= 0)
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> LessThan<TProperty>(Func<TModel, TProperty> propertySelector, TProperty maxValue, Error error) where TProperty : IComparable
        {
            if (propertySelector(_model).CompareTo(maxValue) >= 0)
            {
                _errors.Add(error);
            }
            return this;
        }
        // Date and Time Validations

        public AbstractValidator<TModel> Before(Func<TModel, DateTime> propertySelector, DateTime date, Error error)
        {
            if (propertySelector(_model) >= date)
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> After(Func<TModel, DateTime> propertySelector, DateTime date, Error error)
        {
            if (propertySelector(_model) <= date)
            {
                _errors.Add(error);
            }
            return this;
        }


        // Enum Validations
        public AbstractValidator<TModel> IsInEnum<TProperty>(Func<TModel, TProperty> propertySelector, Error error) where TProperty : Enum
        {
            if (!Enum.IsDefined(typeof(TProperty), propertySelector(_model)))
            {
                _errors.Add(error);
            }
            return this;
        }

        // Custom Validations
        public AbstractValidator<TModel> Must(Func<TModel, bool> predicate, Error error)
        {
            if (!predicate(_model))
            {
                _errors.Add(error);
            }
            return this;
        }



        // Conditional Validations
        public AbstractValidator<TModel> When(Func<TModel, bool> condition, Action<AbstractValidator<TModel>> validationAction)
        {
            if (condition(_model))
            {
                validationAction(this);
            }
            return this;
        }

        public AbstractValidator<TModel> Unless(Func<TModel, bool> condition, Action<AbstractValidator<TModel>> validationAction)
        {
            if (!condition(_model))
            {
                validationAction(this);
            }
            return this;
        }

        // Comparison Validations

        public AbstractValidator<TModel> LessThanOrEqualTo<TProperty>(Func<TModel, TProperty> propertySelector, TProperty targetValue, Error error) where TProperty : IComparable
        {
            if (propertySelector(_model).CompareTo(targetValue) > 0)
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> GreaterThanOrEqualTo<TProperty>(Func<TModel, TProperty> propertySelector, TProperty targetValue, Error error) where TProperty : IComparable
        {
            if (propertySelector(_model).CompareTo(targetValue) < 0)
            {
                _errors.Add(error);
            }
            return this;
        }

        // Boolean Validations
        public AbstractValidator<TModel> True(Func<TModel, bool> propertySelector, Error error)
        {

            if (!propertySelector(_model))
            {
                _errors.Add(error);
            }
            return this;
        }

        public AbstractValidator<TModel> False(Func<TModel, bool> propertySelector, Error error)
        {
            if (propertySelector(_model))
            {
                _errors.Add(error);
            }
            return this;
        }


    }


}
