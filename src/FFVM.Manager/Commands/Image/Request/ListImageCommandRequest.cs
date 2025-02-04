using FFVM.Base.Utility;
using FFVM.Manager.Commands.BaseTypes;
using FFVM.Manager.Commands.BaseTypes.Attributes;
using FFVM.Manager.Models.Enums;

namespace FFVM.Manager.Commands.Image.Request;

public class ListImageCommandRequest : IRepositorySelectable
{
    [CommandParameter("type", order: 1, description: "The type of image list requests, types are (available, installed).", customFormatterType: typeof(EnumParameterFormatter<ListCommandType>))]
    public ListCommandType CommandType { get; set; }

    [RepositoryAliasCommandParameter(order: 2)]
    public string? RepositoryName { get; set; }
}
