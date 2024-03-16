using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Bot.Sources
{
	internal sealed partial class RSSSource : SourceBase
	{
		private static XmlNamespaceManager? _manager;

		public RSSSource(DiscordClient client, Data data, Channels channels) : base(client, data, channels) { }

		public override async ValueTask<bool> RunAsync()
		{
			XPathDocument document = new("http://feeds.feedburner.com/crunchyroll/rss/anime");

			XPathNavigator navigator = document.CreateNavigator();

			if (_manager is null)
			{
				_manager = new(navigator.NameTable);

				_manager.AddNamespace("media", "http://search.yahoo.com/mrss/");
				_manager.AddNamespace("crunchyroll", "http://www.crunchyroll.com/rss");
			}

			DateTime last = await GetLast();

			DateTime copy = last;

			foreach (XPathNavigator nav in navigator.Select($"//item").OfType<XPathNavigator>().Reverse())
			{
				if (DateTime.TryParse(nav.SelectSingleNode(".//pubDate", _manager)?.Value, out DateTime result) && result > last)
				{
					string? dub = nav.SelectSingleNode(".//title", _manager)?.Value is string title ? DubRegex().Match(title).Groups.Values.ElementAtOrDefault(1)?.Value : null;

					if (nav.SelectSingleNode(".//link")?.Value is string url && nav.SelectSingleNode(".//crunchyroll:seriesTitle", _manager)?.Value is string seriesTitle)
					{
						string showText = $"{seriesTitle} {(string.IsNullOrWhiteSpace(dub) ? string.Empty : $"({dub})")}";

						int? episode = nav.SelectSingleNode(".//crunchyroll:episodeNumber", _manager)?.ValueAsInt;

						int? season = nav.SelectSingleNode(".//crunchyroll:season", _manager)?.ValueAsInt;

						string? thumbnail = nav.SelectSingleNode(".//enclosure")?.GetAttribute("url", string.Empty)?.Replace("_thumb", "_full");

						string description = nav.SelectSingleNode(".//description")?.Value ?? string.Empty;

						description = description[(description.LastIndexOf('>') + 1)..];

						await SendNotificationsAsync(new(showText, url, season, episode.GetValueOrDefault(), thumbnail ?? "", description, result));
					}

					last = result;
				}
			}

			if (copy != last)
			{
				await _data.SetLast(last);

				return true;
			}
			else
			{
				return false;
			}
		}

		[GeneratedRegex("\\(([A-Za-z\\-]+) Dub\\)")]
		private static partial Regex DubRegex();
	}
}
