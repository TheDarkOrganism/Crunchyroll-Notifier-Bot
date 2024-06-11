# Crunchyroll Notifier Bot #

<br>

## Overview ##

<br>

**Crunchyroll Notifier Bot** is a discord bot written in .Net 7.<br>
It sends notifications to the user about new episodes on Cruchyroll.<br>
This bot uses the RSS feed found at [Recently Added Anime Videos](http://feeds.feedburner.com/crunchyroll/rss/anime)
provided by Cruchyroll.

<br>

This bot is based on another one of my repositories [Crunchyroll Notifier](https://github.com/TheDarkOrganism/Crunchyroll-Notifier)
which was a similar software except it provided windows notifications. 

<b>

## How to Use ##

<br>

This bot can be ran locally or hosted on a server.

<br>

In order for the bot to show notifications you need to allow "Send Messages"<br>
for the channel you want notifications in.

<br>

### Bot Permissions ###

<br>

Permissions required when creating the bot in the [Discord Developer Portal](https://discord.com/developers/applications/).

<ul>
    <li>bot</li>
    <li>Embed Links</li>
</ul>

<br>

### Config.json ###

<br>

**Interval**: How often to check for new episodes in seconds.<br>
**Type**: Double<br>
**Condition**: Must be greater than or equal to 10.<br>
**Required**: Yes<br>

<br>

**LogLevel**: The minimum level of type of logs to display in the bot output.<br>
**Type**: Enum<br>
**Required**: Yes<br>

<br>

**Token**: The bot token used by the program to connect with discord.<br>
**Type**: String<br>
**Required**: Yes<br>

<br>

&copy; 2024 Richard Whicker
