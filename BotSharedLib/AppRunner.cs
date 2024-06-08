using BotSharedLib.Converters;
using BotSharedLib.Sources;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;

namespace BotSharedLib
{
	public sealed class AppRunner : IDisposable
	{
		private static readonly TimeSpan _errorExitDelay = TimeSpan.FromSeconds(30);

		private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
		{
			Converters = {
				new EnumConverter(),
				new TimeSpanConverter()
			}
		};

		private readonly List<DiscordClient> _discordClients = new();

		private readonly ILogger<AppRunner> _errorLogger;

		private static void ConfigureLogger(ILoggingBuilder builder, LogLevel logLevel)
		{
			builder.AddConsole().SetMinimumLevel(logLevel);
		}

		public AppRunner()
		{
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => ConfigureLogger(builder, LogLevel.Critical));

			_errorLogger = loggerFactory.CreateLogger<AppRunner>();
		}

		public async ValueTask Run(string configPath)
		{
			ArgumentException.ThrowIfNullOrEmpty(configPath, nameof(configPath));

			Config config;

			try
			{
				Config? c = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(configPath), _jsonSerializerOptions);

				if (c is null || string.IsNullOrWhiteSpace(c.Token))
				{
					_errorLogger.LogCritical("Missing Token");

					await Task.Delay(_errorExitDelay);

					Environment.Exit(0);
				}

				config = c;
			}
			catch (FileNotFoundException ex)
			{
				_errorLogger.LogCritical(ex, "Unable to find the config file at \"{Config}\"", configPath);

				await Task.Delay(_errorExitDelay);

				Environment.Exit(2);

				return;
			}
			catch (JsonException ex)
			{
				_errorLogger.LogCritical(ex, "The config file at \"{Config}\" was invalid", configPath);

				await Task.Delay(_errorExitDelay);

				Environment.Exit(13);

				return;
			}
			catch
			{
				_errorLogger.LogCritical("Unable to read token from \"{Config}\"", configPath);

				await Task.Delay(_errorExitDelay);

				Environment.Exit(13);

				return;
			}

			LogLevel logLevel = config.LogLevel;

			DiscordClient discordClient = new(new()
			{
				Token = config.Token,
				TokenType = TokenType.Bot,
				MinimumLogLevel = logLevel,
				ReconnectIndefinitely = true
			});

			_discordClients.Add(discordClient);

			ServiceCollection services = new();

			services.AddLogging(builder => ConfigureLogger(builder, logLevel));

			services.AddSingleton(discordClient);
			services.AddSingleton<RSSSource>();

			ServiceProvider provider = services.BuildServiceProvider();

			discordClient.UseCommandsNext(new()
			{
				Services = provider
			});

			ILogger<AppRunner>? logger = provider.GetService<ILogger<AppRunner>>();

			if (logger is null)
			{
				return;
			}

			discordClient.Ready += async (client, readArgs) =>
			{
				RSSSource? rss = provider.GetService<RSSSource>();

				if (rss is null)
				{
					return;
				}

				using PeriodicTimer timer = new(config.Interval);

				try
				{
					while (await timer.WaitForNextTickAsync())
					{
						logger.LogInformation("Checking for notifications...");

						try
						{
							bool found = await rss.RunAsync();

							if (found)
							{
								logger.LogInformation("Found notifications");
							}
							else
							{
								logger.LogInformation("No notifications found");
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

			await discordClient.ConnectAsync();

			logger.LogInformation("Bot Started");
		}

		public void Dispose()
		{
			foreach (DiscordClient discordClient in _discordClients)
			{
				discordClient.Dispose();
			}
		}
	}
}
