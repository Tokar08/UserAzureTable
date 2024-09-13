using Azure;
using Azure.Data.Tables;

namespace UserAzureTable
{
    public class User : ITableEntity
    {
        public User()
        {
            PartitionKey = nameof(User);
            RowKey = Guid.NewGuid().ToString();
            Timestamp = DateTimeOffset.UtcNow;
            ETag = new ETag();
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string PhoneNumber { get; set; }
        public string PictureUrl { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
