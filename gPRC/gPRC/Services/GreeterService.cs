using Grpc.Core;

using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

namespace gPRC
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override async Task ChangeVoice(IAsyncStreamReader<VoiceRequest> requestStream, IServerStreamWriter<VoiceResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var cur = requestStream.Current.AudioSample;
                await responseStream.WriteAsync(new VoiceResponse()
                {
                    AudioSample = cur
                });
            }
        }
    }
}
