namespace SylviaNG.Prescription.Application.Common.Exceptions
{
    /// <summary>
    /// A request that is well-formed but violates a business rule (e.g. disabling/deleting the
    /// system default template). Maps to HTTP 400, distinct from FluentValidation's
    /// <see cref="FluentValidation.ValidationException"/> which covers shape/field validation.
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
