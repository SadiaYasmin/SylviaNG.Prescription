namespace SylviaNG.Prescription.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}
