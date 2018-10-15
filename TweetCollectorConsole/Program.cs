using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Tweetinvi;

namespace TweetCollectorConsole
{
    static class Program
    {
        private const string ConsumerApiKey = "Al7LMCED4TL6mELVEN9VC1k3q";
        private const string ConsumerApiSecret = "nxqYrh1dLlmzmuXD5NfN3wtfOVBBmHqWqG48K8tGu8NhuT9zT3";
        private const string AccessToken = "50558915-WbZicbfrssfHn4IcsfHbYXzMGFoc5EmYpFvfzyzsM";
        private const string AccessTokenSecret = "aau441Nz7Q1nP1XYQvIPtkDRZRcgUaW2nsFAUeDMBTz5P";
        private const string BlobStorageConnString = "DefaultEndpointsProtocol=https;AccountName=twitterexperimentstorage;AccountKey=9gdD3Jgnb+MEnrhjKwm0Aap5S8DIdmhOzfXqYqY7LzgwHylYm5yWl7FTXU9Xx8e/eBE8Y5/YVDs4o8xWtPkkHg==;EndpointSuffix=core.windows.net";
        private const string BlobContainerName = "raw-tweets";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Connecting to twitter...");
            Auth.SetUserCredentials(ConsumerApiKey, ConsumerApiSecret, AccessToken, AccessTokenSecret);
            var user = User.GetAuthenticatedUser();
            Console.WriteLine($"Succesfully connected as user {user}.");
            Console.ReadKey();

            Console.WriteLine($"Trying to write blob to container {BlobContainerName}...");
            if (CloudStorageAccount.TryParse(BlobStorageConnString, out var storageAccount))
            {
                var blobClient = storageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(BlobContainerName);
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(Guid.NewGuid().ToString());
                await blockBlob.UploadTextAsync("Hello, blob!");
                Console.WriteLine("Success !");
                Console.ReadKey();

                Console.WriteLine($"Reading blobs from container {BlobContainerName}");
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var results = await blobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    blobContinuationToken = results.ContinuationToken;
                    foreach (IListBlobItem item in results.Results)
                    {
                        Console.WriteLine($"Address:{item.Uri}");
                    }
                } while (blobContinuationToken != null);

                Console.ReadKey();
            }
            
        }
    }
}
