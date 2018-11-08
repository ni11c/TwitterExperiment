using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Tweetinvi;
using Tweetinvi.Models;
using Stream = Tweetinvi.Stream;

namespace TweetCollectorConsole
{
    internal static class Program
    {
        #region Constants

        private const string TrackDefault = "dotnet";
        private const string TableName = "Tweet";
        private const string BlobContainerName = "raw-tweets";

        #endregion

        #region Services

        public static async Task Main(string[] args)
        {
            string track = args != null && args.Length > 1 ? args[0] : TrackDefault;
            var builder = new ConfigurationBuilder()
                          .SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json");

            var configuration = builder.Build();


            //TestTwitter(track);
            //await TestAzureStorageAsync();
            await StoreTweetsAsync(track, TableName);
        }

        private static void TestTwitter(string track)
        {
            ConnectToTwitter();

            Console.WriteLine($"Listening to tweets containing the word '{track}'...");
            var stream = Stream.CreateFilteredStream();
            stream.AddTrack(track);
            stream.MatchingTweetReceived += (sender, args) => { Console.WriteLine($"Tweet received: {args.Tweet.Text}"); };
            stream.StartStreamMatchingAllConditions();
        }

        private static async Task TestAzureStorageAsync()
        {
            Console.WriteLine($"Trying to connect to storage account using connection string {AzureStorageAccountConnString}...");
            if (CloudStorageAccount.TryParse(AzureStorageAccountConnString, out var storageAccount))
            {
                Console.WriteLine("Success !");

                //await WriteBlobAsync(storageAccount, BlobContainerName);
                //await ReadBlobsAsync(storageAccount, BlobContainerName);
                await WriteTableEntityAsync(storageAccount, TableName);
            }
        }

        private static async Task WriteBlobAsync(CloudStorageAccount storageAccount, string blobContainerName)
        {
            Console.WriteLine($"Writing blob to blob container {BlobContainerName}...");
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(blobContainerName);
            if (await blobContainer.CreateIfNotExistsAsync())
            {
                var blockBlob = blobContainer.GetBlockBlobReference(Guid.NewGuid().ToString());
                await blockBlob.UploadTextAsync("Hello, blob!");
                Console.WriteLine("Success !");
            }
            else
            {
                Console.WriteLine($"Failed to create blob container '{blobContainerName}'. Abort.");
            }
            Console.ReadKey();
        }

        private static async Task ReadBlobsAsync(CloudStorageAccount storageAccount, string blobContainerName)
        {
            Console.WriteLine($"Reading blobs from container {blobContainerName}...");
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(blobContainerName);
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var results = await blobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                blobContinuationToken = results.ContinuationToken;
                foreach (var item in results.Results)
                {
                    Console.WriteLine($"Address:{item.Uri}");
                }
            }
            while (blobContinuationToken != null);
            Console.ReadKey();
        }

        private static async Task WriteTableEntityAsync(CloudStorageAccount storageAccount, string tableName)
        {
            Console.WriteLine($"Writing test tweet to table {tableName}...");
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                var tweet = TweetEntity.TestTweet();
                var insertOperation = TableOperation.Insert(tweet);
                await table.ExecuteAsync(insertOperation);
                Console.WriteLine("Success !");
            }
            else
            {
                Console.WriteLine($"Failed to create table '{tableName}'. Abort.");
            }
            
            Console.ReadKey();
        }

        #endregion
    }


}