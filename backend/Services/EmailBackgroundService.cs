namespace backend.Services;
public class EmailBackgroundService(
    IEmailQueue queue,
    IEmailSender emailSender,
    ILogger<EmailBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken
    )
    {
        logger.LogInformation(
            "Email background service started"
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await queue.DequeueAsync(
                    stoppingToken
                );
                await emailSender.SendConfirmEmail(
                    message.Email,
                    message.ConfirmationLink,
                    stoppingToken
                );
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while processing email queue"
                );
            }
        }
    }
}