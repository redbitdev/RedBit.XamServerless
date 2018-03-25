﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

// TODO : this is hardcoded to work for demo, might want to refactor if you want to make generic or something else

namespace RedBit.XamServerless.Core
{
    /// <summary>
    /// Manages access to blob
    /// </summary>
    public class BlobManager
    {
        /// <summary>
        /// Default instance for blobmanager
        /// </summary>
        public static BlobManager Default => new BlobManager();

        private string _connectionString = "DefaultEndpointsProtocol=https;AccountName=xamamarinserverlessdemo;AccountKey=cgoO8JXM8N5bKHqRV/dqvoN6BOAo3JoQ51M90QGZG7cnmwgq03o0loOmp+7Vskl/s+djhSkMuiW4VkXUEMUsVg==;EndpointSuffix=core.windows.net";
        private string _originalImageBlobContainerName = "originalimagecontainer";
        private const string URL = "https://xamamarinserverlessdemo.blob.core.windows.net/";

        private BlobManager()
        {
            
        }

        public CloudStorageAccount StorageAccount { get; private set; }

        public CloudBlobClient BlobClient { get; set; }

        public CloudBlobContainer BlobContainer { get; set; }

        private async Task Initialzie()
        {
            if (StorageAccount == null)
            {
                // Retrieve storage account from connection string.
                StorageAccount = CloudStorageAccount.Parse(_connectionString);

                // Create the blob client
                BlobClient = StorageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                BlobContainer = BlobClient.GetContainerReference(_originalImageBlobContainerName);

                // Create the container if it doesn't already exist.
                await BlobContainer.CreateIfNotExistsAsync();
            }
        }

        /// <summary>
        /// Adds original image to blob
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public async Task<string> AddOriginalImage(byte[] buffer)
        {
            // initialize
            await Initialzie();

            // compose the name and upload
            var blobName = $"{Guid.NewGuid().ToString("N")}.png";
            var blob = BlobContainer.GetBlockBlobReference(blobName);
            await blob.UploadFromByteArrayAsync(buffer, 0, buffer.Length);

            // return the filename
            return $"{URL}{_originalImageBlobContainerName}/{blobName}";
        }
    }
}
