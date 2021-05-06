using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Eruru.QQMini.PluginSDKHelper;
using QQMini.PluginSDK.Core;
using QQMini.PluginSDK.Core.Model;

namespace com.eruru.warframe {

	static class Api {

		static readonly BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

		public static StringBuilder FormatMillisecond (long milliseconds) {
			return FormatTimeSpan (new TimeSpan (milliseconds * 10000));
		}

		public static StringBuilder GetTimeText (DateTime dateTime) {
			StringBuilder stringBuilder = new StringBuilder ();
			if (dateTime.Year != 0) {
				stringBuilder.Append ($"{dateTime.Year}年");
			}
			if (dateTime.Month != 0) {
				stringBuilder.Append ($"{dateTime.Month}月");
			}
			if (dateTime.Day != 0) {
				stringBuilder.Append ($"{dateTime.Day}日");
			}
			if (dateTime.Hour != 0) {
				stringBuilder.Append ($"{dateTime.Hour}时");
			}
			if (dateTime.Minute != 0) {
				stringBuilder.Append ($"{dateTime.Minute}分");
			}
			if (dateTime.Second != 0) {
				stringBuilder.Append ($"{dateTime.Second}秒");
			}
			return stringBuilder;
		}

		public static StringBuilder GetRelativeTimeText (DateTime dateTime) {
			return FormatTimeSpan (dateTime - DateTime.Now);
		}

		public static StringBuilder FormatTimeSpan (TimeSpan timeSpan) {
			StringBuilder stringBuilder = new StringBuilder ();
			if (timeSpan.Days != 0) {
				stringBuilder.Append ($"{Math.Abs (timeSpan.Days)}天");
			}
			if (timeSpan.Hours != 0) {
				stringBuilder.Append ($"{Math.Abs (timeSpan.Hours)}时");
			}
			if (timeSpan.Minutes != 0) {
				stringBuilder.Append ($"{Math.Abs (timeSpan.Minutes)}分");
			}
			if (timeSpan.Seconds != 0) {
				stringBuilder.Append ($"{Math.Abs (timeSpan.Seconds)}秒");
			}
			return stringBuilder;
		}

		public static long MinutesToMilliseconds (long minutes) {
			return minutes * 60 * 1000;
		}

		public static long HoursToMillisecond (long hours) {
			return hours * 60 * 60 * 1000;
		}

		public static bool Equals (string a, string b) {
			return string.Equals (a, b, StringComparison.OrdinalIgnoreCase);
		}

		public static bool ContainAnyKeyword (string text, IEnumerable<string> keywords) {
			if (text is null) {
				throw new ArgumentNullException (nameof (text));
			}
			if (keywords is null) {
				throw new ArgumentNullException (nameof (keywords));
			}
			foreach (string keyword in keywords) {
				if (text.Contains (keyword)) {
					return true;
				}
			}
			return false;
		}

		public static bool ContainKeyword (string text, string keyword, out int index, out bool fullMatch) {
			index = 0;
			fullMatch = false;
			if (text is null || keyword is null) {
				return text == keyword;
			}
			if (text.Length == 0 || keyword.Length == 0) {
				return text.Length == keyword.Length;
			}
			int i = 0;
			int count = 0;
			bool matchHead = false;
			while (HasCharacter (text, ref i)) {
				int n = 0;
				int tempI = i;
				bool found = true;
				count++;
				while (HasCharacter (keyword, ref n)) {
					if (tempI >= text.Length) {
						return false;
					}
					if (char.ToUpperInvariant (text[tempI]) != char.ToUpperInvariant (keyword[n])) {
						found = false;
						break;
					}
					if (count == 1) {
						matchHead = true;
					}
					tempI++;
					HasCharacter (text, ref tempI);
					n++;
				}
				if (found) {
					index = i;
					if (tempI >= text.Length && matchHead) {
						fullMatch = true;
					}
					return true;
				}
				i++;
			}
			return false;
		}

		public static List<QMMessage> BroadcastGroupMessage (string text, BroadcastMessageType messageType = BroadcastMessageType.Text) {
			if (text is null) {
				throw new ArgumentNullException (nameof (text));
			}
			return BroadcastGroupMessage (new StringBuilder (text), messageType);
		}
		public static List<QMMessage> BroadcastGroupMessage (StringBuilder stringBuilder, BroadcastMessageType messageType = BroadcastMessageType.Text) {
			if (stringBuilder is null) {
				throw new ArgumentNullException (nameof (stringBuilder));
			}
			List<QMMessage> messages = new List<QMMessage> ();
			Config.Read ((ref Config config) => {
				ReadOnlyCollection<QQ> robotQQs = QMApiV2.GetFrameAllOnlineQQ ();
				foreach (AuthorizeGroup authorizeGroup in config.AuthorizeGroups) {
					if (authorizeGroup.IsExpiry) {
						continue;
					}
					switch (messageType) {
						case BroadcastMessageType.Text: {
							foreach (long robotQQ in robotQQs) {
								Message message = QMMessage.Send (QMMessageType.Group, robotQQ, authorizeGroup.Id, default, stringBuilder);
								messages.Add (new QMMessage (QMMessageType.Group, robotQQ, authorizeGroup.Id, default, (int)message.Id, message.Number, message.Text));
							}
							break;
						}
						case BroadcastMessageType.Json: {
							foreach (long robotQQ in robotQQs) {
								Message message = QMMessage.Send (QMMessageType.GroupJson, robotQQ, authorizeGroup.Id, default, stringBuilder);
								if (message is null) {
									break;
								}
								messages.Add (new QMMessage (QMMessageType.GroupJson, robotQQ, authorizeGroup.Id, default, (int)message.Id, message.Number, message.Text));
							}
							break;
						}
						default:
							throw new NotImplementedException ();
					}
				}
			});
			return messages;
		}

		public static bool CanNotice () {
			bool can = false;
			Config.Read ((ref Config config) => {
				can = DateTime.Now.Hour >= config.StartNoticeTime && DateTime.Now.Hour < config.EndNoticeTime;
			});
			return can;
		}

		public static bool IsAuthorizedGroup (long group) {
			bool isAuthorizedGroup = false;
			Config.Read ((ref Config config) => {
				AuthorizeGroup authorizeGroup = config.AuthorizeGroups.Find (value => value.Id == group);
				if (authorizeGroup?.IsExpiry ?? true) {
					return;
				}
				isAuthorizedGroup = true;
			});
			return isAuthorizedGroup;
		}

		public static T ShallowCopy<T> (T instance) where T : class {
			if (instance is null) {
				return null;
			}
			T newInstance = CreateInstance<T> ();
			foreach (FieldInfo fieldInfo in typeof (T).GetFields (BindingFlags)) {
				fieldInfo.SetValue (newInstance, fieldInfo.GetValue (instance));
			}
			return newInstance;
		}

		static T CreateInstance<T> () {
			Type type = typeof (T);
			if (type.GetConstructor (Type.EmptyTypes) == null) {
				return (T)FormatterServices.GetUninitializedObject (type);
			}
			return (T)Activator.CreateInstance (type);
		}

		static bool HasCharacter (string text, ref int i) {
			if (text is null) {
				throw new ArgumentNullException (nameof (text));
			}
			for (; i < text.Length; i++) {
				if (char.IsWhiteSpace (text[i])) {
					continue;
				}
				return true;
			}
			return false;
		}

	}

}