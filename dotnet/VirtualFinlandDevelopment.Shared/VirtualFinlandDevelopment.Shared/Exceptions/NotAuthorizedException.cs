using System.Globalization;

namespace VirtualFinlandDevelopment.Shared.Exceptions;

[Serializable]
public sealed class NotAuthorizedException : Exception
{
    public NotAuthorizedException()
    {
    }

    public NotAuthorizedException(string message) : base(message)
    {
    }

    public NotAuthorizedException(string message, params object[] args)
        : base(string.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}
