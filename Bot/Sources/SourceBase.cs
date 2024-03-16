namespace Bot.Sources
{
	internal abstract class SourceBase
	{
		protected readonly Data _data;
		private readonly Channels _channels;
		private readonly DiscordClient _client;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
		protected SourceBase(DiscordClient client, Data data, Channels channels)
		{
			_client = client;
			_data = data;
			_channels = channels;
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

			foreach (ChannelModel channelModel in await _channels.GetModelsAsync())
			{
				try
				{
					DiscordChannel channel = await _client.GetChannelAsync(channelModel.Id);

					Console.WriteLine($"Sending notification {notification} to {string.Join('@', channel.Name, channelModel.Id)}");

					await channel.SendMessageAsync(builder);
				}
				catch
				{

					Console.WriteLine($"Uanable to find notify channel with id {channelModel.Id}");
				}
			}
		}

		protected async ValueTask<DateTime> GetLast()
		{
			DateTime? old = await _data.GetLast();

			DateTime now = DateTime.Now;

			if (old is null)
			{
				await _data.SetLast(now);

				return now;
			}
			else
			{
				return old ?? now;
			}
		}
	}
}
