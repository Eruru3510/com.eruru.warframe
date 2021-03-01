using System;

namespace com.eruru.warframe {

	public class Translate : IComparable {

		public string Key;
		public string Value;

		internal int Index;
		internal string Text;

		public Translate () {

		}
		public Translate (string key, string value) {
			Key = key ?? throw new ArgumentNullException (nameof (key));
			Value = value ?? throw new ArgumentNullException (nameof (value));
		}
		internal Translate (string key, string value, int index, string text) {
			Key = key ?? throw new ArgumentNullException (nameof (key));
			Value = value ?? throw new ArgumentNullException (nameof (value));
			Index = index;
			Text = text ?? throw new ArgumentNullException (nameof (text));
		}

		public int CompareTo (object obj) {
			Translate translate = (Translate)obj;
			if (Index == translate.Index) {
				if (Text.Length == translate.Text.Length) {
					return Text.CompareTo (translate.Text);
				}
				return Text.Length.CompareTo (translate.Text.Length);
			}
			return Index.CompareTo (translate.Index);
		}

	}

}