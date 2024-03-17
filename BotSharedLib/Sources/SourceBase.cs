namespace BotSharedLib.Sources
{
	internal abstract class SourceBase
	{
		protected readonly IDataStorageManager _data;
		private readonly IChannelStorageManager _channels;
		private readonly DiscordClient _client;

		protected SourceBase(DiscordClient client, IDataStorageManager data, IChannelStorageManager channels)
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

			foreach (ChannelModel channelModel in _channels.GetAll())
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

		protected async ValueTask<DateTime> GetLastAsync()
		{
			DateTime? old = _data.GetLast();

			DateTime now = DateTime.Now;

			if (old is null)
			{
				_data.SetLast(now);

				await _data.FlushAsync();

				return now;
			}
			else
			{
				return old ?? now;
			}
		}
	}
}
