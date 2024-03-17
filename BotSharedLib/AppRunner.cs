using BotSharedLib.Models;
using BotSharedLib.Sources;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace BotSharedLib
{
	public sealed class AppRunner : IDisposable
	{
		private DiscordClient? _discordClient = null;

		public void Dispose()
		{
			_discordClient?.Dispose();
		}

		public async ValueTask Run(string configPath, IChannelStorageManager channel, IDataStorageManager data)
		{
			ArgumentException.ThrowIfNullOrEmpty(configPath, nameof(configPath));

			Config config;

			try
			{
				Config? c = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(configPath));

				if (c is null || string.IsNullOrWhiteSpace(c.Token))
				{
					throw new Exception("Missing Token");
				}

				config = c;
			}
			catch
			{
				throw new Exception($"Unable to read token from \"{configPath}\"");
			}

			_discordClient = new(new()
			{
				Token = config.Token,
				TokenType = TokenType.Bot,
				MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
			});

			ServiceCollection services = new();

			services.AddSingleton(_discordClient);
			services.AddSingleton(channel);
			services.AddSingleton(data);
			services.AddSingleton<RSSSource>();

			ServiceProvider provider = services.BuildServiceProvider();

			_discordClient.UseCommandsNext(new()
			{
				Services = provider
			});

			SlashCommandsExtension slash = _discordClient.UseSlashCommands(new()
			{
				Services = provider
			});

			slash.RegisterCommands<SlashCommands>();

			_discordClient.Ready += async (client, readArgs) =>
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

			await _discordClient.ConnectAsync();

			Console.WriteLine("Bot Started");
		}
	}
}
