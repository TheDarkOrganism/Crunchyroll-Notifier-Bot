using Bot;
using Bot.Converters;

const string _configPath = "./Config.json";
TimeSpan _errorExitDelay = TimeSpan.FromSeconds(30);
const int _minInterval = 10;

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

Config? config;

try
{
	config = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(_configPath), _jsonSerializerOptions);

	if (config is null)
	{
		logger.LogCritical("Unable to load \"{Config}\"", _configPath);

		await Task.Delay(_errorExitDelay);

		Environment.Exit(13);
	}

	if (string.IsNullOrWhiteSpace(config.Token))
	{
		logger.LogCritical("Missing Token");

		await Task.Delay(_errorExitDelay);

		Environment.Exit(13);
	}

	if (config.Interval.TotalSeconds < _minInterval)
	{
		logger.LogCritical("The interval must be equal or greater than {MinValue} seconds", _minInterval);

		await Task.Delay(_errorExitDelay);

		Environment.Exit(13);
	}
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
catch (Exception ex)
{
	logger.LogCritical(ex, "Unable to read token from \"{Config}\"", _configPath);

	await Task.Delay(_errorExitDelay);

	Environment.Exit(13);

	return;
}

using BotRunner botRunner = new(config);

await botRunner.RunAsync();

await Task.Delay(-1);