namespace Eruru.Localizer {

	public interface ILocalizerConverter<Before, After> {

		After Read (Before value);

		Before Write (After value);

	}

}