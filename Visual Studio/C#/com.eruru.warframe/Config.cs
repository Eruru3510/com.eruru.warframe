using System;
using System.Collections.Generic;
using System.IO;
using Eruru.Json;
using Eruru.QQMini.PluginSDKHelper;
using Eruru.ReaderWriterLockHelper;

namespace com.eruru.warframe {

	public class Config {

		public long Developer { get; set; } = 1633756198;
		public long Master { get; set; }
		public int StartNoticeTime { get; set; } = 6;
		public int EndNoticeTime { get; set; } = 24;
		public long MinimumNoticeInterval { get; set; } = Api.HoursToMillisecond (1);
		public long MaximumNoticeInterval { get; set; } = Api.HoursToMillisecond (4);
		public long MessageReplyTimeout {

			get => _MessageReplyTimeout;

			set {
				_MessageReplyTimeout = value;
				CatchMessageSystem.CatchTimeout = value;
			}

		}
		public string WarframeStatusUrl { get; set; } = "https://api.warframestat.us/pc";
		public long WarframeStatusUpdateInterval {

			get => _WarframeStatusUpdateInterval;

			set {
				if (value < Api.MinutesToMilliseconds (1)) {
					value = Api.MinutesToMilliseconds (1);
				}
				_WarframeStatusUpdateInterval = value;
			}

		}
		public long WarframeStatusUpdateRetryInterval {

			get => _WarframeStatusUpdateRetryInterval;

			set {
				if (value < Api.MinutesToMilliseconds (1)) {
					value = Api.MinutesToMilliseconds (1);
				}
				_WarframeStatusUpdateRetryInterval = value;
			}

		}
		public string TranslateUrl { get; set; } = "https://warframe.huijiwiki.com/api.php?action=query&format=json&prop=revisions&titles=UserDict&formatversion=2&rvprop=content&rvlimit=1";
		public long TranslateUpdateInterval {

			get => _TranslateUpdateInterval;

			set {
				if (value < Api.MinutesToMilliseconds (1)) {
					value = Api.MinutesToMilliseconds (1);
				}
				_TranslateUpdateInterval = value;
				TranslateSystem.TimerInterval = value;
			}

		}
		public long TranslateUpdateRetryInterval {

			get => _TranslateUpdateRetryInterval;

			set {
				if (value < Api.MinutesToMilliseconds (1)) {
					value = Api.MinutesToMilliseconds (1);
				}
				_TranslateUpdateRetryInterval = value;
			}

		}
		public int TranslateMaxResultNumber { get; set; } = 10;
		public string WarframeMarketUrl { get; set; } = "https://warframe.market/error/404";
		public long WarframeMarketUpdateInterval {

			get => _WarframeMarketUpdateInterval;

			set {
				if (value < Api.MinutesToMilliseconds (1)) {
					value = Api.MinutesToMilliseconds (1);
				}
				_WarframeMarketUpdateInterval = value;
				WarframeMarket.TimerInterval = value;
			}

		}
		public long WarframeMarketUpdateRetryInterval {

			get => _WarframeMarketUpdateRetryInterval;

			set {
				if (value < Api.MinutesToMilliseconds (1)) {
					value = Api.MinutesToMilliseconds (1);
				}
				_WarframeMarketUpdateRetryInterval = value;
			}

		}
		public int WarframeMarketMaxResultNumber { get; set; } = 10;
		public int WikiMaxResultNumber { get; set; } = 10;
		public List<long> Groups { get; set; } = new List<long> ();
		public List<long> RelayGroups { get; set; } = new List<long> ();
		public CommandSet Commands = new CommandSet ();
		public SortedDictionary<string, string> Translates { get; set; } = new SortedDictionary<string, string> ();
		public SortedDictionary<string, string> WarframeMarketTranslates { get; set; } = new SortedDictionary<string, string> ();

		static readonly ReaderWriterLockHelper<Config> ReaderWriterLockHelper = new ReaderWriterLockHelper<Config> (new Config ());
		static readonly JsonConfig JsonConfig = new JsonConfig () {
			Compress = false,
			IgnoreDefaultValue = true
		};

		long _MessageReplyTimeout = Api.MinutesToMilliseconds (10);
		long _WarframeStatusUpdateInterval = Api.MinutesToMilliseconds (1);
		long _WarframeStatusUpdateRetryInterval = Api.MinutesToMilliseconds (1);
		long _TranslateUpdateInterval = Api.MinutesToMilliseconds (30);
		long _TranslateUpdateRetryInterval = Api.MinutesToMilliseconds (30);
		long _WarframeMarketUpdateInterval = Api.MinutesToMilliseconds (30);
		long _WarframeMarketUpdateRetryInterval = Api.MinutesToMilliseconds (30);

		public static void Load () {
			QMHelperApi.Debug ("开始加载配置");
			if (File.Exists (Paths.ConfigFile)) {
				try {
					ReaderWriterLockHelper.Write ((ref Config config) => {
						config = JsonConvert.DeserializeFile (Paths.ConfigFile, config, JsonConfig);
					});
					QMHelperApi.Debug ("配置加载完毕");
				} catch (Exception exception) {
					QMHelperApi.Debug ($"配置加载失败{Environment.NewLine}{exception}");
				}
			}
		}

		public static void Save () {
			QMHelperApi.Debug ("开始保存配置");
			try {
				ReaderWriterLockHelper.Read ((ref Config config) => {
					JsonConvert.Serialize (config, Paths.ConfigFile, JsonConfig);
				});
				QMHelperApi.Debug ("配置保存完毕");
			} catch (Exception exception) {
				QMHelperApi.Debug ($"配置保存失败{Environment.NewLine}{exception}");
			}
		}

		public static void Read (ReaderWriterLockHelperAction<Config> action) {
			if (action is null) {
				throw new ArgumentNullException (nameof (action));
			}
			ReaderWriterLockHelper.Read (action);
		}

		public static void Write (ReaderWriterLockHelperAction<Config> action) {
			if (action is null) {
				throw new ArgumentNullException (nameof (action));
			}
			ReaderWriterLockHelper.Write (action);
		}

		public class CommandSet {

			public Dictionary<string, Command> Wiki { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"查询到多个关于\"[{nameof (CommandHeader.Keyword)}]\"的Wiki页面，请输入序号选择（前[{nameof (CommandHeader.MaxResultNumber)}]条）：",
						$"[{nameof (CommandItem.OrderNumber)}]、[{nameof (CommandItem.Title)}]{Environment.NewLine}[{nameof (CommandItem.Url)}]{Environment.NewLine}[{nameof (CommandItem.Description)}]"
					)
				},
				{
					CommandIndexName.StartQuery,
					new Command (
						$"开始查询关于\"[{nameof (CommandHeader.Keyword)}]\"的Wiki页面"
					)
				},
				{
					CommandIndexName.NoResult,
					new Command (
						$"未找到关于\"[{nameof (CommandHeader.Keyword)}]\"的Wiki页面"
					)
				},
				{
					CommandIndexName.OrderNumberError,
					new Command (
						$"请输入1-[{nameof (CommandHeader.ResultNumber)}]之间的序号"
					)
				}
			};
			public Dictionary<string, Command> Translate { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"关于\"[{nameof (CommandHeader.Keyword)}]\"的游戏中英文对照如下（前[{nameof (CommandHeader.MaxResultNumber)}]条）：",
						$"[{nameof (CommandItem.Value)}] - [{nameof (CommandItem.Key)}]"
					)
				},
				{
					CommandIndexName.NoResult,
					new Command (
						$"没有找到关于\"[{nameof (CommandHeader.Keyword)}]\"的游戏中英文对照"
					)
				}
			};
			public Dictionary<string, Command> WarframeMarket { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"查询到多个关于\"[{nameof (CommandHeader.Keyword)}]\"的WM物品，请输入序号选择（前[{nameof (CommandHeader.MaxResultNumber)}]条）：",
						$"[{nameof (CommandItem.OrderNumber)}]、[{nameof (CommandItem.Name)}] - [{nameof (CommandItem.ItemName)}]"
					)
				},
				{
					CommandIndexName.NoResult,
					new Command (
						$"没有找到关于\"[{nameof (CommandHeader.Keyword)}]\"的WM物品{Environment.NewLine}使用\"绑定 <关键字>\"命令可绑定WM物品"
					)
				},
				{
					CommandIndexName.ItemOrderNumberError,
					new Command (
						$"请输入1-[{nameof (CommandHeader.ResultNumber)}]之间的物品序号"
					)
				},
				{
					CommandIndexName.StartQuery,
					new Command (
						$"开始查询WM物品\"[{nameof (CommandItem.Name)}] - [{nameof (CommandItem.ItemName)}]\"的[{nameof (CommandHeader.OrderType)}]列表"
					)
				},
				{
					CommandIndexName.SelectModRank,
					new Command (
						$"查询到WM物品\"[{nameof (CommandItem.Name)}] - [{nameof (CommandItem.ItemName)}]\"的最大MOD等级为[{nameof (CommandHeader.MaxModRank)}]，请输入0-[{nameof (CommandHeader.MaxModRank)}]选择"
					)
				},
				{
					CommandIndexName.ModRankError,
					new Command (
						$"请输入0-[{nameof (CommandHeader.MaxModRank)}]之间的MOD等级"
					)
				},
				{
					CommandIndexName.QueryResult,
					new Command (
						$"WM物品\"[{nameof (CommandItem.Name)}] - [{nameof (CommandItem.ItemName)}]\"的[{nameof (CommandHeader.OrderType)}]列表如下，输入序号可复制私聊信息（前[{nameof (CommandHeader.MaxResultNumber)}]条）：",
						$"[{nameof (CommandItem.OrderNumber)}]、白金[{nameof (CommandItem.Platinum)}] - [{nameof (CommandItem.Status)}] - 数量[{nameof (CommandItem.Quantity)}] - 昵称：[{nameof (CommandItem.InGameName)}]"
					)
				},
				{
					CommandIndexName.QueryResultByModRank,
					new Command (
						$"WM物品\"[{nameof (CommandItem.Name)}] - [{nameof (CommandItem.ItemName)}]\"（MOD等级[{nameof (CommandHeader.ModRank)}]）的[{nameof (CommandHeader.OrderType)}]列表如下，输入序号可复制私聊信息（前[{nameof (CommandHeader.MaxResultNumber)}]条）：",
						$"[{nameof (CommandItem.OrderNumber)}]、白金[{nameof (CommandItem.Platinum)}] - [{nameof (CommandItem.Status)}] - 数量[{nameof (CommandItem.Quantity)}] - 昵称：[{nameof (CommandItem.InGameName)}]"
					)
				},
				{
					CommandIndexName.NoQueryResult,
					new Command (
						$"WM物品\"[{nameof (CommandItem.Name)}] - [{nameof (CommandItem.ItemName)}]\"的[{nameof (CommandHeader.OrderType)}]列表为空"
					)
				},
				{
					CommandIndexName.NoQueryResultByModRank,
					new Command (
						$"WM物品\"[{nameof (CommandItem.Name)}] - [{nameof (CommandItem.ItemName)}]\"（MOD等级[{nameof (CommandHeader.ModRank)}]）的[{nameof (CommandHeader.OrderType)}]列表为空"
					)
				},
				{
					CommandIndexName.PrivateChatOrderNumberError,
					new Command (
						$"请输入1-[{nameof (CommandHeader.ResultNumber)}]之间的私聊序号"
					)
				},
				{
					CommandIndexName.PrivateChatText,
					new Command (
						$"/w [{nameof (CommandItem.InGameName)}] Hi! I want to [{nameof (CommandHeader.ChatOrderType)}]: [{nameof (CommandItem.ItemName)}] for [{nameof (CommandItem.Platinum)}] platinum. (warframe.market)"
					)
				},
				{
					CommandIndexName.PrivateChatTextByModRank,
					new Command (
						$"/w [{nameof (CommandItem.InGameName)}] Hi! I want to [{nameof (CommandHeader.ChatOrderType)}]: [{nameof (CommandItem.ItemName)}] (rank [{nameof (CommandHeader.ModRank)}]) for [{nameof (CommandItem.Platinum)}] platinum. (warframe.market)"
					)
				}
			};
			public Dictionary<string, Command> News { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						"新闻",
						$"[{nameof (WarframeStatusNews.Message)}] - [{nameof (WarframeStatusNews.Date)}]前{Environment.NewLine}[{nameof (WarframeStatusNews.Link)}]"
					)
				}
			};
			public Dictionary<string, Command> Alert { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						"警报",
						$"[{nameof (WarframeStatusAlert.Mission.Reward)}] - [{nameof (WarframeStatusAlert.Mission.Type)}] - [{nameof (WarframeStatusAlert.Mission.Faction)}] - [{nameof (WarframeStatusAlert.Mission.Node)}] - [{nameof (WarframeStatusAlert.Expiry)}]"
					)
				},
				{
					CommandIndexName.NoResult,
					new Command (
						"暂无警报"
					)
				}
			};
			public Dictionary<string, Command> Event { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						"活动",
						$"[{nameof (WarframeStatusEvent.Description)}] - [{nameof (WarframeStatusEvent.Health)}]% - [{nameof (WarframeStatusEvent.Node)}] - [{nameof (WarframeStatusEvent.Expiry)}]"
					)
				},
				{
					CommandIndexName.NoResult,
					new Command (
						"暂无活动"
					)
				}
			};
			public Dictionary<string, Command> Arbitration { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"仲裁 - [{nameof (WarframeStatusArbitration.Expiry)}]{Environment.NewLine}[{nameof (WarframeStatusArbitration.Type)}] - [{nameof (WarframeStatusArbitration.Enemy)}] - [{nameof (WarframeStatusArbitration.Node)}]"
					)
				},
				{
					CommandIndexName.NoResult,
					new Command (
						"赞无仲裁数据"
					)
				}
			};
			public Dictionary<string, Command> Sortie { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"突击 - [{nameof (WarframeStatusSortie.Expiry)}]{Environment.NewLine}击败 [{nameof (WarframeStatusSortie.Boss)}] 的部队 - [{nameof (WarframeStatusSortie.Faction)}]",
						$"[{nameof (WarframeStatusSortie.Variant.MissionType)}] - [{nameof (WarframeStatusSortie.Variant.Modifier)}] - [{nameof (WarframeStatusSortie.Variant.Node)}]"
					)
				}
			};
			public Dictionary<string, Command> OstronsBounty { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"Ostrons赏金 - [{nameof (WarframeStatusSyndicateMission.Expiry)}]",
						$"【[{nameof (WarframeStatusJob.Type)}]】 - [{nameof (WarframeStatusJob.RewardPool)}]"
					)
				}
			};
			public Dictionary<string, Command> EntratiBounty { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"英择谛赏金 - [{nameof (WarframeStatusSyndicateMission.Expiry)}]",
						$"【[{nameof (WarframeStatusJob.Type)}]】 - [{nameof (WarframeStatusJob.RewardPool)}]"
					)
				}
			};
			public Dictionary<string, Command> SolarisUnitedBounty { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"索拉里斯联盟赏金 - [{nameof (WarframeStatusSyndicateMission.Expiry)}]",
						$"【[{nameof (WarframeStatusJob.Type)}]】 - [{nameof (WarframeStatusJob.RewardPool)}]"
					)
				}
			};
			public Dictionary<string, Command> Fissure { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						"虚空裂缝",
						$"[{nameof (WarframeStatusFissure.Tier)}] - [{nameof (WarframeStatusFissure.MissionType)}] - [{nameof (WarframeStatusFissure.EnemyKey)}] - [{nameof (WarframeStatusFissure.Node)}] - [{nameof (WarframeStatusFissure.Expiry)}]"
					)
				}
			};
			public Dictionary<string, Command> Invasion { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						"入侵",
						$"[{nameof (WarframeStatusInvasion.DefendingFaction)}] - [{nameof (WarframeStatusInvasion.DefenderReward)}] - [{nameof (WarframeStatusInvasion.Completion)}]% - [{nameof (WarframeStatusInvasion.AttackerReward)}] - [{nameof (WarframeStatusInvasion.AttackingFaction)}] - [{nameof (WarframeStatusInvasion.Node)}]"
					)
				}
			};
			public Dictionary<string, Command> VoidTrader { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"虚空商人 - [{nameof (WarframeStatusVoidTrader.Location)}]{Environment.NewLine}距离开还有[{nameof (WarframeStatusVoidTrader.Expiry)}]",
						$"[{nameof (WarframeStatusInventory.Item)}] - 杜卡德[{nameof (WarframeStatusInventory.Ducats)}] - 现金[{nameof (WarframeStatusInventory.Credits)}]"
					)
				},
				{
					CommandIndexName.NoResult,
					new Command (
						$"虚空商人 - [{nameof (WarframeStatusVoidTrader.Location)}]{Environment.NewLine}距到来还有[{nameof (WarframeStatusVoidTrader.Activation)}]"
					)
				}
			};
			public Dictionary<string, Command> EarthCycle { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"地球时间{Environment.NewLine}[{nameof (WarframeStatusCycle.Active)}] - 剩余[{nameof (WarframeStatusCycle.Expiry)}]"
					)
				}
			};
			public Dictionary<string, Command> CetusCycle { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"希图斯时间{Environment.NewLine}[{nameof (WarframeStatusCycle.Active)}] - 剩余[{nameof (WarframeStatusCycle.Expiry)}]"
					)
				}
			};
			public Dictionary<string, Command> CambionCycle { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"魔胎之境时间{Environment.NewLine}[{nameof (WarframeStatusCycle.Active)}] - 剩余[{nameof (WarframeStatusCycle.Expiry)}]"
					)
				}
			};
			public Dictionary<string, Command> VallisCycle { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"奥布山谷时间{Environment.NewLine}[{nameof (WarframeStatusCycle.Active)}] - 剩余[{nameof (WarframeStatusCycle.Expiry)}]"
					)
				}
			};
			public Dictionary<string, Command> DailyDeal { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						$"每日献礼 - [{nameof (WarframeStatusFissure.Expiry)}]",
						$"[{nameof (WarframeStatusDailyDeal.Item)}] - 原[{nameof (WarframeStatusDailyDeal.OriginalPrice)}]白金 - 现[{nameof (WarframeStatusDailyDeal.SalePrice)}]白金 - 已售[{nameof (WarframeStatusDailyDeal.Sold)}]/[{nameof (WarframeStatusDailyDeal.Total)}]"
					)
				}
			};
			public Dictionary<string, Command> Nightwave { get; set; } = new Dictionary<string, Command> () {
				{
					CommandIndexName.Default,
					new Command (
						"午夜电波",
						$"[{nameof (WarframeStatusActiveChallenge.Title)}] - [{nameof (WarframeStatusActiveChallenge.Description)}] - [{nameof (WarframeStatusActiveChallenge.Reputation)}] - [{nameof (WarframeStatusActiveChallenge.Expiry)}]"
					)
				},
				{
					CommandIndexName.NoResult,
					new Command (
						"暂无午夜电波任务"
					)
				}
			};

		}

		public class Command {

			public string Header;
			public string Item;
			public string Footer;

			public Command (string header, string item, string footer) {
				Header = header ?? throw new ArgumentNullException (nameof (header));
				Item = item ?? throw new ArgumentNullException (nameof (item));
				Footer = footer ?? throw new ArgumentNullException (nameof (footer));
			}
			public Command (string header, string item) {
				Header = header ?? throw new ArgumentNullException (nameof (header));
				Item = item ?? throw new ArgumentNullException (nameof (item));
			}
			public Command (string header) {
				Header = header ?? throw new ArgumentNullException (nameof (header));
			}

		}

	}

}