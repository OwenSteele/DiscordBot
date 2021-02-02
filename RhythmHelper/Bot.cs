using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RhythmHelper
{
    public class Bot
    {
        private DiscordSocketClient _client;
        private GetInfo _info;
        private BotMethods _methods;

        public async Task MainAsync()
        {
            _info = new();

            _methods = new();

            _client = new();

            _client.MessageReceived += CommandHandler;

            _client.Log += Log;

            var token = File.ReadAllText("RHToken.txt");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
        private Task CommandHandler(SocketMessage socketMessage)
        {
            var handler = new Commands(socketMessage, ref _info, _methods);
            var result = handler.NewCommand();

            if (string.IsNullOrWhiteSpace(result)) return Task.CompletedTask;

            return socketMessage.Channel.SendMessageAsync(result);
        }
    }
}
