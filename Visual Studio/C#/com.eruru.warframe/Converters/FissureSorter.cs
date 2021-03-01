using System;
using Eruru.Json;

namespace com.eruru.warframe {

	class FissureSorter : IJsonConverter<Array, Array> {

		public Array Read (Array value) {
			Array.Sort (value);
			return value;
		}

		public Array Write (Array value) {
			throw new NotImplementedException ();
		}

	}

}
