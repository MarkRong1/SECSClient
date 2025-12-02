using SECSClient.MediatRMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SECSClient.Equipment.Interface
{
    public interface IGPIOController
    {
        Task<bool> Handle(MRRequestrMessage req, CancellationToken ct);
        Task ConnectAsync(CancellationToken ct);
        Task SendAsync(CancellationToken ct);
        Task DisconnectAsync(CancellationToken ct);

    }
}
