using System;
using System.Text;
using Eruru.Json;

namespace com.eruru.warframe {

	public class ItemArrayTranslator : IJsonConverter<string[], string> {

		public string Read (string[] value) {
			StringBuilder stringBuilder = new StringBuilder ();
			for (int i = 0; i < value.Length; i++) {
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