using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

// TODO : this is hardcoded to work for demo, might want to refactor if you want to make generic or something else

namespace RedBit.XamServerless.Core
{
    /// <summary>
    /// Manages access to the table for image processing
    /// </summary>
    public class TableManager
    {
        /// <summary>
        /// Default instance for tablemanager
        /// </summary>
        public static TableManager Default => new TableManager();

        private string _connectionString = "DefaultEndpointsProtocol=https;AccountName=xamamarinserverlessdemo;AccountKey=cgoO8JXM8N5bKHqRV/dqvoN6BOAo3JoQ51M90QGZG7cnmwgq03o0loOmp+7Vskl/s+djhSkMuiW4VkXUEMUsVg==;EndpointSuffix=core.windows.net";
        private string _imageTableName = "imagetable";

        private TableManager()
        {

        }

        private async Task Initialzie()
        {
            if (StorageAccount == null)
            {
                // Retrieve storage account from connection string.
                StorageAccount = CloudStorageAccount.Parse(_connectionString);

                // Create the table client
                TableClient = StorageAccount.CreateCloudTableClient();

                // Retrieve reference to a previously created container.
                CloudTable = TableClient.GetTableReference(_imageTableName);

                // Create the container if it doesn't already exist.
                await CloudTable.CreateIfNotExistsAsync();
            }
        }

        public CloudStorageAccount StorageAccount { get; private set; }
        public CloudTableClient TableClient { get; set; }
        public CloudTable CloudTable { get; private set; }

        public async Task<string> AddOriginalImage(string url)
        {
            // initialize
            await Initialzie();

            // compose the record and add it
            var record = new ImageEntity { OriginalImageUrl = url };
            var result = await CloudTable.ExecuteAsync(TableOperation.Insert(record));

            // return the row key
            return record.RowKey;
        }

        public async Task UpdateRecord(ImageEntity imageEntity)
        {
            // initialize
            await Initialzie();
            var result = await CloudTable.ExecuteAsync(TableOperation.Merge(imageEntity));
        }

    }

}
