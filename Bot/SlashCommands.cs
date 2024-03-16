using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;

namespace Bot
{
	internal sealed class SlashCommands : ApplicationCommandModule
	{
		private readonly Channels _channels;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
		public SlashCommands(Channels channels)
		{
			_channels = channels;
		}

		private static async Task<(bool, string)> HandleChannelAsync(InteractionContext context, Func<ChannelModel, Task<bool>> func)
		{
			DiscordChannel channel = context.Channel;

			ulong channelId = channel.Id;

			ChannelModel model = new(channelId);

			return (await func(model), string.Join('@', channel.Name, channelId));
		}

		[SlashCommand("join", "Joins the current channel.")]
		[RequireRoles(RoleCheckMode.Any, "Owner", "Admin")]
		public async Task Join(InteractionContext context)
		{
			(bool success, string responseText) = await HandleChannelAsync(context, _channels.SaveModelAsync);

			await context.CreateResponseAsync(success ? $"Joined channel {responseText}" : $"Already in channel {responseText}", true);
		}

		[SlashCommand("leave", "Leaves the current channel.")]
		[RequireRoles(RoleCheckMode.Any, "Owner", "Admin")]
		public async Task Leave(InteractionContext context)
		{
			(bool success, string responseText) = await HandleChannelAsync(context, _channels.DeleteModelAsync);

			await context.CreateResponseAsync(success ? $"Left channel {responseText}" : $"Already left channel {responseText}", true);
		}
	}
}
