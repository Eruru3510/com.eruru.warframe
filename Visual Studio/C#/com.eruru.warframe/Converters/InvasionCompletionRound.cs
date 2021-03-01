using System;
using Eruru.Json;

namespace com.eruru.warframe {

	class InvasionCompletionRound : IJsonConverter<float, float> {

		public float Read (float value) {
			return (float)Math.Round (value, 1);
		}

		public float Write (float value) {
			throw new NotImplementedException ();
		}

	}

}