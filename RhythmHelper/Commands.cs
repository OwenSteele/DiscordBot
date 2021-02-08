using Discord.WebSocket;
using GooBot;
using RhythmHelper.Data.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using static BotMethods;


namespace RhythmHelper
{
    public class Commands
    {
        private readonly SocketMessage _socketMessage;
        private readonly GetInfo _info;
        private readonly BotMethodsNative _methods;
        private readonly Dictionary<string, (string, Func<string>)> _allCommands;

        private Guild _guild;
        private User _user;
        private string _cmd;
        private string _msg;
        private User[] _mentions;

        public int MsgVal { get; }

        public Commands(SocketMessage socketMessage, ref GetInfo info, BotMethodsNative methods, int msgVal)
        {
            Log.Information($"{MsgVal}Ctor [Commands] Commands(SocketMessage socketMessage, ref GetInfo info, BotMethods methods) Thread:{Thread.CurrentThread.ManagedThreadId}");

            _socketMessage = socketMessage;
            _info = info;
            _methods = methods;
            _msg = null;
            _mentions = null;

            MsgVal = msgVal;

            _allCommands = new Dictionary<string, (string, Func<string>)> {
                {
                   "help",
                   ($"shows help (also accessed with just entering the prefix)",
                   ShowHelp)
                },
                {
                    "search",
                    ($"returns search results from youtube. - *e.g. '!!search gooey robots'*",
                    SearchYoutube)
                },
                {
                   "searchlimit",
                   ($"set video return limit - *e.g. !!searchlimit 7*",
                   SetSearchLimit)
                },
                {
                   "searchrestrict",
                   ($"set how closely a video must match the search options '{RestrictType.Off}', '{RestrictType.Partial}', '{RestrictType.Full}'",
                   SetSearchRestriction)
                },
                {
                   "searchlength",
                   ($"set the min or max length of the videos returned (can set only min or max - *e.g. !!searchlength min 00:20:11 max 01:00:00*",
                   SetSearchLength)
                },
                {
                   "dice",
                   ($"roll a crypto-dice (optional: [mutliple] mentions = rolls for those people)e.g.*!!dice @someone @someoneElse* = dice is rolled for each person.",
                   DiceRoller)
                },
                {
                   "dicedefault",
                   ($"set the default number of side for the diceroll - *e.g. !!dicedefault 20*",
                   SetDiceRollerDefault)
                },
                {
                   "dicedist",
                   ($"Show the distribution of the crypto-die. INCLUDE iteration count.",
                   GetDiceRollerDistribution)
                },
                {
                   "changeprefix",
                   ($"change the prefix for the bot (1 to 3 chars long) - *e.g. !!changeprefix @@@*",
                   SetPrefix)
                },
                {
                   "resetprefix",
                   ($"reset the prefix to '!!' - does not require current prefix",
                   ResetPrefix)
                },
                {
                   "botfeedback",
                   ($"give me feedback on: issues, desires (for new commands), changes etc.",
                   GiveFeedback)
                },
                {
                   "opoints",
                   ($"give someone some Ø points by a mention (can't self award!) - *!!opoint @someone* = give that person an Ø point.",
                   AwardOPoints)
                },
                {
                   "checkopoints",
                   ($"check Ø points by a (multiple) mentions - *!!checkopoints @someone* = to see all mentioned people[s] Ø points",
                   CheckOPoints)
                },
                {
                   "resetopoints",
                   ($"Bot Owner only",
                   ResetOPoints)
                },
                {
                   "setopointer",
                   ($"Bot Owner only",
                   SetOPointer)
                }
            };
        }
        public string NewCommand()
        {
            Log.Information($"{MsgVal}Exe [Commands] NewCommand() Thread:{Thread.CurrentThread.ManagedThreadId}");

            if (_socketMessage.Author.IsBot)
                return null;

            if (_socketMessage.Channel as SocketGuildChannel == null)
                return DirectMessage(_socketMessage);

            _guild = _info.GetGuildAsync(_socketMessage).Result;

            var prefix = "!!";

            if (!string.IsNullOrWhiteSpace(_guild.CommandPrefix))
                prefix = _guild.CommandPrefix;

            var message = _socketMessage.Content.TrimStart().ToLower();

            if (message.Equals("!!resetprefix"))
            {
                _msg = message;
                return ResetPrefix();
            }

            if (!message.StartsWith(prefix))
                return null;

            _user = _info.GetUserAsync(_socketMessage).Result;

            _cmd = message[prefix.Length..].Split(' ')[0].ToLower();

            if (string.IsNullOrWhiteSpace(_cmd)) _cmd = "help";

            var command = _allCommands.GetValueOrDefault(_cmd);

            if (command == (null,null))
                return $"I don't know that command sorry, check the spelling or type ***{_guild.CommandPrefix}*** for all commands!";

            var mentions = _socketMessage.MentionedUsers.ToArray();

            if(mentions.Length == 0 && message.Contains("@everyone"))
                Array.Resize(ref mentions, _guild.Users.Count);

            _mentions = new User[mentions.Length];

            for (int i = 0; i < _mentions.Length; i++)
            {
                User mentionedUser;

                if (mentions[i] != null)
                {
                    mentionedUser = _info.GetUserAsync(_socketMessage, mentions[i]).Result;
                    message = message.Replace(mentions[i].Mention, "").TrimEnd();
                }
                else
                {
                    mentionedUser = _guild.Users.ElementAt(i);
                    message = message.Replace("@everyone", "").TrimEnd();
                }

                _mentions[i] = mentionedUser;
            }

            var req = message[(message.IndexOf(' ') + 1)..];

            if (!req.Equals($"{prefix}{_cmd}")) _msg = req;

            if (command.Item2 == null) return command.Item1;
            else
            {
                var result = command.Item2();

                if (string.IsNullOrWhiteSpace(result)) result = "Error occurred";

                Log.Debug($"{MsgVal}Rtn [Commands] NewCommand() Thread:{Thread.CurrentThread.ManagedThreadId} returning command.");

                return result;
            }
        }
        private string ShowHelp()
        {
            Log.Information($"{MsgVal}Exe [Commands] ShowHelp() Thread:{Thread.CurrentThread.ManagedThreadId}");

            string result = $"Thank you for using GØØ Bot, {_socketMessage.Author.Mention}.\n" +
                $"GØØ commands start with: **'{_guild.CommandPrefix}'** e.g. **{_guild.CommandPrefix}search**\n\n" +
                $"**Current commands I can do:**\n";

            var sb = new StringBuilder(result);

            var pos = 0;

            foreach (var cmd in _allCommands)
            {
                sb.Append($"{(pos % 2 == 0 ? ":face_vomiting:": ":ghost:")} ***'{_guild.CommandPrefix}{cmd.Key}'*** - {cmd.Value.Item1}'\n");
                pos++;
            }

            Log.Debug($"{MsgVal}Rtn [Commands] ShowHelp() Thread:{Thread.CurrentThread.ManagedThreadId} \"Help message\"");

            return sb.ToString();
        }

        private string CheckOPoints()
        {
            Log.Information($"{MsgVal}Exe [Commands] CheckOPoints() Thread:{Thread.CurrentThread.ManagedThreadId}");

            if (_mentions.Length == 0)
                return $"***You*** have ```{_user.OPoints}``` Ø POINTS!!!\n";
            
            if (_mentions.Length == 1)
                return $"***{_mentions[0].Username}*** has ```{_mentions[0].OPoints}``` Ø POINTS!!!\n";

            var sb = new StringBuilder();

            foreach (var mtn in _mentions.OrderBy(x => x.OPoints))
                sb.Append($"***{mtn.Username}*** has __{mtn.OPoints}__ Ø POINTS!!!\n");

            Log.Debug($"{MsgVal}Rtn [Commands] CheckOPoints() Thread:{Thread.CurrentThread.ManagedThreadId} \"multiple opoints\"");

            return sb.ToString();
        }

        private string SetOPointer()
        {
            Log.Information($"{MsgVal}Exe [Commands] SetOPointer() Thread:{Thread.CurrentThread.ManagedThreadId}");

            if (!_user.UserId.Equals("327478216149172225")) return "You are not allowed to do this sorry.";

            if (_mentions.Length == 0) return $"Must mention someone";

            if(!int.TryParse(_msg ?? "1", out int value))
                if(!(value != 0 || value != 1)) return $"Value must be 0 or 1.";

            var state = value != 0;

            if (_mentions.Length == 1)
            {
                if (_mentions[0].UserId.Equals("327478216149172225") && state == false) 
                    return "Cannot revoke this users permissions";

                if (_mentions[0].OPointer == state) return "Already set to this state";

               _mentions[0].OPointer = state;
                var user = _info.UpdateUserAsync(_mentions[0]).Result;

                return $"***{user.Username}*** can {(user.OPointer ? "now":"no longer")} give Ø points. {(user.OPointer ? ":genie:" : ":no_entry_sign:")} ";
            }

            if (!_user.UserId.Equals("327478216149172225")) return "You are not allowed to do this sorry (2).";

            var sb = new StringBuilder();

            foreach (var mtn in _mentions)
            {
                if (mtn.UserId.Equals("327478216149172225") && state == false) continue;

                if (mtn.OPointer == state) continue;


                mtn.OPointer = state;
                var user = _info.UpdateUserAsync(mtn).Result;

                sb.Append($"***{user.Username}*** can {(user.OPointer ? "now" : "no longer")} give Ø points.{(user.OPointer ? ":genie:" : ":no_entry_sign:")}");
            }

            Log.Debug($"{MsgVal}Rtn [Commands] SetOPointer() Thread:{Thread.CurrentThread.ManagedThreadId} \"multiple opoints\"");

            if (string.IsNullOrWhiteSpace(sb.ToString())) return "No one has these privileges to revoke.";

            return sb.ToString();
        }

        private string ResetOPoints()
        {
            Log.Information($"{MsgVal}Exe [Commands] ResetOPoints() Thread:{Thread.CurrentThread.ManagedThreadId}");

            if (!_user.UserId.Equals("327478216149172225")) return "You are not allowed to reset Ø points.";

            if (_mentions.Length == 0)
                return $"Must mention someone to reset their Ø points";

            if (_mentions.Length == 1)
            {
                if (_mentions[0].OPoints == 0) 
                    return $"***{_mentions[0].Username}*** never had any Ø points anyway! :disappointed_relieved:";

                var currentPoints = _mentions[0].OPoints;

                _mentions[0].OPoints = 0;
                var user = _info.UpdateUserAsync(_mentions[0]).Result;

                return $"***{user.Username}***'s Ø points have been set to 0 again :disappointed_relieved: (from: {currentPoints})";
            }

            var sb = new StringBuilder();

            foreach (var mtn in _mentions)
            {
                if (mtn.OPoints == 0) continue;

                var currentPoints = mtn.OPoints;

                mtn.OPoints = 0;
                var user = _info.UpdateUserAsync(mtn).Result;

                sb.Append($"***{user.Username}***'s Ø points have been set to 0 again :disappointed_relieved: (from: {currentPoints})\n");
            }

            Log.Debug($"{MsgVal}Rtn [Commands] ResetOPoints() Thread:{Thread.CurrentThread.ManagedThreadId} \"multiple opoints\"");

            if (string.IsNullOrWhiteSpace(sb.ToString())) return "No one had Ø points to reset";

            return sb.ToString();
        }

        private string AwardOPoints()
        {
            Log.Information($"{MsgVal}Exe [Commands] AwardOPoints() Thread:{Thread.CurrentThread.ManagedThreadId}");

            if (!_user.OPointer) return "Sorry little chicken, you aren't allowed to give Ø points yet.";

            if (_mentions.Length == 0)
                return $"You need to mention someone (make sure it is a blue mention!)";

            if(!int.TryParse(_msg, out int points)) return "Must enter a whole number";

            if (_mentions.Length == 1)
            {
                if(_mentions[0].UserId.Equals(_user.UserId) && !_user.UserId.Equals("327478216149172225"))
                {
                    _user.OPoints--;

                    if (_user.OPoints < 0) _user.OPoints = 0;

                    var abuser = _info.UpdateUserAsync(_user).Result;

                    return $"You can't self award Ø points! :skull_crossbones: __This abuse means you have lost an Ø point!__ **You now have {abuser.OPoints} Ø point(s).**";
                }

                _mentions[0].OPoints += points;

                var user = _info.UpdateUserAsync(_mentions[0]).Result;

                return $"***{user.Username}*** has been awarded {points} Ø points, and now has a total of: {user.OPoints}";
            }

            if (!_user.UserId.Equals("327478216149172225")) return "You can't award  Ø points to multiple people at once, sorry :trumpet:";

            var sb = new StringBuilder();

            foreach (var mtn in _mentions)
            {
                mtn.OPoints = 0;
                var user = _info.UpdateUserAsync(mtn).Result;

                sb.Append($"***{user.Username}***'s Ø points have been set to 0 again :disappointed_relieved: \n");
            }

            Log.Debug($"{MsgVal}Rtn [Commands] AwardOPoints() Thread:{Thread.CurrentThread.ManagedThreadId} \"multiple opoints\"");

            return sb.ToString();
        }

        public string DirectMessage(SocketMessage socketMessage)
        {
            Log.Information($"{MsgVal}Exe [Commands] DirectMessage() Thread:{Thread.CurrentThread.ManagedThreadId}");

            var user = socketMessage.Author;

            Log.Debug($"{MsgVal}Rtn DirectMessage() Thread:{Thread.CurrentThread.ManagedThreadId} \"Direct message reply\"");

            return $"Hi there, {user.Username}! Thanks for using GØØ Bot.\n\n" +
                $"I'm afraid I can't do much in direct message, message me in a server instead!\n\n" +
                $@"Made by Ø_#0921 - **github: OwenSteele**";
        }

        private string ResetPrefix()
        {
            Log.Information($"{MsgVal}Exe [Commands] ResetPrefix() Thread:{Thread.CurrentThread.ManagedThreadId}");

            if (_guild == null) return "Something went horribly wrong: Could get you guild information sorry";

            var prefixLength = _guild.CommandPrefix.Length;

            if (_guild.CommandPrefix == _msg[..prefixLength])
                return "***!!*** is already the prefix for your server to send commands to me. There's no need to reset";

            if (string.IsNullOrWhiteSpace(_info.ChangeGuildPrefixAsync(_guild, "!!").Result))
                return "Something went horribly wrong: Could get you guild information sorry";

            Log.Debug($"{MsgVal}Rtn [Commands] ResetPrefix() Thread:{Thread.CurrentThread.ManagedThreadId} \"reset prefix\"");

            return "Command prefix has been reset. Use ***!!*** to get me to do your bidding";
        }

        private string SetPrefix()
        {
            Log.Information($"{MsgVal}Exe [Commands] SetPrefix() Thread:{Thread.CurrentThread.ManagedThreadId}");

            var newPrefix = _msg;

            if (newPrefix == _guild.CommandPrefix) return "This is already the prefix";

            if (newPrefix == "!") return "Cannot use this prefix";

            if (newPrefix.Length < 1 || newPrefix.Length > 3) return "Must be between 1 to 3 characters long";

            if (Regex.Matches(newPrefix, @"[a-zA-Z]").Count > 0) return "Cannot contain letters";

            var result = _info.ChangeGuildPrefixAsync(_guild, newPrefix).Result;

            if (string.IsNullOrWhiteSpace(result)) return "Could not change prefix";

            Log.Debug($"{MsgVal}Rtn [Commands] SetPrefix() Thread:{Thread.CurrentThread.ManagedThreadId} \"prefix\n");

            return $"Prefix has been changed to *'{result}'*, all commands must now start with ***{result}***";
        }

        
        private string DiceRoller()
        {
            Log.Information($"{MsgVal}Exe [Commands] DiceRoller() Thread:{Thread.CurrentThread.ManagedThreadId}");

            int value;

            if (string.IsNullOrWhiteSpace(_msg) || _msg.Equals("!!dice")) value = _guild.DiceDefault;
            else if (!int.TryParse(_msg, out value)) return "Must enter a whole number";

            if (value < 2) return "max limit must be higher than one";

            var rollValue = RNG.Roll(value);

            if (_mentions.Length == 0)
                return $":game_die: **{value}-sided** You rolled a dice for: {GetNumberEmojis(rollValue)}\n";

            if (_mentions.Length == 1)
                return $":game_die: **{value}-sided** Dice Rolled for ***{_mentions[0].Username}***: {GetNumberEmojis(rollValue)}\n";

            var sb = new StringBuilder();

            foreach (var mtn in _mentions)
            {
                sb.Append($":game_die: **{value}-sided** Dice Rolled for ***{mtn.Username}***: {GetNumberEmojis(rollValue)}\n");
                rollValue = RNG.Roll(value);
            }

            Log.Debug($"{MsgVal}Rtn [Commands] DiceRoller() Thread:{Thread.CurrentThread.ManagedThreadId} \"dice rolled\"");

            return sb.ToString();
        }
        private string SetDiceRollerDefault()
        {
            Log.Information($"{MsgVal}Exe [Commands] SetDiceRollerDefault() Thread:{Thread.CurrentThread.ManagedThreadId}");

            int value;

            if (string.IsNullOrWhiteSpace(_msg)) return $":game_die: Current Dice roll range is ***1-{_guild.DiceDefault}***";
            else if (!int.TryParse(_msg, out value)) return "Must enter a whole number";

            if (value < 2) return "max limit must be higher than one";

            if (value.Equals(_guild.DiceDefault)) return "This is already the default dice roll";

            var result = _info.ChangeGuildDiceDefaultAsync(_guild, value).Result;

            if (result < 0) return "Dice default value was not changed";

            Log.Debug($"{MsgVal}Rtn [Commands] SetDiceRollerDefault() Thread:{Thread.CurrentThread.ManagedThreadId} {result}");

            return $":game_die: Dice rolls now have a default range of **1 - {result}** if no number is given when rolling the dice";
        }
        private string GetDiceRollerDistribution()
        {
            Log.Information($"{MsgVal}Exe [Commands] GetDiceRollerDistribution() Thread:{Thread.CurrentThread.ManagedThreadId}");

            int value;

            var warning = "";

            if (string.IsNullOrWhiteSpace(_msg)) return $"must enter iteration value";
            else if (!int.TryParse(_msg, out value)) return "Must enter a whole number";

            if (value > 100000)
            {
                warning = "***Capped at 10000***\n";
                value = 100000;
            }

            return warning + RNG.Distribution(_guild.DiceDefault,value);
        }

        private string SetSearchLength()
        {
            Log.Information($"{MsgVal}Exe [Commands] SetSearchLength() Thread:{Thread.CurrentThread.ManagedThreadId}");

            if (string.IsNullOrWhiteSpace(_msg)) return "You need to set some lengths for me to work with. Min or Max, or Both";

            var msgParts = _msg.Split(' ');

            var incorrectFormat = $"I can't read this sorry, check your format matches this style: **{_guild.CommandPrefix}searchlength min 00:00:00**";

            if (!(msgParts.Length % 2 == 0) || msgParts.Length == 0)
                return incorrectFormat;

            var min = _guild.VideoLengthMin;
            var max = _guild.VideoLengthMax;

            for (int i = 0; i< msgParts.Length; i +=2)
            {
                try
                {
                    if (msgParts[i].Contains("min")) min = TimeSpan.Parse(msgParts[i + 1]);
                    else if (msgParts[i].Contains("max")) max = TimeSpan.Parse(msgParts[i + 1]);
                }
                catch (Exception)
                {
                    return incorrectFormat;
                }
            }            

            if (min.TotalMilliseconds >= max.TotalMilliseconds) return "Min length time must be less than the max length time";

            if (min.TotalMilliseconds < 0 || max.TotalMilliseconds < 0) return "Cannot have negative times, DUH!";

            if (min.TotalDays >= 1 || max.TotalDays >= 1) return "Cannot set the lengths to be over a day";

            if (max.TotalHours > 2) return "Rythm can't play songs over two hours long";

            if (_guild.VideoLengthMin.TotalMilliseconds == min.TotalMilliseconds &&
                _guild.VideoLengthMax.TotalMilliseconds == max.TotalMilliseconds)
                return "These are already the min and max video lengths!";

            var result = _info.ChangeGuildVideoLengthsAsync(_guild, min, max).Result;

            if (!result.HasValue) return "Couldn't change the video length bounds.";

            var tMin = result.GetValueOrDefault().Item1;
            var tMax = result.GetValueOrDefault().Item2;

            var textMin = $"{(tMin.TotalDays >= 1 ? (tMin.Days +" days, ") : "")} {(tMin.TotalHours >= 1 ? (tMin.Hours + " hours, ") : "")} {(tMin.TotalMinutes >= 1 ? (tMin.Minutes + " minutes, ") : "")} {(tMin.TotalSeconds >= 1 ? (tMin.Seconds + " seconds") : "")}";
            var textMax = $"{(tMin.TotalDays >= 1 ? (tMax.Days + " days, ") : "")} {(tMax.TotalHours >= 1 ? (tMax.Hours + " hours, ") : "")} {(tMax.TotalMinutes >= 1 ? (tMax.Minutes + " minutes, ") : "")} {(tMax.TotalSeconds >= 1 ? (tMax.Seconds + " seconds") : "")}";

            Log.Debug($"{MsgVal}Rtn [Commands] SetSearchLength() Thread:{Thread.CurrentThread.ManagedThreadId} \"\"");

            return $"Search will only show videos of min length: **{textMin}** and a max length: **{textMax}**";
        }

        private string SetSearchRestriction()
        {
            Log.Information($"{MsgVal}Exe [Commands] SetSearchRestriction() Thread:{Thread.CurrentThread.ManagedThreadId}");

            RestrictType restriction;

            if (_msg == "off") restriction = RestrictType.Off;
            else if (_msg == "partial") restriction = RestrictType.Partial;
            else if (_msg == "full") restriction = RestrictType.Full;

            else return "invalid input (off/partial/full)";

            var result = _info.ChangeGuildRestrictionAsync(_guild, restriction).Result;

            if (string.IsNullOrWhiteSpace(result)) return "Restriction was not changed";

            Log.Debug($"{MsgVal}Rtn [Commands] SetSearchRestriction() Thread:{Thread.CurrentThread.ManagedThreadId} \"{restriction}\"");

            return $"Search restrict set to: {restriction}";
        }

        private string SetSearchLimit()
        {
            Log.Information($"{MsgVal}Exe [Commands] SetSearchLimit() Thread:{Thread.CurrentThread.ManagedThreadId}");

            if (!int.TryParse(_msg, out int value)) return "Must enter a whole number";

            var result = _info.ChangeGuildLimitAsync(_guild, value).Result;

            if (result < 0) return "Limit was not changed";

            Log.Debug($"{MsgVal}Rtn [Commands] SetSearchLimit() Thread:{Thread.CurrentThread.ManagedThreadId} {result}");

            return $"Search return limit changed to {result}";
        }

        private string SearchYoutube()
        {
            Log.Information($"{MsgVal}Exe [Commands] SearchYoutube() Thread:{Thread.CurrentThread.ManagedThreadId}");

            if (string.IsNullOrWhiteSpace(_msg)) return "Must enter something to search for!";

            var videos = GetVideos(_guild.Limit, _guild.Restrict.ToString(), _msg, _guild.VideoLengthMin,_guild.VideoLengthMax);

            var videosInfo = new List<string>();

            var messageLength = 0;

            foreach (var video in videos)
            {
                string partial = $"{videosInfo.Count + 1}- ***{video.Title}*** By {video.Channel} (**{video.Published}**), {video.Views}, **{video.Time}** - *Copy this to play*:" +
                    $"```!play www.youtube.com{video.Link}```";

                if (messageLength + partial.Length >= 2000) break;

                videosInfo.Add(partial);

                messageLength += partial.Length;
            }

            Log.Debug($"{MsgVal}Rtn [Commands] SearchYoutube() Thread:{Thread.CurrentThread.ManagedThreadId} \"videos: {videosInfo.Count}\"");

            var result = string.Join("", videosInfo);

            if (string.IsNullOrWhiteSpace(result)) 
                return $"No videos found for search: '{_msg}'. {(_guild.Restrict.Equals(RestrictType.Off) ? "Try to simplify the search." : "Try to reduce the restriction level.")}";

            return string.Join("", videosInfo);
        }
        private string GiveFeedback()
        {
            Log.Information($"{MsgVal}Exe [Commands] GiveFeedback() Thread:{Thread.CurrentThread.ManagedThreadId}");

            var sb = new StringBuilder(DateTime.Now.ToString());

            sb.Append($" - Guild: '{_guild.Name}' ");
            sb.Append($"User: '{_user.Username}' ('{_user.Discriminator}')");
            sb.Append($"\n  Message: '{_msg}'");

            var path = @$"..\..\..\Feedback\botfeedback {DateTime.Now:yyyy-MM-dd}.txt";

            try
            {
                using StreamWriter sw = File.AppendText(path);

                sw.WriteLineAsync(sb.ToString()).Wait();
            }
            catch (Exception ex)
            {
                Log.Error($"EXH [Commands] GiveFeedback() Thread:{Thread.CurrentThread.ManagedThreadId} \"{ex.Message}\"");

                return "Couldn't post feedback sorry - DM @Ø_#0921 instead?";
            }

            Log.Debug($"{MsgVal}Rtn [Commands] GiveFeedback() Thread:{Thread.CurrentThread.ManagedThreadId} \"Feedback thanks\"");

            return $"Thank you for your feedback, {_user.Username}!";
        }

    }
}
