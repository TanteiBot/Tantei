using System.IO;
using System.Text;
using Newtonsoft.Json;
using PaperMalKing.Data;

namespace PaperMalKing
{
	public sealed class Program
	{
		static void Main(string[] args)
		{
			var json = "";
			using (var fs = File.OpenRead("config.json"))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				json = sr.ReadToEnd();

			var botConfig = JsonConvert.DeserializeObject<BotConfig>(json);
			var bot = new PaperMalKingBot(botConfig);
			bot.Start().GetAwaiter().GetResult();
		}
	}
}