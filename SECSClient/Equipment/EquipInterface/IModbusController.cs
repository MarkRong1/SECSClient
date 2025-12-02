using SECSClient.MediatRMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SECSClient.Equipment.Interface
{
    public interface IModbusController
    {
        Task<bool> Handle(MRRequestrMessage req, CancellationToken ct);
        Task ConnectAsync(CancellationToken ct);
        Task SendAsync(string? data,CancellationToken ct);
        Task DisconnectAsync(CancellationToken ct);

    }
}
