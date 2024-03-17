namespace BotSharedLib.Models
{
	public sealed class ChannelModel : IEquatable<ChannelModel>
	{
		public required ulong Id { get; init; }

		public bool Equals(ChannelModel? other)
		{
			return Id == other?.Id;
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as ChannelModel);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
