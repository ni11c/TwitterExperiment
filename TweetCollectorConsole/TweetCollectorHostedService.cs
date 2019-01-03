using Microsoft.Extensions.Hosting;
using Nde.TwitterExperiment.CloudStorage;
using Nde.TwitterExperiment.CloudStorage.Azure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nde.TwitterExperiment.TweetCollectorConsole
{
    public class TweetCollectorHostedService : BackgroundService
    {
        public TweetCollectorHostedService()
        {
            // todo parameters here
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //ICloudStorage azureStorage = new AzureStorage("connStringHere");
            //var twitter = new Twitter.Core.Twitter(azureStorage);
            //twitter.Connect("apiKey", "apiSecret", "token", "tokenSecret");
            //twitter.StoreTweetsAsync(track, tweet => tweet.Map());

            throw new NotImplementedException();
        }
    }
}
