using SECSClient.MediatRMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SECSClient.Equipment.Interface
{
    public interface IPLCController
    {
        Task<bool> Handle(MRRequestrMessage req, CancellationToken ct);
        Task StartAsync(CancellationToken ct);
        Task StopAsync(CancellationToken ct);
        Task RestartAsync(CancellationToken ct);

    }
}
