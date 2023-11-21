using Flaminco.Validation.Models;
using System.ComponentModel.DataAnnotations;

namespace Flaminco.Validation.Abstractions;

public interface IValidationAsyncHandler<TInput> where TInput : notnull
{
    Task<Result> Handler(TInput input, CancellationToken cancellationToken = default);

    bool TryDataAnnotationValidate(TInput model, out Error[] errors)
    {
        ICollection<ValidationResult> results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

        errors = isValid
            ? Array.Empty<Error>()
            : results.Where(a => a.MemberNames.Any()).Select(error => new Error(error.MemberNames.First(), error.ErrorMessage)).ToArray();

        return isValid;
    }
}
