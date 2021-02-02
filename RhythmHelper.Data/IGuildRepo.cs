using RhythmHelper.Data.Entities;
using System;
using System.Threading.Tasks;

namespace RhythmHelper.Data
{
    public interface IGuildRepo
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
        Task<bool> SaveChangesAsync();

        Task<Guild> GetGuildById(string guildId);

        Task<User> GetUserById(string userId);
        Task<User> GetUserByDiscriminator(string discriminator);

        Task<Role[]> GetGuildRolesById(string guildId);
        Task<Role> GetRoleById(string roleId);
        Task<Role> GetGuildRoleByName(string guildId, string roleName);
    }
}
