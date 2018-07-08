using System;
using System.Data.SqlClient;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	/// <summary>
	/// SortOrder→Stringコンバータ
	/// </summary>
	public class SortOrderToStringConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (!(value is SortOrder so)) {
				return null;
			}
			switch (so) {
				case SortOrder.Ascending:
					return "昇順";
				case SortOrder.Descending:
					return "降順";
				case SortOrder.Unspecified:
					return null;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
