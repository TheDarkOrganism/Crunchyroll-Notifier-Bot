﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharedLib
{
	public interface IStorageManagerBase<TModel>
		where TModel : notnull
	{
		public bool Set(TModel model);

		public bool Delete(TModel model);

		public IEnumerable<TModel> GetAll();

		public ValueTask FlushAsync();
	}
}
