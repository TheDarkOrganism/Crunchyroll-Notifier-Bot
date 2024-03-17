namespace Bot.DataAccess
{
	internal sealed class Data : DataAccessBase<DataItemModel>, IDataStorageManager
	{
		public Data() : base(nameof(Data)) { }

		protected override string PrimaryKey => nameof(DataItemModel.Name);

		private DataItemModel? GetModel(string name)
		{
			return _models.FirstOrDefault(item => item.Name == name);
		}

		public void SetLast(DateTime dateTime)
		{
			DataItemModel? itemModel = GetModel("LastCheck");

			if (itemModel is not null)
			{
				_models.Remove(itemModel);
			}

			_models.Add(new()
			{
				Name = "LastCheck",
				Value = dateTime.ToString()
			});
		}

		public DateTime? GetLast()
		{
			return DateTime.TryParse(GetModel("LastCheck")?.Value, out DateTime result) ? result : null;
		}
	}
}
