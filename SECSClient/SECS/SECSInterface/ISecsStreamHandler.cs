using SECSClient.Logging;

namespace SECSClient.Contracts
{
    public interface ISecsStreamHandler
    {
        int Stream { get; }

        Task HandleAsync(Secs4Net.PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct);
    }
}
