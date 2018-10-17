using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Tweetinvi;
using Tweetinvi.Models;

namespace TweetCollectorConsole
{
    internal static class Program
    {
        #region Constants

        // Twitter
        private const string AccessToken = "50558915-WbZicbfrssfHn4IcsfHbYXzMGFoc5EmYpFvfzyzsM";
        private const string AccessTokenSecret = "aau441Nz7Q1nP1XYQvIPtkDRZRcgUaW2nsFAUeDMBTz5P";
        private const string ConsumerApiKey = "Al7LMCED4TL6mELVEN9VC1k3q";
        private const string ConsumerApiSecret = "nxqYrh1dLlmzmuXD5NfN3wtfOVBBmHqWqG48K8tGu8NhuT9zT3";
        private const string Track = "brexit";

        // Azure
        private const string StorageAccountConnString = "DefaultEndpointsProtocol=https;AccountName=twitterexperimentstorage;AccountKey=9gdD3Jgnb+MEnrhjKwm0Aap5S8DIdmhOzfXqYqY7LzgwHylYm5yWl7FTXU9Xx8e/eBE8Y5/YVDs4o8xWtPkkHg==;EndpointSuffix=core.windows.net";
        private const string TableName = "Tweet";
        private const string BlobContainerName = "raw-tweets";

        #endregion

        #region Services

        public static async Task Main()
        {
            //TestTwitter();
            //await TestAzureStorage();
            await StoreTweets(Track, TableName);
        }

        private static void TestTwitter()
        {
            ConnectToTwitter();

            Console.WriteLine($"Listening to tweets containing the word '{Track}'...");
            var stream = Stream.CreateFilteredStream();
            stream.AddTrack(Track);
            stream.MatchingTweetReceived += (sender, args) => { Console.WriteLine($"Tweet received: {args.Tweet.Text}"); };
            stream.StartStreamMatchingAllConditions();
        }

        private static void ConnectToTwitter()
        {
            Console.WriteLine("Connecting to twitter...");
            Auth.SetUserCredentials(ConsumerApiKey, ConsumerApiSecret, AccessToken, AccessTokenSecret);
            var user = User.GetAuthenticatedUser();
            Console.WriteLine($"Succesfully connected as user {user}.");
        }

        private static async Task StoreTweets(string track, string tableName)
        {
            ConnectToTwitter();

            Console.WriteLine($"Listening to tweets containing the word '{Track}' and store them in azure table...");
            if (CloudStorageAccount.TryParse(StorageAccountConnString, out var storageAccount))
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
            }
        }

        private static async Task TestAzureStorage()
        {
            Console.WriteLine($"Trying to connect to storage account using connection string {StorageAccountConnString}...");
            if (CloudStorageAccount.TryParse(StorageAccountConnString, out var storageAccount))
            {
                Console.WriteLine("Success !");

                //await WriteBlob(storageAccount, BlobContainerName);
                //await ReadBlobs(storageAccount, BlobContainerName);
                await WriteTableEntity(storageAccount, TableName);
            }
        }

        private static async Task WriteBlob(CloudStorageAccount storageAccount, string blobContainerName)
        {
            Console.WriteLine($"Writing blob to blob container {BlobContainerName}...");
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(blobContainerName);
            var blockBlob = blobContainer.GetBlockBlobReference(Guid.NewGuid().ToString());
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
                foreach (var item in results.Results)
                {
                    Console.WriteLine($"Address:{item.Uri}");
                }
            }
            while (blobContinuationToken != null);
            Console.ReadKey();
        }

        private static async Task WriteTableEntity(CloudStorageAccount storageAccount, string tableName)
        {
            Console.WriteLine($"Writing test tweet to table {tableName}...");
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            var tweet = TweetEntity.TestTweet();
            var insertOperation = TableOperation.Insert(tweet);
            await table.ExecuteAsync(insertOperation);
            Console.WriteLine("Success !");
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