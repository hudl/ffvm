using FFVM.Base.Utility;

namespace FFVM.Manager.Commands.BaseTypes.Attributes;

internal class RepositoryAliasCommandParameterAttribute(int order = -1, bool isrequired = false) 
    : CommandParameterAttribute("repo", description: "A short name or alias for the repository version, default is selected repository.", order: order, isrequired: isrequired)
{
}
