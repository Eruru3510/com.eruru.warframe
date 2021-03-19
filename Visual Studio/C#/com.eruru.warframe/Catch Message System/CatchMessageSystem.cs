using System;
using System.Collections.Generic;
using Eruru.QQMini.PluginSDKHelper;
using Eruru.ReaderWriterLockHelper;

namespace com.eruru.warframe {

	static class CatchMessageSystem {

		public static long CatchTimeout { get; set; } = Api.MinutesToMilliseconds (10);

		static readonly ReaderWriterLockHelper<List<CatchMessage>> ReaderWriterLockHelper = new ReaderWriterLockHelper<List<CatchMessage>> (new List<CatchMessage> ());

		public static void Add (QMMessage<MessagePermissionLevel> message, Func<QMMessage<MessagePermissionLevel>, bool> func) {
			if (message is null) {
				throw new ArgumentNullException (nameof (message));
			}
			if (func is null) {
				throw new ArgumentNullException (nameof (func));
			}
			ReaderWriterLockHelper.Write ((ref List<CatchMessage> catchMessages) => {
				catchMessages.Add (new CatchMessage (message, func));
			});
		}

		public static void Execute (QMMessage<MessagePermissionLevel> message) {
			if (message is null) {
				throw new ArgumentNullException (nameof (message));
			}
			ReaderWriterLockHelper.Read ((ref List<CatchMessage> catchMessages) => {
				int lastTimeoutIndex = -1;
				for (int i = 0; i < catchMessages.Count; i++) {
					if (catchMessages[i].Message.DateTime.AddMilliseconds (CatchTimeout) < DateTime.Now) {
						lastTimeoutIndex = i;
						continue;
					}
					if (catchMessages[i].Message.Group == message.Group && catchMessages[i].Message.QQ == message.QQ) {
						if (catchMessages[i].Func (message)) {
							ReaderWriterLockHelper.Write ((ref List<CatchMessage> subCatchMessages) => {
								subCatchMessages.RemoveAt (i--);
							});
							break;
						}
					}
				}
				if (lastTimeoutIndex > -1) {
					ReaderWriterLockHelper.Write ((ref List<CatchMessage> subCatchMessages) => {
						subCatchMessages.RemoveRange (0, lastTimeoutIndex + 1);
					});
				}
			});
		}

		public static void RemoveAll (QMMessage<MessagePermissionLevel> message) {
			if (message is null) {
				throw new ArgumentNullException (nameof (message));
			}
			ReaderWriterLockHelper.Write ((ref List<CatchMessage> catchMessages) => {
				catchMessages.RemoveAll (current => current.Message.Group == message.Group && current.Message.QQ == message.QQ);
			});
		}

	}

}