using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.Common.RateLimiter;
using PaperMalKing.MyAnimeList.Wrapper.Models.List;
using PaperMalKing.MyAnimeList.Wrapper.Models.List.Types;
using PaperMalKing.MyAnimeList.Wrapper.Models.Rss.Types;

using Xunit;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests
{
	public class UnitTest1
	{
		[Fact]
		public async Task Test1()
		{
			var cl = new HttpClient();
			cl.DefaultRequestHeaders.Add("user-agent",
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
			cl.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
			cl.DefaultRequestHeaders.Add("accept-language", "uk-UA,uk;q=0.9");
			cl.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
			cl.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
			cl.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
			cl.DefaultRequestHeaders.Add("sec-fetch-site", "none");
			var mc = new MyAnimeListClient(NullRateLimiter<MyAnimeListClient>.Instance, NullLogger<MyAnimeListClient>.Instance,
				cl);
			var u = await mc.GetUserAsync("0EedgYy");
			Console.WriteLine(u);
		}

		[Fact]
		public async Task Test2()
		{
			var cl = new HttpClient();
			cl.DefaultRequestHeaders.Add("user-agent",
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
			cl.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
			cl.DefaultRequestHeaders.Add("accept-language", "uk-UA,uk;q=0.9");
			cl.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
			cl.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
			cl.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
			cl.DefaultRequestHeaders.Add("sec-fetch-site", "none");
			var mc = new MyAnimeListClient(NullRateLimiter<MyAnimeListClient>.Instance, NullLogger<MyAnimeListClient>.Instance,
				cl);
			var a = await mc.GetLatestListUpdatesAsync<AnimeListEntry, AnimeListType>("0EedgYy");
			var m = await mc.GetLatestListUpdatesAsync<MangaListEntry, MangaListType>("0EedgYy");
			Console.WriteLine();
		}

		[Fact]
		public async Task Test3()
		{
			var cl = new HttpClient();
			cl.DefaultRequestHeaders.Add("user-agent",
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
			cl.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
			cl.DefaultRequestHeaders.Add("accept-language", "uk-UA,uk;q=0.9");
			cl.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
			cl.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
			cl.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
			cl.DefaultRequestHeaders.Add("sec-fetch-site", "none");
			var mc = new MyAnimeListClient(NullRateLimiter<MyAnimeListClient>.Instance, NullLogger<MyAnimeListClient>.Instance,
				cl);
			var a = await mc.GetRecentRssUpdatesAsync<AnimeRssFeed>("0EedgYy");
			var m = await mc.GetRecentRssUpdatesAsync<MangaRssFeed>("0EedgYy");
			var f = a.First();
			var ff = m.First();
			Console.WriteLine();
		}
	}
}