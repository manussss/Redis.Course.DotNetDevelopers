using StackExchange.Redis;

namespace Redis.Course.DotNetDevelopers.Worker
{
    public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = new ConfigurationOptions
            {
                EndPoints = { configuration.GetConnectionString("Redis")! }
            };

            var muxer = await ConnectionMultiplexer.ConnectAsync(options);

            var db = muxer.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                //sets
                var allUsersSet = "users";
                var activeUsersSet = "users:state:active";
                var inactiveUsersSet = "users:state:inactive";
                var offlineUsersSet = "users:state:offline";
                db.KeyDelete([allUsersSet, activeUsersSet, inactiveUsersSet, offlineUsersSet]);

                db.SetAdd(activeUsersSet, ["User:1", "User:2"]);
                db.SetAdd(inactiveUsersSet, ["User:3", "User:4"]);
                db.SetAdd(offlineUsersSet, ["User:5", "User:6", "User:7"]);

                db.SetCombineAndStore(SetOperation.Union, allUsersSet, [activeUsersSet, inactiveUsersSet, offlineUsersSet]);

                var user6Offline = db.SetContains(offlineUsersSet, "User:6");
                logger.LogInformation("User:6 offline: {user6Offline}", user6Offline);
                logger.LogInformation("All Users In one shot: {usersSet}", string.Join(", ", db.SetMembers(allUsersSet)));
                logger.LogInformation("All Users with scan  : {usersSet}", string.Join(", ", db.SetScan(allUsersSet)));

                logger.LogInformation("Moving User:1 from active to offline");
                var moved = db.SetMove(activeUsersSet, offlineUsersSet, "User:1");
                logger.LogInformation("Move Successful: {moved}", moved);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
