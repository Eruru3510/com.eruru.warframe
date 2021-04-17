using System;
using System.Text;
using Eruru.Json;

namespace com.eruru.warframe {

	public class BountyItemArrayTranslator : IJsonConverter<string[], string> {

		public string Read (string[] value) {
			StringBuilder stringBuilder = new StringBuilder ();
			Config.Read ((ref Config config) => {
				for (int i = 0; i < value.Length; i++) {
					if (Api.ContainAnyKeyword (value[i], config.BountyItemBlacklistKeywords)) {
						continue;
					}
					if (stringBuilder.Length > 0) {
						stringBuilder.Append ('、');
					}
					stringBuilder.Append (TranslateSystem.TranslateItem (value[i]));
				}
			});
			return stringBuilder.ToString ();
		}

		public string[] Write (string value) {
			throw new NotImplementedException ();
		}

	}

}