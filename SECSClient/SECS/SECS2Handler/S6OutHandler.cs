using MediatR;
using Secs4Net;
using SECSClient.Contracts;
using SECSClient.Logging;
using SECSClient.MediatRMessages;

namespace SECSClient.SECS.SECS2Handler
{
    public class S6OutHandler : IRequestHandler<S6F11OutCommand>
    {
        private readonly ISecsGem _gem;
        public S6OutHandler(ISecsGem gem)
        {
            _gem = gem;
        }

        public async Task HandleAsync(PrimaryMessageWrapper e, LogBuffer buffer, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        public async Task Handle(S6F11OutCommand request, CancellationToken ct)
        {
            var msg = new SecsMessage(6, 11)
            {
                SecsItem = request.Report,
                Name = "EventReport"
            };
            await _gem.SendAsync(msg, ct);
        }
    }
}
