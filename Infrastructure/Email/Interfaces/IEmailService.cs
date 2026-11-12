namespace Infrastructure.Email.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken = default);
}
