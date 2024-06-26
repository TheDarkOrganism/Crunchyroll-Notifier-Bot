﻿using DSharpPlus;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Bot
{
	internal sealed partial class BotRunner : IDisposable
	{
		#region Member Variables

		private readonly ILogger<BotRunner> _logger;
		private readonly DiscordClient _client;

		private bool _isRunning = false;

		#endregion

		public BotRunner(Config config)
		{
			#region Setup Logger

			LogLevel logLevel = config.LogLevel;

			using (ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(logLevel)))
			{
				_logger = loggerFactory.CreateLogger<BotRunner>();
			}

			#endregion

			#region Setup Client

			_client = new(new()
			{
				Token = config.Token,
				TokenType = TokenType.Bot,
				MinimumLogLevel = logLevel,
				ReconnectIndefinitely = true
			});

			_client.GuildDownloadCompleted += async (client, readArgs) =>
			{
				using PeriodicTimer timer = new(config.Interval);

				XmlNamespaceManager? manager = null;

				DateTime last = DateTime.Now;

				#region Main Loop

				while (await timer.WaitForNextTickAsync())
				{
					_logger.LogInformation("Checking for notifications...");

					int found = 0;

					DateTime lastOffset = last.AddHours(2);

					try
					{
						#region Read RSS Feed

						XPathNavigator navigator = new XPathDocument("http://feeds.feedburner.com/crunchyroll/rss/anime").CreateNavigator();

						if (manager is null)
						{
							manager = new(navigator.NameTable);

							manager.AddNamespace("media", "http://search.yahoo.com/mrss/");
							manager.AddNamespace("crunchyroll", "http://www.crunchyroll.com/rss");
						}

						#endregion

						_logger.LogDebug("Last = {Last}", last);

						#region Read RSS Data

						foreach (XPathNavigator nav in navigator.Select("//item").OfType<XPathNavigator>().Reverse())
						{
							if (DateTime.TryParse(nav.SelectSingleNode(".//pubDate", manager)?.Value, out DateTime result) && result > last && result <= lastOffset)
							{
								found++;

								string? dub = nav.SelectSingleNode(".//title", manager)?.Value is string title ? DubRegex().Match(title).Groups.Values.ElementAtOrDefault(1)?.Value : null;

								_logger.LogTrace("Dub = {Dub}", dub);

								if (nav.SelectSingleNode(".//link")?.Value is string url && nav.SelectSingleNode(".//crunchyroll:seriesTitle", manager)?.Value is string seriesTitle)
								{
									string showText = $"{seriesTitle} {(string.IsNullOrWhiteSpace(dub) ? string.Empty : $"({dub})")}";

									_logger.LogTrace("Title = {ShowText}", showText);

									int? episode = nav.SelectSingleNode(".//crunchyroll:episodeNumber", manager)?.ValueAsInt;

									_logger.LogTrace("Episode = {Episode}", episode);

									int? season = nav.SelectSingleNode(".//crunchyroll:season", manager)?.ValueAsInt;

									_logger.LogTrace("Season = {Season}", season);

									string? thumbnail = nav.SelectSingleNode(".//enclosure")?.GetAttribute("url", string.Empty)?.Replace("_thumb", "_full");

									_logger.LogTrace("Thumbnail = {Thumbnail}", thumbnail);

									string description = nav.SelectSingleNode(".//description")?.Value ?? string.Empty;

									description = description[(description.LastIndexOf('>') + 1)..];

									_logger.LogTrace("Description = {Description}", description);

									#region Create Discord Embed

									DiscordEmbedBuilder builder = new()
									{
										ImageUrl = thumbnail ?? string.Empty,
										Description = description,
										Title = showText,
										Timestamp = result,
										Url = url
									};

									if (season is not null)
									{
										_ = builder.AddField("Season", season.ToString(), true);
									}

									_ = builder.AddField("Episode", episode.ToString(), true);

									#endregion

									#region Send Messages

									foreach (DiscordGuild guild in client.Guilds.Values)
									{
										foreach ((ulong id, DiscordChannel channel) in guild.Channels)
										{
											foreach (DiscordOverwrite channelOverride in channel.PermissionOverwrites)
											{
												if (channelOverride.Allowed == Permissions.SendMessages && channelOverride.Type == OverwriteType.Member && (await channelOverride.GetMemberAsync()) == client.CurrentUser)
												{
													try
													{
														_logger.LogDebug("Sending notification to {Name}@{ID}", channel.Name, id);

														await channel.SendMessageAsync(builder);
													}
													catch
													{
														_logger.LogDebug("Uanable to find notify channel with id {ID}", id);
													}

													break;
												}
											}
										}
									}

									#endregion
								}

								last = result;

								_logger.LogDebug("Updated Last = {Last}", last);
							}
						}

						#endregion
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "Failed to load RSS feed");
					}

					if (found > 0)
					{
						_logger.LogInformation("Found {Count} notification(s)", found);
					}
					else
					{
						_logger.LogInformation("No notifications found");
					}

				}

				#endregion
			};

			#endregion
		}

		[GeneratedRegex("\\(([A-Za-z\\-]+) Dub\\)")]
		private static partial Regex DubRegex();

		public async Task RunAsync()
		{
			if (!_isRunning)
			{
				await _client.ConnectAsync();

				_isRunning = true;

				_logger.LogInformation("Bot Started");
			}
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}
