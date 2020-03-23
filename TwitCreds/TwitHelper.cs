using Tweetinvi;

namespace TwitCreds
{
    public class TwitHelper
    {
        static bool twitCredsAreSet = false;
        public static void SetCreds()
        {
            if (twitCredsAreSet) return;
            twitCredsAreSet = true;

            Auth.SetUserCredentials(TwitCreds.Settings.CONSUMER_KEY, TwitCreds.Settings.CONSUMER_SECRET,
                TwitCreds.Settings.ACCESS_TOKEN, TwitCreds.Settings.ACCESS_TOKEN_SECRET);
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;

        }
    }
}
