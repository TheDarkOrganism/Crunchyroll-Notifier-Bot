using BotSharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharedLib
{
	public interface IDataStorageManager : IStorageManagerBase<DataItemModel>
	{
		public DateTime? GetLast();

		public void SetLast(DateTime last);
	}
}
