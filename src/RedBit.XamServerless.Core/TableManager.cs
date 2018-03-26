using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
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

            // add to queue for processing of image
            await AddToQueue(record);

            // return the row key
            return record.RowKey;
        }

        public async Task UpdateRecord(ImageEntity imageEntity)
        {
            // initialize
            await Initialzie();
            var result = await CloudTable.ExecuteAsync(TableOperation.Merge(imageEntity));
        }
        
        /// <summary>
        /// Adds the image entity to the queue
        /// </summary>
        /// <param name="record">entity to add</param>
        private async Task AddToQueue(ImageEntity record)
        {
            // TODO: should probably be optimized
            var qc = StorageAccount.CreateCloudQueueClient();
            var queue = qc.GetQueueReference("imageprocessingqueue");
            await queue.CreateIfNotExistsAsync();
            await queue.AddMessageAsync(new Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage(JsonConvert.SerializeObject(record)));
        }

        /// <summary>
        /// Gets the status of a row
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UploadResponse> GetStatus(string id)
        {
            // initialize
            await Initialzie();

            var result = await CloudTable.ExecuteAsync(TableOperation.Retrieve<ImageEntity>(ImageEntity.PARTITION_KEY, id));
            if (result.HttpStatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var entity = result.Result as ImageEntity;
                if (entity == null)
                    return null;

                if (string.IsNullOrEmpty(entity.MediumImageUrl) || string.IsNullOrEmpty(entity.ExtraSmallImageUrl) || string.IsNullOrEmpty(entity.SmallImageUrl))
                {
                    return new UploadResponse
                    {
                        Id = entity.RowKey,
                        Url = entity.OriginalImageUrl,
                    };
                }
                else
                {
                    return new UploadResponse
                    {
                        Id = entity.RowKey,
                        Url = entity.OriginalImageUrl,
                        Images = new Images
                        {
                            MediumImageUrl = entity.MediumImageUrl,
                            ExtraSmallImageUrl = entity.ExtraSmallImageUrl,
                            SmallImageUrl = entity.SmallImageUrl,
                            OriginalImageUrl = entity.OriginalImageUrl
                        }
                    };
                }
            }
            else
                return null;
        }
    }

}
