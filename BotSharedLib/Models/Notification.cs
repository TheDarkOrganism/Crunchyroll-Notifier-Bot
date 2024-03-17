namespace BotSharedLib.Models
{
	internal sealed record Notification(string ShowText, string Url, int? Season, int Episode, string Thumbnail, string Description, DateTime ReleaseDate) { }
}
