using System.Security;

namespace FFVM.Base.IO.BaseTypes;

public interface IInputsProvider
{
    bool GetYN(string promptMessage);
    string GetValue(string promptMessage);
    string GetValue(string promptMessage, Predicate<string> isPromptValid);
    SecureString GetSecretValue(string promptMessage);
    SecureString GetSecretValue(string promptMessage, Predicate<SecureString> isPromptValid);
}
