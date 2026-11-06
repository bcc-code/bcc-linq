namespace BccCode.Linq.Server.Exceptions;

public class IncorrectTypeForOperandException : Exception
{
    public IncorrectTypeForOperandException(string operand, string correctType)
        : base($"Incorrect type for \"{operand}\", it should be {correctType}")
    {
    }
}
