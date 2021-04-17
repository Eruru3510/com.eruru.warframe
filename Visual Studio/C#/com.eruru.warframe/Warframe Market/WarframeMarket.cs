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

		public List<WarframeMarketItem> Items = new List<WarframeMarketItem> ();

		static readonly System.Timers.Timer Timer = new System.Timers.Timer () {
			AutoReset = false
		};
		static readonly ReaderWriterLockHelper<WarframeMarket> ReaderWriterLockHelper = new ReaderWriterLockHelper<WarframeMarket> (new WarframeMarket ());
		static readonly Http Http = new Http ();
		static readonly HttpRequestInformation HttpRequestInformation = new HttpRequestInformation ();

		public static async Task Start () {
			Timer.Elapsed += async (sender, e) => {
				await Update (UpdateType.Network);
			};
			await Update (UpdateType.First);
		}

		public static async Task Update (UpdateType updateType) {
			try {
				string type;
				switch (updateType) {
					case UpdateType.First:
						type = "（首次）";
						break;
					case UpdateType.Network:
						type = string.Empty;
						break;
					case UpdateType.Cache:
						type = "（缓存）";
						break;
					default:
						throw new NotImplementedException ();
				}
				QMHelperApi.Debug ($"开始更新Warframe Market{type}");
				string html = null;
				switch (updateType) {
					case UpdateType.First:
					case UpdateType.Cache:
						if (File.Exists (Paths.WarframeMarketCacheFile)) {
							html = File.ReadAllText (Paths.WarframeMarketCacheFile);
						}
						break;
					case UpdateType.Network:
						await Task.Run (() => {
							string url = null;
							Config.Read ((ref Config config) => {
								url = config.WarframeMarketUrl;
							});
							html = Http.Request (url, HttpRequestInformation);
						});
						break;
				}
				HtmlDocument htmlDocument = HtmlDocument.Parse (html);
				string json = GetJson (htmlDocument);
				await Task.Run (() => {
					ReaderWriterLockHelper.Write ((ref WarframeMarket instance) => {
						instance = JsonConvert.Deserialize (json, instance);
						Config.Read ((ref Config config) => {
							foreach (KeyValuePair<string, string> translate in config.WarframeMarketTranslates) {
								Add (translate.Key, translate.Value);
							}
						});
					});
				});
				switch (updateType) {
					case UpdateType.First:
						TimerInterval = 1;
						break;
					case UpdateType.Cache:
						break;
					case UpdateType.Network:
						File.WriteAllText (Paths.WarframeMarketCacheFile, html);
						Config.Read ((ref Config config) => {
							TimerInterval = config.WarframeMarketUpdateInterval;
						});
						break;
				}
				QMHelperApi.Debug ($"Warframe Market更新完毕，将在{DateTime.Now.AddMilliseconds (TimerInterval)}后再次更新");
			} catch (Exception exception) {
				switch (updateType) {
					case UpdateType.First:
						TimerInterval = 1;
						break;
					case UpdateType.Cache:
						break;
					case UpdateType.Network:
						Config.Read ((ref Config config) => {
							TimerInterval = config.WarframeMarketUpdateRetryInterval;
						});
						break;
				}
				QMHelperApi.Debug ($"Warframe Market更新失败，将在{DateTime.Now.AddMilliseconds (TimerInterval)}后再次更新{Environment.NewLine}{exception}");
			}
		}

		public static List<WarframeMarketItem> Search (string keyword) {
			if (keyword is null) {
				throw new ArgumentNullException (nameof (keyword));
			}
			List<WarframeMarketItem> results = new List<WarframeMarketItem> ();
			ReaderWriterLockHelper.Read ((ref WarframeMarket instance) => {
				Search (instance.Items);
			});
			return results;
			void Search (IEnumerable<WarframeMarketItem> items) {
				foreach (WarframeMarketItem item in items) {
					if (Api.ContainKeyword (item.Name, keyword, out int index, out bool fullMatch)) {
						WarframeMarketItem newItem = item.Clone ();
						newItem.Index = index;
						newItem.Text = item.Name;
						if (fullMatch) {
							results.Clear ();
							results.Add (newItem);
							return;
						}
						results.Add (newItem);
						continue;
					}
					if (Api.ContainKeyword (item.ItemName, keyword, out index, out fullMatch)) {
						WarframeMarketItem newItem = item.Clone ();
						newItem.Index = index;
						newItem.Text = item.ItemName;
						if (fullMatch) {
							results.Clear ();
							results.Add (newItem);
							return;
						}
						results.Add (newItem);
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

		public static void Add (string key, string value) {
			if (key is null) {
				throw new ArgumentNullException (nameof (key));
			}
			if (value is null) {
				throw new ArgumentNullException (nameof (value));
			}
			WarframeMarketItem item = Get (value)?.Clone ();
			if (item != null) {
				item.Name = key;
				ReaderWriterLockHelper.Write ((ref WarframeMarket instance) => {
					instance.Items.Add (item);
				});
			}
		}

		public static bool Remove (string key) {
			if (key is null) {
				throw new ArgumentNullException (nameof (key));
			}
			bool found = false;
			ReaderWriterLockHelper.Read ((ref WarframeMarket instance) => {
				if (Get (key, out int index) == null) {
					return;
				}
				ReaderWriterLockHelper.Write ((ref WarframeMarket subInstance) => {
					subInstance.Items.RemoveAt (index);
				});
			});
			return found;
		}

		static WarframeMarketItem Get (string name, out int index) {
			if (name is null) {
				throw new ArgumentNullException (nameof (name));
			}
			WarframeMarketItem resultItem = null;
			int result = -1;
			ReaderWriterLockHelper.Read ((ref WarframeMarket instance) => {
				for (int i = 0; i < instance.Items.Count; i++) {
					if (instance.Items[i].ItemName == name) {
						result = i;
						resultItem = instance.Items[i];
						break;
					}
				}
			});
			index = result;
			return resultItem;
		}
		static WarframeMarketItem Get (string name) {
			if (name is null) {
				throw new ArgumentNullException (nameof (name));
			}
			return Get (name, out _);
		}

	}

}