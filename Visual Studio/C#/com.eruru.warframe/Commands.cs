using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
			Config.Read ((ref Config config) => {
				if (config.Groups.Contains (group)) {
					message.Reply ($"群\"{group}\"已存在");
					return;
				}
				Config.Write ((ref Config subConfig) => {
					subConfig.Groups.Add (group);
				});
				message.Reply ($"添加群\"{group}\"成功");
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
			Config.Write ((ref Config config) => {
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
			Config.Read ((ref Config config) => {
				if (config.RelayGroups.Contains (group)) {
					message.Reply ($"通知群\"{group}\"已存在");
					return;
				}
				Config.Write ((ref Config subConfig) => {
					subConfig.RelayGroups.Add (group);
				});
				message.Reply ($"添加通知群\"{group}\"成功");
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
			Config.Write ((ref Config config) => {
				if (config.RelayGroups.Remove (group)) {
					message.Reply ($"移除通知群\"{group}\"成功");
					return;
				}
				message.Reply ($"通知群\"{group}\"不存在");
			});
		}

		public static void AddTranslate (QMMessage<MessagePermissionLevel> message, string key, string value) {
			message.Reply ($"开始添加翻译\"{key}\" = \"{value}\"");
			Config.Write ((ref Config config) => {
				config.Translates[key] = value;
			});
			TranslateSystem.Add (key, value);
			message.Reply ($"添加翻译\"{key}\" = \"{value}\"成功");
		}

		public static void RemoveTranslate (QMMessage<MessagePermissionLevel> message, string key) {
			Config.Read ((ref Config config) => {
				if (config.Translates.TryGetValue (key, out string value)) {
					Config.Write ((ref Config subConfig) => {
						subConfig.Translates.Remove (key);
					});
					TranslateSystem.Remove (key);
					message.Reply ($"删除翻译\"{key}\"=\"{value}\"成功");
					return;
				}
				message.Reply ($"翻译键\"{key}\"不存在");
			});
		}

		public static void AddWarframeMarketTranslate (QMMessage<MessagePermissionLevel> message, string key, string value) {
			message.Reply ($"开始添加WM翻译\"{key}\" = \"{value}\"");
			Config.Write ((ref Config config) => {
				config.WarframeMarketTranslates[key] = value;
			});
			warframe.WarframeMarket.Add (key, value);
			message.Reply ($"添加WM翻译\"{key}\" = \"{value}\"成功");
		}

		public static void RemoveWarframeMarketTranslate (QMMessage<MessagePermissionLevel> message, string key) {
			Config.Read ((ref Config config) => {
				if (config.WarframeMarketTranslates.TryGetValue (key, out string value)) {
					Config.Write ((ref Config subConfig) => {
						subConfig.WarframeMarketTranslates.Remove (key);
					});
					warframe.WarframeMarket.Remove (key);
					message.Reply ($"删除WM翻译\"{key}\"=\"{value}\"成功");
					return;
				}
				message.Reply ($"WM翻译键\"{key}\"不存在");
			});
		}

		public static void Translate (QMMessage<MessagePermissionLevel> message, params string[] arguments) {
			string keyword = string.Join (" ", arguments);
			List<Translate> translates = TranslateSystem.Search (keyword);
			Config.Read ((ref Config config) => {
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
				object[] items = new object[] { item };
				int i = 0;
				WarframeStatus.GetInformation (
					stringBuilder,
					config.Commands.Translate[CommandIndexName.Default].Header, new object[] { header },
					config.Commands.Translate[CommandIndexName.Default].Item, translates, translate => i < header.MaxResultNumber, translate => {
						item.Key = translate.Key;
						item.Value = translate.Value;
						i++;
						return items;
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
			int parameterCount = startIndex;
			CommandHeader header = new CommandHeader () {
				OrderType = orderType == WarframeMarketOrderType.Sell ? "卖家" : "买家",
				ChatOrderType = orderType == WarframeMarketOrderType.Sell ? "buy" : "sell",
			};
			if (arguments.Length >= 2 && int.TryParse (arguments[arguments.Length - 1], out int modRank)) {
				parameterCount++;
				header.PreSelectedMod = modRank;
			}
			string keyword = string.Join (" ", arguments, startIndex, arguments.Length - parameterCount);
			keyword = Regex.Replace (keyword, @"[A-Za-z]+", match => Api.Equals (match.Value, "P") ? "Prime" : match.Value);
			header.Keyword = keyword;
			CatchMessageSystem.RemoveAll (message);
			List<WarframeMarketItem> items = warframe.WarframeMarket.Search (keyword);
			CommandItem commandItem = new CommandItem ();
			object[] commandItems = new object[] { commandItem };
			header.ResultNumber = items.Count;
			Config.Read ((ref Config config) => {
				header.MaxResultNumber = config.WarframeMarketMaxResultNumber;
				if (items.Count == 0) {
					message.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.NoResult].Header, header));
					CatchMessageSystem.Add (message, receivedMessage => {
						return ProcessBind (receivedMessage);
					});
					return;
				}
				if (items.Count == 1) {
					WarframeMarketSelectedItem (message, header, items[0], orderType);
					return;
				}
				items.Sort ();
				message.Reply (GetSelectItemInformation (config));
				CatchSelectItem (item => WarframeMarketSelectedItem (message, header, item, orderType));
			});
			void CatchSelectItem (Action<WarframeMarketItem> action) {
				CatchMessageSystem.Add (message, receivedMessage => {
					if (ProcessBind (receivedMessage)) {
						return true;
					}
					if (int.TryParse (receivedMessage, out int orderNumber)) {
						if (orderNumber > 0 && orderNumber <= items.Count) {
							action (items[orderNumber - 1]);
							return true;
						}
						Config.Read ((ref Config subConfig) => {
							receivedMessage.Reply (Localizer.Execute (subConfig.Commands.WarframeMarket[CommandIndexName.ItemOrderNumberError].Header, header));
						});
					}
					return false;
				});
			}
			bool ProcessBind (QMMessage<MessagePermissionLevel> receivedMessage) {
				if (receivedMessage.Text.StartsWith ("绑定")) {
					string bindKeyword = receivedMessage.Text.Substring (3);
					header.Keyword = bindKeyword;
					items = warframe.WarframeMarket.Search (bindKeyword);
					if (items.Count == 0) {
						return false;
					}
					if (items.Count == 1) {
						AddWarframeMarketTranslate (receivedMessage, keyword, items[0].ItemName);
						return true;
					}
					items.Sort ();
					header.ResultNumber = items.Count;
					Config.Read ((ref Config subConfig) => {
						message.Reply (GetSelectItemInformation (subConfig));
					});
					CatchSelectItem (item => AddWarframeMarketTranslate (receivedMessage, keyword, item.ItemName));
					return true;
				}
				return false;
			}
			StringBuilder GetSelectItemInformation (Config subConfig) {
				int i = 0;
				return WarframeStatus.GetInformation (
					null,
					subConfig.Commands.WarframeMarket[CommandIndexName.Default].Header, new object[] { header },
					subConfig.Commands.WarframeMarket[CommandIndexName.Default].Item, items, item => i < subConfig.WarframeMarketMaxResultNumber, item => {
						commandItem.OrderNumber = i + 1;
						commandItem.Name = item.Name;
						commandItem.ItemName = item.ItemName;
						i++;
						return commandItems;
					},
					subConfig.Commands.WarframeMarket[CommandIndexName.Default].Footer, new object[] { header }
				);
			}
		}
		static void WarframeMarketSelectedItem (QMMessage<MessagePermissionLevel> message, CommandHeader header, WarframeMarketItem item, WarframeMarketOrderType orderType) {
			Task.Run (() => {
				Config.Read ((ref Config config) => {
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
				if (header.PreSelectedMod >= 0) {
					if (header.PreSelectedMod > header.MaxModRank) {
						header.PreSelectedMod = header.MaxModRank;
					}
					WarframeMarketSelectedModRank (message, header, item, orderType, itemPage, header.PreSelectedMod);
					return;
				}
				Config.Read ((ref Config config) => {
					message.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.ModRankError].Header, header, item));
					CatchMessageSystem.Add (message, receivedMessage => {
						if (int.TryParse (receivedMessage, out int modRank)) {
							if (modRank >= 0 && modRank <= header.MaxModRank) {
								WarframeMarketSelectedModRank (message, header, item, orderType, itemPage, modRank);
								return true;
							}
							Config.Read ((ref Config subConfig) => {
								receivedMessage.Reply (Localizer.Execute (subConfig.Commands.WarframeMarket[CommandIndexName.ModRankError].Header, header, item));
							});
						}
						return false;
					});
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
			Config.Read ((ref Config config) => {
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
				object[] commandItems = new object[] { commandItem };
				WarframeStatus.GetInformation (
					stringBuilder,
					config.Commands.WarframeMarket[headerIndexName].Header, new object[] { header, item },
					config.Commands.WarframeMarket[headerIndexName].Item, orders, order => i < header.MaxResultNumber, order => {
						commandItem.OrderNumber = i + 1;
						commandItem.Platinum = order.Platinum;
						commandItem.Status = GetStatusText (order.User.Status);
						commandItem.Quantity = order.Quantity;
						commandItem.InGameName = order.User.InGameName;
						i++;
						return commandItems;
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
						Config.Read ((ref Config subConfig) => {
							receivedMessage.Reply (Localizer.Execute (subConfig.Commands.WarframeMarket[CommandIndexName.PrivateChatOrderNumberError].Header, header, item));
						});
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
			Config.Read ((ref Config config) => {
				if (header.MaxModRank == 0) {
					message.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.PrivateChatText].Header, header, item, order, order.User));
					return;
				}
				message.Reply (Localizer.Execute (config.Commands.WarframeMarket[CommandIndexName.PrivateChatTextByModRank].Header, header, item, order, order.User));
			});
		}

		public static void Wiki (QMMessage<MessagePermissionLevel> message, params string[] args) {
			string keyword = string.Join (" ", args);
			CommandHeader header = new CommandHeader () {
				Keyword = keyword
			};
			Config.Read ((ref Config config) => {
				message.Reply (Localizer.Execute (config.Commands.Wiki[CommandIndexName.StartQuery].Header, header));
			});
			CatchMessageSystem.RemoveAll (message);
			Http http = new Http ();
			HttpRequestInformation httpRequestInformation = new HttpRequestInformation () {
				Url = "https://warframe.huijiwiki.com/index.php",
				QueryStringParameters = {
					{ "search", HttpApi.UrlEncode (keyword) },
					{ "title",  "Special:Search" }
				}
			};
			HtmlDocument htmlDocument = HtmlDocument.Parse (http.Request (httpRequestInformation));
			List<HtmlElement> lis = htmlDocument.QuerySelectorAll (".mw-search-results li");
			if (lis.Count == 0) {
				Wiki (message, keyword, $"https://warframe.huijiwiki.com/wiki/{HttpApi.UrlEncode (keyword)}", htmlDocument);
				return;
			}
			List<WikiResult> results = new List<WikiResult> (lis.Count);
			foreach (HtmlElement li in lis) {
				HtmlElement a = li.QuerySelector (".mw-search-result-heading a");
				string title = a.GetAttribute ("title");
				if (title is null) {
					continue;
				}
				if (title.Contains (keyword)) {
					results.Add (new WikiResult (
						title,
						$"https://warframe.huijiwiki.com{a.GetAttribute ("href")}",
						li.GetElementByClassName ("searchresult").TextContent)
					);
				}
			}
			if (results.Count == 0) {
				Config.Read ((ref Config config) => {
					message.Reply (Localizer.Execute (config.Commands.Wiki[CommandIndexName.NoResult].Header, header));
				});
				return;
			}
			header.ResultNumber = results.Count;
			StringBuilder stringBuilder = new StringBuilder ();
			CommandItem commandItem = new CommandItem ();
			object[] commandItems = new object[] { commandItem };
			Config.Read ((ref Config config) => {
				int i = 0;
				header.MaxResultNumber = config.WikiMaxResultNumber;
				WarframeStatus.GetInformation (stringBuilder,
					config.Commands.Wiki[CommandIndexName.Default].Header, new object[] { header },
					config.Commands.Wiki[CommandIndexName.Default].Item, results, item => i < header.MaxResultNumber, item => {
						commandItem.OrderNumber = i + 1;
						commandItem.Title = item.Title;
						commandItem.Url = item.Url;
						commandItem.Description = item.Description;
						i++;
						return commandItems;
					},
					config.Commands.Wiki[CommandIndexName.Default].Footer, new object[] { header }
				);
			});
			message.Reply (stringBuilder);
			CatchMessageSystem.Add (message, receivedMessage => {
				if (int.TryParse (receivedMessage.Text, out int i)) {
					if (i >= 1 && i <= results.Count) {
						WikiResult wikiResult = results[i - 1];
						header.Keyword = wikiResult.Title;
						Config.Read ((ref Config config) => {
							message.Reply (Localizer.Execute (config.Commands.Wiki[CommandIndexName.StartQuery].Header, header));
						});
						Wiki (message, wikiResult.Title, wikiResult.Url, HtmlDocument.Parse (new Http ().Request (wikiResult.Url)));
					} else {
						Config.Read ((ref Config config) => {
							message.Reply (Localizer.Execute (config.Commands.Wiki[CommandIndexName.OrderNumberError].Header, header));
						});
					}
				}
				return false;
			});
		}
		static void Wiki (QMMessage<MessagePermissionLevel> message, string title, string url, HtmlDocument htmlDocument) {
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.AppendLine ($"Wiki {title}");
			stringBuilder.AppendLine (url);
			stringBuilder.Append (htmlDocument.QuerySelector (".mw-parser-output > p")?.TextContent);
			message.Reply (stringBuilder);
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
		public string ChatOrderType;
		public int PreSelectedMod = -1;
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
		public string Title;
		public string Url;
		public string Description;

	}

}