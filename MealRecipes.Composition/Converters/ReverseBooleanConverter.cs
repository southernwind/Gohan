using System;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	/// <summary>
	/// bool値反転コンバータ
	/// </summary>
	public class ReverseBooleanConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value is bool b) {
				return !b;
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value is bool b) {
				return !b;
			}
			return null;
		}
	}
}
