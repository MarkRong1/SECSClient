using MediatR;
using Secs4Net;
using SECSClient.Contracts;
using SECSClient.Logging;

namespace SECSClient.SECS.SECS2Handler
{
    public sealed class S1InHandler : ISecsStreamHandler
    {
        public int Stream => 1;
        private readonly IServiceProvider _provider;
        private readonly IMediator _mediator;

        public S1InHandler(IServiceProvider provider, IMediator mediator)
        {
            _provider = provider;
            _mediator = mediator;
        }

        public async Task HandleAsync(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            // 若 PrimaryMessage 需釋放，這裡也可使用 using，但通常在外層已 using 即可
            var primary = e.PrimaryMessage;
            var f = primary.F;

            //buffer.Add($"📩 [S1] 收到 F{f} {(primary.Name ?? string.Empty)}");
            //buffer.Add($"    Item: {primary.SecsItem}");

            switch (f)
            {
                case 3: // S1F3
                    await HandleF3Async(e, buffer, ct);
                    break;
                case 13: // S1F13
                    await HandleF13Async(e, buffer, ct);
                    break;
                case 15: // 視協議調整含意
                    buffer.Add("🔔 S1F15（依協議為 Deselect/Request OFFLINE 等）");
                    break;

                default:
                    buffer.Add($"ℹ️ [S1] 未定義的 F{f}，暫不處理");
                    break;
            }
        }
        private async Task HandleF3Async(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            var requestItems = e.PrimaryMessage.SecsItem;

            // 如果 itemCount = 0，回覆空 List
            var replyItems = new List<Item>();
            for (int i = 0; i < requestItems.Count; i++)
            {
                replyItems.Add(Item.A("1"));
            }

            var sReturn = new SecsMessage(1, 4)
            {
                Name = "EquipmentStatusData",
                SecsItem = Item.L(replyItems.ToArray()) // 如果 replyItems 為空，Item.L() 會是空 List
            };

            await e.TryReplyAsync(sReturn, ct);
        }
        private async Task HandleF13Async(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            //buffer.Add("🔔 S1F13（Establish Communication Request）");
            var sReturn = new SecsMessage(1, 14)
            {
                Name = "EstablishCommunicationAck",
                SecsItem = Item.L(
                    Item.B(0),
                    Item.L(Item.A("MyModel"), Item.A("v1.0.0"))
                )
            };
            await e.TryReplyAsync(sReturn, ct);
            //buffer.Add("➡️ 已回覆 S1F14（Establish Communication Acknowledge）");
        }
    }
}