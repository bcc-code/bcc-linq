namespace BccCode.Linq.Server.Exceptions;

public class GettingMethodByReflectionException : Exception
{
    public GettingMethodByReflectionException(string typeName)
        : base($"Could not get method from type \"{typeName}\" by reflection.")
    {
    }
    public GettingMethodByReflectionException(string methodName, string typeName)
        : base($"Could not get method \"{methodName}\" from type \"{typeName}\" by reflection.")
    {
    }
}
