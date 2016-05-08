using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace GamerBot.Notification
{
  class Program
  {
    static void Main(string[] args)
    {
      string auth = ConfigurationManager.AppSettings["XBoxAPIAuthToken"];
      string slackChannelEndPoint = ConfigurationManager.AppSettings["SlackEndPoint"];
      string channelOverride = ConfigurationManager.AppSettings["SlackChannelOverride"];
      string tokenList = ConfigurationManager.AppSettings["GamerTokenList"];

      Console.Out.WriteLine("Auth: " + !String.IsNullOrEmpty(auth));
      Console.Out.WriteLine("Slack: " + slackChannelEndPoint);
      Console.Out.WriteLine("Channel Override: " + channelOverride);
      Console.Out.WriteLine("Token List: " + tokenList);

      TimeSpan checkInterval = TimeSpan.FromMinutes(Convert.ToDouble(ConfigurationManager.AppSettings["PollingInterval"]));

      while (true)
      {
        DateTime pollStart = DateTime.Now;
        Console.Out.WriteLine(pollStart.ToShortTimeString() + " Polling");

        foreach (string token in tokenList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
          using (WebClient webClient = new WebClient())
          {
            webClient.Headers.Add("X-AUTH", auth);
            webClient.Headers.Add("Accept-Language", "en-US");
            var json = webClient.DownloadString("https://xboxapi.com/v2/" + token + "/activity");

            try
            {
              RootObject result = new JavaScriptSerializer().Deserialize<RootObject>(json);

              foreach (var activityItem in result.activityItems
                      .Where(item_ => !String.IsNullOrEmpty(item_.date) && item_.activityItemType == "Achievement" && DateTime.Parse(item_.date) > pollStart.Subtract(checkInterval))
                      .OrderByDescending(item_ => DateTime.Parse(item_.date)))
              {
                string message = "{" + (!String.IsNullOrEmpty(channelOverride) ? "\"channel\": \"" + channelOverride + "\", " : "") + "\"attachments\": [{\"fallback\": \"" + activityItem.gamertag + " unlocked an achievement\", \"title\": \"" + activityItem.gamertag + " unlocked an achievement for " + activityItem.gamerscore + " gamerscore\", \"thumb_url\": \"" + activityItem.activity.achievementIcon + "&format=png&w=128&h=128\", \"fields\": [{\"title\": \"Title\", \"value\": \"" + activityItem.itemText + "\"},{\"title\": \"Description\", \"value\": \"" + activityItem.achievementDescription + "\"}]}]}";

                SendMessage(slackChannelEndPoint, message);
              }

            }
            catch (Exception ex)
            {
              Console.Error.WriteLine(ex.Message);
              continue;
            }

          }
        }

        Thread.Sleep(checkInterval);
      }
    }

    private static bool SendMessage(string endpoint, string message)
    {
      bool success = true;

      try
      {
        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(message))
        {
          WebRequest req = WebRequest.Create(endpoint);
          req.Proxy = null;
          req.Method = "POST";
          req.ContentType = "application/x-www-form-urlencoded";

          byte[] reqData = Encoding.UTF8.GetBytes(message);
          req.ContentLength = reqData.Length;

          using (Stream reqStream = req.GetRequestStream())
          {
            reqStream.Write(reqData, 0, reqData.Length);
          }

          using (WebResponse response = req.GetResponse()) { /* Ignore for now */ }
        }
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine(ex);
        success = false;
      }

      return success;
    }



    public class Activity
    {
      public string startTime { get; set; }
      public string endTime { get; set; }
      public int numShares { get; set; }
      public int numLikes { get; set; }
      public int numComments { get; set; }
      public object ugcCaption { get; set; }
      public string activityItemType { get; set; }
      public double userXuid { get; set; }
      public string date { get; set; }
      public string contentType { get; set; }
      public int titleId { get; set; }
      public string platform { get; set; }
      public string sandboxid { get; set; }
      public object userKey { get; set; }
      public string scid { get; set; }
      public int? sharedSourceUser { get; set; }
      public string achievementScid { get; set; }
      public int? achievementId { get; set; }
      public string achievementType { get; set; }
      public string achievementIcon { get; set; }
      public int? gamerscore { get; set; }
      public string achievementName { get; set; }
      public string achievementDescription { get; set; }
      public bool? isSecret { get; set; }
      public string dateRecorded { get; set; }
      public string clipId { get; set; }
      public object clipName { get; set; }
      public string clipScid { get; set; }
      public string clipImage { get; set; }
      public string clipType { get; set; }
      public string clipCaption { get; set; }
      public bool? savedByUser { get; set; }
    }

    public class AuthorInfo
    {
      public string name { get; set; }
      public string secondName { get; set; }
      public string imageUrl { get; set; }
      public string authorType { get; set; }
      public double id { get; set; }
    }

    public class ActivityItem
    {
      public string startTime { get; set; }
      public int sessionDurationInMinutes { get; set; }
      public string contentImageUri { get; set; }
      public string bingId { get; set; }
      public string contentTitle { get; set; }
      public string vuiDisplayName { get; set; }
      public string platform { get; set; }
      public int titleId { get; set; }
      public Activity activity { get; set; }
      public string userImageUriMd { get; set; }
      public string userImageUriXs { get; set; }
      public string description { get; set; }
      public string date { get; set; }
      public bool hasUgc { get; set; }
      public string activityItemType { get; set; }
      public string contentType { get; set; }
      public string shortDescription { get; set; }
      public string itemText { get; set; }
      public string itemImage { get; set; }
      public string shareRoot { get; set; }
      public string feedItemId { get; set; }
      public string itemRoot { get; set; }
      public bool hasLiked { get; set; }
      public AuthorInfo authorInfo { get; set; }
      public string gamertag { get; set; }
      public string realName { get; set; }
      public string displayName { get; set; }
      public string userImageUri { get; set; }
      public double userXuid { get; set; }
      public string endTime { get; set; }
      public string achievementScid { get; set; }
      public int? achievementId { get; set; }
      public string achievementType { get; set; }
      public string achievementIcon { get; set; }
      public int? gamerscore { get; set; }
      public string achievementName { get; set; }
      public string achievementDescription { get; set; }
      public bool? isSecret { get; set; }
      public bool? hasAppAward { get; set; }
      public bool? hasArtAward { get; set; }
      public string clipId { get; set; }
      public string clipThumbnail { get; set; }
      public string downloadUri { get; set; }
      public string clipName { get; set; }
      public string clipCaption { get; set; }
      public string clipScid { get; set; }
      public string dateRecorded { get; set; }
      public int? viewCount { get; set; }
    }

    public class RootObject
    {
      public int numItems { get; set; }
      public List<ActivityItem> activityItems { get; set; }
      public double pollingToken { get; set; }
      public int pollingIntervalSeconds { get; set; }
      public double contToken { get; set; }
    }
  }
}
