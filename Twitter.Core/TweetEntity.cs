using System;
using System.Collections.Generic;
using System.Text;

namespace Nde.TwitterExperiment.Twitter.Core
{
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

}
