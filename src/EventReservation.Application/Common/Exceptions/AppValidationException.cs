namespace EventReservation.Application.Common.Exceptions;

public sealed class AppValidationException : Exception
{
    public AppValidationException(
        IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}