using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Eruru.Http;
using Eruru.Json;
using Eruru.Localizer;
using Eruru.QQMini.PluginSDKHelper;
using Eruru.ReaderWriterLockHelper;
using Microsoft.VisualBasic;
using Timer = Eruru.Timer.Timer;

namespace com.eruru.warframe {

	public class WarframeStatus {

		[JsonField (typeof (NewsReverser))]
		public WarframeStatusNews[] News;
		public WarframeStatusEvent[] Events;
		public WarframeStatusAlert[] Alerts;
		public WarframeStatusSortie Sortie;
		public WarframeStatusSyndicateMission[] SyndicateMissions {

			get => _SyndicateMissions;

			set {
				_SyndicateMissions = value;
				OstronsBounty = Get ("Ostrons");
				EntratiBounty = Get ("Entrati");
				SolarisUnitedBounty = Get ("索拉里斯联盟");
				WarframeStatusSyndicateMission Get (string syndicate) {
					foreach (WarframeStatusSyndicateMission syndicateMission in value) {
						if (syndicateMission.Syndicate == syndicate) {
							return syndicateMission;
						}
					}
					return null;
				}
			}

		}
		[JsonField (typeof (FissureSorter))]
		public WarframeStatusFissure[] Fissures;
		public WarframeStatusInvasion[] Invasions;
		public WarframeStatusVoidTrader VoidTrader;
		public WarframeStatusArbitration Arbitration;
		public WarframeStatusCycle EarthCycle;
		public WarframeStatusCycle CetusCycle;
		public WarframeStatusCycle CambionCycle;
		public WarframeStatusCycle VallisCycle;
		public WarframeStatusDailyDeal[] DailyDeals;
		public WarframeStatusNightwave Nightwave;
		[JsonIgnoreField]
		public WarframeStatusSyndicateMission OstronsBounty;
		[JsonIgnoreField]
		public WarframeStatusSyndicateMission EntratiBounty;
		[JsonIgnoreField]
		public WarframeStatusSyndicateMission SolarisUnitedBounty;

		static readonly Timer Timer = new Timer ();
		static readonly ReaderWriterLockHelper<WarframeStatus> ReaderWriterLockHelper = new ReaderWriterLockHelper<WarframeStatus> (new WarframeStatus ());
		static readonly Http Http = new Http ();
		static readonly HttpRequestInformation HttpRequestInformation = new HttpRequestInformation () {
			Request = httpWebRequest => httpWebRequest.Headers.Set (HttpRequestHeader.AcceptLanguage, "zh-cn")
		};

		WarframeStatusSyndicateMission[] _SyndicateMissions;

		public static async Task Start () {
			Timer.Elapsed += async (sender, e) => {
				await Update ();
			};
			await Update (true);
		}

		public static async Task Update (bool cache = false) {
			try {
				QMHelperApi.Debug ($"开始更新游戏状态{(cache ? "（来自缓存）" : null)}");
				Timer.Clear ();
				string json = null;
				if (cache) {
					if (File.Exists (Paths.WarframeStatusCacheFile)) {
						json = File.ReadAllText (Paths.WarframeStatusCacheFile);
					}
				} else {
					await Task.Run (() => {
						string url = null;
						Config.Read ((ref Config config) => {
							url = config.WarframeStatusUrl;
						});
						json = Http.Request (url, HttpRequestInformation);
					});
				}
				json = Strings.StrConv (json, VbStrConv.SimplifiedChinese);
				ReaderWriterLockHelper.Write ((ref WarframeStatus instance) => {
					instance = JsonConvert.Deserialize (json, instance);
				});
				if (cache) {
					Timer.Add (DateTime.Now.AddMilliseconds (1));
				} else {
					File.WriteAllText (Paths.WarframeStatusCacheFile, json);
					Config.Read ((ref Config config) => {
						Timer.Add (DateTime.Now.AddMilliseconds (config.WarframeStatusUpdateInterval));
					});
				}
				QMHelperApi.Debug ($"游戏状态更新完毕，将在{Timer.Next}后再次更新");
			} catch (Exception exception) {
				if (cache) {
					Timer.Add (DateTime.Now.AddMilliseconds (1));
				} else {
					Config.Read ((ref Config config) => {
						Timer.Add (DateTime.Now.AddMilliseconds (config.WarframeStatusUpdateRetryInterval));
					});
				}
				QMHelperApi.Debug ($"游戏状态更新失败，将在{Timer.Next}后再次更新{Environment.NewLine}{exception}");
			}
		}

		public static void Add (DateTime dateTime) {
			Timer.Add (dateTime);
		}

		public static StringBuilder GetNewsInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					stringBuilder = GetInformation (
						stringBuilder,
						tempConfig.Commands.News[CommandIndexName.Default].Header, null,
						tempConfig.Commands.News[CommandIndexName.Default].Item, instance.News, null, item => new object[] { item },
						tempConfig.Commands.News[CommandIndexName.Default].Footer, null
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetEventInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					if ((instance.Events?.Length ?? 0) == 0) {
						stringBuilder = Localizer.Execute (stringBuilder, tempConfig.Commands.Event[CommandIndexName.NoResult].Header);
						return;
					}
					stringBuilder = GetInformation (
						stringBuilder,
						tempConfig.Commands.Event[CommandIndexName.Default].Header, null,
						tempConfig.Commands.Event[CommandIndexName.Default].Item, instance.Events, null, item => new object[] { item },
						tempConfig.Commands.Event[CommandIndexName.Default].Footer, null
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetAlertInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					if ((instance.Alerts?.Length ?? 0) == 0) {
						stringBuilder = Localizer.Execute (stringBuilder, tempConfig.Commands.Alert[CommandIndexName.NoResult].Header);
						return;
					}
					stringBuilder = GetInformation (
						stringBuilder,
						tempConfig.Commands.Alert[CommandIndexName.Default].Header, null,
						tempConfig.Commands.Alert[CommandIndexName.Default].Item, instance.Alerts, null, item => new object[] { item, item.Mission },
						tempConfig.Commands.Alert[CommandIndexName.Default].Footer, null
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetArbitrationInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					if (string.IsNullOrEmpty (instance.Arbitration.Node)) {
						stringBuilder = Localizer.Execute (stringBuilder, tempConfig.Commands.Arbitration[CommandIndexName.NoResult].Header);
						return;
					}
					stringBuilder = Localizer.Execute (stringBuilder, tempConfig.Commands.Arbitration[CommandIndexName.Default].Header, instance.Arbitration);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetSortieInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					stringBuilder = GetInformation (
						stringBuilder,
						tempConfig.Commands.Sortie[CommandIndexName.Default].Header, new object[] { instance.Sortie },
						tempConfig.Commands.Sortie[CommandIndexName.Default].Item, instance.Sortie.Variants, null, item => new object[] { item },
						tempConfig.Commands.Sortie[CommandIndexName.Default].Footer, new object[] { instance.Sortie }
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetOstronsBountyInformation (StringBuilder stringBuilder = null) {
			ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
				stringBuilder = GetBountyInformation (stringBuilder, instance.OstronsBounty, config => config.Commands.OstronsBounty);
			});
			return stringBuilder;
		}

		public static StringBuilder GetEntratiBountyInformation (StringBuilder stringBuilder = null) {
			ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
				stringBuilder = GetBountyInformation (stringBuilder, instance.EntratiBounty, config => config.Commands.EntratiBounty);
			});
			return stringBuilder;
		}

		public static StringBuilder GetSolarisUnitedBountyInformation (StringBuilder stringBuilder = null) {
			ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
				stringBuilder = GetBountyInformation (stringBuilder, instance.SolarisUnitedBounty, config => config.Commands.SolarisUnitedBounty);
			});
			return stringBuilder;
		}

		public static StringBuilder GetFissureInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					stringBuilder = GetInformation (
						stringBuilder,
						tempConfig.Commands.Fissure[CommandIndexName.Default].Header, null,
						tempConfig.Commands.Fissure[CommandIndexName.Default].Item, instance.Fissures, null, item => new object[] { item },
						tempConfig.Commands.Fissure[CommandIndexName.Default].Footer, null
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetInvasionInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					stringBuilder = GetInformation (
						stringBuilder,
						tempConfig.Commands.Invasion[CommandIndexName.Default].Header, null,
						tempConfig.Commands.Invasion[CommandIndexName.Default].Item, instance.Invasions, item => !item.Completed, item => new object[] { item },
						tempConfig.Commands.Invasion[CommandIndexName.Default].Footer, null
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetVoidTraderInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					if (instance.VoidTrader.Active) {
						stringBuilder = GetInformation (
							stringBuilder,
							tempConfig.Commands.VoidTrader[CommandIndexName.Default].Header, new object[] { instance.VoidTrader },
							tempConfig.Commands.VoidTrader[CommandIndexName.Default].Item, instance.VoidTrader.Inventory, null, item => new object[] { item },
							tempConfig.Commands.VoidTrader[CommandIndexName.Default].Footer, new object[] { instance.VoidTrader }
						);
						return;
					}
					stringBuilder = Localizer.Execute (stringBuilder, tempConfig.Commands.VoidTrader[CommandIndexName.NoResult].Header, instance.VoidTrader);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetEarthCycleInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					stringBuilder = Localizer.Execute (stringBuilder, tempConfig.Commands.EarthCycle[CommandIndexName.Default].Header, instance.EarthCycle);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetCetusCycleInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					stringBuilder = Localizer.Execute (stringBuilder, tempConfig.Commands.CetusCycle[CommandIndexName.Default].Header, instance.CetusCycle);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetCambionCycleInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					stringBuilder = Localizer.Execute (stringBuilder, tempConfig.Commands.CambionCycle[CommandIndexName.Default].Header, instance.CambionCycle);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetVallisCycleInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					stringBuilder = Localizer.Execute (stringBuilder, tempConfig.Commands.VallisCycle[CommandIndexName.Default].Header, instance.VallisCycle);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetDailyDealInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					stringBuilder = GetInformation (
						stringBuilder,
						tempConfig.Commands.DailyDeal[CommandIndexName.Default].Header, new object[] { instance.DailyDeals[0] },
						tempConfig.Commands.DailyDeal[CommandIndexName.Default].Item, instance.DailyDeals, null, item => new object[] { item },
						tempConfig.Commands.DailyDeal[CommandIndexName.Default].Footer, new object[] { instance.DailyDeals[0] }
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetNightwaveInformation (StringBuilder stringBuilder = null) {
			Config.Read ((ref Config config) => {
				Config tempConfig = config;
				ReaderWriterLockHelper.Read ((ref WarframeStatus instance) => {
					if (!instance.Nightwave.Active) {
						stringBuilder = Localizer.Execute (stringBuilder, tempConfig.Commands.Nightwave[CommandIndexName.NoResult].Header);
						return;
					}
					stringBuilder = GetInformation (
						stringBuilder,
						tempConfig.Commands.Nightwave[CommandIndexName.Default].Header, null,
						tempConfig.Commands.Nightwave[CommandIndexName.Default].Item, instance.Nightwave.ActiveChallenges, null, item => new object[] { item },
						tempConfig.Commands.Nightwave[CommandIndexName.Default].Footer, null
					);
				});
			});
			return stringBuilder;
		}

		internal static StringBuilder GetInformation<Item> (
			StringBuilder stringBuilder,
			string header, object[] headers = null,
			string item = null, IEnumerable<Item> items = null, Func<Item, bool> itemFilter = null, Func<Item, object[]> itemFunc = null,
			string footer = null, object[] footers = null
		) {
			if (header is null) {
				throw new ArgumentNullException (nameof (header));
			}
			if (stringBuilder is null) {
				stringBuilder = new StringBuilder ();
			}
			stringBuilder = Localizer.Execute (stringBuilder, header, headers);
			if (items != null) {
				foreach (var variant in items) {
					if (itemFilter?.Invoke (variant) ?? true) {
						stringBuilder.AppendLine ();
						stringBuilder = Localizer.Execute (stringBuilder, item, itemFunc?.Invoke (variant));
					}
				}
			}
			if (!string.IsNullOrEmpty (footer)) {
				stringBuilder.AppendLine ();
				stringBuilder = Localizer.Execute (stringBuilder, footer, footers);
			}
			return stringBuilder;
		}

		static StringBuilder GetBountyInformation (StringBuilder stringBuilder, WarframeStatusSyndicateMission syndicateMission, Func<Config, Dictionary<string, Config.Command>> func) {
			if (stringBuilder is null) {
				stringBuilder = new StringBuilder ();
			}
			if (syndicateMission is null) {
				throw new ArgumentNullException (nameof (syndicateMission));
			}
			if (func is null) {
				throw new ArgumentNullException (nameof (func));
			}
			Config.Read ((ref Config config) => {
				var commandSet = func (config);
				stringBuilder = GetInformation (
					stringBuilder,
					commandSet[CommandIndexName.Default].Header, new object[] { syndicateMission },
					commandSet[CommandIndexName.Default].Item, syndicateMission.Jobs, item => item.RewardPool.Length > 0, item => new object[] { item },
					commandSet[CommandIndexName.Default].Footer, new object[] { syndicateMission }
				);
			});
			return stringBuilder;
		}

	}

}