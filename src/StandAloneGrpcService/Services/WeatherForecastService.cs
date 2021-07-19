using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Server.Grpc.Services
{
    public class WeatherForecastService: Weather.WeatherBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public override Task<ForecastListReply> GetForecast(
            Google.Protobuf.WellKnownTypes.Empty request,
            ServerCallContext context)
        {
            var rng = new Random();
            var items = Enumerable.Range(1, 5).Select(index => new ForecastReply
                {
                    Time = DateTime.UtcNow.ToTimestamp(),
                    Temperature= rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();

            return Task.FromResult(new ForecastListReply
            {
                Items = {items}
            });

        }

    }
}
