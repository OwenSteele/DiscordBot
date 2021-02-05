using Discord.WebSocket;
using RhythmHelper.Data;
using RhythmHelper.Data.Entities;
using Serilog;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RhythmHelper
{
    public class GetInfo
    {
        private readonly GuildRepo _guildRepo;
        private SocketGuild _socketGuild;

        public GetInfo()
        {
            Log.Information($"Ctor [GetInfo] GetInfo() Thread:{Thread.CurrentThread.ManagedThreadId}");

            _guildRepo = new GuildRepo();

            Log.Debug($"Fin [GetInfo] GetInfo() Thread:{Thread.CurrentThread.ManagedThreadId} \"init repo\"");
        }

        public async Task<Guild> GetGuildAsync(SocketMessage socketMessage)
        {
            Log.Information($"Exe [GetInfo] GetGuildAsync Thread:{Thread.CurrentThread.ManagedThreadId}");

            var channel = socketMessage.Channel as SocketGuildChannel;
            _socketGuild = channel.Guild;

            var result = await _guildRepo.GetGuildById(_socketGuild.Id.ToString());

            if (result == null)
            {
                var addResult = await AddNewGuildAsync(_socketGuild);

                if (addResult == null) return null;

                return addResult;
            }

            var user = await GetUserAsync(socketMessage);

            var existing = result.Users.FirstOrDefault(u => u.UserId == user.UserId);

            if (existing == null)
            {
                result.Users.Add(user);
                result = await UpdateGuildAsync(result);
            }

            if (_socketGuild.Owner != null)
                if (result.Owner == null && user.UserId == _socketGuild.Owner.Id.ToString())
                {
                    result.Owner = user;
                    result = await UpdateGuildAsync(result);
                }

            Log.Debug($"Rtn [GetInfo] GetGuildAsync Thread:{Thread.CurrentThread.ManagedThreadId} {result}");

            return result;
        }

        private async Task<Guild> UpdateGuildAsync(Guild guild)
        {
            Log.Information($"Exe [GetInfo] UpdateGuildAsync Thread:{Thread.CurrentThread.ManagedThreadId}");

            _guildRepo.Update(guild);

            if (!await _guildRepo.SaveChangesAsync()) return guild;

            Log.Debug($"Rtn [GetInfo] UpdateGuildAsync Thread:{Thread.CurrentThread.ManagedThreadId} \"changes saved\"");

            return await _guildRepo.GetGuildById(guild.GuildId);
        }

        public async Task<User> GetUserAsync(SocketMessage socketMessage, SocketUser mention = null)
        {
            Log.Information($"Exe [GetInfo] GetUserAsync(SocketMessage socketMessage) Thread:{Thread.CurrentThread.ManagedThreadId}");

            var channel = socketMessage.Channel as SocketGuildChannel;
            _socketGuild = channel.Guild;

            var user = socketMessage.Author as SocketGuildUser;

            var userId= user.Id.ToString();

            if (mention != null) user = mention as SocketGuildUser;

            if (user == null && mention != null) userId = mention.Id.ToString();

            var result = await _guildRepo.GetUserById(userId);

            if (result == null)
            {
                var addResult = await AddNewUserAsync(user);

                if (addResult == null) return null;

                return addResult;
            }

            if (result.Guild == null) result = await UpdateUserAsync(result);

            Log.Debug($"Rtn [GetInfo] GetUserAsync(SocketMessage socketMessage) Thread:{Thread.CurrentThread.ManagedThreadId} {result}");

            return result;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            Log.Information($"Exe [GetInfo] UpdateUserAsync Thread:{Thread.CurrentThread.ManagedThreadId}");

            if (user.Guild == null)
            {
                var guild = await _guildRepo.GetGuildById(_socketGuild.Id.ToString());

                if (guild == null) return user;

                user.Guild = guild;

            }

            _guildRepo.Update(user);

            if (!await _guildRepo.SaveChangesAsync()) return user;

            Log.Debug($"Rtn [GetInfo] UpdateUserAsync Thread:{Thread.CurrentThread.ManagedThreadId} \"changes saved\"");

            return await _guildRepo.GetUserById(user.UserId);
        }

        private async Task<Guild> AddNewGuildAsync(SocketGuild guild)
        {
            Log.Information($"Exe [GetInfo] AddNewGuildAsync Thread:{Thread.CurrentThread.ManagedThreadId}");

            var newEntity = new Guild
            {
                GuildId = guild.Id.ToString(),
                Name = guild.Name,
                Limit = 10,
                DiceDefault = 6,
                Restrict = RestrictType.Off,
                CommandPrefix = "!!"
            };

            _guildRepo.Add(newEntity);

            if (!await _guildRepo.SaveChangesAsync()) return null;

            Log.Debug($"Rtn [GetInfo] AddNewGuildAsync Thread:{Thread.CurrentThread.ManagedThreadId} \"changes saved\"");

            return await _guildRepo.GetGuildById(newEntity.GuildId);
        }

        private async Task<User> AddNewUserAsync(SocketGuildUser user)
        {
            Log.Information($"Exe [GetInfo] AddNewUserAsync Thread:{Thread.CurrentThread.ManagedThreadId}");

            var existing = await _guildRepo.GetUserById(user.Id.ToString());

            if (existing != null) return existing;

            var newEntity = new User
            {
                UserId = user.Id.ToString(),
                Username = user.Username,
                Discriminator = user.Discriminator,
                OPointer = false,
                OPoints = 0
            };

            _guildRepo.Add(newEntity);

            if (!await _guildRepo.SaveChangesAsync()) return null;

            Log.Debug($"Rtn [GetInfo] AddNewUserAsync Thread:{Thread.CurrentThread.ManagedThreadId} \"changes saved\"");

            return await _guildRepo.GetUserById(newEntity.UserId);
        }

        public async Task<int> ChangeGuildLimitAsync(Guild guild, int limit)
        {
            Log.Information($"Exe [GetInfo] ChangeGuildLimitAsync Thread:{Thread.CurrentThread.ManagedThreadId}");

            guild.Limit = (limit < 1 || limit > 10) ? 10 : limit;

            _guildRepo.Update(guild);

            if (!await _guildRepo.SaveChangesAsync()) return -1;

            Log.Debug($"Rtn [GetInfo] ChangeGuildLimitAsync Thread:{Thread.CurrentThread.ManagedThreadId} \"changes saved\"");

            return limit;
        }
        public async Task<string> ChangeGuildPrefixAsync(Guild guild, string prefix)
        {
            Log.Information($"Exe [GetInfo] ChangeGuildPrefixAsync Thread:{Thread.CurrentThread.ManagedThreadId}");

            if (prefix.Length < 1 && prefix.Length > 3) return null;

            guild.CommandPrefix = prefix;

            _guildRepo.Update(guild);

            if (!await _guildRepo.SaveChangesAsync()) return null;

            Log.Debug($"Rtn [GetInfo] ChangeGuildPrefixAsync Thread:{Thread.CurrentThread.ManagedThreadId} \"changes saved\"");

            return prefix;
        }

        public async Task<string> ChangeGuildRestrictionAsync(Guild guild, RestrictType restrict)
        {
            Log.Information($"Exe [GetInfo] ChangeGuildRestrictionAsync Thread:{Thread.CurrentThread.ManagedThreadId}");

            guild.Restrict = restrict;

            _guildRepo.Update(guild);

            if (!await _guildRepo.SaveChangesAsync()) return null;

            Log.Debug($"Rtn [GetInfo] ChangeGuildRestrictionAsync Thread:{Thread.CurrentThread.ManagedThreadId} \"changes saved\"");

            return restrict.ToString();
        }

        public async Task<int> ChangeGuildDiceDefaultAsync(Guild guild, int value)
        {
            Log.Information($"Exe [GetInfo] ChangeGuildDiceDefaultAsync Thread:{Thread.CurrentThread.ManagedThreadId}");

            guild.DiceDefault = value;

            _guildRepo.Update(guild);

            if (!await _guildRepo.SaveChangesAsync()) return -1;

            Log.Debug($"Rtn [GetInfo] ChangeGuildDiceDefaultAsync Thread:{Thread.CurrentThread.ManagedThreadId} \"changes saved\"");

            return value;
        }
    }
}
