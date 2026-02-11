using LowCodeHub.Operations;

namespace Flaminco.POC.Operations
{
    [Operation]
    public partial class AttachmentGetAllOperation
    {
        private readonly ILogger<AttachmentGetAllOperation> _logger;
        public AttachmentGetAllOperation(ILogger<AttachmentGetAllOperation> logger)
        {
            _logger = logger;
        }

        public async Task<List<string>> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executing AttachmentGetAllOperation");

            // Simulate fetching attachments (replace with actual logic)
            await Task.Delay(500, cancellationToken); // Simulate async work

            List<string> attachments = new List<string>
            {
                "Attachment1.pdf",
                "Attachment2.docx",
                "Attachment3.xlsx"
            };
            _logger.LogInformation("Fetched {Count} attachments", attachments.Count);

            return attachments;
        }
    }
}
