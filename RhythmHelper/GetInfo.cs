using Discord.WebSocket;
using RhythmHelper.Data;
using RhythmHelper.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace RhythmHelper
{
    public class GetInfo
    {
        private readonly GuildRepo _guildRepo;
        private SocketGuild _socketGuild;

        public GetInfo()
        {
            _guildRepo = new GuildRepo();
        }

        public async Task<Guild> GetGuildAsync(SocketMessage socketMessage)
        {
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

            if (result.Owner == null && user.UserId == _socketGuild.Owner.Id.ToString())
            {
                result.Owner = user;
                result = await UpdateGuildAsync(result);
            }

            return result;
        }

        private async Task<Guild> UpdateGuildAsync(Guild guild)
        {
            _guildRepo.Update(guild);

            if (!await _guildRepo.SaveChangesAsync()) return guild;

            return await _guildRepo.GetGuildById(guild.GuildId);
        }

        public async Task<User> GetUserAsync(SocketMessage socketMessage)
        {
            var channel = socketMessage.Channel as SocketGuildChannel;
            _socketGuild = channel.Guild;
            var user = socketMessage.Author as SocketGuildUser;

            var result = await _guildRepo.GetUserById(user.Id.ToString());

            if (result == null)
            {
                var addResult = await AddNewUserAsync(user);

                if (addResult == null) return null;

                return addResult;
            }

            if (result.Guild == null) result = await UpdateUserAsync(result);

            return result;
        }

        private async Task<User> UpdateUserAsync(User user)
        {
            if (user.Guild == null)
            {
                var guild = await _guildRepo.GetGuildById(_socketGuild.Id.ToString());

                if (guild == null) return user;

                user.Guild = guild;

            }

            _guildRepo.Update(user);

            if (!await _guildRepo.SaveChangesAsync()) return user;

            return await _guildRepo.GetUserById(user.UserId);
        }

        private async Task<Guild> AddNewGuildAsync(SocketGuild guild)
        {

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

            return await _guildRepo.GetGuildById(newEntity.GuildId);
        }

        private async Task<User> AddNewUserAsync(SocketGuildUser user)
        {
            var existing = await _guildRepo.GetUserById(user.Id.ToString());

            if (existing != null) return existing;

            var newEntity = new User
            {
                UserId = user.Id.ToString(),
                Username = user.Username,
                Discriminator = user.Discriminator
            };

            _guildRepo.Add(newEntity);

            if (!await _guildRepo.SaveChangesAsync()) return null;

            return await _guildRepo.GetUserById(newEntity.UserId);
        }

        public async Task<int> ChangeGuildLimitAsync(Guild guild, int limit)
        {
            guild.Limit = (limit < 1 || limit > 10) ? 10 : limit;

            _guildRepo.Update(guild);

            if (!await _guildRepo.SaveChangesAsync()) return -1;

            return limit;
        }
        public async Task<string> ChangeGuildPrefixAsync(Guild guild, string prefix)
        {
            if (prefix.Length < 1 && prefix.Length > 3) return null;

            guild.CommandPrefix = prefix;

            _guildRepo.Update(guild);

            if (!await _guildRepo.SaveChangesAsync()) return null;

            return prefix;
        }

        public async Task<string> ChangeGuildRestrictionAsync(Guild guild, RestrictType restrict)
        {
            guild.Restrict = restrict;

            _guildRepo.Update(guild);

            if (!await _guildRepo.SaveChangesAsync()) return null;

            return restrict.ToString();
        }

        public async Task<int> ChangeGuildDiceDefaultAsync(Guild guild, int value)
        {
            guild.DiceDefault = value;

            _guildRepo.Update(guild);

            if (!await _guildRepo.SaveChangesAsync()) return -1;

            return value;
        }
    }
}
