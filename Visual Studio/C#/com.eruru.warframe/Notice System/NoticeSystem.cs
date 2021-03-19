using System;
using System.Collections.Generic;
using Eruru.ReaderWriterLockHelper;

namespace com.eruru.warframe {

	public static class NoticeSystem {

		public static int StartTime { get; set; } = 6;
		public static int EndTime { get; set; } = 24;
		public static double MinimumInterval { get; set; } = Api.MinutesToMilliseconds (30);
		public static double MaximumInterval { get; set; } = Api.MinutesToMilliseconds (90);

		static readonly Random Random = new Random ();
		static readonly ReaderWriterLockHelper<List<Notice>> ReaderWriterLockHelper = new ReaderWriterLockHelper<List<Notice>> (new List<Notice> ());

		public static bool CanNotice () {
			return DateTime.Now.Hour >= StartTime && DateTime.Now.Hour < EndTime;
		}

		public static double GetInterval () {
			return MinimumInterval + (MaximumInterval - MinimumInterval) * Random.NextDouble ();
		}

		public static void Add (string id, Action action) {
			if (id is null) {
				throw new ArgumentNullException (nameof (id));
			}
			if (action is null) {
				throw new ArgumentNullException (nameof (action));
			}
			ReaderWriterLockHelper.Write ((ref List<Notice> notices) => {
				notices.Add (new Notice (id, action));
			});
		}

	}

}