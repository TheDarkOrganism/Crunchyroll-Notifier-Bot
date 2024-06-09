using Bot.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
	internal sealed class BotRunner
	{
		private static readonly JsonSerializerOptions _options = new()
		{
			Converters = {
				new EnumConverter(),
				new TimeSpanConverter()
			}
		};
	}
}
