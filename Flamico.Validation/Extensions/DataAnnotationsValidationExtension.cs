using Flaminco.Validation.Models;
using System.ComponentModel.DataAnnotations;

namespace Flaminco.Validation.Extensions
{
    public static class DataAnnotationsValidationExtension
    {
        public static bool TryDataAnnotationValidate<TInput>(this TInput model, out Error[] errors) where TInput : notnull
        {
            ICollection<ValidationResult> results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

            errors = isValid
                ? Array.Empty<Error>()
                : results.Where(a => a.MemberNames.Any()).Select(error => new Error(error.MemberNames.First(), error.ErrorMessage)).ToArray();

            return isValid;
        }
    }
}
