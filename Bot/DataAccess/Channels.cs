namespace Bot.DataAccess
{
	internal sealed class Channels : DataAccessBase<ChannelModel>, IChannelStorageManager
	{
		protected override string PrimaryKey => nameof(ChannelModel.Id);

		public Channels() : base(nameof(Channels)) { }
	}
}
