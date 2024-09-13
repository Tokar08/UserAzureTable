using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;

namespace UserAzureTable
{
    public class UserService
    {
        private readonly TableClient _tableClient;
        private readonly BlobContainerClient _blobContainerClient;

        public UserService(string connectionString)
        {
            var tableServiceClient = new TableServiceClient(connectionString);
            _tableClient = tableServiceClient.GetTableClient("Users");
            _tableClient.CreateIfNotExists();

            _blobContainerClient = new BlobServiceClient(connectionString).GetBlobContainerClient("user-pictures");
            _blobContainerClient.CreateIfNotExists();
        }

        public async Task AddUserAsync(User user, Stream pictureStream, string pictureFileName)
        {
            try
            {
                var blobClient = _blobContainerClient.GetBlobClient(pictureFileName);
                await blobClient.UploadAsync(pictureStream, overwrite: true);
                user.PictureUrl = blobClient.Uri.ToString();

                await _tableClient.AddEntityAsync(user);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("User added successfully!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error while adding user: {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }


        public async Task DeleteUserAsync(string partitionKey, string rowKey)
        {
            try
            {
                Response<User> userResponse = await _tableClient.GetEntityAsync<User>(partitionKey, rowKey);
                User user = userResponse.Value;
                string pictureFileName = Path.GetFileName(user.PictureUrl);
                await _tableClient.DeleteEntityAsync(partitionKey, rowKey);

                var blobClient = _blobContainerClient.GetBlobClient(pictureFileName);

                if (await blobClient.ExistsAsync())
                {
                    await blobClient.DeleteAsync();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Picture {pictureFileName} not found in blob storage!");
                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine($"User deleted successfully!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error while deleting user: {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                var users = new List<User>();
                await foreach (var page in _tableClient.QueryAsync<User>().AsPages())
                {
                    users.AddRange(page.Values);
                }
                return users;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error while getting users: {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }
    }
}
