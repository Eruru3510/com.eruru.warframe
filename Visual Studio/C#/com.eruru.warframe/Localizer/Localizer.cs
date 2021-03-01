using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Eruru.Localizer {

	public static class Localizer {

		static readonly BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

		public static StringBuilder Execute (StringBuilder stringBuilder, string text, params object[] instances) {
			if (stringBuilder is null) {
				stringBuilder = new StringBuilder ();
			}
			return Execute (stringBuilder, text, null, instances);
		}
		public static StringBuilder Execute (StringBuilder stringBuilder, string text, Dictionary<string, object> dictionary) {
			if (stringBuilder is null) {
				stringBuilder = new StringBuilder ();
			}
			return Execute (stringBuilder, text, dictionary, null);
		}
		public static StringBuilder Execute (string text, params object[] instances) {
			return Execute (null, text, null, instances);
		}
		public static StringBuilder Execute (string text, Dictionary<string, object> dictionary) {
			return Execute (null, text, dictionary, null);
		}

		static StringBuilder Execute (StringBuilder stringBuilder, string text, Dictionary<string, object> dictionary, params object[] instances) {
			if (stringBuilder is null) {
				stringBuilder = new StringBuilder ();
			}
			if (text is null) {
				return stringBuilder;
			}
			if (dictionary is null && instances is null) {
				stringBuilder.Append (text);
				return stringBuilder;
			}
			for (int i = 0; i < text.Length; i++) {
				if (text[i] == '[') {
					i++;
					string name = ReadTo (text, ref i, ']');
					if (dictionary is null) {
						stringBuilder.Append (GetValue (name, instances) ?? $"[{name}]");
						continue;
					}
					if (dictionary.TryGetValue (name, out object value)) {
						stringBuilder.Append (value);
						continue;
					}
					stringBuilder.Append ($"[{name}]");
					continue;
				}
				stringBuilder.Append (text[i]);
			}
			return stringBuilder;
		}

		static string ReadTo (string text, ref int i, char end) {
			StringBuilder stringBuilder = new StringBuilder ();
			for (; i < text.Length; i++) {
				if (text[i] == end) {
					return stringBuilder.ToString ();
				}
				stringBuilder.Append (text[i]);
			}
			throw new Exception ($"没有结束");
		}

		static object GetValue (string name, object[] instances) {
			foreach (object instance in instances) {
				MemberInfo[] memberInfos = instance.GetType ().GetMember (name, MemberTypes.Field | MemberTypes.Property, BindingFlags);
				if (memberInfos.Length == 0) {
					continue;
				}
				switch (memberInfos[0].MemberType) {
					case MemberTypes.Field: {
						FieldInfo fieldInfo = (FieldInfo)memberInfos[0];
						LocalizerField field = fieldInfo.GetCustomAttribute<LocalizerField> ();
						object value = fieldInfo.GetValue (instance);
						return field?.Read (value) ?? value;
					}
					case MemberTypes.Property: {
						PropertyInfo propertyInfo = (PropertyInfo)memberInfos[0];
						LocalizerField field = propertyInfo.GetCustomAttribute<LocalizerField> ();
						object value = propertyInfo.GetValue (instance, null);
						return field?.Read (value) ?? value;
					}
				}
			}
			return null;
		}

	}

}