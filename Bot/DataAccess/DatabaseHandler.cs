namespace Bot.DataAccess
{
	/// <summary>
	/// A handler for a <typeparamref name="TInput"/>
	/// the database object.
	/// </summary>
	/// <typeparam name="TInput"></typeparam>
	/// <typeparam name="TOutput"></typeparam>
	/// <param name="input">
	/// The database object to handle.
	/// </param>
	/// <returns>
	/// <typeparamref name="TOutput"/>
	/// as the handled <typeparamref name="TInput"/>.
	/// </returns>
	internal delegate TOutput DatabaseHandler<TInput, out TOutput>(TInput input) where TInput : IDisposable;
}
