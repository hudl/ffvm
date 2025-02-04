namespace FFVM.Base.Commands.BaseTypes;

public interface ICommandHandler
{
    string CommandName { get; }
    string CommandDescription { get; }
    Type? CommandParametersType { get; }
    bool IsCommand();
    bool IsValid();
    Task<int> Process();
}
