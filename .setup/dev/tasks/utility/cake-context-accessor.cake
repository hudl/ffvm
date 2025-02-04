/// <summary>
/// This class simplifies static access to the global Cake context, for straightforward
/// access from static helpers that aren't able to take advantage of the CakeMethodAliasAttribute
/// feature available to addins.
/// </summary>
public class CakeContextAccessor
{
    public static ICakeContext Context {get;set;}
}

CakeContextAccessor.Context = Context;
