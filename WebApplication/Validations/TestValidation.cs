using ErrorOr;
using Flaminco.Validation.Abstractions;
using Flaminco.Validation.Extensions;
using System.ComponentModel.DataAnnotations;
using IValidatableObject = Flaminco.Validation.Abstractions.IValidatableObject;

namespace WebApplication1.Validations
{
    public class Person : IValidatableObject
    {
        [MinLength(3)]
        public string Name { get; set; }
        [Range(18, 55)]
        public int Age { get; set; }
    }
    public class PersonValidation : IValidationHandler<Person>
    {
        public async ValueTask<ErrorOr<Success>> Handler(Person input, CancellationToken cancellationToken = default)
        {
            ErrorOr<Success> dataAnnotationErrors = input.TryDataAnnotationValidate();

            return dataAnnotationErrors;
        }
    }
}
