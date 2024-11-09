namespace Flaminco.QueryableExtensions.Models;

public class AttachedPagedList<TItem, TAttachment> : PagedList<TItem>
{
    public TAttachment? Attachment { get; set; }
}