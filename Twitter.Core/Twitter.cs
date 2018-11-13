using System;
using Nde.TwitterExperiment.CloudStorage;
using Tweetinvi;
using Tweetinvi.Models;

namespace Nde.TwitterExperiment.Twitter.Core
{
    public class Twitter
    {
        #region Properties

        private IAuthenticatedUser AuthenticatedUser { get; set; }
        private ICloudStorage Storage { get; }

        #endregion

        #region Initialization

        public Twitter(ICloudStorage storage)
        {
            Storage = storage ?? throw new ArgumentNullException($"argument {nameof(storage)} cannot be null.");
        }

        #endregion

        #region Services

        public void Connect(string consumerApiKey, string consumerApiSecret, string accessToken, string accessTokenSecret)
        {
            Console.WriteLine("Connecting to twitter...");
            Auth.SetUserCredentials(consumerApiKey, consumerApiSecret, accessToken, accessTokenSecret);
            AuthenticatedUser = User.GetAuthenticatedUser();
            Console.WriteLine($"Succesfully connected as user {AuthenticatedUser}.");
        }

        public void StoreTweetsAsync<T>(string track, Func<ITweet, T> map, string tableName = "")
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                tableName = $"tweets-{track}";
            }

            var stream = Stream.CreateFilteredStream();
            stream.AddTrack(track);

            bool success;
            stream.MatchingTweetReceived += async (sender, args) =>
            {
                Console.WriteLine($"Tweet received: {args.Tweet.Text}");
                var tweetEntity = map(args.Tweet);
                success = await Storage.InsertToTable(tweetEntity, tableName);
                if (!success)
                {
                    Console.WriteLine($"Failed to store tweet to storage table {tableName}.");
                }
            };
            stream.StartStreamMatchingAllConditions();
            Console.WriteLine($"Listening to tweets containing the word '{track}' and store them in table {tableName}...");
        }

        #endregion
    }
}