using Flaminco.Validation.Models;

namespace Flaminco.ManualMapper.Abstractions;

public interface IValidationHandler<TInput>
{
    Result Handler(TInput input);
}