namespace Bot.DataAccess
{
	internal sealed class Channels : DataAccessBase<ChannelModel>
	{
		protected override string PrimaryKey => nameof(ChannelModel.Id);

		public Channels() : base(nameof(Channels)) { }
	}
}
