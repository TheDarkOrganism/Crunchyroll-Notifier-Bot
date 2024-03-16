using Dapper;
using System.Data.SQLite;

namespace Bot.DataAccess
{
	public abstract class DataAccessBase<TModel>
	{
		private readonly string _name;

		private const string _dataSource = "./Database.db";

		private readonly string _saveSql;

		private readonly string _deleteSql;

		private readonly string _primaryKeySql;

		private static readonly string _connectionString = new SQLiteConnectionStringBuilder()
		{
			DataSource = _dataSource,
			Version = 3
		}.ConnectionString;

		protected abstract string PrimaryKey { get; }

		protected DataAccessBase(string name)
		{
			_name = name;

			IEnumerable<string> properties = typeof(TModel).GetProperties().Select(p => p.Name);

			_saveSql = $"insert into {name} ({string.Join(", ", properties)}) values({string.Join(", ", properties.Select(p => $"@{p}"))})";

			_primaryKeySql = $" where {PrimaryKey} = {$"@{PrimaryKey}"}";

			_deleteSql = $"delete from {name}{_primaryKeySql}";
		}

		private static T? HandleConnection<T>(DatabaseHandler<IDbConnection, T> handler)
		{
			using SQLiteConnection connection = new()
			{
				ConnectionString = _connectionString
			};

			return File.Exists(_dataSource) ? handler(connection) : default;
		}

		private static T? HandleTransaction<T>(DatabaseHandler<IDbTransaction, T> handler)
		{
			return HandleConnection(connection =>
			{
				connection.Open();

				using IDbTransaction transaction = connection.BeginTransaction();

				T result = handler(transaction);

				try
				{
					transaction.Commit();

					return result;
				}
				catch
				{
					try
					{
						transaction.Rollback();
					}
					catch (Exception ex)
					{
						throw new TransactionRollbackException(transaction, ex);
					}

					return default;
				}
			});
		}

		private static async Task<int> ExecuteAsync<T>(T? data, string sql)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(sql, nameof(sql));

			Task<int>? result = HandleTransaction(transaction => transaction.Connection?.ExecuteAsync(new CommandDefinition(sql, data, transaction)));


			return result is null || result.IsFaulted ? default : await result;
		}

		public async Task<IEnumerable<TModel>> GetModelsAsync()
		{
			Task<IEnumerable<TModel>>? result = HandleConnection(connection => connection.QueryAsync<TModel>($"select * from {_name}"));

			return result is null || result.IsFaulted ? [] : await result;
		}

		public async Task<bool> Contains(TModel model)
		{
			return await ExecuteAsync(model, $"select * from {_name}{_primaryKeySql}") > 0;
		}

		public async Task<bool> SaveModelAsync(TModel model)
		{
			return await ExecuteAsync(model, _saveSql) > 0;
		}

		public async Task<bool> DeleteModelAsync(TModel model)
		{
			return await ExecuteAsync(model, _deleteSql) > 0;
		}
	}
}
