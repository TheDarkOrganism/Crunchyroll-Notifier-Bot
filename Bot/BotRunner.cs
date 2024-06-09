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

		private readonly ILogger<BotRunner> _logger;
		private readonly TimeSpan _interval;

		public BotRunner(Config config)
		{
			LogLevel logLevel = config.LogLevel;

			using (ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(logLevel)))
			{
				_logger = loggerFactory.CreateLogger<BotRunner>();
			}

			_interval = config.Interval;
		}
	}
}
