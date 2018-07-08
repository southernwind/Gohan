using System;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	/// <summary>
	/// Uri→Stringコンバータ
	/// </summary>
	public class UriToStringConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value is Uri uri) {
				return uri.ToString();
			}
			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value is string urlText) {
				return new Uri(urlText);
			}
			return null;
		}
	}
}
