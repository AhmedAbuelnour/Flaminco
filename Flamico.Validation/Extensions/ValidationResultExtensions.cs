using Flaminco.Validation.Models;
using System.ComponentModel.DataAnnotations;

namespace Flaminco.Validation.Extensions
{
    public static class ValidationResultExtensions
    {
        public static Result ToResult(this List<ValidationResult> errors)
        {
            return Result.Failure(errors.Select(a => new Error(a.MemberNames.FirstOrDefault(), a.ErrorMessage)));
        }
    }
}
