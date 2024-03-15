using System.Data;

namespace Bot.DataAccess
{
	/// <summary>
	/// Represents errors that occur during a <see cref="IDbTransaction.Rollback"/>.
	/// </summary>
	public sealed class TransactionRollbackException : InvalidOperationException
	{
		/// <summary>
		/// Creates a new <see cref="TransactionRollbackException"/>
		/// from the <paramref name="transaction"/>
		/// and the <paramref name="exception"/>.
		/// </summary>
		/// <param name="transaction">
		/// The <see cref="IDbTransaction"/>
		/// that caused the <paramref name="exception"/>.
		/// </param>
		/// <param name="exception">
		/// The exception caused
		/// by the <see cref="IDbTransaction.Rollback"/>.
		/// </param>
		internal TransactionRollbackException(IDbTransaction transaction, Exception exception) : base(exception.Message.Replace(".", $" for the database {transaction.Connection?.Database}.")) { }
	}
}
