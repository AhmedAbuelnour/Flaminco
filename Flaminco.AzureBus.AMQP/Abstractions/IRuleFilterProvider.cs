using Azure.Messaging.ServiceBus.Administration;

namespace Flaminco.AzureBus.AMQP.Abstractions
{
    /// <summary>
    /// Represents a provider that generates a specific type of <see cref="RuleFilter"/>.
    /// </summary>
    public interface IRuleFilterProvider
    {
        /// <summary>
        /// Gets the <see cref="RuleFilter"/> used to filter messages based on certain criteria.
        /// </summary>
        /// <returns>
        /// A <see cref="RuleFilter"/> instance or <c>null</c> if no filter is applicable.
        /// </returns>
        RuleFilter? GetRuleFilter();
    }

}
