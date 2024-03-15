using Bot.DataAccess;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Bot
{
	public sealed class SlashCommands : ApplicationCommandModule
	{
		private readonly Channels _channels;

		public SlashCommands()
		{
			_channels = new();
		}

		private static async Task<(bool, string)> HandleChannelAsync(InteractionContext context, Func<ChannelModel, Task<bool>> func)
		{
			DiscordChannel channel = context.Channel;

			ulong channelId = channel.Id;

			ChannelModel model = new()
			{
				Id = channelId
			};

			return (await func(model), string.Join('@', channel.Name, channelId));
		}

		[SlashCommand("join", "Joins the current channel.")]
		public async Task Join(InteractionContext context)
		{
			(bool success, string responseText) = await HandleChannelAsync(context, _channels.SaveModelAsync);

			await context.CreateResponseAsync(success ? $"Joined channel {responseText}" : $"Already in channel {responseText}");
		}

		[SlashCommand("leave", "Leaves the current channel.")]
		public async Task Leave(InteractionContext context)
		{
			(bool success, string responseText) = await HandleChannelAsync(context, _channels.DeleteModelAsync);

			await context.CreateResponseAsync(success ? $"Left channel {responseText}" : $"Already left channel {responseText}");
		}
	}
}
