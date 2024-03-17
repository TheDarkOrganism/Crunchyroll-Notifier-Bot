namespace BotSharedLib
{
	public interface IDataStorageManager : IStorageManagerBase<DataItemModel>
	{
		public DateTime? GetLast();

		public void SetLast(DateTime last);
	}
}
