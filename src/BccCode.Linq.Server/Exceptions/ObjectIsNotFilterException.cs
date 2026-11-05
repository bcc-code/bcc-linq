namespace BccCode.Linq.Server.Exceptions;

public class ObjectIsNotFilterException : Exception
{
    public ObjectIsNotFilterException() : base($"Given object is not of type Filter.")
    {
    }
}
