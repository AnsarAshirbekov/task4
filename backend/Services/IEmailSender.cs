namespace backend.Services;
public interface IEmailSender
{
    Task SendConfirmEmail(
        string email,
        string confirmLink,
        CancellationToken cancellationToken = default
    );
}