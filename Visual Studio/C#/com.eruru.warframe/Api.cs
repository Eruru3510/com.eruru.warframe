using System;
using System.Collections.ObjectModel;
using System.Text;
using Eruru.QQMini.PluginSDKHelper;
using QQMini.PluginSDK.Core;
using QQMini.PluginSDK.Core.Model;

namespace com.eruru.warframe {

	static class Api {

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

		public static bool ContainKeywords (string text, string[] keywords) {
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

		public static bool ContainKeyword (string text, string keyword, out int index) {
			index = 0;
			if (text is null || keyword is null) {
				return text == keyword;
			}
			if (text.Length == 0 || keyword.Length == 0) {
				return text.Length == keyword.Length;
			}
			int i = 0;
			while (HasCharacter (text, ref i)) {
				int n = 0;
				int tempI = i;
				bool found = true;
				while (HasCharacter (keyword, ref n)) {
					if (tempI >= text.Length) {
						return false;
					}
					if (char.ToLower (text[tempI]) != char.ToLower (keyword[n])) {
						found = false;
						break;
					}
					tempI++;
					HasCharacter (text, ref tempI);
					n++;
				}
				if (found) {
					index = i;
					return true;
				}
				i++;
			}
			return false;
		}

		public static void BroadcastGroupMessage (string text) {
			if (text is null) {
				throw new ArgumentNullException (nameof (text));
			}
			BroadcastGroupMessage (new StringBuilder (text));
		}
		public static void BroadcastGroupMessage (StringBuilder stringBuilder) {
			if (stringBuilder is null) {
				throw new ArgumentNullException (nameof (stringBuilder));
			}
			Config.Read ((ref Config config) => {
				ReadOnlyCollection<QQ> robotQQs = QMApiV2.GetFrameAllOnlineQQ ();
				foreach (long robotQQ in robotQQs) {
					foreach (long group in config.Groups) {
						QMMessage<MessagePermissionLevel>.Send (QMMessageType.Group, robotQQ, group, default, stringBuilder);
					}
				}
			});
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