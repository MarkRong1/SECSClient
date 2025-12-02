using MediatR;
using Secs4Net;
using SECSClient.Contracts;
using SECSClient.Logging;

namespace SECSClient.SECS.SECS2Handler
{
    public sealed class S5InHandler : ISecsStreamHandler
    {
        public int Stream => 5;
        private readonly IServiceProvider _provider;
        private readonly IMediator _mediator;

        public S5InHandler(IServiceProvider provider, IMediator mediator)
        {
            _provider = provider;
            _mediator = mediator;
        }

        public async Task HandleAsync(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            var primary = e.PrimaryMessage;
            var f = primary.F;

            //buffer.Add($"📩 [S2] 收到 F{f} {(primary.Name ?? string.Empty)}");
            //buffer.Add($"    Item: {primary.SecsItem}");

            switch (f)
            {
                case 3: // S2F13 Equipment Status Request
                    await HandleF3Async(e, buffer, ct);
                    break;
                default:
                    buffer.Add($"ℹ️ [S2] 未定義的 F{f}，暫不處理");
                    break;
            }
        }

        private async Task HandleF3Async(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            var sReturn = new SecsMessage(5, 4)
            {
                SecsItem = Item.B(0)
            };
            await e.TryReplyAsync(sReturn, ct);
        }
    }
}