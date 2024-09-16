using StackExchange.Redis;

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

                logger.LogInformation("ping: {ping}", (await db.PingAsync()).TotalMilliseconds);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
