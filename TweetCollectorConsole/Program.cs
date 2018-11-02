﻿using System;
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

        private static void ConnectToTwitter()
        {
            Console.WriteLine("Connecting to twitter...");
            
            Auth.SetUserCredentials(TwitterConsumerApiKey, TwitterConsumerApiSecret, TwitterAccessToken, TwitterAccessTokenSecret);
            var user = User.GetAuthenticatedUser();
            Console.WriteLine($"Succesfully connected as user {user}.");
        }

        private static async Task StoreTweetsAsync(string track, string tableName = "")
        {
            ConnectToTwitter();

            if (string.IsNullOrWhiteSpace(tableName))
                tableName = $"tweets-{track}";

            if (CloudStorageAccount.TryParse(AzureStorageAccountConnString, out var storageAccount))
            {
                var stream = Stream.CreateFilteredStream();
                stream.AddTrack(Track);

                var tableClient = storageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference(tableName);
                await table.CreateIfNotExistsAsync();

                stream.MatchingTweetReceived += async (sender, args) =>
                {
                    Console.WriteLine($"Tweet received: {args.Tweet.Text}");
                    var tweetEntity = args.Tweet.Map();
                    var insertOperation = TableOperation.Insert(tweetEntity);
                    await table.ExecuteAsync(insertOperation);
                };
                stream.StartStreamMatchingAllConditions();
                Console.WriteLine($"Listening to tweets containing the word '{track}' and store them in azure table {tableName}...");
            }
            else
            {
                Console.WriteLine($"Unable to connect to storage account with connection string {AzureStorageAccountConnString}.");
            }
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

    public class TweetEntity : TableEntity
    {
        #region Constants

        private const string DefaultPartitionKey = "67ca5b5d-8ce1-4d0c-a785-061796ea4313";

        #endregion

        #region Properties

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string HashTags { get; set; }
        public bool IsRetweet { get; set; }
        public int RetweetCount { get; set; }
        public bool Retweeted { get; set; }
        public string Source { get; set; }
        public string Text { get; set; }

        #endregion

        #region Initialization

        public TweetEntity(string rowKey = null, string partitionKey = DefaultPartitionKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey ?? Guid.NewGuid().ToString();
        }

        #endregion

        #region Services

        public static TweetEntity TestTweet()
        {
            return new TweetEntity
            {
                CreatedAt = DateTime.Now,
                CreatedBy = "TweetCollectorConsole",
                Text = "This is a test tweet entity"
            };
        }

        #endregion
    }

    public static class TweetMapper
    {
        #region Services

        public static TweetEntity Map(this ITweet tweet)
        {
            if (tweet == null)
            {
                throw new ArgumentNullException("Supplied tweet is null. Err!");
            }

            return new TweetEntity
            {
                CreatedAt = tweet.CreatedAt,
                CreatedBy = tweet.CreatedBy.Name,
                Text = tweet.Text,
                Source = tweet.Source,
                Retweeted = tweet.Retweeted,
                RetweetCount = tweet.RetweetCount,
                IsRetweet = tweet.IsRetweet,
                HashTags = string.Join(',', tweet.Hashtags.Select(hashtag => hashtag.Text))
            };
        }

        #endregion
    }
}