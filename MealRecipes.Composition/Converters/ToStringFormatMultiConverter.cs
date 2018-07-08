using System;
using System.Globalization;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	/// <summary>
	/// Uri→Stringコンバータ
	/// </summary>
	public class ToStringFormatMultiConverter : IMultiValueConverter {
		/// <summary>
		/// </summary>
		/// <param name="values">
		/// [0] 変換するオブジェクト
		/// [1] フォーマット
		/// [2]以降は無視
		/// </param>
		/// <param name="targetType">未使用</param>
		/// <param name="parameter">未使用</param>
		/// <param name="culture">未使用</param>
		/// <returns></returns>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			dynamic target = values[0];
			if (values[1] is string format) {
				return target.ToString(format);
			}
			return null;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
