using MediatR;
using Microsoft.Extensions.Hosting;
using SECSClient.Equipment.Interface;
using SECSClient.Logging;
using SECSClient.MediatRMessages;

namespace SECSClient.Equipment.GPIO
{

    public class GPIOController_FT232H : BackgroundService, INotificationHandler<MRPublishMessage>, IGPIOController
    {
        private readonly IMediator _mediator;
        private readonly LogBuffer _buffer;
        private readonly string _logFolder="GPIO";
        public GPIOController_FT232H(IMediator mediator, LogBuffer buffer)
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
            if (!string.Equals(req.Receiver, "GPIO", StringComparison.OrdinalIgnoreCase))
                return false;

            switch (req.Receiver?.ToUpperInvariant())
            {
                case "CONNECT": await ConnectAsync(ct); return true;
                //case "SEND": await SendAsync(req.Arguments?["data"], ct); return true;
                case "DISCONNECT": await DisconnectAsync(ct); return true;
                default: return false;
            }
        }

        // 你的 Socket 內部操作方法(可選)
        public Task ConnectAsync(CancellationToken ct)
        {
            _buffer.AddFor("Modbus", "Modbus background service connected."); // 只寫檔，不更新 UI
            return  Task.CompletedTask;
        }
        public Task SendAsync(CancellationToken ct) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;


    }

}