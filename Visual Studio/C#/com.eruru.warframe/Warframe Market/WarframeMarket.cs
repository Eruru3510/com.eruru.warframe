using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Eruru.Html;
using Eruru.Http;
using Eruru.Json;
using Eruru.QQMini.PluginSDKHelper;
using Eruru.ReaderWriterLockHelper;

namespace com.eruru.warframe {

	public class WarframeMarket {

		public static double TimerInterval {

			get => Timer.Interval;

			set {
				Timer.Interval = value;
				Timer.Enabled = true;
			}

		}

		public WarframeMarketItem[] Items;

		static readonly System.Timers.Timer Timer = new System.Timers.Timer () {
			AutoReset = false
		};
		static readonly ReaderWriterLockHelper ReaderWriterLockHelper = new ReaderWriterLockHelper ();
		static readonly Http Http = new Http ();
		static readonly HttpRequestInformation HttpRequestInformation = new HttpRequestInformation ();

		static WarframeMarket Instance = new WarframeMarket ();

		public static async Task Start () {
			Timer.Elapsed += async (sender, e) => {
				await Update ();
			};
			await Update (true);
		}

		public static async Task Update (bool cache = false) {
			try {
				QMHelperApi.Debug ($"开始更新Warframe Market{(cache ? "（来自缓存）" : null)}");
				string html = null;
				if (cache) {
					if (File.Exists (Paths.WarframeMarketCacheFile)) {
						html = File.ReadAllText (Paths.WarframeMarketCacheFile);
					}
				} else {
					await Task.Run (() => {
						Config.Read (config => {
							html = Http.Request (config.WarframeMarketUrl, HttpRequestInformation);
						});
					});
				}
				HtmlDocument htmlDocument = HtmlDocument.Parse (html);
				string json = GetJson (htmlDocument);
				await Task.Run (() => {
					ReaderWriterLockHelper.Write (() => {
						Instance = JsonConvert.Deserialize (json, Instance);
					});
				});
				if (cache) {
					TimerInterval = 1;
				} else {
					File.WriteAllText (Paths.WarframeMarketCacheFile, html);
					Config.Read (config => {
						TimerInterval = config.WarframeMarketUpdateInterval;
					});
				}
				QMHelperApi.Debug ($"Warframe Market更新完毕，将在{DateTime.Now.AddMilliseconds (TimerInterval)}后再次更新");
			} catch (Exception exception) {
				if (cache) {
					TimerInterval = 1;
				} else {
					Config.Read (config => {
						TimerInterval = config.WarframeMarketUpdateRetryInterval;
					});
				}
				QMHelperApi.Debug ($"Warframe Market更新失败，将在{DateTime.Now.AddMilliseconds (TimerInterval)}后再次更新{Environment.NewLine}{exception}");
			}
		}

		public static List<WarframeMarketItem> Search (string keyword) {
			if (keyword is null) {
				throw new ArgumentNullException (nameof (keyword));
			}
			List<WarframeMarketItem> results = new List<WarframeMarketItem> ();
			ReaderWriterLockHelper.Read (() => {
				Search (Instance.Items);
			});
			return results;
			void Search (IEnumerable<WarframeMarketItem> items) {
				foreach (WarframeMarketItem item in items) {
					if (Api.ContainKeyword (item.Name, keyword, out int index)) {
						results.Add (new WarframeMarketItem (item.UrlName, item.ItemName, item.Name, index, item.Name));
						continue;
					}
					if (Api.ContainKeyword (item.ItemName, keyword, out index)) {
						results.Add (new WarframeMarketItem (item.UrlName, item.ItemName, item.Name, index, item.ItemName));
					}
				}
			}
		}

		public static string GetJson (HtmlDocument htmlDocument) {
			if (htmlDocument is null) {
				throw new ArgumentNullException (nameof (htmlDocument));
			}
			return htmlDocument.GetElementById ("application-state").TextContent;
		}

	}

}