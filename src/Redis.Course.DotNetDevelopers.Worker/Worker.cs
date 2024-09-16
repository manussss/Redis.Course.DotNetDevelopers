using StackExchange.Redis;
using System.Diagnostics;

namespace Redis.Course.DotNetDevelopers.Worker
{
    public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var options = new ConfigurationOptions
                {
                    EndPoints = { configuration.GetConnectionString("Redis")! }
                };

                var muxer = await ConnectionMultiplexer.ConnectAsync(options);

                var db = muxer.GetDatabase();

                var stopwatch = Stopwatch.StartNew();

                // un-pipelined commands incur the added cost of an extra round trip
                //result: ~380ms
                for (var i = 0; i < 1000; i++)
                {
                    await db.PingAsync();
                }

                logger.LogInformation("1000 un-pipelined commands took: {elapsedMs}ms to execute", stopwatch.ElapsedMilliseconds);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
