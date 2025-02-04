using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Utility;

namespace FFVM.Base.Commands.BaseTypes;

public abstract class BaseCommandHandler<TParameterType>(
    ProgramContext _context, 
    ILogger _logger, 
    string _commandName, 
    string _commandDescription) : ICommandHandler
    where TParameterType : class, new()
{
    protected const string DockerHubRepositoryType = "DOCKER_HUB";
    protected const string AwsEcrRepositoryType = "AWS_ECR";

    protected readonly string _commandName = _commandName;
    protected readonly string _commandDescription = _commandDescription;
    protected readonly ProgramContext _context = _context;
    protected readonly ILogger _logger = _logger;
    protected TParameterType? _commandParameters;

    public string CommandName => _commandName;
    public string CommandDescription => _commandDescription;
    public Type CommandParametersType => typeof(TParameterType);
    protected TParameterType? Parameters => _commandParameters;
    public bool IsValid()
    {
        _commandParameters = new TParameterType();
        CommandParameterUtility.ParseArgumentsIntoCommandParameters(_commandParameters, _context.Arguments);
        return _commandParameters != null;
    } 
    public bool IsCommand() => _context.Command.Equals(CommandName, StringComparison.Ordinal);
    public abstract Task<int> Process();
}
