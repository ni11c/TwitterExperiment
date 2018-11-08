using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tweetinvi.Models;

namespace Nde.TwitterExperiment.Twitter.Core
{

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
