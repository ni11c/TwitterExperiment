using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace Nde.TwitterExperiment.Twitter.Core
{
    public class Twitter
    {
        public IAuthenticatedUser AuthenticatedUser { get; private set; }

        public void Connect(string consumerApiKey, string consumerApiSecret, string accessToken, string accessTokenSecret)
        {
            Console.WriteLine("Connecting to twitter...");
            Auth.SetUserCredentials(consumerApiKey, consumerApiSecret, accessToken, accessTokenSecret);
            AuthenticatedUser = User.GetAuthenticatedUser();
            Console.WriteLine($"Succesfully connected as user {AuthenticatedUser}.");
        }

        public async Task StoreTweetsAsync(string track, string tableName = "")
        {
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

    }
}
