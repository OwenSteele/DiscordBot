# DiscordBot
Discord bot in C# and F# with EF Core MSSQL

# Written by Owen Steele (c) 2021

# Use the bot

* The bot is always running at my on-prem server
* follow this link to add the bot to you server: https://discord.com/oauth2/authorize?client_id=805433284048715836&scope=bot
  * Or this link, if the above doesn't work: https://owensteele.co.uk/discordbot.html
  
Once the bot is in the server, simply type:
```!!```
For all of the commands/help
 
# Functions and Commands

* The bot was built for a few uses, for friends on Discord (e.g. Ø points)


* To improve upon the search of Rythm Bot
  * can change/set the minimum and maximum length of the videos returned
  * can require a title match to the search time, of varying levels
  * can limit the number of videos returned
  * **Returns concrete commands for Rythm, to copy and paste**
  
* To be a dice roller
  * uses cryptographically generated random values for dice rolls
  * can set the default dice roll, or a one-off dice roll
  * can roll the dice for yourself or multiple other people (good for DnD DM's)
  * shows dice roll distrubition for confidence in randomness

* Remembers your settings for users and servers, even if the bot is removed and then readded!

* Some commands are written in **F#** for lightweight structure and fast responses

* You can give me feedback on the bot with one of the commands! This will help to improve/fix and add features

# Design

* The bot is built in C# with a defined SQL database for storage of states and settings.

* The bot uses Tasks and Async design to handle every command - meaning that a failed command does not disrupt bot performance

* Uses Entity Framework to allows for the creation of the schema from C# objects

* Static libraries are built in F# for much lighter and more condensed code, allow for faster bot repsonses

* Uses Serilog lib for file and terminal logging, to enable quick and precise debugging from files

* Originally tested with F# xUnit and C# xUnit
