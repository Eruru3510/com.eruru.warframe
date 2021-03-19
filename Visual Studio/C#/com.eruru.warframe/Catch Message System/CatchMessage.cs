using System;
using Eruru.QQMini.PluginSDKHelper;

namespace com.eruru.warframe {

	public class CatchMessage {

		public QMMessage<MessagePermissionLevel> Message { get; set; }
		public Func<QMMessage<MessagePermissionLevel>, bool> Func { get; set; }

		public CatchMessage (QMMessage<MessagePermissionLevel> message, Func<QMMessage<MessagePermissionLevel>, bool> func) {
			Message = message ?? throw new ArgumentNullException (nameof (message));
			Func = func ?? throw new ArgumentNullException (nameof (func));
		}

	}

}