using BotSharedLib.Models;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace BotSharedLib
{
	internal sealed class SlashCommands : ApplicationCommandModule
	{
		private readonly IChannelStorageManager _channels;

		public SlashCommands(IChannelStorageManager channels)
		{
			_channels = channels;
		}

		private async Task<(bool, string)> HandleChannelAsync(InteractionContext context, Func<ChannelModel, bool> func)
		{
			DiscordChannel channel = context.Channel;

			ulong channelId = channel.Id;

			ChannelModel model = new()
			{
				Id = channelId
			};

			bool success = func(model);

			if (success)
			{
				await _channels.FlushAsync();
			}

			return (success, string.Join('@', channel.Name, channelId));
		}

		[SlashCommand("join", "Joins the current channel.")]
		[RequireRoles(RoleCheckMode.Any, "Owner", "Admin")]
		public async Task Join(InteractionContext context)
		{
			(bool success, string responseText) = await HandleChannelAsync(context, _channels.Set);

			await context.CreateResponseAsync(success ? $"Joined channel {responseText}" : $"Already in channel {responseText}", true);
		}

		[SlashCommand("leave", "Leaves the current channel.")]
		[RequireRoles(RoleCheckMode.Any, "Owner", "Admin")]
		public async Task Leave(InteractionContext context)
		{
			(bool success, string responseText) = await HandleChannelAsync(context, _channels.Delete);

			await context.CreateResponseAsync(success ? $"Left channel {responseText}" : $"Already left channel {responseText}", true);
		}
	}
}
