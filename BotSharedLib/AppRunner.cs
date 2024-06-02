using BotSharedLib.Sources;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace BotSharedLib
{
	public sealed class AppRunner : IDisposable
	{
		private DiscordClient? _discordClient = null;
		private static readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

		public void Dispose()
		{
			_discordClient?.Dispose();
		}

		public async ValueTask Run(string configPath)
		{
			ArgumentException.ThrowIfNullOrEmpty(configPath, nameof(configPath));

			Config config;

			try
			{
				Config? c = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(configPath));

				if (c is null || string.IsNullOrWhiteSpace(c.Token))
				{
					Console.Write("Missing Token");

					await Task.Delay(TimeSpan.FromSeconds(30));

					Environment.Exit(0);
				}

				config = c;
			}
			catch
			{
				Console.Write($"Unable to read token from \"{configPath}\"");

				await Task.Delay(TimeSpan.FromSeconds(30));

				Environment.Exit(0);

				return;
			}

			_discordClient = new(new()
			{
				Token = config.Token,
				TokenType = TokenType.Bot,
				MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
				ReconnectIndefinitely = true
			});

			ServiceCollection services = new();

			services.AddSingleton(_discordClient);
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

			_discordClient.Ready += async (client, readArgs) =>
			{
				RSSSource? rss = provider.GetService<RSSSource>();

				if (rss is null)
				{
					return;
				}

				using PeriodicTimer timer = new(_interval);

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
