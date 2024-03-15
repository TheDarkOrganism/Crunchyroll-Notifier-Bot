using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Bot.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class TextChannelAttribute : CheckBaseAttribute
	{
		public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
		{
			return Task.FromResult(ctx.Channel.Type == DSharpPlus.ChannelType.Text);
		}
	}
}
