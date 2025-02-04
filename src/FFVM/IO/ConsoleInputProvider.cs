using FFVM.Base.IO.BaseTypes;
using System.Security;

namespace FFVM.Base.IO; // Note: actual namespace depends on the project name.

public class ConsoleInputProvider(ILogger _logger) : IInputsProvider
{
    public const string InvalidReponseString = "Your response was invalid, please try again.";

    public bool GetYN(string promptMessage)
    {
        Console.ForegroundColor = ConsoleColor.White;
        var promptValue = GetValue($"{promptMessage} (Y/N):",
            p => !string.IsNullOrWhiteSpace(p)
             && (p.Equals("y", StringComparison.OrdinalIgnoreCase)
              || p.Equals("n", StringComparison.OrdinalIgnoreCase)));
        return promptValue.Equals("y", StringComparison.OrdinalIgnoreCase);
    }
    public string GetValue(string promptMessage) => GetValue(promptMessage, s => !string.IsNullOrWhiteSpace(s));
    public string GetValue(string promptMessage, Predicate<string> isPromptValid)
    {
        string? promptValue;
        var showInvalidPrompt = false;
        Console.ForegroundColor = ConsoleColor.White;
        do
        {
            if (showInvalidPrompt)
            {
                _logger.WriteStdErr(InvalidReponseString);
            }

            showInvalidPrompt = true;
            Console.Write(promptMessage);
            promptValue = Console.ReadLine() ?? string.Empty;
        }
        while (!isPromptValid(promptValue));

        return promptValue;
    }
    public SecureString GetSecretValue(string promptMessage) => GetSecretValue(promptMessage, s => s.Length > 0);
    public SecureString GetSecretValue(string promptMessage, Predicate<SecureString> isPromptValid)
    {
        SecureString? password;
        var showInvalidPrompt = false;
        Console.ForegroundColor = ConsoleColor.White;
        do
        {
            if (showInvalidPrompt)
            {
                _logger.WriteStdErr(InvalidReponseString);
            }

            password = new();
            showInvalidPrompt = true;
            Console.Write(promptMessage);
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password.RemoveAt(password.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password.AppendChar(keyInfo.KeyChar);
                }
            }
            while (key != ConsoleKey.Enter);
            Console.WriteLine();

        }
        while (!isPromptValid(password));

        return password;
    }
}