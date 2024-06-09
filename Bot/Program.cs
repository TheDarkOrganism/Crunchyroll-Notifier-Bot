using Bot;
using Bot.Converters;

const string _configPath = "./Config.json";
TimeSpan _errorExitDelay = TimeSpan.FromSeconds(30);

JsonSerializerOptions _jsonSerializerOptions = new()
{
	Converters = {
		new EnumConverter(),
		new TimeSpanConverter()
	}
};

ILogger<Program> logger;

using (ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Critical)))
{
	logger = loggerFactory.CreateLogger<Program>();
}

Config config;

try
{
	Config? c = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(_configPath), _jsonSerializerOptions);

	if (c is null || string.IsNullOrWhiteSpace(c.Token))
	{
		logger.LogCritical("Missing Token");

		await Task.Delay(_errorExitDelay);

		Environment.Exit(0);
	}

	config = c;
}
catch (FileNotFoundException ex)
{
	logger.LogCritical(ex, "Unable to find the config file at \"{Config}\"", _configPath);

	await Task.Delay(_errorExitDelay);

	Environment.Exit(2);

	return;
}
catch (JsonException ex)
{
	logger.LogCritical(ex, "The config file at \"{Config}\" was invalid", _configPath);

	await Task.Delay(_errorExitDelay);

	Environment.Exit(13);

	return;
}
catch
{
	logger.LogCritical("Unable to read token from \"{Config}\"", _configPath);

	await Task.Delay(_errorExitDelay);

	Environment.Exit(13);

	return;
}

BotRunner botRunner = new(config);