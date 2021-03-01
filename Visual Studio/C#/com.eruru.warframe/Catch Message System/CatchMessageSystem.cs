using System;
using System.Collections.Generic;
using Eruru.QQMini.PluginSDKHelper;
using Eruru.ReaderWriterLockHelper;

namespace com.eruru.warframe {

	static class CatchMessageSystem {

		public static long CatchTimeout { get; set; } = Api.MinutesToMilliseconds (10);

		static readonly ReaderWriterLockHelper ReaderWriterLockHelper = new ReaderWriterLockHelper ();
		static readonly List<KeyValuePair<QMMessage<MessagePermissionLevel>, Func<QMMessage<MessagePermissionLevel>, bool>>> Messages = new List<KeyValuePair<QMMessage<MessagePermissionLevel>, Func<QMMessage<MessagePermissionLevel>, bool>>> ();

		public static void Add (QMMessage<MessagePermissionLevel> message, Func<QMMessage<MessagePermissionLevel>, bool> func) {
			if (message is null) {
				throw new ArgumentNullException (nameof (message));
			}
			if (func is null) {
				throw new ArgumentNullException (nameof (func));
			}
			ReaderWriterLockHelper.Write (() => {
				Messages.Add (new KeyValuePair<QMMessage<MessagePermissionLevel>, Func<QMMessage<MessagePermissionLevel>, bool>> (message, func));
			});
		}

		public static void Execute (QMMessage<MessagePermissionLevel> message) {
			if (message is null) {
				throw new ArgumentNullException (nameof (message));
			}
			ReaderWriterLockHelper.Read (() => {
				int lastTimeoutIndex = -1;
				for (int i = 0; i < Messages.Count; i++) {
					if (Messages[i].Key.DateTime.AddMilliseconds (CatchTimeout) < DateTime.Now) {
						lastTimeoutIndex = i;
						continue;
					}
					if (Messages[i].Key.Group == message.Group && Messages[i].Key.QQ == message.QQ) {
						if (Messages[i].Value.Invoke (message)) {
							ReaderWriterLockHelper.Write (() => {
								Messages.RemoveAt (i--);
							});
							break;
						}
					}
				}
				if (lastTimeoutIndex > -1) {
					ReaderWriterLockHelper.Write (() => {
						Messages.RemoveRange (0, lastTimeoutIndex + 1);
					});
				}
			});
		}

		public static void Clear (QMMessage<MessagePermissionLevel> message) {
			if (message is null) {
				throw new ArgumentNullException (nameof (message));
			}
			ReaderWriterLockHelper.Write (() => {
				Messages.RemoveAll (current => current.Key.Group == message.Group && current.Key.QQ == message.QQ);
			});
		}

	}

}