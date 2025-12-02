using MediatR;
using SECSClient.Equipment.Interface;
using SECSClient.Equipment.Socket;
using SECSClient.MediatRMessages;
using SECSClient.Services;

namespace SECSClient.Equipment
{
    public sealed class EquipControllerDispatcher : IRequestHandler<MRRequestrMessage, bool>
    {
        private readonly IPLCController _plc;
        private readonly ISocketController _socket;

        public EquipControllerDispatcher(IPLCController plc, ISocketController socket)
        {
            _plc = plc;
            _socket = socket;
        }

        public async Task<bool> Handle(MRRequestrMessage req, CancellationToken ct)
        {
            switch (req.Receiver?.ToUpperInvariant())
            {
                case "PLC":
                    return await _plc.Handle(req, ct);     // 若 PlcMonitorService 自己也實作了 Handle，可直接呼叫
                case "SOCKET":
                    return await _socket.Handle(req, ct);  // 同理
                default:
                    return false;
            }
        }
    }
}