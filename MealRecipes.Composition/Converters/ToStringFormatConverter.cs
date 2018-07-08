using System;
using System.Globalization;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	/// <summary>
	/// Uri→Stringコンバータ
	/// </summary>
	public class ToStringFormatConverter : IValueConverter {
		/// <summary>
		/// </summary>
		/// <param name="value">変換するオブジェクト</param>
		/// <param name="targetType">未使用</param>
		/// <param name="parameter">フォーマット</param>
		/// <param name="culture">未使用</param>
		/// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (!(parameter is string format)) {
				return null;
			}
			dynamic target = value;
			return target?.ToString(format);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
