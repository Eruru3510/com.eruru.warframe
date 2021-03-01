using System;
using System.Text;
using Eruru.Localizer;

namespace com.eruru.warframe {

	public class RelativeTimeConverter : ILocalizerConverter<DateTime, StringBuilder> {

		public StringBuilder Read (DateTime value) {
			return Api.GetRelativeTimeText (value);
		}

		public DateTime Write (StringBuilder value) {
			throw new NotImplementedException ();
		}

	}

}