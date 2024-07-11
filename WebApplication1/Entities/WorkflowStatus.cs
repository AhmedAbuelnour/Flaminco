using Flaminco.ImmutableLookups;

namespace WebApplication1.Entities
{
    public record WorkflowStatus(int Id, int Code, string Module, Dictionary<string, string> Description) : ImmutableLookupEntityBase<int>(Id, Code, Module);
}
