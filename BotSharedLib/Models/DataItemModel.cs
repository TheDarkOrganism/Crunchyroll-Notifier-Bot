namespace BotSharedLib.Models
{
	public sealed class DataItemModel : IEquatable<DataItemModel>
	{
		public required string Name { get; init; }

		public required string Value { get; init; }

		public bool Equals(DataItemModel? other)
		{
			return Name == other?.Name && Value == other.Value;
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as DataItemModel);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
