using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Eruru.Localizer {

	class LocalizerConverter {

		public readonly Type BeforeType;

		readonly object Instance;
		readonly Type Type;
		readonly Type AfterType;
		readonly MethodInfo ReadMethod;
		readonly MethodInfo WriteMethod;
		readonly object[] ReadParameters = new object[1];
		readonly object[] WriteParameters = new object[1];

		public LocalizerConverter (Type type) {
			Type = type ?? throw new ArgumentNullException (nameof (type));
			Type interfaceType = Type.GetInterface (typeof (ILocalizerConverter<object, object>).Name);
			if (interfaceType is null) {
				throw new Exception ($"{type}需要实现{typeof (ILocalizerConverter<object, object>)}接口");
			}
			Type[] types = interfaceType.GetGenericArguments ();
			BeforeType = types[0];
			AfterType = types[1];
			ReadMethod = interfaceType.GetMethod (nameof (ILocalizerConverter<object, object>.Read), new Type[] { BeforeType });
			WriteMethod = interfaceType.GetMethod (nameof (ILocalizerConverter<object, object>.Write), new Type[] { AfterType });
			Instance = FormatterServices.GetUninitializedObject (type);
		}

		public object Read (object value) {
			ReadParameters[0] = Convert.ChangeType (value, BeforeType);
			return Convert.ChangeType (ReadMethod.Invoke (Instance, ReadParameters), ReadMethod.ReturnType);
		}

		public object Write (object value) {
			WriteParameters[0] = Convert.ChangeType (value, AfterType);
			return Convert.ChangeType (WriteMethod.Invoke (Instance, WriteParameters), WriteMethod.ReturnType);
		}

		public override int GetHashCode () {
			return Type.GetHashCode ();
		}

	}

}