using System;
using Eruru.Json;

namespace com.eruru.warframe {

	public class ItemTranslator : IJsonConverter<string, string> {

		public string Read (string value) {
			return TranslateSystem.TranslateItem (value);
		}

		public string Write (string value) {
			throw new NotImplementedException ();
		}

	}

}