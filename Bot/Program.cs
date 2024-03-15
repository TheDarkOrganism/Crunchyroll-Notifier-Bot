using Bot;
using DSharpPlus;
using DSharpPlus.SlashCommands;
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
	MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
});

SlashCommandsExtension slash = discord.UseSlashCommands();

slash.RegisterCommands<SlashCommands>();

await discord.ConnectAsync();

Console.WriteLine("Bot Started");

_ = Console.ReadKey();