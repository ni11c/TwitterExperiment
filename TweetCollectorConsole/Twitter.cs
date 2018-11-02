using System;
using System.Collections.Generic;
using System.Text;

namespace Nde.TwitterExperiment.TweetCollectorConsole
{
    public class TwitterSettings
    {
        public string ConsumerApiKey { get; }
        public string ConsumerApiSecret { get; }
        public string AccessToken { get; }
        public string AccessTokenSecret { get; }

        public TwitterSettings(string consumerApiKey, string consumerApiSecret, string accessToken, string accessTokenSecret)
        {
            ConsumerApiKey = consumerApiKey;
            ConsumerApiSecret = consumerApiSecret;
            AccessToken = accessToken;
            AccessTokenSecret = accessTokenSecret;
        }
    }
}
