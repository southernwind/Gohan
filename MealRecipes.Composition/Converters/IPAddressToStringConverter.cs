using System;
using System.Net;
using System.Windows.Data;

namespace SandBeige.MealRecipes.Composition.Converters {
	/// <summary>
	/// IPAddress→Stringコンバータ
	/// </summary>
	public class IPAddressToStringConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value is IPAddress ip) {
				return ip.ToString();
			}
			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value is string ipText) {
				if (IPAddress.TryParse(ipText, out var ip)) {
					return ip;
				}
			}
			return null;
		}
	}
}
