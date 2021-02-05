using Microsoft.EntityFrameworkCore;
using RhythmHelper.Data.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RhythmHelper.Data
{
    public class GuildRepo : IGuildRepo
    {
        private readonly GuildContext _context;

        public GuildRepo()
        {
            Log.Information($"Exe {new StackTrace().GetFrame(0).GetMethod()} Thread:{Thread.CurrentThread.ManagedThreadId}");

            _context = new GuildContext();
        }

        public void Add<T>(T entity) where T : class
        {
            Log.Information($"Exe {new StackTrace().GetFrame(0).GetMethod()} Thread:{Thread.CurrentThread.ManagedThreadId}");

            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            Log.Information($"Exe {new StackTrace().GetFrame(0).GetMethod()} Thread:{Thread.CurrentThread.ManagedThreadId}");

            _context.Remove(entity);
        }

        public void Update<T>(T entity) where T : class
        {
            Log.Information($"Exe {new StackTrace().GetFrame(0).GetMethod()} Thread:{Thread.CurrentThread.ManagedThreadId}");

            _context.Update(entity);
        }

        public async Task<Guild> GetGuildById(string guildId)
        {
            Log.Information($"Exe {new StackTrace().GetFrame(0).GetMethod()} Thread:{Thread.CurrentThread.ManagedThreadId}");

            IQueryable<Guild> query = _context.Guilds
                .Include(g => g.Users)
                .Include(g => g.Roles);

            query = query.Where(q => q.GuildId == guildId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Role[]> GetGuildRolesById(string guildId)
        {
            Log.Information($"Exe {new StackTrace().GetFrame(0).GetMethod()} Thread:{Thread.CurrentThread.ManagedThreadId}");

            IQueryable<Role> query = _context.Roles;

            query = query.Where(q => q.Guild.GuildId == guildId);

            return await query.ToArrayAsync();
        }

        public async Task<Role> GetRoleById(string roleId)
        {
            Log.Information($"Exe {new StackTrace().GetFrame(0).GetMethod()} Thread:{Thread.CurrentThread.ManagedThreadId}");

            IQueryable<Role> query = _context.Roles;

            query = query.Where(q => q.RoleId == roleId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByDiscriminator(string discriminator)
        {
            Log.Information($"Exe {new StackTrace().GetFrame(0).GetMethod()} Thread:{Thread.CurrentThread.ManagedThreadId}");

            IQueryable<User> query = _context.Users;

            query = query.Where(q => q.Discriminator == discriminator);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<User> GetUserById(string userId)
        {
            Log.Information($"Exe {new StackTrace().GetFrame(0).GetMethod()} Thread:{Thread.CurrentThread.ManagedThreadId}");

            IQueryable<User> query = _context.Users;

            query = query.Where(q => q.UserId == userId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            Log.Information($"Exe {new StackTrace().GetFrame(0).GetMethod()} Thread:{Thread.CurrentThread.ManagedThreadId}");

            return (await _context.SaveChangesAsync()) > 0;
        }

        public async Task<Role> GetGuildRoleByName(string guildId, string roleName)
        {
            Log.Information($"Exe {new StackTrace().GetFrame(0).GetMethod()} Thread:{Thread.CurrentThread.ManagedThreadId}");

            IQueryable<Role> query = _context.Roles
                .Include(q => q.Guild);

            query = query.Where(q => q.Name == roleName && q.Guild.GuildId == guildId);

            return await query.FirstOrDefaultAsync();
        }
    }
}
