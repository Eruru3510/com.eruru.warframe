using System;
using Eruru.Json;

namespace com.eruru.warframe {

	class NewsReverser : IJsonConverter<WarframeStatusNews[], WarframeStatusNews[]> {

		public WarframeStatusNews[] Read (WarframeStatusNews[] value) {
			Array.Reverse (value);
			return value;
		}

		public WarframeStatusNews[] Write (WarframeStatusNews[] value) {
			throw new NotImplementedException ();
		}

	}

}