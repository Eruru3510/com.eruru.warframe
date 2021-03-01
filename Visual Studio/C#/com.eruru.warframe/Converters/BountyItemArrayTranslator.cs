using System;
using System.Text;
using Eruru.Json;

namespace com.eruru.warframe {

	public class BountyItemArrayTranslator : IJsonConverter<string[], string> {

		static readonly string[] BlackList = {
			"Cryotic",
			"Credits Cache",
			"Endo",
			"Grokdrul",
			"Morphics",
			"Neural Sensors",
			"Lens",
			"Kuva",
			"Cetus Wisp",
			"Breath Of The Eidolon",
			"Pattern Mismatch. Results inaccurate.",
			"Pustulite"
		};

		public string Read (string[] value) {
			StringBuilder stringBuilder = new StringBuilder ();
			for (int i = 0; i < value.Length; i++) {
				if (Api.ContainKeywords (value[i], BlackList)) {
					continue;
				}
				if (stringBuilder.Length > 0) {
					stringBuilder.Append ('、');
				}
				stringBuilder.Append (TranslateSystem.TranslateItem (value[i]));
			}
			return stringBuilder.ToString ();
		}

		public string[] Write (string value) {
			throw new NotImplementedException ();
		}

	}

}