using FFVM.Base.Shell.Models;

namespace FFVM.Base.Shell.BaseTypes;

public interface IFFUtilityEmulator
{
    Task<int> Run(EmulatorRequest emulatorRequest);
}