using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	public class ContainsConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if (!(values[1] is IEnumerable)) {
				return false;
			}
			return Enumerable.Contains((dynamic)values[1], values[0]);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
