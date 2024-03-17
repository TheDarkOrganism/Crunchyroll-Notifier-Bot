using Dapper;
using System.Data.SQLite;

namespace Bot.DataAccess
{
	public abstract class DataAccessBase<TModel> : IStorageManagerBase<TModel>
		where TModel : notnull, IEquatable<TModel>
	{
		private readonly string _name;

		private readonly string _dataSource;

		private readonly string _saveSql;

		private readonly string _deleteSql;

		private readonly string _primaryKeySql;

		private readonly string _connectionString;

		protected readonly List<TModel> _models;

		protected abstract string PrimaryKey { get; }

		protected DataAccessBase(string name)
		{
			_name = name;

			_dataSource = $"./Database.db";

			_connectionString = new SQLiteConnectionStringBuilder()
			{
				DataSource = _dataSource,
				Version = 3
			}.ConnectionString;

			_models = new();

			IEnumerable<string> properties = typeof(TModel).GetProperties().Select(p => p.Name);

			_saveSql = $"insert into {name} ({string.Join(", ", properties)}) values({string.Join(", ", properties.Select(p => $"@{p}"))})";

			_primaryKeySql = $" where {PrimaryKey} = {$"@{PrimaryKey}"}";

			_deleteSql = $"delete from {name}{_primaryKeySql}";
		}

		private T? HandleConnection<T>(DatabaseHandler<IDbConnection, T> handler)
		{
			using SQLiteConnection connection = new()
			{
				ConnectionString = _connectionString
			};

			return File.Exists(_dataSource) ? handler(connection) : default;
		}

		private T? HandleTransaction<T>(DatabaseHandler<IDbTransaction, T> handler)
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

		private async Task<int> ExecuteAsync<T>(T? data, string sql)
		{
			ArgumentException.ThrowIfNullOrEmpty(sql, nameof(sql));

			Task<int>? result = HandleTransaction(transaction => transaction.Connection?.ExecuteAsync(new CommandDefinition(sql, data, transaction)));


			return result is null || result.IsFaulted ? default : await result;
		}

		public async Task<IEnumerable<TModel>> GetModelsAsync()
		{
			Task<IEnumerable<TModel>>? result = HandleConnection(connection => connection.QueryAsync<TModel>($"select * from {_name}"));

			return result is null || result.IsFaulted ? Enumerable.Empty<TModel>() : await result;
		}

		public async Task<bool> SaveModelAsync(TModel model)
		{
			return await ExecuteAsync(model, _saveSql) > 0;
		}

		public bool Set(TModel model)
		{
			if (!_models.Contains(model))
			{
				_models.Add(model);

				return true;
			}
			else
			{
				return false;
			}
		}

		public bool Delete(TModel model)
		{
			return _models.Remove(model);
		}

		public IEnumerable<TModel> GetAll()
		{
			return _models;
		}

		public async ValueTask FlushAsync()
		{
			foreach (TModel model in _models.Except(await GetModelsAsync()))
			{
				_ = await SaveModelAsync(model);
			}
		}
	}
}
