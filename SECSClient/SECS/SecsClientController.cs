using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Secs4Net;
using SECSClient.Contracts;
using SECSClient.Logging;
using SECSClient.MediatRMessages;

namespace SECSClient.Services
{
    /// <summary>
    /// Passive SECS/HSMS client 背景服務：
    /// 1) 啟動連線（綁定 appsettings.json 的 Ip/Port）
    /// 2) 監聽所有 Primary 訊息，顯示在 UI
    /// 3) 以 SxF(F+1) 形式回覆 Secondary（示範用途，可依設備協議改寫）
    /// </summary>

    public sealed class SecsClientController : BackgroundService, INotificationHandler<MRPublishMessage>
    {
        private readonly ILogger<SecsClientController> _logger;
        private readonly ISecsConnection _connection;
        private readonly ISecsGem _gem;
        private readonly LogBuffer _buffer;
        private readonly SecsGemOptions _options;
        private readonly Dictionary<int, ISecsStreamHandler> _streamHandlers;
        private readonly IMediator _mediator;

        public volatile bool IsSelected = false;

        public SecsClientController(
            ILogger<SecsClientController> logger,
            ISecsConnection hsmsConnection,
            ISecsGem secsGem,
            LogBuffer buffer,
            IOptions<SecsGemOptions> options,
            IEnumerable<ISecsStreamHandler> streamHandlers,
            IMediator mediator)
        {
            _logger = logger;
            _connection = hsmsConnection;
            _gem = secsGem;
            _buffer = buffer;
            _options = options.Value;
            _streamHandlers = new Dictionary<int, ISecsStreamHandler>();
            _mediator = mediator;
            foreach (var handler in streamHandlers)
            {
                _streamHandlers[handler.Stream] = handler;
            }
        }

        // BackgroundService
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            try
            {
                ShowCurrentSecsSettings();
                _connection.Start(ct);
                _buffer.Add("🚀 Passive HSMS listening… Waiting for Host to connect.");
                await foreach (var e in _gem.GetPrimaryMessageAsync(ct))
                {
                    try
                    {
                        // 注意：這裡使用 e.PrimaryMessage（可能是 IDisposable，保留 using）
                        var primary = e.PrimaryMessage;
                        // （可選）維護全域狀態
                        if (primary.S == 1 && primary.F == 13) IsSelected = true;
                        if (primary.S == 1 && primary.F == 15) IsSelected = false;

                        // ✅ 採方案 B：直接把 Secs4Net.PrimaryMessageWrapper e 傳入
                        if (_streamHandlers.TryGetValue(primary.S, out var handler))
                        {
                            await handler.HandleAsync(e, _buffer, ct);
                        }
                        else
                        {
                            _buffer.Add($"🔔 收到未定義的 S{primary.S}（無對應 Handler）");
                        }
                    }
                    catch (Exception ex)
                    {
                        _buffer.Add($"❌ 處理訊息時發生錯誤：{ex.Message}");
                        _logger.LogError(ex, "處理 SECS Primary Message 時發生例外");
                    }
                }
            }
            catch (Exception ex)
            {
                _buffer.Add($"❌ 背景服務執行時發生錯誤：{ex.Message}");
                _logger.LogError(ex, "SecsClientService.ExecuteAsync 發生例外");
            }
        }
        // MediatR Notification
        public async Task Handle(MRPublishMessage note, CancellationToken ct)
        {
            _buffer.Add("Receive PLC publish message " + note.JsonData);
            await Task.CompletedTask;
        }

        // MediatR Request by ControllerSispatcher
        public async Task<bool> Handle(MRRequestrMessage req, CancellationToken ct)
        {
            if (!string.Equals(req.Receiver, "SOCKET", StringComparison.OrdinalIgnoreCase))
                return false;

            switch (req.Type?.ToUpperInvariant())
            {
                case "CONNECT": await ConnectAsync(ct); return true;
                //case "SEND": await SendAsync(req.Arguments?["data"], ct); return true;
                case "DISCONNECT": await DisconnectAsync(ct); return true;
                default: return false;
            }
        }

        // 你的 SecsClient 內部操作方法(可選)
        public Task ConnectAsync(CancellationToken ct) => Task.CompletedTask;
        public Task SendAsync(string? data, CancellationToken ct) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;

        private void ShowCurrentSecsSettings()
        {
            _buffer.Add("🔍 目前 SECS/HSMS 設定：");
            _buffer.Add($"    模式: {(_options.IsActive ? "Active" : "Passive")}");
            _buffer.Add($"    IP: {_options.IpAddress}");
            _buffer.Add($"    Port: {_options.Port}");
            _buffer.Add($"    DeviceId: {_options.DeviceId}");
            _buffer.Add($"    T3 Timeout: {_options.T3} ms");
            _buffer.Add($"    T5 Timeout: {_options.T5} ms");
            _buffer.Add($"    T6 Timeout: {_options.T6} ms");
            _buffer.Add($"    T7 Timeout: {_options.T7} ms");
            _buffer.Add($"    T8 Timeout: {_options.T8} ms");
            _buffer.Add($"    LinkTest Interval: {_options.LinkTestInterval} ms");
            _logger.LogInformation("Current SECS/HSMS Settings: {@options}", _options);
        }

        /// <summary>
        /// 服務停止時額外清理（可選）。
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _buffer.Add("🛑 Stopping SECS client…");
            }
            finally
            {
                await base.StopAsync(cancellationToken);
            }
        }
    }
}