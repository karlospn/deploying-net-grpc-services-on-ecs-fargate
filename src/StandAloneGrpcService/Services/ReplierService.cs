using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Server.Grpc
{
    public class ReplierService : Replyer.ReplyerBase
    {
        private readonly ILogger<ReplierService> _logger;

        public ReplierService(ILogger<ReplierService> logger)
        {
            _logger = logger;
        }

        public override Task<ReplierReply> SaySomething(ReplierRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ReplierReply
            {
                Message = "You just said: " + request.Message
            });
        }
    }
}
