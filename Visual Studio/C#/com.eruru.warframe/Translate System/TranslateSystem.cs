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
		static readonly ReaderWriterLockHelper ReaderWriterLockHelper = new ReaderWriterLockHelper ();
		static readonly Dictionary<string, Translate> Translates = new Dictionary<string, Translate> ();
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
						Config.Read (config => {
							json = Http.Request (config.TranslateUrl, HttpRequestInformation);
						});
					});
				}
				await Task.Run (() => {
					Config.Read (config => {
						JsonValue content = JsonObject.Parse (json)["query"]["pages"][0]["revisions"][0]["content"];
						JsonObject jsonObject = JsonObject.Parse (content)["Text"];
						int total = jsonObject.Count + config.Translates.Count;
						ReaderWriterLockHelper.Write (() => {
							Translates.Clear ();
							int i = 0;
							foreach (JsonKey key in jsonObject) {
								Translates.Add (key.Name, new Translate (key.Name, key));
								i++;
							}
							foreach (var translate in config.Translates) {
								Translates.Add (translate.Key, new Translate (translate.Key, translate.Value));
								i++;
							}
						});
					});
				});
				if (cache) {
					TimerInterval = 1;
				} else {
					File.WriteAllText (Paths.TranslateCacheFile, json);
					Config.Read (config => {
						TimerInterval = config.TranslateUpdateInterval;
					});
				}
				QMHelperApi.Debug ($"中英文对照更新完毕，将在{DateTime.Now.AddMilliseconds (TimerInterval)}后再次更新");
			} catch (Exception exception) {
				if (cache) {
					TimerInterval = 1;
				} else {
					Config.Read (config => {
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
			if (Translates.TryGetValue (text, out Translate translate)) {
				return translate.Value;
			}
			StringBuilder stringBuilder = new StringBuilder (text);
			string[] tokens = text.Split (' ');
			for (int i = tokens.Length - 1; i > 0; i--) {
				for (int n = 0; n + i <= tokens.Length; n++) {
					string value = string.Join (" ", tokens, n, i);
					if (Translates.TryGetValue (value, out translate)) {
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
			ReaderWriterLockHelper.Read (() => {
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
			});
			return stringBuilder.ToString ();
		}

		public static Translate Get (string text, TranslateSearchType searchType = TranslateSearchType.All) {
			if (text is null) {
				throw new ArgumentNullException (nameof (text));
			}
			Translate translate = null;
			ReaderWriterLockHelper.Read (() => {
				foreach (var current in Translates) {
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
			List<Translate> translates = new List<Translate> ();
			ReaderWriterLockHelper.Read (() => {
				foreach (var translate in Translates) {
					if (searchType.HasFlag (TranslateSearchType.KeyOnly)) {
						if (Api.ContainKeyword (translate.Key, keyword, out int index)) {
							translates.Add (new Translate (translate.Key, translate.Value.Value, index, translate.Key));
							continue;
						}
					}
					if (searchType.HasFlag (TranslateSearchType.ValueOnly)) {
						if (Api.ContainKeyword (translate.Value.Value, keyword, out int index)) {
							translates.Add (new Translate (translate.Key, translate.Value.Value, index, translate.Value.Value));
						}
					}
				}
			});
			return translates;
		}

		public static void Add (string key, string value) {
			if (key is null) {
				throw new ArgumentNullException (nameof (key));
			}
			if (value is null) {
				throw new ArgumentNullException (nameof (value));
			}
			ReaderWriterLockHelper.Write (() => {
				if (!Translates.TryGetValue (key, out Translate translate)) {
					translate = new Translate {
						Key = key
					};
					Translates.Add (key, translate);
				}
				translate.Value = value;
			});
		}

		public static bool Remove (string key) {
			if (key is null) {
				throw new ArgumentNullException (nameof (key));
			}
			bool found = false;
			ReaderWriterLockHelper.Write (() => {
				found = Translates.Remove (key);
			});
			return found;
		}

	}

}