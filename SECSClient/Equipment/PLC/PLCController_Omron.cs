using CommunityToolkit.HighPerformance;
using MediatR;
using Microsoft.Extensions.Hosting;
using SECSClient.Equipment.Interface;
using SECSClient.Logging;
using SECSClient.MediatRMessages;


public class PlcMonitorController : BackgroundService, INotificationHandler<MRPublishMessage>, IPLCController
{
    private readonly IMediator _mediator;
    private readonly LogBuffer _buffer;
    private readonly string _logFolder = "PLC";
    public PlcMonitorController(IMediator mediator, LogBuffer buffer)
    {
        _mediator = mediator;
        _buffer = buffer;
    }

    // BackgroundService
    protected override Task ExecuteAsync(CancellationToken ct)
    {
        // 背景輪詢/連線維護…
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
        if (!string.Equals(req.Receiver, "PLC", StringComparison.OrdinalIgnoreCase))
            return false;

        switch (req.Type?.ToUpperInvariant())
        {
            case "START": await StartAsync(ct); return true;
            case "STOP": await StopAsync(ct); return true;
            case "RESTART": await RestartAsync(ct); return true;
            default: return false;
        }
    }

    // 你的 PLC 內部操作方法(可選)
    public async Task StartAsync(CancellationToken ct)
    {
        _buffer.AddFor(_logFolder, "PLC Start!!");
        await _mediator.Publish(new MRPublishMessage
        {
            Sender = "PLC",
            Type = "START",
            sType = "TEST",
            JsonData = "TT"
        }, ct);
    }
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    public Task RestartAsync(CancellationToken ct) => Task.CompletedTask;
}

