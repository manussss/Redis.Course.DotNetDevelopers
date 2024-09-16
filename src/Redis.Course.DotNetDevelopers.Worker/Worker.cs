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

                // un-pipelined ~380ms
                // implicitly Pipelined ~5ms

                // If we run out async tasks to StackExchange.Redis concurrently, the library
                // will automatically manage pipelining of these commands to Redis, making
                // them significantly more performant as we remove most of the round trips to Redis.
                var pingTasks = new List<Task<TimeSpan>>();

                for (var i = 0; i < 1000; i++)
                {
                    pingTasks.Add(db.PingAsync());
                }

                await Task.WhenAll(pingTasks);

                logger.LogInformation("1000 automatically pipelined tasks took: {elapsedMs}ms to execute, first result: {result}",
                    stopwatch.ElapsedMilliseconds,
                    pingTasks[0].Result);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
