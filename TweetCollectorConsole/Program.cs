using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Tweetinvi;

namespace TweetCollectorConsole
{
    static class Program
    {
        // Twitter
        private const string ConsumerApiKey = "Al7LMCED4TL6mELVEN9VC1k3q";
        private const string ConsumerApiSecret = "nxqYrh1dLlmzmuXD5NfN3wtfOVBBmHqWqG48K8tGu8NhuT9zT3";
        private const string AccessToken = "50558915-WbZicbfrssfHn4IcsfHbYXzMGFoc5EmYpFvfzyzsM";
        private const string AccessTokenSecret = "aau441Nz7Q1nP1XYQvIPtkDRZRcgUaW2nsFAUeDMBTz5P";
        
        // Azure
        private const string StorageAccountConnString = "DefaultEndpointsProtocol=https;AccountName=twitterexperimentstorage;AccountKey=9gdD3Jgnb+MEnrhjKwm0Aap5S8DIdmhOzfXqYqY7LzgwHylYm5yWl7FTXU9Xx8e/eBE8Y5/YVDs4o8xWtPkkHg==;EndpointSuffix=core.windows.net";
        private const string BlobContainerName = "raw-tweets";
        private const string TableName = "Tweet";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Connecting to twitter...");
            Auth.SetUserCredentials(ConsumerApiKey, ConsumerApiSecret, AccessToken, AccessTokenSecret);
            var user = User.GetAuthenticatedUser();
            Console.WriteLine($"Succesfully connected as user {user}.");
            Console.ReadKey();

            Console.WriteLine($"Trying to connect to storage account using connection string {StorageAccountConnString}...");
            if (CloudStorageAccount.TryParse(StorageAccountConnString, out var storageAccount))
            {
                Console.WriteLine("Success !");
                await WriteBlob(storageAccount, BlobContainerName);
                await ReadBlobs(storageAccount, BlobContainerName);
                await WriteTableEntity(storageAccount, TableName);
            }
        }

        private static async Task WriteBlob(CloudStorageAccount storageAccount, string blobContainerName)
        {
            Console.WriteLine($"Writing blob to blob container {BlobContainerName}...");
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(blobContainerName);
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(Guid.NewGuid().ToString());
            await blockBlob.UploadTextAsync("Hello, blob!");
            Console.WriteLine("Success !");
            Console.ReadKey();
        }

        private static async Task ReadBlobs(CloudStorageAccount storageAccount, string blobContainerName)
        {
            Console.WriteLine($"Reading blobs from container {blobContainerName}...");
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(blobContainerName);
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var results = await blobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                blobContinuationToken = results.ContinuationToken;
                foreach (IListBlobItem item in results.Results)
                {
                    Console.WriteLine($"Address:{item.Uri}");
                }
            }
            while (blobContinuationToken != null);

            Console.ReadKey();
        }

        private static async Task WriteTableEntity(CloudStorageAccount storageAccount, string tableName)
        {
            Console.WriteLine($"Writing fake tweet to table {tableName}...");
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            var tweet = new TweetEntity(123, "Hello TweetCollector !");
            TableOperation insertOperation = TableOperation.Insert(tweet);
            await table.ExecuteAsync(insertOperation);
            Console.WriteLine("Success !");
        }
    }

    public class TweetEntity : TableEntity 
    {
        public int TweetNumber {get;}
        public string Text{get;}

        public TweetEntity(int tweetNumber, string text)
        {
            TweetNumber = tweetNumber;
            Text = text;
        }
    }
}
