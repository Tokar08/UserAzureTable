using Microsoft.Extensions.Configuration;
using UserAzureTable;

var config = new ConfigurationBuilder()
            .AddJsonFile("config.json")
            .Build();

string connectionString = config.GetConnectionString("Default") ?? throw new NullReferenceException("Connection string not found");
var userService = new UserService(connectionString);


string[] imagePaths = 
{

};


var users = new List<User>
{
    new User{ FirstName = "John", LastName = "Doe", Age = 42, PhoneNumber = "096-7421-1011" },
    new User{ FirstName = "Tom", LastName = "Tomson", Age = 18, PhoneNumber = "050-386-2825" }
};


var addTasks = users.Zip(imagePaths, async (user, imagePath) =>
{
    if (File.Exists(imagePath))
    {
        using var pictureStream = File.OpenRead(imagePath);
        await userService.AddUserAsync(user, pictureStream, Path.GetFileName(imagePath));
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Image file not found: {imagePath}");
        Console.ResetColor();
    }
});

await Task.WhenAll(addTasks);

Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine(new string('-', 40));
Console.ResetColor();



Console.ForegroundColor = ConsoleColor.Yellow;
var allUsers = await userService.GetAllUsersAsync();
allUsers.ForEach(user =>
    Console.WriteLine($"{user.FirstName} {user.LastName}, Age: {user.Age}, Phone: {user.PhoneNumber}, Picture URL: {user.PictureUrl}")
);
Console.ResetColor();


Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine(new string('-', 40));
Console.ResetColor();

var deleteTasks = allUsers.Select(user => userService.DeleteUserAsync(user.PartitionKey, user.RowKey));
await Task.WhenAll(deleteTasks);

