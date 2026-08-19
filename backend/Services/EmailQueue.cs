using System.Threading.Channels;

namespace backend.Services;
public record EmailMessage(
    string Email,
    string ConfirmationLink
);
public interface IEmailQueue
{
    ValueTask EnqueueAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default
    );
    ValueTask<EmailMessage> DequeueAsync(
        CancellationToken cancellationToken
    );
}
public class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _queue;
    public EmailQueue()
    {
        _queue = Channel.CreateBounded<EmailMessage>(
            new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            }
        );
    }
    public ValueTask EnqueueAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default
    )
    {
        return _queue.Writer.WriteAsync(
            message,
            cancellationToken
        );
    }
    public ValueTask<EmailMessage> DequeueAsync(
        CancellationToken cancellationToken
    )
    {
        return _queue.Reader.ReadAsync(
            cancellationToken
        );
    }
}