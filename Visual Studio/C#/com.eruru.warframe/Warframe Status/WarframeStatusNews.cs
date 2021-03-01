using System;
using Eruru.Localizer;

namespace com.eruru.warframe {

	public class WarframeStatusNews {

		public string Message;
		public string Link;
		[LocalizerField (typeof (RelativeTimeConverter))]
		public DateTime Date;

	}

}