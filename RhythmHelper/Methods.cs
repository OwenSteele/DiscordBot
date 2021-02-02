using RhythmHelper.Data.Entities;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace RhythmHelper
{
    public class BotMethods
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

        public YTVideo[] GetVideos(int limit, RestrictType restrict, string search)
        {
            var pageJson = GetPageData(search);

            int amount = (limit < 1 || limit > 10) ? 10 : limit;

            var videos = new List<YTVideo>();

            int i = 0;
            int charPos = 0;
            int pageLen = pageJson.Length;

            var titleInnerHtml = new string[] { "\"title\":{\"runs\":[{\"text\":\"", "\"}]" };
            var channelInnerHtml = new string[] { "\"longBylineText\":{\"runs\":[{\"text\":\"", "\"," };
            var publishedInnerHtml = new string[] { "\"publishedTimeText\":{\"simpleText\":\"", "\"}" };
            var lengthInnerHtml = new string[] { "\"lengthText\":{\"accessibility\":{\"accessibilityData\":{\"label\":\"", "\"}" };
            var viewInnerHtml = new string[] { "\"viewCountText\":{\"simpleText\":\"", "\"}" };
            var navInnerHtml = new string[] { "\"navigationEndpoint\":{\"", "\"}}," };
            var linkInnerHtml = new string[] { "\"url\":\"", "\"," };

            do
            {
                var subPage = pageJson[charPos..];

                int titleStart = subPage.IndexOf(titleInnerHtml[0]) + titleInnerHtml[0].Length;
                int titleEnd = subPage.IndexOf(titleInnerHtml[1], titleStart);
                int channelStart = subPage.IndexOf(channelInnerHtml[0]) + channelInnerHtml[0].Length;
                int channelEnd = subPage.IndexOf(channelInnerHtml[1], channelStart);
                int publishedStart = subPage.IndexOf(publishedInnerHtml[0]) + publishedInnerHtml[0].Length;
                int publishedEnd = subPage.IndexOf(publishedInnerHtml[1], publishedStart);
                int lengthStart = subPage.IndexOf(lengthInnerHtml[0]) + lengthInnerHtml[0].Length;
                int lengthEnd = subPage.IndexOf(lengthInnerHtml[1], lengthStart);
                int viewStart = subPage.IndexOf(viewInnerHtml[0]) + viewInnerHtml[0].Length;
                int viewEnd = subPage.IndexOf(viewInnerHtml[1], viewStart);
                int navStart = subPage.IndexOf(navInnerHtml[0], viewEnd) + navInnerHtml[0].Length;
                int navEnd = subPage.IndexOf(navInnerHtml[1], navStart);

                var videoTitle = subPage[titleStart..titleEnd];

                var videoChannel = subPage[channelStart..channelEnd];
                var videoPublished = subPage[publishedStart..publishedEnd];
                var videoLength = subPage[lengthStart..lengthEnd];
                var videoViews = subPage[viewStart..viewEnd];
                var videoNav = subPage[navStart..navEnd];

                int linkStart = videoNav.IndexOf(linkInnerHtml[0]) + linkInnerHtml[0].Length;
                int linkEnd = videoNav.IndexOf(linkInnerHtml[1], linkStart);

                var videoLink = videoNav[linkStart..linkEnd];

                if (!videoTitle.ToLower().Contains("searches related to") &&
                    RestrictCheck(videoTitle, search, restrict))
                    videos.Add(new YTVideo(
                        videoTitle, videoPublished, videoChannel, videoLength, videoViews, videoLink));

                charPos += navEnd + linkEnd;
                i++;

                if (i >= amount) break;

            } while (charPos < pageLen);

            return videos.ToArray();
        }
        private bool RestrictCheck(string t, string s, RestrictType r)
        {
            string[] sWords = s.Split(' ');

            if (r == RestrictType.Off) return true;

            else if (r == RestrictType.Partial)
            {
                foreach (var word in sWords)
                    if (t.Contains(word))
                        return true;
            }
            else
            {
                int c = sWords.Length;
                int matches = 0;

                foreach (var word in sWords)
                    if (t.Contains(word))
                    {
                        t = t.Replace(word, "");
                        matches++;
                    }

                return c == matches;
            }

            return false;
        }
        private string GetPageData(string queryString)
        {
            string urlAddress = "https://www.youtube.com/results?search_query=" + queryString.Replace(' ', '+');

            var request = (HttpWebRequest)WebRequest.Create(urlAddress);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream;

                if (string.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                string data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();

                string jsonStartTag = "estimatedResults";
                int jsonStartTagIndex = data.IndexOf(jsonStartTag);

                return data[jsonStartTagIndex..];
            }
            return "Error";
        }

        public string GetNumberEmojis(int value)
        {
            var numbers = value.ToString();

            var emojis = new string[numbers.Length];

            for (int i = 0; i < numbers.Length; i++)
                emojis[i] = _numberEmojis.GetValueOrDefault(numbers[i].ToString());

            return string.Join("", emojis);
        }
    }
}