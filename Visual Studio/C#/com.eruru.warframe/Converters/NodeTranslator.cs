using System;
using Eruru.Json;

namespace com.eruru.warframe {

	public class NodeTranslator : IJsonConverter<string, string> {

		public string Read (string value) {
			return TranslateSystem.TranslateNode (value);
		}

		public string Write (string value) {
			throw new NotImplementedException ();
		}

	}

}