using Bot;
using Bot.Sources;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

const string configPath = "./Config.json";

Config config;

try
{
	Config? c = await JsonSerializer.DeserializeAsync<Config>(File.OpenRead(configPath));

	if (c is null || string.IsNullOrWhiteSpace(c.Token))
	{
		Console.WriteLine("Missing Token");

		_ = Console.ReadKey();

		return;
	}

	config = c;
}
catch
{
	Console.WriteLine($"Unable to read token from \"{configPath}\"");

	_ = Console.ReadKey();

	return;
}

using DiscordClient discord = new(new()
{
	Token = config.Token,
	TokenType = TokenType.Bot,
	MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
});

ServiceCollection services = new();

services.AddSingleton(discord);
services.AddSingleton<Channels>();
services.AddSingleton<Data>();
services.AddSingleton<RSSSource>();

ServiceProvider provider = services.BuildServiceProvider();

discord.UseCommandsNext(new()
{
	Services = provider
});

SlashCommandsExtension slash = discord.UseSlashCommands(new()
{
	Services = provider
});

slash.RegisterCommands<SlashCommands>();

discord.Ready += async (client, readArgs) =>
{
	RSSSource? rss = provider.GetService<RSSSource>();

	if (rss is null)
	{
		return;
	}

	using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));

	try
	{
		while (await timer.WaitForNextTickAsync())
		{
			Console.WriteLine("Checking for notifications...");

			try
			{
				bool found = await rss.RunAsync();

				if (found)
				{
					Console.WriteLine("Found notifications");
				}
				else
				{
					Console.WriteLine("No notifications found");
				}
			}
			catch
			{
				continue;
			}
		}
	}
	catch { }
};

await discord.ConnectAsync();

Console.WriteLine("Bot Started");

await Task.Delay(-1);