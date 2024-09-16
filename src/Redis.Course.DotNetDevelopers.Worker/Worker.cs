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

                //set and get strings
                var instructorNameKey = new RedisKey("instructors:1:name");

                db.StringSet(instructorNameKey, "Steve");
                var instructor1Name = db.StringGet(instructorNameKey);

                logger.LogInformation("Instructor 1's name is: {name}", instructor1Name);

                db.StringAppend(instructorNameKey, " Lorello");
                instructor1Name = db.StringGet(instructorNameKey);
                logger.LogInformation("Instructor 1's full name is: {name}", instructor1Name);

                //string numerics
                var tempKey = "temperature";
                db.StringSet(tempKey, 42);
                var tempAsLong = db.StringIncrement(tempKey, 5);
                logger.LogInformation("New temperature: {tempAsLong}", tempAsLong);

                tempAsLong = db.StringIncrement(tempKey);
                logger.LogInformation("New temperature: {tempAsLong}", tempAsLong);

                var tempAsDouble = db.StringIncrement(tempKey, .5);
                logger.LogInformation("New temperature: {tempAsDouble}", tempAsDouble);

                //expiration
                db.StringSet("temporaryKey", "hello world", expiry: TimeSpan.FromSeconds(1));
                var getTempKey = db.StringGet("temporaryKey");
                logger.LogInformation("Temporary key: {value}", getTempKey);

                await Task.Delay(1000, stoppingToken);

                getTempKey = db.StringGet("temporaryKey");
                logger.LogInformation("Temporary key after expire: {value}", getTempKey);

                var conditionalKey = "ConditionalKey";
                var conditionalKeyText = "this has been set";
                // You can also specify a condition for when you want to set a key
                // For example, if you only want to set a key when it does not exist
                // you can by specifying the NotExists condition
                var wasSet = db.StringSet(conditionalKey, conditionalKeyText, when: When.NotExists);
                logger.LogInformation("Key set: {wasSet}", wasSet);
                // Of course, after the key has been set, if you try to set the key again
                // it will not work, and you will get false back from StringSet
                wasSet = db.StringSet(conditionalKey, "this text doesn't matter since it won't be set", when: When.NotExists);
                logger.LogInformation("Key set: {wasSet}", wasSet);

                // You can also use When.Exists, to set the key only if the key already exists
                wasSet = db.StringSet(conditionalKey, "we reset the key!");
                logger.LogInformation("Key set: {wasSet}", wasSet);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
