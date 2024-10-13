namespace WebApplication1.HostedServices
{
    public static class SharedInteractionDTO
    {
        public class Response
        {
            public Guid InteractionId { get; set; }
            public string ClipName { get; set; }
            public int ClipId { get; set; }

        }

        public class Request
        {
            public string TeacherId { get; set; }
            public string ClipName { get; set; }
            public Guid InteractionId { get; set; }
            public IEnumerable<int> Courses { get; set; }

        }
    }

    //public class PersonHandler(ApplicationDbContext context, ISender _sender) : IMessageReceivedHandler<string>, IMessageFaultHandler<string>
    //{
    //    public async Task Handle(MessageReceivedEvent<string> notification, CancellationToken cancellationToken)
    //    {
    //        // await context.Roles.ToListAsync(cancellationToken);
    //        Console.WriteLine($"I got a new message saying: {notification.Message}");
    //    }

    //    public async Task Handle(MessageFaultEvent<string> notification, CancellationToken cancellationToken)
    //    {
    //        Console.WriteLine($"fault message from: {notification.Name}, and queue: {notification.Queue}");
    //    }
    //}

}
