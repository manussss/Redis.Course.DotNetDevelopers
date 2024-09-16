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
                // explicit pipelining with IBatch ~5ms

                var pingTasks = new List<Task<TimeSpan>>();

                // Batches allow you to more intentionally group together the commands that you want to send to Redis.
                // If you employee a batch, all commands in the batch will be sent to Redis in one contiguous block, with no
                // other commands from the client interleaved. Of course, if there are other clients to Redis, commands from those
                // other clients may be interleaved with your batched commands.
                var batch = db.CreateBatch();

                // restart stopwatch
                stopwatch.Restart();

                for (var i = 0; i < 1000; i++)
                {
                    pingTasks.Add(batch.PingAsync());
                }

                batch.Execute();
                await Task.WhenAll(pingTasks);

                logger.LogInformation("1000 automatically pipelined tasks took: {elapsedMs}ms to execute, first result: {result}",
                    stopwatch.ElapsedMilliseconds,
                    pingTasks[0].Result);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
