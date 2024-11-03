namespace Flaminco.AzureBus.AMQP.Attributes;

/// <summary>
///     Specifies that a class is a consumer of messages from a specific topic and subscription in a messaging system.
///     This attribute can only be applied to classes and is used to associate the class with a topic, subscription, and an
///     optional rule filter.
/// </summary>
/// <remarks>
///     Constructor to require Topic and Subscription.
/// </remarks>
/// <param name="topic">The name of the topic.</param>
/// <param name="subscription">The name of the subscription.</param>
/// <param name="ruleFilterType">The type of the rule filter.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TopicConsumerAttribute(string topic, string subscription, Type? ruleFilterType) : Attribute
{
    /// <summary>
    ///     Gets the name of the topic that the class will consume messages from.
    /// </summary>
    public string Topic { get; } = topic ?? throw new ArgumentNullException(nameof(topic));

    /// <summary>
    ///     Gets the name of the subscription within the topic that the class will consume messages from.
    /// </summary>
    public string Subscription { get; } = subscription ?? throw new ArgumentNullException(nameof(subscription));

    /// <summary>
    ///     The type of rule filter provider that will be resolved from the DI container.
    /// </summary>
    public Type? RuleFilterType { get; set; } = ruleFilterType;
}