namespace Tgs.Payment.AlfaPay;

internal class ApiFailException : Exception
{
    public ApiFailException()
    {
    }

    public ApiFailException(string? message) : base(message)
    {
    }
}
