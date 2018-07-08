using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	/// <summary>
	/// string配列→カンマ区切り文字コンバータ
	/// </summary>
	public class StringArrayToCsvConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is string[] strArr) {
				return string.Join(",", strArr);
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is string str) {
				return str.Split(',').Select(x => x.Trim()).ToArray();
			}
			return value;
		}
	}
}
