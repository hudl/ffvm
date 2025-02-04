using FFVM.Manager.Commands.BaseTypes;
using FFVM.Manager.Commands.BaseTypes.Attributes;

namespace FFVM.Manager.Commands.Repository.Request;

public class RemoveRepositoryCommandRequest : IRepositorySelectable
{
    [RepositoryAliasCommandParameter(order: 1, isrequired: true)]
    public string? RepositoryName { get; set; }
}
