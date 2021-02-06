using Discord;
using Discord.WebSocket;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Sinks.SystemConsole;

namespace RhythmHelper
{
    public class Bot
    {
        private DiscordSocketClient _client;
        private GetInfo _info;
        private BotMethods _methods;

        private int _message = 0;

        public async Task MainAsync()
        {
            Directory.CreateDirectory(@"..\..\..\Logs\");
            Directory.CreateDirectory(@"..\..\..\Feedback\");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(
                @$"..\..\..\Logs\{DateTime.Now:yyyy-MM-dd HH}hrs log.txt")
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();

            Log.Warning("\n\n\n");

            _info = new();

            _methods = new();

            _client = new();

            _client.MessageReceived += CommandHandler;

            _client.Log += BotLog;

            var token = File.ReadAllText("RHToken.txt");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task BotLog(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
        private Task CommandHandler(SocketMessage socketMessage)
        {
            var msgVal = _message;

            if (socketMessage.Author.IsBot || string.IsNullOrWhiteSpace(socketMessage.Content))
                return Task.CompletedTask;

            _message++;
            string result = null;
            string guildName = null;

            var channel = socketMessage.Channel as SocketGuildChannel;
            var guild = channel.Guild;

            if (guild != null) guildName = guild.Name;

            Log.Debug($"        ---****{msgVal} CMD [Bot] ({(guildName ?? "GuildNameNull")}) ({socketMessage.Author.Username}) Thread:{Thread.CurrentThread.ManagedThreadId} \"{socketMessage.Content.Replace("\n", " [CRLF] ")}\"");

            try
            {
                var handler = new Commands(socketMessage, ref _info, _methods, msgVal);
                result = handler.NewCommand();

                if (string.IsNullOrWhiteSpace(result)) return Task.CompletedTask;

            }
            catch (Exception ex)
            {
                Log.Error($"---ERROR [Bot] ({(guildName ?? "GuildNameNull")}) ({socketMessage.Author.Username}) Thread:{Thread.CurrentThread.ManagedThreadId} Error: '{ex.Message}' \"{result.Replace("\n", " [CRLF] ")}\"");
            }

            Log.Debug($"        ---****{msgVal} MSG [Bot] ({(guildName ?? "GuildNameNull")}) ({socketMessage.Author.Username}) Thread:{Thread.CurrentThread.ManagedThreadId} \"{result.Replace("\n", " [CRLF] ")}\"");

            if (result.Length >= 2000) result = result[..1999];

            return socketMessage.Channel.SendMessageAsync(result);
        }
    }
}
