using MediatR;
using Secs4Net;
using SECSClient.Logging;
using SECSClient.MediatRMessages;

namespace SECSClient.SECS.SECS2Handler
{
    public class S5OutHandler : IRequestHandler<S5F1OutCommand>
    {
        private readonly ISecsGem _gem;
        public S5OutHandler(ISecsGem gem)
        {
            _gem = gem;
        }
        public async Task HandleAsync(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task Handle(S5F1OutCommand request, CancellationToken ct)
        {
            var msg = new SecsMessage(5, 1)
            {
                SecsItem = request.Alarm,
                Name = "AlarmReport"
            };
            await _gem.SendAsync(msg, ct);
        }
    }
}
