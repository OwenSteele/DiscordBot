using RhythmHelper.Data.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static BotMethods;

namespace RhythmHelper
{
    public class BotMethodsNative
    {
        private static readonly Dictionary<string, string> _numberEmojis = new Dictionary<string, string>
        {
            {"0", ":zero:" },
            {"1", ":one:" },
            {"2", ":two:" },
            {"3", ":three:" },
            {"4", ":four:" },
            {"5", ":five:" },
            {"6", ":six:" },
            {"7", ":seven:" },
            {"8", ":eight:" },
            {"9", ":nine:" }
        };

        public YTVideo[] GetVideos(int limit, RestrictType restrict, string search, TimeSpan min, TimeSpan max)
        {
            Log.Information($"Exe [BotMethods] GetVideos() Thread:{Thread.CurrentThread.ManagedThreadId}");

            var pageJson = GetPageData(search);

            int amount = (limit < 1 || limit > 10) ? 10 : limit;

            var videos = new List<YTVideo>();

            int i = 0;
            int charPos = 0;
            int pageLen = pageJson.Length;

            do
            {
                var subPage = pageJson[charPos..];

                var newVideo = GetVideo(pageJson[charPos..]);

                if (!newVideo.Title.ToLower().Contains("searches related to") &&
                    RestrictCheck(newVideo.Title.ToLower(), search.ToLower(), restrict) &&
                    VideoLengthBounds(newVideo.Time.ToLower(), min, max))
                    videos.Add(new YTVideo(
                        newVideo.Title, newVideo.Published, newVideo.Channel, newVideo.Time, newVideo.Views, newVideo.Link));

                charPos += newVideo.CharPosition;
                i++;

                if (i >= amount) break;

            } while (charPos < pageLen);

            Log.Debug($"Rtn [BotMethods] GetVideos() Thread:{Thread.CurrentThread.ManagedThreadId} \"videos->arr: {videos.Count}\"");

            return videos.ToArray();
        }

        private bool VideoLengthBounds(string length, TimeSpan min, TimeSpan max)
        {
            Log.Information($"Exe [BotMethods] VideoLengthBounds() Thread:{Thread.CurrentThread.ManagedThreadId}");

            try
            {
                var words = length.Split(' ');

                if (words[1].Contains("hour"))
                    if (int.Parse(words[0].Trim()) >= 2) return false;

                int hours = 0;
                int minutes = 0;
                int seconds = 0;

                for (int i = 0; i < words.Length; i += 2)
                {
                    if (words[i+1].Contains("hour")) hours = int.Parse(words[i].Trim());
                    else if (words[i + 1].Contains("minute")) minutes = int.Parse(words[i].Trim());
                    else if (words[i + 1].Contains("second")) seconds = int.Parse(words[i].Trim());
                }

                if(hours >= 2) return false;

                var videoTime = TimeSpan.Parse($"{hours}:{minutes}:{seconds}");

                if (videoTime > max || videoTime < min) return false;
            }
            catch (Exception ex)
            {
                Log.Error($"ERR [BotMethods] VideoLengthBounds() Thread:{Thread.CurrentThread.ManagedThreadId} \"{ex.Message}\"");

                return false;
            }

            Log.Debug($"Rtn [BotMethods] VideoLengthBounds() Thread:{Thread.CurrentThread.ManagedThreadId} \"video within bounds\"");

            return true;
        }

        private bool RestrictCheck(string t, string s, RestrictType r)
        {
            Log.Information($"Exe [BotMethods] RestrictCheck() Thread:{Thread.CurrentThread.ManagedThreadId}");

            string[] sWords = s.ToLower().Split(' ');

            if (r == RestrictType.Off) return true;

            else if (r == RestrictType.Partial)
            {
                foreach (var word in sWords)
                    if (t.ToLower().Contains(word))
                        return true;
            }
            else
            {
                int c = sWords.Length;
                int matches = 0;

                foreach (var word in sWords)
                    if (t.ToLower().Contains(word))
                    {
                        t = t.Replace(word, "");
                        matches++;
                    }

                return c == matches;
            }

            Log.Debug($"Rtn [BotMethods] RestrictCheck() Thread:{Thread.CurrentThread.ManagedThreadId} \"No RestrictType enum Match\"");

            return false;
        }
        //private string GetPageData(string queryString)
        //{
        //    Log.Information($"Exe [BotMethods] GetPageData() Thread:{Thread.CurrentThread.ManagedThreadId}");

        //    if (string.IsNullOrWhiteSpace(queryString)) return null;

        //    string urlAddress = "https://www.youtube.com/results?search_query=" + queryString.Replace(' ', '+');

        //    var request = (HttpWebRequest)WebRequest.Create(urlAddress);
        //    var response = (HttpWebResponse)request.GetResponse();

        //    if (response.StatusCode == HttpStatusCode.OK)
        //    {
        //        Stream receiveStream = response.GetResponseStream();
        //        StreamReader readStream;

        //        if (string.IsNullOrWhiteSpace(response.CharacterSet))
        //            readStream = new StreamReader(receiveStream);
        //        else
        //            readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

        //        string data = readStream.ReadToEnd();

        //        response.Close();
        //        readStream.Close();

        //        string jsonStartTag = "estimatedResults";
        //        int jsonStartTagIndex = data.IndexOf(jsonStartTag);

        //        return data[jsonStartTagIndex..];
        //    }

        //    Log.Error($"Rtn [BotMethods] GetPageData() Thread:{Thread.CurrentThread.ManagedThreadId} \"{response.StatusCode}\"");

        //    return "Error";
        //}

        //public string GetNumberEmojis(int value)
        //{
        //    Log.Information($"Exe [BotMethods] GetNumberEmojis() Thread:{Thread.CurrentThread.ManagedThreadId}");

        //    var numbers = value.ToString();

        //    var emojis = new string[numbers.Length];

        //    for (int i = 0; i < numbers.Length; i++)
        //        emojis[i] = _numberEmojis.GetValueOrDefault(numbers[i].ToString());

        //    Log.Debug($"Rtn [BotMethods] GetNumberEmojis() Thread:{Thread.CurrentThread.ManagedThreadId} \"emojis# {emojis.Length}\"");

        //    return string.Join("", emojis);
        //}

        public async Task<bool> PostFeedbackToLogFileAsync(string log)
        {
            Log.Information($"Exe [BotMethods] PostFeedbackToLogFileAsync() Thread:{Thread.CurrentThread.ManagedThreadId}");

            var path = @$"..\..\..\Feedback\botfeedback {DateTime.Now:yyyy-MM-dd}.txt";

            try
            {
                using StreamWriter sw = File.AppendText(path);

                await sw.WriteLineAsync(log);
            }
            catch (Exception ex)
            {
                Log.Error($"EXH [BotMethods] PostFeedbackToLogFileAsync() Thread:{Thread.CurrentThread.ManagedThreadId} \"{ex.Message}\"");

                return false;
            }

            Log.Debug($"Rtn [BotMethods] PostFeedbackToLogFileAsync() Thread:{Thread.CurrentThread.ManagedThreadId} \"feedback posted to file: {path}\"");

            return true;
        }
    }
}