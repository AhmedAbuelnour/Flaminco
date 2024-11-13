using ErrorOr;
using Flaminco.Validation.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Flaminco.Validation.Abstractions;

/// <summary>
///     Defines a handler for validating input asynchronously.
/// </summary>
/// <typeparam name="TInput">The type of the input to validate.</typeparam>
public interface IValidationHandler<TInput> where TInput : IValidatableObject
{
    /// <summary>
    ///     Handles the asynchronous validation of the input.
    /// </summary>
    /// <param name="input">The input to validate.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation, containing an <see cref="ErrorOr{T}" /> result indicating
    ///     success or failure.
    /// </returns>
    ValueTask<ErrorOr<Success>> Handler(TInput input, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Validates the model using data annotations.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <returns>An <see cref="ErrorOr{T}" /> result containing validation errors or success.</returns>
    public virtual ErrorOr<Success> TryDataAnnotationValidate(TInput model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, true);
        return results.ConvertToErrorOr();
    }
}