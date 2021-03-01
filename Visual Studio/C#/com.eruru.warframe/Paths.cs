using System;
using System.IO;

namespace com.eruru.warframe {

	public static class Paths {

		public static string DataDirectory { get; private set; }
		public static string ConfigFile { get; private set; }
		public static string CachesDirectory { get; private set; }
		public static string WarframeStatusCacheFile { get; private set; }
		public static string TranslateCacheFile { get; private set; }
		public static string WarframeMarketCacheFile { get; private set; }

		public static void Initialize (string dataDirectory) {
			if (dataDirectory is null) {
				throw new ArgumentNullException (nameof (dataDirectory));
			}
			DataDirectory = dataDirectory;
			ConfigFile = $@"{DataDirectory}Config.json";
			CachesDirectory = $@"{DataDirectory}Caches\";
			Directory.CreateDirectory (CachesDirectory);
			WarframeStatusCacheFile = $@"{CachesDirectory}Warframe Status.json";
			TranslateCacheFile = $@"{CachesDirectory}Warframe Huiji Userdict.json";
			WarframeMarketCacheFile = $@"{CachesDirectory}Warframe Market.html";
		}

	}

}