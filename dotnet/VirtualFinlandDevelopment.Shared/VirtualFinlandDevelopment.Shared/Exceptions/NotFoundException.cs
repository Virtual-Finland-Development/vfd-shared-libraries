using System.Globalization;

namespace VirtualFinlandDevelopment.Shared.Exceptions;

[Serializable]
public sealed class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, params object[] args) : base(string.Format(CultureInfo.CurrentCulture,
        message, args))
    {
    }
}
