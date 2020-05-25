using System.IO;
using System.Text;
using YamlDotNet;
using PaperMalKing.Data;
using YamlDotNet.Serialization;

namespace PaperMalKing
{
	public sealed class Program
	{
		static void Main(string[] args)
		{
			var yaml = "";
			using (var fs = File.OpenRead("config.yml"))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				yaml = sr.ReadToEnd();
			var deserializer = new Deserializer();
			var botConfig = deserializer.Deserialize<BotConfig>(yaml);
			var bot = new PaperMalKingBot(botConfig);
			bot.Start().GetAwaiter().GetResult();
		}
	}
}