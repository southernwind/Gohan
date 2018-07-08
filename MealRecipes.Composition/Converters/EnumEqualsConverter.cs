using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	public class EnumEqualsConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null || parameter == null) {
				return false;
			}
			return value.Equals(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is bool b && b) {
				return parameter;
			}
			return DependencyProperty.UnsetValue;
		}
	}
}
