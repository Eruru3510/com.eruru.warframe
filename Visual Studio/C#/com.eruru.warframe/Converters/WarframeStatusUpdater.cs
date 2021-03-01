using System;
using Eruru.Json;

namespace com.eruru.warframe {

	class WarframeStatusUpdater : IJsonConverter<DateTime, DateTime> {

		public DateTime Read (DateTime value) {
			WarframeStatus.Add (value);
			return value;
		}

		public DateTime Write (DateTime value) {
			throw new NotImplementedException ();
		}

	}

}