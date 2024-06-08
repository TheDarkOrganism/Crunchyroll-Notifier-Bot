namespace BotSharedLib.Sources
{
	internal abstract class SourceBase
	{
		private readonly DiscordClient _client;
		private readonly ILogger<SourceBase> _logger;

		protected DateTime _last;

		protected SourceBase(DiscordClient client, ILogger<SourceBase> logger)
		{
			_client = client;
			_logger = logger;
			_last = DateTime.Now;
		}

		public abstract ValueTask<bool> RunAsync();

		protected async ValueTask SendNotificationsAsync(Notification notification)
		{
			DiscordEmbedBuilder builder = new()
			{
				ImageUrl = notification.Thumbnail,
				Description = notification.Description,
				Title = notification.ShowText,
				Timestamp = notification.ReleaseDate,
				Url = notification.Url
			};

			if (notification.Season is not null)
			{
				_ = builder.AddField("Season", notification.Season.ToString(), true);
			}

			_ = builder.AddField("Episode", notification.Episode.ToString(), true);

			foreach (DiscordGuild guild in _client.Guilds.Values)
			{
				foreach ((ulong id, DiscordChannel channel) in guild.Channels)
				{
					foreach (DiscordOverwrite channelOverride in channel.PermissionOverwrites)
					{
						if (channelOverride.Allowed == Permissions.SendMessages && channelOverride.Type == OverwriteType.Member && (await channelOverride.GetMemberAsync()) == _client.CurrentUser)
						{
							try
							{
								_logger.LogDebug("Sending notification {Notification} to {Name}@{ID}", notification, channel.Name, id);

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
		}
	}
}
