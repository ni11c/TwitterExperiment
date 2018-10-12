using System;
using Tweetinvi;

namespace TweetCollectorConsole
{
    static class Program
    {
        static void Main(string[] args)
        {
            Auth.SetUserCredentials("CONSUMER_KEY", "CONSUMER_SECRET", "USER_ACCESS_TOKEN", "USER_ACCESS_SECRET");
            var user = User.GetAuthenticatedUser();
            Console.WriteLine(user);
            Console.ReadKey();
        }
    }
}
