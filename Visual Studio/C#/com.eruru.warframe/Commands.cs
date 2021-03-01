using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Eruru.Html;
using Eruru.Http;
using Eruru.Json;
using Eruru.Localizer;
using Eruru.QQMini.PluginSDKHelper;


namespace com.eruru.warframe {

	public class Commands {

		public static void Help (QMMessage<MessagePermissionLevel> message) {
			StringBuilder stringBuilder = new StringBuilder ();
			int i = 0;
			Program.TextCommandSystem.ForEach (textCommand => {
				if (message.Tag < textCommand.PermissionLevel) {
					return;
				}
				if (stringBuilder.Length > 0) {
					stringBuilder.AppendLine ();
				}
				for (int n = 0; n < textCommand.Names.Count; n++) {
					if (n != 0) {
						stringBuilder.Append ('、');
					}
					stringBuilder.Append (textCommand.Names[n]);
					stringBuilder.Append ($" {textCommand.Tag}");
				}
				i++;
			});
			message.Reply (stringBuilder);
		}

		public static void LoadConfig (QMMessage<MessagePermissionLevel> message) {
			Config.Load ();
			message.Reply ("加载配置成功");
		}

		public static void SaveConfig (QMMessage<MessagePermissionLevel> message) {
			Config.Save ();
			message.Reply ("保存配置成功");
		}

		public static void AddGroup (QMMessage<MessagePermissionLevel> message) {
			AddGroupByGroup (message, message.Group);
		}

		public static void AddGroupByGroup (QMMessage<MessagePermissionLevel> message, long group) {
			if (group < 10000) {
				message.Reply ($"群号\"{group}\"无效");
				return;
			}
			Config.Read (config => {
				if (!config.Groups.Contains (group)) {
					Config.Write (config1 => {
						config.Groups.Add (group);
					});
					message.Reply ($"添加群\"{group}\"成功");
					return;
				}
				message.Reply ($"群\"{group}\"已存在");
			});
		}

		public static void RemoveGroup (QMMessage<MessagePermissionLevel> message) {
			RemoveGroupByGroup (message, message.Group);
		}

		public static void RemoveGroupByGroup (QMMessage<MessagePermissionLevel> message, long group) {
			if (group < 10000) {
				message.Reply ($"群号\"{group}\"无效");
				return;
			}
			Config.Write (config => {
				if (config.Groups.Remove (group)) {
					message.Reply ($"移除群\"{group}\"成功");
					return;
				}
				message.Reply ($"群\"{group}\"不存在");
			});
		}

		public static void AddRelayGroup (QMMessage<MessagePermissionLevel> message) {
			AddRelayGroupByGroup (message, message.Group);
		}

		public static void AddRelayGroupByGroup (QMMessage<MessagePermissionLevel> message, long group) {
			if (group < 10000) {
				message.Reply ($"群号\"{group}\"无效");
				return;
			}
			Config.Read (config => {
				if (!config.RelayGroups.Contains (group)) {
					Config.Write (config1 => {
						config.RelayGroups.Add (group);
					});
					message.Reply ($"添加通知群\"{group}\"成功");
					return;
				}
				message.Reply ($"通知群\"{group}\"已存在");
			});
		}

		public static void RemoveRelayGroup (QMMessage<MessagePermissionLevel> message) {
			RemoveRelayGroupByGroup (message, message.Group);
		}

		public static void RemoveRelayGroupByGroup (QMMessage<MessagePermissionLevel> message, long group) {
			if (group < 10000) {
				message.Reply ($"群号\"{group}\"无效");
				return;
			}
			Config.Write (config => {
				if (config.RelayGroups.Remove (group)) {
					message.Reply ($"移除通知群\"{group}\"成功");
					return;
				}
				message.Reply ($"通知群\"{group}\"不存在");
			});
		}

		public static void AddTranslate (QMMessage<MessagePermissionLevel> message, string key, string value) {
			Config.Write (config => {
				config.Translates[key] = value;
			});
			TranslateSystem.Add (key, value);
			message.Reply ($"添加翻译\"{key}\" = \"{value}\"成功");
		}

		public static void RemoveTranslate (QMMessage<MessagePermissionLevel> message, string key) {
			Config.Read (config => {
				if (config.Translates.TryGetValue (key, out string value)) {
					Config.Write (config1 => {
						config.Translates.Remove (key);
					});
					TranslateSystem.Remove (key);
					message.Reply ($"删除翻译\"{key}\"=\"{value}\"成功");
					return;
				}
				message.Reply ($"翻译键\"{key}\"不存在");
			});
		}

		public static void Translate (QMMessage<MessagePermissionLevel> message, params string[] arguments) {
			string keyword = string.Join (" ", arguments);
			List<Translate> translates = TranslateSystem.Search (keyword);
			Config.Read (config => {
				CommandHeader header = new CommandHeader () {
					Keyword = keyword,
					MaxResultNumber = config.TranslateMaxResultNumber
				};
				if (translates.Count == 0) {
					message.Reply (Localizer.Execute (config.Commands.Translate[CommandIndexName.NoResult].Header, header));
					return;
				}
				translates.Sort ();
				StringBuilder stringBuilder = new StringBuilder ();
				CommandItem item = new CommandItem ();
				int i = 0;
				WarframeStatus.GetInformation (
					stringBuilder,
					config.Commands.Translate[CommandIndexName.Default].Header, new object[] { header },
					config.Commands.Translate[CommandIndexName.Default].Item, translates, translate => i < config.TranslateMaxResultNumber, translate => {
						item.Key = translate.Key;
						item.Value = translate.Value;
						i++;
						return new object[] { item };
					},
					config.Commands.Translate[CommandIndexName.Default].Footer, new object[] { header }
				);
				message.Reply (stringBuilder);
			});
		}

		public static void WarframeMarket (QMMessage<MessagePermissionLevel> message, params string[] arguments) {
			WarframeMarketOrderType orderType = WarframeMarketOrderType.Sell;
			int startIndex = 0;
			switch (arguments[0]) {
				case "卖家":
				case "买":
					orderType = WarframeMarketOrderType.Sell;
					startIndex = 1;
					break;
				case "买家":
				case "卖":
					orderType = WarframeMarketOrderType.Buy;
					startIndex = 1;
					break;
			}
			string keyword = string.Join (" ", arguments, startIndex, arguments.Length - startIndex);
			CatchMessageSystem.Clear (message);
			List<WarframeMarketItem> items = warframe.WarframeMarket.Search (keyword);
			Config.Read (config => {
				CommandHeader header = new CommandHeader () {
					Keyword = keyword,
					MaxResultNumber = config.TranslateMaxResultNumber,
					OrderType = orderType == WarframeMarketOrderType.Sell ? "卖家" : "买家",
					ResultNumber = items.Count
				};
				if (items.Count == 0) {
					message.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.NoResult].Header, header));
					return;
				}
				if (items.Count == 1) {
					WarframeMarketSelectedItem (message, header, items[0], orderType);
					return;
				}
				items.Sort ();
				StringBuilder stringBuilder = new StringBuilder ();
				CommandItem commandItem = new CommandItem ();
				int i = 0;
				WarframeStatus.GetInformation (
					stringBuilder,
					config.Commands.WarframeMarket[CommandIndexName.Default].Header, new object[] { header },
					config.Commands.WarframeMarket[CommandIndexName.Default].Item, items, item => i < config.WarframeMarketMaxResultNumber, item => {
						commandItem.OrderNumber = i + 1;
						commandItem.Name = item.Name;
						commandItem.ItemName = item.ItemName;
						i++;
						return new object[] { commandItem };
					},
					config.Commands.WarframeMarket[CommandIndexName.Default].Footer, new object[] { header }
				);
				message.Reply (stringBuilder);
				CatchMessageSystem.Add (message, receivedMessage => {
					if (int.TryParse (receivedMessage, out int orderNumber)) {
						if (orderNumber > 0 && orderNumber <= items.Count) {
							Task.Run (() => {
								WarframeMarketSelectedItem (receivedMessage, header, items[orderNumber - 1], orderType);
							});
							return true;
						}
						receivedMessage.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.ItemOrderNumberError].Header, header));
					}
					return false;
				});
			});
		}
		static void WarframeMarketSelectedItem (QMMessage<MessagePermissionLevel> message, CommandHeader header, WarframeMarketItem item, WarframeMarketOrderType orderType) {
			Config.Read (config => {
				message.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.StartQuery].Header, header, item));
			});
			Http http = new Http ();
			HttpRequestInformation httpRequestInformation = new HttpRequestInformation ();
			string html = http.Request ($@"https://warframe.market/items/{item.UrlName}", httpRequestInformation);
			HtmlDocument htmlDocument = HtmlDocument.Parse (html);
			string json = warframe.WarframeMarket.GetJson (htmlDocument);
			WarframeMarketItemPage itemPage = JsonConvert.Deserialize<WarframeMarketItemPage> (json);
			header.MaxModRank = itemPage.Include.Item.ItemsInSet[0].ModMaxRank;
			if (header.MaxModRank == 0) {
				WarframeMarketSelectedModRank (message, header, item, orderType, itemPage, header.MaxModRank);
				return;
			}
			Config.Read (config => {
				message.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.ModRankError].Header, header, item));
				CatchMessageSystem.Add (message, receivedMessage => {
					if (int.TryParse (receivedMessage, out int modRank)) {
						if (modRank >= 0 && modRank <= header.MaxModRank) {
							WarframeMarketSelectedModRank (message, header, item, orderType, itemPage, modRank);
							return true;
						}
						receivedMessage.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.ModRankError].Header, header, item));
					}
					return false;
				});
			});
		}
		static void WarframeMarketSelectedModRank (QMMessage<MessagePermissionLevel> message, CommandHeader header, WarframeMarketItem item, WarframeMarketOrderType orderType, WarframeMarketItemPage itemPage, int modRank) {
			header.ModRank = modRank;
			List<WarframeMarketOrder> orders = new List<WarframeMarketOrder> ();
			foreach (var order in itemPage.Payload.Orders) {
				if (order.Visible && order.ModRank == modRank && order.OrderType == orderType) {
					orders.Add (order);
				}
			}
			header.ResultNumber = orders.Count;
			Config.Read (config => {
				if (orders.Count == 0) {
					if (header.MaxModRank == 0) {
						message.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.NoQueryResult].Header, header, item));
						return;
					}
					message.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.NoQueryResultByModRank].Header, header, item));
					return;
				}
				int sortDirection = orderType == WarframeMarketOrderType.Sell ? 1 : -1;
				orders.Sort (new Comparison<WarframeMarketOrder> ((a, b) => {
					if (a.User.Status == b.User.Status) {
						return a.Platinum.CompareTo (b.Platinum) * sortDirection;
					}
					return a.User.Status.CompareTo (b.User.Status);
				}));
				StringBuilder stringBuilder = new StringBuilder ();
				string headerIndexName = header.MaxModRank == 0 ? CommandIndexName.QueryResult : CommandIndexName.QueryResultByModRank;
				int i = 0;
				CommandItem commandItem = new CommandItem ();
				WarframeStatus.GetInformation (
					stringBuilder,
					config.Commands.WarframeMarket[headerIndexName].Header, new object[] { header, item },
					config.Commands.WarframeMarket[headerIndexName].Item, orders, order => i < config.WarframeMarketMaxResultNumber, order => {
						commandItem.OrderNumber = i + 1;
						commandItem.Platinum = order.Platinum;
						commandItem.Status = GetStatusText (order.User.Status);
						commandItem.Quantity = order.Quantity;
						commandItem.InGameName = order.User.InGameName;
						i++;
						return new object[] { commandItem };
					},
					config.Commands.WarframeMarket[headerIndexName].Footer, new object[] { header, item }
				);
				message.Reply (stringBuilder);
				CatchMessageSystem.Add (message, receivedMessage => {
					if (int.TryParse (receivedMessage, out int orderNumber)) {
						if (orderNumber > 0 && orderNumber <= orders.Count) {
							WarframeMarketSelectedOrder (receivedMessage, header, item, orders[orderNumber - 1]);
							return false;
						}
						receivedMessage.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.PrivateChatOrderNumberError].Header, header, item));
					}
					return false;
				});
			});
			string GetStatusText (WarframeMarketUserStatus status) {
				switch (status) {
					case WarframeMarketUserStatus.InGame:
						return "游戏中";
					case WarframeMarketUserStatus.Online:
						return "在网站";
					case WarframeMarketUserStatus.Offline:
						return "离线";
					default:
						throw new NotImplementedException ();
				}
			}
		}
		static void WarframeMarketSelectedOrder (QMMessage<MessagePermissionLevel> message, CommandHeader header, WarframeMarketItem item, WarframeMarketOrder order) {
			Config.Read (config => {
				if (header.MaxModRank == 0) {
					message.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.PrivateChatText].Header, header, item, order, order.User));
					return;
				}
				message.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.PrivateChatTextByModRank].Header, header, item, order, order.User));
			});
		}

		public static void News (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetNewsInformation ());
		}

		public static void Event (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetEventInformation ());
		}

		public static void Alert (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetAlertInformation ());
		}

		public static void Arbitration (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetArbitrationInformation ());
		}

		public static void Sortie (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetSortieInformation ());
		}

		public static void OstronsBounty (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetOstronsBountyInformation ());
		}

		public static void EntratiBounty (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetEntratiBountyInformation ());
		}

		public static void SolarisUnitedBounty (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetSolarisUnitedBountyInformation ());
		}

		public static void Fissure (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetFissureInformation ());
		}

		public static void Invasion (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetInvasionInformation ());
		}

		public static void VoidTrader (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetVoidTraderInformation ());
		}

		public static void EarthCycle (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetEarthCycleInformation ());
		}

		public static void CetusCycle (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetCetusCycleInformation ());
		}

		public static void CambionCycle (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetCambionCycleInformation ());
		}

		public static void VallisCycle (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetVallisCycleInformation ());
		}

		public static void Cetus (QMMessage<MessagePermissionLevel> message) {
			StringBuilder stringBuilder = WarframeStatus.GetCetusCycleInformation ();
			stringBuilder.AppendLine ();
			stringBuilder.AppendLine ();
			stringBuilder = WarframeStatus.GetOstronsBountyInformation (stringBuilder);
			message.Reply (stringBuilder);
		}

		public static void Cambion (QMMessage<MessagePermissionLevel> message) {
			StringBuilder stringBuilder = WarframeStatus.GetCambionCycleInformation ();
			stringBuilder.AppendLine ();
			stringBuilder.AppendLine ();
			stringBuilder = WarframeStatus.GetEntratiBountyInformation (stringBuilder);
			message.Reply (stringBuilder);
		}

		public static void Vallis (QMMessage<MessagePermissionLevel> message) {
			StringBuilder stringBuilder = WarframeStatus.GetVallisCycleInformation ();
			stringBuilder.AppendLine ();
			stringBuilder.AppendLine ();
			stringBuilder = WarframeStatus.GetSolarisUnitedBountyInformation (stringBuilder);
			message.Reply (stringBuilder);
		}

		public static void DailyDeal (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetDailyDealInformation ());
		}

		public static void Nightwave (QMMessage<MessagePermissionLevel> message) {
			message.Reply (WarframeStatus.GetNightwaveInformation ());
		}

	}

	public class CommandHeader {

		public string Keyword;
		public int ModRank;
		public int MaxModRank;
		public int MaxResultNumber;
		public string OrderType;
		public int ResultNumber;

	}

	public class CommandItem {

		public int OrderNumber;
		public string Key;
		public string Value;
		public string Name;
		public string ItemName;
		public int Platinum;
		public string Status;
		public int Quantity;
		public string InGameName;

	}

}