using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Eruru.Http;
using Eruru.Json;
using Eruru.QQMini.PluginSDKHelper;
using Eruru.ReaderWriterLockHelper;

namespace com.eruru.warframe {

	public static class TranslateSystem {

		public static double TimerInterval {

			get => Timer.Interval;

			set {
				Timer.Interval = value;
				Timer.Enabled = true;
			}

		}

		static readonly System.Timers.Timer Timer = new System.Timers.Timer () {
			AutoReset = false
		};
		static readonly ReaderWriterLockHelper<Dictionary<string, Translate>> ReaderWriterLockHelper =
			new ReaderWriterLockHelper<Dictionary<string, Translate>> (new Dictionary<string, Translate> ());
		static readonly Http Http = new Http ();
		static readonly HttpRequestInformation HttpRequestInformation = new HttpRequestInformation ();

		public static async Task Start () {
			Timer.Elapsed += async (sender, e) => {
				await Update ();
			};
			await Update (true);
		}

		public static async Task Update (bool cache = false) {
			try {
				QMHelperApi.Debug ($"开始更新中英文对照{(cache ? "（来自缓存）" : null)}");
				string json = null;
				if (cache) {
					if (File.Exists (Paths.TranslateCacheFile)) {
						json = File.ReadAllText (Paths.TranslateCacheFile);
					}
				} else {
					await Task.Run (() => {
						string url = null;
						Config.Read ((ref Config config) => {
							url = config.TranslateUrl;
						});
						json = Http.Request (url, HttpRequestInformation);
					});
				}
				await Task.Run (() => {
					Config.Read ((ref Config config) => {
						JsonValue content = JsonObject.Parse (json)["query"]["pages"][0]["revisions"][0]["content"];
						JsonObject jsonObject = JsonObject.Parse (content)["Text"];
						int total = jsonObject.Count + config.Translates.Count;
						Config tempConfig = config;
						ReaderWriterLockHelper.Write ((ref Dictionary<string, Translate> translates) => {
							translates.Clear ();
							foreach (JsonKey key in jsonObject) {
								translates.Add (key.Name, new Translate (key.Name, key));
							}
							foreach (var translate in tempConfig.Translates) {
								translates.Add (translate.Key, new Translate (translate.Key, translate.Value));
							}
						});
					});
				});
				if (cache) {
					TimerInterval = 1;
				} else {
					File.WriteAllText (Paths.TranslateCacheFile, json);
					Config.Read ((ref Config config) => {
						TimerInterval = config.TranslateUpdateInterval;
					});
				}
				QMHelperApi.Debug ($"中英文对照更新完毕，将在{DateTime.Now.AddMilliseconds (TimerInterval)}后再次更新");
			} catch (Exception exception) {
				if (cache) {
					TimerInterval = 1;
				} else {
					Config.Read ((ref Config config) => {
						TimerInterval = config.TranslateUpdateRetryInterval;
					});
				}
				QMHelperApi.Debug ($"中英文对照更新失败，将在{DateTime.Now.AddMilliseconds (TimerInterval)}后再次更新{Environment.NewLine}{exception}");
			}
		}

		public static string TranslateItem (string text) {
			if (text is null) {
				throw new ArgumentNullException (nameof (text));
			}
			Translate translate = Get (text, TranslateSearchType.KeyOnly);
			if (translate != null) {
				return translate.Value;
			}
			StringBuilder stringBuilder = new StringBuilder (text);
			string[] tokens = text.Split (' ');
			for (int i = tokens.Length - 1; i > 0; i--) {
				for (int n = 0; n + i <= tokens.Length; n++) {
					string value = string.Join (" ", tokens, n, i);
					translate = Get (value, TranslateSearchType.KeyOnly);
					if (translate != null) {
						stringBuilder.Replace (value, translate.Value);
					}
				}
			}
			return stringBuilder.ToString ();
		}

		public static string TranslateNode (string text) {
			if (text is null) {
				throw new ArgumentNullException (nameof (text));
			}
			StringBuilder stringBuilder = new StringBuilder ();
			Match match = Regex.Match (text, @"(.*?) ?($|\((.*?)\))");
			switch (match.Groups[1].Value) {
				case "Plains of Eidolon":
					stringBuilder.Append ("夜灵平野");
					break;
				case "Orb Vallis":
					stringBuilder.Append ("奥布山谷");
					break;
				default:
					stringBuilder.Append (match.Groups[1].Value);
					break;
			}
			if (match.Groups[3].Length > 0) {
				stringBuilder.Append ($" ({TranslateItem (match.Groups[3].Value)})");
			}
			return stringBuilder.ToString ();
		}

		public static Translate Get (string text, TranslateSearchType searchType = TranslateSearchType.All) {
			if (text is null) {
				throw new ArgumentNullException (nameof (text));
			}
			Translate translate = null;
			ReaderWriterLockHelper.Read ((ref Dictionary<string, Translate> translates) => {
				foreach (var current in translates) {
					if (searchType.HasFlag (TranslateSearchType.KeyOnly)) {
						if (Api.Equals (current.Key, text)) {
							translate = current.Value;
							break;
						}
					}
					if (searchType.HasFlag (TranslateSearchType.ValueOnly)) {
						if (Api.Equals (current.Value.Value, text)) {
							translate = current.Value;
							break;
						}
					}
				}
			});
			return translate;
		}

		public static List<Translate> Search (string keyword, TranslateSearchType searchType = TranslateSearchType.All) {
			if (keyword is null) {
				throw new ArgumentNullException (nameof (keyword));
			}
			List<Translate> results = new List<Translate> ();
			ReaderWriterLockHelper.Read ((ref Dictionary<string, Translate> translates) => {
				foreach (var translate in translates) {
					if (searchType.HasFlag (TranslateSearchType.KeyOnly)) {
						if (Api.ContainKeyword (translate.Key, keyword, out int index)) {
							Translate temp = translate.Value.Clone ();
							temp.Index = index;
							temp.Text = translate.Key;
							results.Add (temp);
							continue;
						}
					}
					if (searchType.HasFlag (TranslateSearchType.ValueOnly)) {
						if (Api.ContainKeyword (translate.Value.Value, keyword, out int index)) {
							Translate temp = translate.Value.Clone ();
							temp.Index = index;
							temp.Text = translate.Value.Value;
							results.Add (temp);
						}
					}
				}
			});
			return results;
		}

		public static void Add (string key, string value) {
			if (key is null) {
				throw new ArgumentNullException (nameof (key));
			}
			if (value is null) {
				throw new ArgumentNullException (nameof (value));
			}
			ReaderWriterLockHelper.Write ((ref Dictionary<string, Translate> translates) => {
				if (!translates.TryGetValue (key, out Translate translate)) {
					translate = new Translate {
						Key = key
					};
					translates.Add (key, translate);
				}
				translate.Value = value;
			});
		}

		public static bool Remove (string key) {
			if (key is null) {
				throw new ArgumentNullException (nameof (key));
			}
			bool found = false;
			ReaderWriterLockHelper.Write ((ref Dictionary<string, Translate> translates) => {
				found = translates.Remove (key);
			});
			return found;
		}

	}

}