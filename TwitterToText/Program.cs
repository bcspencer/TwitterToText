using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LinqToTwitter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TwitterToText
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = new Task(GetStreamOfTweets);
            task.Start();
            task.Wait();
            Console.ReadLine();
        }

        static async void GetStreamOfTweets()
        {
            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = "",
                    ConsumerSecret = "",
                    AccessToken = "",
                    AccessTokenSecret = ""
                }
            };
            var context = new TwitterContext(auth);
            //int count = 0;
            await (from strm in context.Streaming
                   where strm.Type == StreamingType.Filter
                   //this is the twiiter user id of @GK_3D found using http://mytwitterid.com/
                   && strm.Follow == "841349892501053440"
                   //&& strm.fo.Track == "GK_3D"
                   select strm)
                .StartAsync(async strm =>
                {
                    if (!string.IsNullOrEmpty(strm.Content))
                    {
                        WriteTweetToFile(ParseContent(strm.Content));
                    }
                });
        }

        private static void WriteTweetToFile(string parsedContent)
        {
            var filePath = @"c:\Tmp\tweets.txt";
            if (File.Exists(filePath))
            {
                File.WriteAllText(filePath, parsedContent);
            }
            else
            {
                File.Create(filePath).Close();
                File.WriteAllText(filePath, parsedContent);
            }
        }

        private static string ParseContent(string strmContent)
        {
            var removeString = "@GK_3D";
            var parsedStream = JObject.Parse(strmContent);
            var tweetText = parsedStream.SelectToken("text").ToString();
            var subStringIndex = tweetText.IndexOf(removeString);
            tweetText = (subStringIndex < 0)
                ? tweetText
                : tweetText.Remove(subStringIndex, removeString.Length);
            string profanityFiltered;
            using (var client = new WebClient())
            {
                profanityFiltered = client.DownloadString("http://www.purgomalum.com/service/json?text=" + tweetText).ToString();
            }
            return JObject.Parse(profanityFiltered).SelectToken("result").ToString();
        }
    }
}
