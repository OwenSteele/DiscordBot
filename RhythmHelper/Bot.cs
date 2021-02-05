﻿using Discord;
using Discord.WebSocket;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(
                @$"..\..\..\Logs\{DateTime.Now:yyyy-MM-dd HH}hrs log.txt")
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

            if (socketMessage.Author.IsBot)
                return Task.CompletedTask;

            _message++;

            Log.Debug($"        ---****{msgVal} CMD [Bot] Thread:{Thread.CurrentThread.ManagedThreadId} \"{socketMessage.Content.Replace("\n"," [CRLF] ")}\"");

            var handler = new Commands(socketMessage, ref _info, _methods, msgVal);
            var result = handler.NewCommand();

            if (string.IsNullOrWhiteSpace(result)) return Task.CompletedTask;

            Log.Debug($"        ---****{msgVal} MSG [Bot] Thread:{Thread.CurrentThread.ManagedThreadId} \"{result.Replace("\n", " [CRLF] ")}\"");

            return socketMessage.Channel.SendMessageAsync(result);
        }
    }
}
