using System;
using System.Timers;

namespace com.eruru.warframe {

	public class Notice {

		public string Id { get; set; }
		public Action Action { get; set; }

		readonly Timer Timer = new Timer () {
			AutoReset = false
		};

		double TimerInterval {

			set {
				Timer.Interval = value;
				Timer.Enabled = true;
			}

		}

		public Notice (string id, Action action) {
			if (id is null) {
				throw new ArgumentNullException (nameof (id));
			}
			if (action is null) {
				throw new ArgumentNullException (nameof (action));
			}
			Id = id;
			Action = action;
			Timer.Elapsed += (sender, e) => {
				TimerInterval = NoticeSystem.GetInterval ();
				if (NoticeSystem.CanNotice ()) {
					Console.Beep ();
					Action ();
				}
			};
			TimerInterval = NoticeSystem.GetInterval ();
		}

	}

}