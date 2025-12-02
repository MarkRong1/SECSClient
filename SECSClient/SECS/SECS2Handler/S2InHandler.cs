using MediatR;
using Secs4Net;
using SECSClient.Contracts;
using SECSClient.Logging;
using SECSClient.MediatRMessages;

namespace SECSClient.SECS.SECS2Handler
{
    public sealed class S2InHandler : ISecsStreamHandler
    {
        public int Stream => 2;
        private readonly IServiceProvider _provider;
        private readonly IMediator _mediator;

        public S2InHandler(IServiceProvider provider, IMediator mediator)
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
                case 13: // S2F13 Equipment Status Request
                    await HandleF13Async(e, buffer, ct);
                    break;
                case 33: // S2F33 Define Report
                    await HandleF33Async(e, buffer, ct);
                    break;
                case 35: // S2F35 Link Event Report
                    await HandleF35Async(e, buffer, ct);
                    break;
                case 37: // S2F37 Enable/Disable Event Report
                    await HandleF37Async(e, buffer, ct);
                    break;
                case 41: // S2F41 RCMD
                    await HandleF41Async(e, buffer, ct);
                    break;
                default:
                    buffer.Add($"ℹ️ [S2] 未定義的 F{f}，暫不處理");
                    break;
            }
        }

        private async Task HandleF13Async(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            var requestItems = e.PrimaryMessage.SecsItem;

            // 如果 itemCount = 0，回覆空 List
            var replyItems = new List<Item>();
            for (int i = 0; i < requestItems.Count; i++)
            {
                replyItems.Add(Item.A("1"));
            }

            var sReturn = new SecsMessage(2, 14)
            {
                Name = "EquipmentConstantData",
                SecsItem = Item.L(replyItems.ToArray()) // 如果 replyItems 為空，Item.L() 會是空 List
            };

            await e.TryReplyAsync(sReturn, ct);
        }

        private async Task HandleF33Async(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            //buffer.Add("🔔 S2F33（Define Report Request）");
            var sReturn = new SecsMessage(2, 34)
            {
                SecsItem = Item.B(0)
            };
            await e.TryReplyAsync(sReturn, ct);
            //buffer.Add("➡️ 已回覆 S2F34 Define Report Acknowledge）");
        }

        private async Task HandleF35Async(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            //buffer.Add("🔔 S2F35（Link Event Report）");
            var sReturn = new SecsMessage(2, 36)
            {
                SecsItem = Item.B(0)
            };
            await e.TryReplyAsync(sReturn, ct);
            //buffer.Add("➡️ 已回覆 S2F36 Link Event Report Acknowledge）");
        }

        private async Task HandleF37Async(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            //buffer.Add("🔔 S2F37 Enable/Disable Event Report）");
            var sReturn = new SecsMessage(2, 38)
            {
                SecsItem = Item.B(0)
            };
            await e.TryReplyAsync(sReturn, ct);
            //buffer.Add("➡️ 已回覆 S2F38（Enable/Disable Event Report Acknowledge）");
        }

        private async Task HandleF41Async(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            var sReturn = new SecsMessage(2, 42) { SecsItem = Item.B(0) };
            await e.TryReplyAsync(sReturn, ct);

            string rcmd = ParseRcmd(e.PrimaryMessage);
            bool started = await _mediator.Send(new MRRequestrMessage { Receiver = "PLC", Type = rcmd }, ct);
            if (started)
            {
                await _mediator.Send(new S6F11OutCommand
                {
                    Report = Item.L(Item.A("PLC Triggered"), Item.B(new byte[] { 0x01 }))
                }, ct);
            }
            else
            {
                buffer.Add($"找不到對應的裝置控制器: {rcmd}");
            }
        }

        private string ParseRcmd(SecsMessage msg)
        {
            var root = msg.SecsItem;
            if (root?.Items == null)
                return string.Empty;

            var rcmdItem = root.Items[0];
            return rcmdItem.Format == SecsFormat.ASCII ? rcmdItem.GetString() : rcmdItem.ToString();
        }

    }
}