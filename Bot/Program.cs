using Bot;
using DSharpPlus;
using System.Text.Json;

const string configPath = "./Config.json";

Config config;

try
{
	Config? c = await JsonSerializer.DeserializeAsync<Config>(File.OpenRead(configPath));

	if (c is null || string.IsNullOrWhiteSpace(c.Token))
	{
		Console.WriteLine("Missing Token");

		return;
	}

	config = c;
}
catch
{
	Console.WriteLine($"Unable to read token from \"{configPath}\"");

	return;
}

using DiscordClient discord = new(new()
{
	Token = config.Token,
	TokenType = TokenType.Bot,
	Intents = DiscordIntents.MessageContents
});

await discord.ConnectAsync();

Console.WriteLine("Bot Started");