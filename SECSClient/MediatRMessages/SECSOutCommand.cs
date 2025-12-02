using MediatR;
using Secs4Net;

namespace SECSClient.MediatRMessages
{
    // S6F11 事件報告指令
    public class S6F11OutCommand : IRequest
    {
        public Item Report { get; set; }
    }

    // S5F1 Alarm 報告指令
    public class S5F1OutCommand : IRequest
    {
        public Item Alarm { get; set; }
    }
}