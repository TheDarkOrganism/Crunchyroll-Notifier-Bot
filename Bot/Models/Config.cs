namespace Bot.Models
{
	internal sealed record Config(TimeSpan Interval, LogLevel LogLevel, string Token) { }
}
