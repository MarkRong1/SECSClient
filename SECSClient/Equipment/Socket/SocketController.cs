using MediatR;
using Microsoft.Extensions.Hosting;
using SECSClient.Equipment.Interface;
using SECSClient.Logging;
using SECSClient.MediatRMessages;

namespace SECSClient.Equipment.Socket
{

    public class SocketController : BackgroundService, INotificationHandler<MRPublishMessage>,ISocketController
    {
        private readonly IMediator _mediator;
        private readonly LogBuffer _buffer;
        private readonly string _logFolder = "SOCKET";
        public SocketController(IMediator mediator, LogBuffer buffer)
        {
            _mediator = mediator;
            _buffer = buffer;
        }

        // BackgroundService
        protected override Task ExecuteAsync(CancellationToken ct)
        {
            // Socket 監控/循環邏輯
            return Task.CompletedTask;
        }

        // MediatR Notification
        public async Task Handle(MRPublishMessage note, CancellationToken ct)
        {
            //_buffer.Add("Socket receive PLC message " + note.Sender);
            await Task.CompletedTask;
        }

        // MediatR Request by ControllerSispatcher
        public async Task<bool> Handle(MRRequestrMessage req, CancellationToken ct)
        {
            if (!string.Equals(req.Receiver, "SOCKET", StringComparison.OrdinalIgnoreCase))
                return false;

            switch (req.Receiver?.ToUpperInvariant())
            {
                case "CONNECT": await ConnectAsync(ct); return true;
                //case "SEND": await SendAsync(req.Arguments?["data"], ct); return true;
                case "DISCONNECT": await DisconnectAsync(ct); return true;
                default: return false;
            }
        }

        // 你的 Socket 內部操作方法(
        public Task ConnectAsync(CancellationToken ct) => Task.CompletedTask;
        public Task SendAsync(string? data, CancellationToken ct) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;
    }

}