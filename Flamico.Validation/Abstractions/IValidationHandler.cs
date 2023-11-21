using Flaminco.Validation.Models;

namespace Flaminco.ManualMapper.Abstractions;

public interface IValidationHandler<TInput> where TInput : notnull
{
    Result Handler(TInput input);
}