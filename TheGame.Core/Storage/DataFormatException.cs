namespace TheGame.Core.Storage;

public sealed class DataFormatException : Exception
{
    public DataFormatException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
