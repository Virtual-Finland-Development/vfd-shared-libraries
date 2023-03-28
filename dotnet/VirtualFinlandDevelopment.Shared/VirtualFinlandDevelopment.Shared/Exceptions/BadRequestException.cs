using System.Globalization;

namespace VirtualFinlandDevelopment.Shared.Exceptions;

[Serializable]
public sealed class BadRequestException : Exception
{
    public BadRequestException()
    {
    }

    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, List<ValidationErrorDetail> validationErrors) : base(message)
    {
        ValidationErrors = validationErrors;
    }

    public BadRequestException(string message, params object[] args)
        : base(string.Format(CultureInfo.CurrentCulture, message, args))
    {
    }

    public List<ValidationErrorDetail>? ValidationErrors { get; }
}
