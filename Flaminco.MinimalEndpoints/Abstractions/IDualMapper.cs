namespace Flaminco.MinimalEndpoints.Abstractions
{

    /// <summary>
    /// Interface for dual mapping between two types.
    /// </summary>
    /// <typeparam name="TFrom">The type to map from.</typeparam>
    /// <typeparam name="TTo">The type to map to.</typeparam>
    public interface IDualMapper<TFrom, TTo>
        where TFrom : new()
        where TTo : new()
    {
        /// <summary>
        /// Maps an object from type <typeparamref name="TFrom"/> to type <typeparamref name="TTo"/>.
        /// </summary>
        /// <param name="from">The object to map from.</param>
        /// <returns>The mapped object of type <typeparamref name="TTo"/>.</returns>
        TTo MapTo(TFrom from);

        /// <summary>
        /// Maps an object from type <typeparamref name="TTo"/> to type <typeparamref name="TFrom"/>.
        /// </summary>
        /// <param name="to">The object to map from.</param>
        /// <returns>The mapped object of type <typeparamref name="TFrom"/>.</returns>
        TFrom MapFrom(TTo to);
    }

}
