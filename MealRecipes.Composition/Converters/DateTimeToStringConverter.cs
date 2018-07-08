using System;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	/// <summary>
	/// DateTime→Stringコンバータ
	/// </summary>
	public class DateTimeToStringConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value is DateTime datetime) {
				return datetime.ToString("yyyy/MM/dd");
			}
			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (!(value is string str)) {
				return null;
			}
			if (string.IsNullOrEmpty(str)) {
				return null;
			}
			return DateTime.ParseExact(str, "yyyy/M/d", null);
		}
	}
}
