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

                //lists
                var fruitKey = "fruits";
                var vegetableKey = "vegetables";
                db.KeyDelete([fruitKey, vegetableKey]);

                //push to the left
                db.ListLeftPush(fruitKey, ["Banana", "Mango", "Apple", "Pepper", "Kiwi", "Grape"]);

                logger.LogInformation("The first fruit in the list is: {fruit}", db.ListGetByIndex(fruitKey, 0));
                logger.LogInformation("The last fruit in the list is: {fruit}", db.ListGetByIndex(fruitKey, -1));

                //push to the right
                db.ListRightPush(vegetableKey, ["Potato", "Carrot", "Asparagus", "Beet", "Garlic", "Tomato"]);
                logger.LogInformation("The first veg in the list is: {fruit}", db.ListGetByIndex(vegetableKey, 0));

                //enumerate a list
                logger.LogInformation("Fruit indexes 0 to -1: {fruits}", string.Join(", ", db.ListRange(fruitKey)));
                logger.LogInformation("Vegetables index 0 to -2: {vegs}", string.Join(", ", db.ListRange(vegetableKey, 0, -2)));

                //list as queue
                logger.LogInformation("Enqueuing Celery");
                db.ListLeftPush(vegetableKey, "Celery");
                logger.LogInformation("Dequeued: {veg}", db.ListRightPop(vegetableKey));

                //list as stack
                logger.LogInformation("Pushing Grapefruit");
                db.ListLeftPush(fruitKey, "Grapefruit");

                //searching lists
                logger.LogInformation("Position of Mango: {mango}", db.ListPosition(fruitKey, "Mango"));

                //lists size
                logger.LogInformation("There are {len} fruits in our Fruit List", db.ListLength(fruitKey));

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
