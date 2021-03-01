using System;
using System.Collections.Generic;

namespace Eruru.Localizer {

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
	public class LocalizerField : Attribute {

		public bool HasConverter {

			get => Converters?.Length > 0;

		}
		public Type ConverterReadType {

			get => Converters[0].BeforeType;

		}

		static readonly Dictionary<int, LocalizerConverter> CachedConverters = new Dictionary<int, LocalizerConverter> ();

		LocalizerConverter[] Converters;

		public LocalizerField (params Type[] converters) {
			if (converters is null) {
				throw new ArgumentNullException (nameof (converters));
			}
			SetConverters (converters);
		}

		public object Read (object value) {
			if (HasConverter) {
				for (int i = 0; i < Converters.Length; i++) {
					value = Converters[i].Read (value);
				}
			}
			return value;
		}

		public object Write (object value) {
			if (HasConverter) {
				for (int i = Converters.Length - 1; i >= 0; i--) {
					value = Converters[i].Read (value);
				}
			}
			return value;
		}

		void SetConverters (Type[] converters) {
			if (converters is null) {
				throw new ArgumentNullException (nameof (converters));
			}
			Converters = Array.ConvertAll (converters, converter => {
				if (converter is null) {
					throw new ArgumentNullException (nameof (converter));
				}
				if (CachedConverters.TryGetValue (converter.GetHashCode (), out LocalizerConverter cachedConverter)) {
					return cachedConverter;
				}
				cachedConverter = new LocalizerConverter (converter);
				CachedConverters.Add (cachedConverter.GetHashCode (), cachedConverter);
				return cachedConverter;
			});
		}

	}

}