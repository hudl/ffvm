namespace FFVM.Base.Config.BaseTypes;

public interface IConfigurationProvider
{
    Configuration Configuration { get; }
    Task Save();
}
