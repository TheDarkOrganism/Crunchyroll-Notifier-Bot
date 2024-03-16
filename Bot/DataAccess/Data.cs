namespace Bot.DataAccess
{
	internal sealed class Data : DataAccessBase<DataItemModel>
	{
		public Data() : base(nameof(Data)) { }

		protected override string PrimaryKey => nameof(DataItemModel.Name);

		public async ValueTask<bool> SetLast(DateTime dateTime)
		{
			return await SaveModelAsync(new()
			{
				Name = "LastCheck",
				Value = dateTime.ToString()
			});
		}

		private async ValueTask<DataItemModel?> GetModel(string name)
		{
			return (await GetModelsAsync()).FirstOrDefault(item => item.Name == name);
		}

		public async ValueTask<DateTime?> GetLast()
		{
			return DateTime.TryParse((await GetModel("LastCheck"))?.Value, out DateTime result) ? result : null;
		}
	}
}
