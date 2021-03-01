using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Eruru.QQMini.PluginSDKHelper;
using Eruru.TextCommand;
using QQMini.PluginSDK.Core;
using QQMini.PluginSDK.Core.Model;

namespace com.eruru.warframe {

	public class Program : PluginBase {

		public static TextCommandSystem<QMMessage<MessagePermissionLevel>, MessagePermissionLevel> TextCommandSystem { get; private set; }
			= new TextCommandSystem<QMMessage<MessagePermissionLevel>, MessagePermissionLevel> ();

		public override PluginInfo PluginInfo => _PluginInfo;

		static readonly PluginInfo _PluginInfo = new PluginInfo () {
			Author = "Eruru",
			Description = "Warframe",
			Name = "Warframe",
			PackageId = "com.eruru.warframe",
			Version = new Version (1, 0, 0, 0)
		};

		public override void OnInitialize () {
			Task.Run (async () => {
#if DEBUG
				Paths.Initialize (QMHelperApi.GetPluginDataDirectory ());
#else
				Paths.Initialize ($"Data/{PluginInfo.PackageId}/");
#endif
				Config.Load ();
				ApplyConfig ();
				TextCommandSystem.Add<Commands> (nameof (Commands.Help), "帮助", MessagePermissionLevel.Friend, nameof (Commands.Help));
				TextCommandSystem.Add<Commands> (nameof (Commands.News), "新闻", MessagePermissionLevel.Friend, nameof (Commands.News));
				TextCommandSystem.Add<Commands> (nameof (Commands.Event), new string[] {
					"活动",
					"事件"
				}, MessagePermissionLevel.Friend, nameof (Commands.Event));
				TextCommandSystem.Add<Commands> (nameof (Commands.Alert), "警报", MessagePermissionLevel.Friend, nameof (Commands.Alert));
				TextCommandSystem.Add<Commands> (nameof (Commands.Sortie), "突击", MessagePermissionLevel.Friend, nameof (Commands.Sortie));
				TextCommandSystem.Add<Commands> (nameof (Commands.OstronsBounty), new string[] {
					"地球赏金",
					"希图斯赏金"
				}, MessagePermissionLevel.Friend, nameof (Commands.OstronsBounty));
				TextCommandSystem.Add<Commands> (nameof (Commands.EntratiBounty), new string[] {
					"火卫二赏金",
					"殁世幽都赏金"
				}, MessagePermissionLevel.Friend, nameof (Commands.EntratiBounty));
				TextCommandSystem.Add<Commands> (nameof (Commands.SolarisUnitedBounty), new string[] {
					"金星赏金",
					"福尔图娜赏金"
				}, MessagePermissionLevel.Friend, nameof (Commands.SolarisUnitedBounty));
				TextCommandSystem.Add<Commands> (nameof (Commands.Fissure), new string[] {
					"裂缝",
					"虚空",
					"虚空裂缝"
				}, MessagePermissionLevel.Friend, nameof (Commands.Fissure));
				TextCommandSystem.Add<Commands> (nameof (Commands.Invasion), "入侵", MessagePermissionLevel.Friend, nameof (Commands.Invasion));
				TextCommandSystem.Add<Commands> (nameof (Commands.VoidTrader), new string[] {
					"奸商",
					"商人",
					"虚空商人"
				}, MessagePermissionLevel.Friend, nameof (Commands.VoidTrader));
				TextCommandSystem.Add<Commands> (nameof (Commands.Arbitration), "仲裁", MessagePermissionLevel.Friend, nameof (Commands.Arbitration));
				TextCommandSystem.Add<Commands> (nameof (Commands.DailyDeal), new string[] {
					"每日献礼",
					"献礼"
				}, MessagePermissionLevel.Friend, nameof (Commands.DailyDeal));
				TextCommandSystem.Add<Commands> (nameof (Commands.Nightwave), new string[] {
					"午夜电波",
					"电波"
				}, MessagePermissionLevel.Friend, nameof (Commands.Nightwave));
				TextCommandSystem.Add<Commands> (nameof (Commands.EarthCycle), new string[] {
					"地球时间",
					"地球"
				}, MessagePermissionLevel.Friend, nameof (Commands.EarthCycle));
				TextCommandSystem.Add<Commands> (nameof (Commands.CetusCycle), new string[] {
					"希图斯时间",
					"平原时间",
					"平野时间",
					"夜灵平原时间",
					"夜灵平野时间"
				}, MessagePermissionLevel.Friend, nameof (Commands.CetusCycle));
				TextCommandSystem.Add<Commands> (nameof (Commands.CambionCycle), new string[] {
					"火卫二时间",
					"魔胎之境时间"
				}, MessagePermissionLevel.Friend, nameof (Commands.CambionCycle));
				TextCommandSystem.Add<Commands> (nameof (Commands.VallisCycle), new string[] {
					"金星时间",
					"山谷时间",
					"奥布山谷时间"
				}, MessagePermissionLevel.Friend, nameof (Commands.VallisCycle));
				TextCommandSystem.Add<Commands> (nameof (Commands.Cetus), new string[] {
					"希图斯",
					"平原",
					"平野",
					"夜灵平原",
					"夜灵平野"
				}, MessagePermissionLevel.Friend, nameof (Commands.Cetus));
				TextCommandSystem.Add<Commands> (nameof (Commands.Cambion), new string[] {
					"火卫二",
					"魔胎之境"
				}, MessagePermissionLevel.Friend, nameof (Commands.Cambion));
				TextCommandSystem.Add<Commands> (nameof (Commands.Vallis), new string[] {
					"金星",
					"山谷",
					"奥布山谷"
				}, MessagePermissionLevel.Friend, nameof (Commands.Vallis));
				TextCommandSystem.Add<Commands> (nameof (Commands.Translate), "翻译", "<关键字>", MessagePermissionLevel.Friend, nameof (Commands.Translate));
				TextCommandSystem.Add<Commands> (nameof (Commands.WarframeMarket), "WM", "[卖家|买家|买|卖] <关键字>", MessagePermissionLevel.Friend, nameof (Commands.WarframeMarket));
				TextCommandSystem.Add<Commands> (nameof (Commands.LoadConfig), "加载配置", MessagePermissionLevel.Master, nameof (Commands.LoadConfig));
				TextCommandSystem.Add<Commands> (nameof (Commands.SaveConfig), "保存配置", MessagePermissionLevel.Master, nameof (Commands.SaveConfig));
				TextCommandSystem.Add<Commands> (nameof (Commands.AddGroup), "添加群", MessagePermissionLevel.Master, nameof (Commands.AddGroup));
				TextCommandSystem.Add<Commands> (nameof (Commands.AddGroupByGroup), "添加群", "<群号>", MessagePermissionLevel.Master, nameof (Commands.AddGroupByGroup));
				TextCommandSystem.Add<Commands> (nameof (Commands.RemoveGroup), "删除群", MessagePermissionLevel.Master, nameof (Commands.RemoveGroup));
				TextCommandSystem.Add<Commands> (nameof (Commands.RemoveGroupByGroup), "删除群", "<群号>", MessagePermissionLevel.Master, nameof (Commands.RemoveGroupByGroup));
				TextCommandSystem.Add<Commands> (nameof (Commands.AddRelayGroup), "添加转发群", MessagePermissionLevel.Master, nameof (Commands.AddRelayGroup));
				TextCommandSystem.Add<Commands> (nameof (Commands.AddRelayGroupByGroup), "添加转发群", "<群号>", MessagePermissionLevel.Master, nameof (Commands.AddRelayGroupByGroup));
				TextCommandSystem.Add<Commands> (nameof (Commands.RemoveRelayGroup), "删除转发群", MessagePermissionLevel.Master, nameof (Commands.RemoveRelayGroup));
				TextCommandSystem.Add<Commands> (nameof (Commands.RemoveRelayGroupByGroup), "删除转发群", "<群号>", MessagePermissionLevel.Master, nameof (Commands.RemoveRelayGroupByGroup));
				TextCommandSystem.Add<Commands> (nameof (Commands.AddTranslate), "添加翻译", "<英文> <中文>", MessagePermissionLevel.Master, nameof (Commands.AddTranslate));
				TextCommandSystem.Add<Commands> (nameof (Commands.RemoveTranslate), "删除翻译", "<英文>", MessagePermissionLevel.Master, nameof (Commands.RemoveTranslate));
				TextCommandSystem.PermissionError += TextCommandSystem_PermissionError;
				NoticeSystem.Add (nameof (Commands.News), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetNewsInformation ());
				});
				NoticeSystem.Add (nameof (Commands.Event), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetEventInformation ());
				});
				NoticeSystem.Add (nameof (Commands.Alert), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetAlertInformation ());
				});
				NoticeSystem.Add (nameof (Commands.Sortie), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetSortieInformation ());
				});
				NoticeSystem.Add (nameof (Commands.Fissure), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetFissureInformation ());
				});
				NoticeSystem.Add (nameof (Commands.Invasion), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetInvasionInformation ());
				});
				NoticeSystem.Add (nameof (Commands.VoidTrader), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetVoidTraderInformation ());
				});
				NoticeSystem.Add (nameof (Commands.DailyDeal), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetDailyDealInformation ());
				});
				NoticeSystem.Add (nameof (Commands.EarthCycle), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetEarthCycleInformation ());
				});
				NoticeSystem.Add (nameof (Commands.CetusCycle), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetCetusCycleInformation ());
				});
				NoticeSystem.Add (nameof (Commands.CambionCycle), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetCambionCycleInformation ());
				});
				NoticeSystem.Add (nameof (Commands.VallisCycle), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetVallisCycleInformation ());
				});
				NoticeSystem.Add (nameof (Commands.Nightwave), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetNightwaveInformation ());
				});
				NoticeSystem.Add (nameof (Commands.Arbitration), () => {
					Api.BroadcastGroupMessage (WarframeStatus.GetArbitrationInformation ());
				});
				await TranslateSystem.Start ();
				WarframeMarket.Start ();
				WarframeStatus.Start ();
			});
		}

		public override void OnOpenSettingMenu () {

		}

		public override QMEventHandlerTypes OnReceiveGroupMessage (QMGroupMessageEventArgs e) {
			OnReceiveMessage (new QMMessage<MessagePermissionLevel> (QMMessageType.Group, e.RobotQQ, e.FromGroup, e.FromQQ, e.Message.Id, e.Message.Number, e.Message));
			return QMEventHandlerTypes.Continue;
		}

		public override QMEventHandlerTypes OnReceiveGroupTempMessage (QMGroupPrivateMessageEventArgs e) {
			OnReceiveMessage (new QMMessage<MessagePermissionLevel> (QMMessageType.GroupTemp, e.RobotQQ, e.FromGroup, e.FromQQ, e.Message.Id, e.Message.Number, e.Message));
			return QMEventHandlerTypes.Continue;
		}

		public override QMEventHandlerTypes OnReceiveFriendMessage (QMPrivateMessageEventArgs e) {
			OnReceiveMessage (new QMMessage<MessagePermissionLevel> (QMMessageType.Friend, e.RobotQQ, e.FromQQ, e.Message.Id, e.Message.Number, e.Message));
			return QMEventHandlerTypes.Continue;
		}

		public override void OnUninitialize () {
			Config.Save ();
		}

		public void ApplyConfig () {
			Config.Read (config => {
				NoticeSystem.StartTime = config.StartNoticeTime;
				NoticeSystem.EndTime = config.EndNoticeTime;
				NoticeSystem.MinimumInterval = config.MinimumNoticeInterval;
				NoticeSystem.MaximumInterval = config.MaximumNoticeInterval;
				CatchMessageSystem.CatchTimeout = config.MessageReplyTimeout;
			});
		}

		public void OnReceiveMessage (QMMessage<MessagePermissionLevel> message) {
			if (message is null) {
				throw new ArgumentNullException (nameof (message));
			}
			Config.Read (config => {
				if (message.Group > 0 && config.RelayGroups.Contains (message.Group)) {
					Api.BroadcastGroupMessage (message);
				}
				MessagePermissionLevel messagePermissionLevel;
				if (message.QQ == config.Developer) {
					messagePermissionLevel = MessagePermissionLevel.Developer;
				} else if (message.QQ == config.Master) {
					messagePermissionLevel = MessagePermissionLevel.Master;
				} else {
					switch (message.Type) {
						case QMMessageType.Friend:
							messagePermissionLevel = MessagePermissionLevel.Friend;
							break;
						case QMMessageType.GroupTemp:
							messagePermissionLevel = MessagePermissionLevel.GroupTemp;
							break;
						case QMMessageType.Group:
							messagePermissionLevel = MessagePermissionLevel.Group;
							break;
						default:
							throw new NotImplementedException (message.Type.ToString ());
					}
				}
				if (messagePermissionLevel < MessagePermissionLevel.Master) {
					if (message.Group > 0 && !config.Groups.Contains (message.Group)) {
						return;
					}
				}
				message.Tag = messagePermissionLevel;
				Task.Run (() => {
					try {
						CatchMessageSystem.Execute (message);
						TextCommandSystem.Execute (message.Text, message, messagePermissionLevel);
					} catch (Exception exception) {
						message.Reply (exception);
					}
				});
			});
		}

		private void TextCommandSystem_PermissionError (QMMessage<MessagePermissionLevel> message) {
			message.Reply ("权限不足");
		}

	}

}