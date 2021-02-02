using Discord.WebSocket;
using RhythmHelper.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RhythmHelper
{
    public class Commands
    {
        private readonly SocketMessage _socketMessage;
        private readonly GetInfo _info;
        private readonly BotMethods _methods;
        private readonly Dictionary<string, (string, Func<string>)> _allCommands;

        private Guild _guild;
        private User _user;
        private string _cmd;
        private string _msg;

        public Commands(SocketMessage socketMessage, ref GetInfo info, BotMethods methods)
        {
            _socketMessage = socketMessage;
            _info = info;
            _methods = methods;
            _msg = null;

            _allCommands = new Dictionary<string, (string, Func<string>)> {
                {
                   "help",
                   ($"shows help (also accessed with just entering the prefix)",
                   ShowHelp)
                },
                {
                    "search",
                    ("returns search results from youtube.\nYou can limit the results returned (1 to 10) and restrict results to only word matches.\n " +
                    "E.g. '!!search (5,restrict) ...' = only returns 5 results that match one search word at least",
                    SearchYoutube)
                },
                {
                   "searchlimit",
                   ("set video return limit",
                   SetSearchLimit)
                },
                {
                   "searchrestrict",
                   ($"set how closely a video must match the search options '{RestrictType.Off}', '{RestrictType.Partial}', '{RestrictType.Full}'",
                   SetSearchRestriction)
                },
                {
                   "dice",
                   ("roll a dice (optional: followed by the limit)",
                   DiceRoller)
                },
                {
                   "dicedefault",
                   ("set the default number of side for the diceroll command",
                   SetDiceRollerDefault)
                },
                {
                   "changeprefix",
                   ("change the prefix for the bot (must be between 1 to 3 characters long)",
                   SetPrefix)
                }
            };
        }

        public string NewCommand()
        {
            _guild = _info.GetGuildAsync(_socketMessage).Result;

            var prefix = "!!";

            if (!string.IsNullOrWhiteSpace(_guild.CommandPrefix))
                prefix = _guild.CommandPrefix;

            var message = _socketMessage.Content.TrimStart().ToLower();

            if (!message.StartsWith(prefix) || _socketMessage.Author.IsBot)
                return null;


            var user = _info.GetUserAsync(_socketMessage).Result;

            var mentions = _socketMessage.MentionedUsers;

            _cmd = message[prefix.Length..].Split(' ')[0].ToLower();

            if (string.IsNullOrWhiteSpace(_cmd)) _cmd = "help";

            var command = _allCommands.GetValueOrDefault(_cmd);

            if (command.Equals(null))
                return $"Your command '{_cmd}' was not recognised - type '!!help' for commands.";

            var req = message[(message.IndexOf(' ') + 1)..];

            if (!req.Equals($"{prefix}{_cmd}")) _msg = req;

            if (command.Item2 == null) return command.Item1;
            else
            {
                var result = command.Item2();

                if (string.IsNullOrWhiteSpace(result)) result = "Error occurred";

                return result;
            }
        }
        private string SetPrefix()
        {
            var newPrefix = _msg;

            if (newPrefix == _guild.CommandPrefix) return "This is already the prefix";

            if (newPrefix == "!") return "Cannot use this prefix";

            if (newPrefix.Length < 1 || newPrefix.Length > 3) return "Must be between 1 to 3 characters long";

            if (Regex.Matches(newPrefix, @"[a-zA-Z]").Count > 0) return "Cannot contain letters";

            var result = _info.ChangeGuildPrefixAsync(_guild, newPrefix).Result;

            if (string.IsNullOrWhiteSpace(result)) return "Could not change prefix";

            return $"Prefix has been changed to *'{result}'*, all commands must now start with ***{result}***";
        }

        private string ShowHelp()
        {
            string result = $"Thank you for using RhyHelp Bot, {_socketMessage.Author.Mention}.\n" +
                $"RhyHelp commands start with: **'{_guild.CommandPrefix}'** e.g. **{_guild.CommandPrefix}search**\n\n" +
                $"**Current commands I can do:**\n";

            var sb = new StringBuilder(result);

            foreach (var cmd in _allCommands)
            {
                sb.Append($":face_vomiting: ***'{_guild.CommandPrefix}{cmd.Key}'*** - {cmd.Value.Item1}'\n\n");
            }

            return sb.ToString();
        }

        private string DiceRoller()
        {
            int value;

            if (string.IsNullOrWhiteSpace(_msg) || _msg == "!!dice") value = _guild.DiceDefault;
            else if (!int.TryParse(_msg, out value)) return "Must enter a whole number";

            if (value < 2) return "max limit must be higher than one";

            var r = new Random();

            var roll = r.Next(1, value);

            return $":game_die: **{value}-sided** Dice Rolled: {_methods.GetNumberEmojis(roll)}";
        }
        private string SetDiceRollerDefault()
        {
            int value;

            if (string.IsNullOrWhiteSpace(_msg)) return $":game_die: Current Dice roll range is ***1-{_guild.DiceDefault}***";
            else if (!int.TryParse(_msg, out value)) return "Must enter a whole number";

            if (value < 2) return "max limit must be higher than one";

            if (value == _guild.DiceDefault) return "This is already the default dice roll";

            var result = _info.ChangeGuildDiceDefaultAsync(_guild, value).Result;

            if (result < 0) return "Dice default value was not changed";

            return $":game_die: Dice rolls now have a default range of **1 - {result}** if no number is given when rolling the dice";
        }

        private string SetSearchRestriction()
        {
            RestrictType restriction;

            if (_msg == "off") restriction = RestrictType.Off;
            else if (_msg == "partial") restriction = RestrictType.Partial;
            else if (_msg == "full") restriction = RestrictType.Full;

            else return "invalid input (off/partial/full)";

            var result = _info.ChangeGuildRestrictionAsync(_guild, restriction).Result;

            if (string.IsNullOrWhiteSpace(result)) return "Restriction was not changed";

            return $"Search restrict set to: {restriction}";
        }

        private string SetSearchLimit()
        {
            if (!int.TryParse(_msg, out int value)) return "Must enter a whole number";

            var result = _info.ChangeGuildLimitAsync(_guild, value).Result;

            if (result < 0) return "Limit was not changed";

            return $"Search return limit changed to {result}";
        }

        private string SearchYoutube()
        {
            var videos = _methods.GetVideos(_guild.Limit, _guild.Restrict, _msg);

            var videosInfo = new List<string>();

            var messageLength = 0;

            foreach (var video in videos)
            {
                string partial = $"{videosInfo.Count + 1}- ***{video.Title}*** By {video.Channel} (**{video.Published}**), {video.ViewsCount}, **{video.VideoLength}** - *Copy this to play*:" +
                    $"```!play www.youtube.com{video.Link}```";

                if (messageLength + partial.Length >= 2000) break;

                videosInfo.Add(partial);

                messageLength += partial.Length;
            }

            return string.Join("", videosInfo);
        }

    }
}
