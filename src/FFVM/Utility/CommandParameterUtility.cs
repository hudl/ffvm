using FFVM.Base.Utility.BaseTypes;

namespace FFVM.Base.Utility;

public class CommandParameterUtility
{
    public static bool IsNamedParameter(string value) => value.StartsWith("--");
    public static bool IsParameterValue(string value) => !IsNamedParameter(value);
    public static Dictionary<string, CommandParameterPair> GetPropertiesAndAttributes<TObjectType>()
        where TObjectType : class => GetPropertiesAndAttributes(typeof(TObjectType));
    public static Dictionary<string, CommandParameterPair> GetPropertiesAndAttributes(Type? objectType)
    {
        if (objectType == null) return [];

        var rawObjectProperties = objectType?.GetProperties();
        return rawObjectProperties
            ?.Select(pi => new CommandParameterPair(pi))
            ?.Where(pi => pi.Attribute != null)
            ?.ToDictionary(pi => pi.Attribute!.Name, pi => pi) ?? [];
    }

    public static void ParseArgumentsIntoCommandParameters<TObjectType>(TObjectType targetObject, List<string> args)
        where TObjectType : class
    {
        var rawObjectProperties = typeof(TObjectType).GetProperties();
        var propertiesAndPairedAttribute = GetPropertiesAndAttributes<TObjectType>();
        var propertiesList = propertiesAndPairedAttribute.Select(kvp => kvp.Value).ToList();

        var argumentIndex = 0;
        var namedArgumentsPresent = false; 
        while (argumentIndex < args.Count)
        {
            var argumentValue = args[argumentIndex];
            if (!IsNamedParameter(argumentValue) && namedArgumentsPresent)
            {
                throw new ArgumentException($"Positional arguments must all appear before named arguments: '{argumentValue}' at position {argumentIndex}");
            }

            if (!IsNamedParameter(argumentValue))
            {
                var positionalParameterData = propertiesList.FirstOrDefault(prop => prop?.Attribute?.Order == (argumentIndex + 1)) 
                    ?? throw new ArgumentException($"Positional argument is invalid, and has no matching command settings: '{argumentValue}' at position {argumentIndex} ");
                SetPropertyValue(targetObject, positionalParameterData, args, argumentIndex);
            }
            else
            {
                namedArgumentsPresent = true; 
                var parameterName = args[argumentIndex][2..];
                if (!propertiesAndPairedAttribute.TryGetValue(parameterName, out var parameterData))
                {
                    throw new ArgumentException($"Named argument is invalid, and has no matching command settings: '{parameterName}'");
                }
                if ((parameterData?.Attribute?.IsValueless ?? true) && argumentIndex + 1 == args.Count)
                {
                    throw new ArgumentException($"Parameter is missing a required value {args[argumentIndex]}");
                }
                
                argumentIndex++;
                SetPropertyValue(targetObject, parameterData, args, argumentIndex);
            }

            argumentIndex++; 
        }
    }

    private static void SetPropertyValue<TObjectType>(TObjectType targetObject, CommandParameterPair? parameterData, List<string> arguments, int argumentIndex)
        where TObjectType : class
    {
        if (parameterData?.Attribute == null)
        {
            return; 
        }

        if (parameterData.Attribute.IsValueless && IsNamedParameter(arguments[argumentIndex]))
        {
            throw new ArgumentException($"Parameter is missing a required value {arguments[argumentIndex]}");
        }

        if (!parameterData.Attribute.IsValueless)
        {
            parameterData.PropertyInfo.SetValue(targetObject, true);
            return;
        }

        object? objectValue = arguments[argumentIndex];
        if (parameterData.Attribute.CustomFormatterType != null)
        {
            var customFormatter = Activator.CreateInstance(parameterData.Attribute.CustomFormatterType) as ICommandParameterFormatter
                ?? throw new ArgumentException($"Parameter formatter is invalid {arguments[argumentIndex]}");
            objectValue = customFormatter.FormatValue(arguments[argumentIndex]);
        }

        parameterData.PropertyInfo.SetValue(targetObject, objectValue);
    }
}
