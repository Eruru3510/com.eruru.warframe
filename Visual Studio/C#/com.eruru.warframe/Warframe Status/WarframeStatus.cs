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
		static readonly ReaderWriterLockHelper ReaderWriterLockHelper = new ReaderWriterLockHelper ();
		static readonly Http Http = new Http ();
		static readonly HttpRequestInformation HttpRequestInformation = new HttpRequestInformation () {
			Request = httpWebRequest => httpWebRequest.Headers.Set (HttpRequestHeader.AcceptLanguage, "zh-cn")
		};

		static WarframeStatus Instance = new WarframeStatus ();

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
						Config.Read (config => {
							json = Http.Request (config.WarframeStatusUrl, HttpRequestInformation);
						});
					});
				}
				json = Strings.StrConv (json, VbStrConv.SimplifiedChinese);
				ReaderWriterLockHelper.Write (() => {
					Instance = JsonConvert.Deserialize (json, Instance);
				});
				if (cache) {
					Timer.Add (DateTime.Now.AddMilliseconds (1));
				} else {
					File.WriteAllText (Paths.WarframeStatusCacheFile, json);
					Config.Read (config => {
						Timer.Add (DateTime.Now.AddMilliseconds (config.WarframeStatusUpdateInterval));
					});
				}
				QMHelperApi.Debug ($"游戏状态更新完毕，将在{Timer.Next}后再次更新");
			} catch (Exception exception) {
				if (cache) {
					Timer.Add (DateTime.Now.AddMilliseconds (1));
				} else {
					Config.Read (config => {
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
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					stringBuilder = GetInformation (
						stringBuilder,
						config.Commands.News[CommandIndexName.Default].Header, null,
						config.Commands.News[CommandIndexName.Default].Item, Instance.News, null, item => new object[] { item },
						config.Commands.News[CommandIndexName.Default].Footer, null
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetEventInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					if ((Instance.Events?.Length ?? 0) == 0) {
						stringBuilder = Localizer.Execute (stringBuilder, config.Commands.Event[CommandIndexName.NoResult].Header);
						return;
					}
					stringBuilder = GetInformation (
						stringBuilder,
						config.Commands.Event[CommandIndexName.Default].Header, null,
						config.Commands.Event[CommandIndexName.Default].Item, Instance.Events, null, item => new object[] { item },
						config.Commands.Event[CommandIndexName.Default].Footer, null
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetAlertInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					if ((Instance.Alerts?.Length ?? 0) == 0) {
						stringBuilder = Localizer.Execute (stringBuilder, config.Commands.Alert[CommandIndexName.NoResult].Header);
						return;
					}
					stringBuilder = GetInformation (
						stringBuilder,
						config.Commands.Alert[CommandIndexName.Default].Header, null,
						config.Commands.Alert[CommandIndexName.Default].Item, Instance.Alerts, null, item => new object[] { item, item.Mission },
						config.Commands.Alert[CommandIndexName.Default].Footer, null
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetArbitrationInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					if (string.IsNullOrEmpty (Instance.Arbitration.Node)) {
						stringBuilder = Localizer.Execute (stringBuilder, config.Commands.Arbitration[CommandIndexName.NoResult].Header);
						return;
					}
					stringBuilder = Localizer.Execute (stringBuilder, config.Commands.Arbitration[CommandIndexName.Default].Header, Instance.Arbitration);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetSortieInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					stringBuilder = GetInformation (
						stringBuilder,
						config.Commands.Sortie[CommandIndexName.Default].Header, new object[] { Instance.Sortie },
						config.Commands.Sortie[CommandIndexName.Default].Item, Instance.Sortie.Variants, null, item => new object[] { item },
						config.Commands.Sortie[CommandIndexName.Default].Footer, new object[] { Instance.Sortie }
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetOstronsBountyInformation (StringBuilder stringBuilder = null) {
			ReaderWriterLockHelper.Read (() => {
				stringBuilder = GetBountyInformation (stringBuilder, Instance.OstronsBounty, config => config.Commands.OstronsBounty);
			});
			return stringBuilder;
		}

		public static StringBuilder GetEntratiBountyInformation (StringBuilder stringBuilder = null) {
			ReaderWriterLockHelper.Read (() => {
				stringBuilder = GetBountyInformation (stringBuilder, Instance.EntratiBounty, config => config.Commands.EntratiBounty);
			});
			return stringBuilder;
		}

		public static StringBuilder GetSolarisUnitedBountyInformation (StringBuilder stringBuilder = null) {
			ReaderWriterLockHelper.Read (() => {
				stringBuilder = GetBountyInformation (stringBuilder, Instance.SolarisUnitedBounty, config => config.Commands.SolarisUnitedBounty);
			});
			return stringBuilder;
		}

		public static StringBuilder GetFissureInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					stringBuilder = GetInformation (
						stringBuilder,
						config.Commands.Fissure[CommandIndexName.Default].Header, null,
						config.Commands.Fissure[CommandIndexName.Default].Item, Instance.Fissures, null, item => new object[] { item },
						config.Commands.Fissure[CommandIndexName.Default].Footer, null
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetInvasionInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					stringBuilder = GetInformation (
						stringBuilder,
						config.Commands.Invasion[CommandIndexName.Default].Header, null,
						config.Commands.Invasion[CommandIndexName.Default].Item, Instance.Invasions, item => !item.Completed, item => new object[] { item },
						config.Commands.Invasion[CommandIndexName.Default].Footer, null
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetVoidTraderInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					if (Instance.VoidTrader.Active) {
						stringBuilder = GetInformation (
							stringBuilder,
							config.Commands.VoidTrader[CommandIndexName.Default].Header, new object[] { Instance.VoidTrader },
							config.Commands.VoidTrader[CommandIndexName.Default].Item, Instance.VoidTrader.Inventory, null, item => new object[] { item },
							config.Commands.VoidTrader[CommandIndexName.Default].Footer, new object[] { Instance.VoidTrader }
						);
						return;
					}
					stringBuilder = Localizer.Execute (stringBuilder, config.Commands.VoidTrader[CommandIndexName.NoResult].Header, Instance.VoidTrader);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetEarthCycleInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					stringBuilder = Localizer.Execute (stringBuilder, config.Commands.EarthCycle[CommandIndexName.Default].Header, Instance.EarthCycle);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetCetusCycleInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					stringBuilder = Localizer.Execute (stringBuilder, config.Commands.CetusCycle[CommandIndexName.Default].Header, Instance.CetusCycle);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetCambionCycleInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					stringBuilder = Localizer.Execute (stringBuilder, config.Commands.CambionCycle[CommandIndexName.Default].Header, Instance.CambionCycle);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetVallisCycleInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					stringBuilder = Localizer.Execute (stringBuilder, config.Commands.VallisCycle[CommandIndexName.Default].Header, Instance.VallisCycle);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetDailyDealInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					stringBuilder = GetInformation (
						stringBuilder,
						config.Commands.DailyDeal[CommandIndexName.Default].Header, new object[] { Instance.DailyDeals[0] },
						config.Commands.DailyDeal[CommandIndexName.Default].Item, Instance.DailyDeals, null, item => new object[] { item },
						config.Commands.DailyDeal[CommandIndexName.Default].Footer, new object[] { Instance.DailyDeals[0] }
					);
				});
			});
			return stringBuilder;
		}

		public static StringBuilder GetNightwaveInformation (StringBuilder stringBuilder = null) {
			Config.Read (config => {
				ReaderWriterLockHelper.Read (() => {
					if (!Instance.Nightwave.Active) {
						stringBuilder = Localizer.Execute (stringBuilder, config.Commands.Nightwave[CommandIndexName.NoResult].Header);
						return;
					}
					stringBuilder = GetInformation (
						stringBuilder,
						config.Commands.Nightwave[CommandIndexName.Default].Header, null,
						config.Commands.Nightwave[CommandIndexName.Default].Item, Instance.Nightwave.ActiveChallenges, null, item => new object[] { item },
						config.Commands.Nightwave[CommandIndexName.Default].Footer, null
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
			Config.Read (config => {
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