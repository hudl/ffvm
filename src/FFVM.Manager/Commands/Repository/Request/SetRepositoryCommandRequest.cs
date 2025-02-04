using FFVM.Manager.Commands.BaseTypes.Attributes;

namespace FFVM.Manager.Commands.Repository.Request;

public class SetRepositoryCommandRequest
{
    [RepositoryAliasCommandParameter(order: 1, isrequired: true)]
    public string? RepositoryName { get; set; }
}
