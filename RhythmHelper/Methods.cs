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
    }
}