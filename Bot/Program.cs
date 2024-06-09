ILogger<Program> logger;

using (ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Critical)))
{
	logger = loggerFactory.CreateLogger<Program>();
}