namespace RuleFilterParser.Exceptions;

public class ObjectIsNotFilterException : Exception
{
    public ObjectIsNotFilterException() : base($"Given object is not of type Filter.")
    {
    }
}