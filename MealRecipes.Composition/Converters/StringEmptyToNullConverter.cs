using System;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	/// <summary>
	/// 数値→Stringコンバータ
	/// </summary>
	public class StringEmptyToNullConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (!(value is string str)) {
				return null;
			}
			if (string.IsNullOrEmpty(str)) {
				return null;
			}
			return value;
		}
	}
}
