using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	public class DoubleComputeConverter : IMultiValueConverter {
		/// <summary>
		/// </summary>
		/// <param name="values">計算に使用する値</param>
		/// <param name="targetType">未使用</param>
		/// <param name="parameter">計算式</param>
		/// <param name="culture">未使用</param>
		/// <returns></returns>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if (!(parameter is string formula) || !values.All(val => val is double d && !double.IsNaN(d) || val is int)) {
				return null;
			}
			var dt = new DataTable();
			var result = dt.Compute(string.Format(formula, values), "");

			if (!double.TryParse(result.ToString(), out var doubleResult)) {
				return null;
			}
			return doubleResult;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
