using System;

namespace com.eruru.warframe {

	struct WikiResult {

		public string Title;
		public string Url;
		public string Description;

		public WikiResult (string title, string url, string description) {
			Title = title ?? throw new ArgumentNullException (nameof (title));
			Url = url ?? throw new ArgumentNullException (nameof (url));
			Description = description ?? throw new ArgumentNullException (nameof (description));
		}

	}

}