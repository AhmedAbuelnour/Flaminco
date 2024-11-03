using Flaminco.AzureBus.AMQP.Abstractions;

namespace WebApplication1.Consumers;

public class MessageBox : IMessage
{
    public string NotifierId { get; set; }
    public int CourseId { get; set; }
    public int NotificationTypeId { get; set; }
    public string? Content { get; set; }
    public IEnumerable<string>? NotifiedIds { get; set; }
    public string? Metadata { get; set; }
}