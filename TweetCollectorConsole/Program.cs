using System;
using Tweetinvi;

namespace TweetCollectorConsole
{
    static class Program
    {
        private const string ConsumerApiKey = "Al7LMCED4TL6mELVEN9VC1k3q";
        private const string ConsumerApiSecret = "nxqYrh1dLlmzmuXD5NfN3wtfOVBBmHqWqG48K8tGu8NhuT9zT3";
        private const string AccessToken = "50558915-WbZicbfrssfHn4IcsfHbYXzMGFoc5EmYpFvfzyzsM";
        private const string AccessTokenSecret = "aau441Nz7Q1nP1XYQvIPtkDRZRcgUaW2nsFAUeDMBTz5P";

        static void Main(string[] args)
        {
            Auth.SetUserCredentials(ConsumerApiKey, ConsumerApiSecret, AccessToken, AccessTokenSecret);
            var user = User.GetAuthenticatedUser();
            Console.WriteLine(user);
            Console.ReadKey();
        }
    }
}
