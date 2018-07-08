using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SandBeige.MealRecipes.Composition.Converters {
	/// <summary>
	/// Byte[]→ImageSourceコンバータ
	/// </summary>
	public class BinaryToImageSourceConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (!(value is byte[] binary)) {
				return null;
			}
			using (var ms = new MemoryStream(binary)) {
				var bi = new BitmapImage();
				bi.BeginInit();
				bi.CacheOption = BitmapCacheOption.OnLoad;
				bi.StreamSource = ms;
				bi.EndInit();
				bi.Freeze();
				return bi;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (!(value is BitmapSource bs)) {
				return null;
			}
			var encoder = new PngBitmapEncoder();
			using (var stream = new MemoryStream()) {
				encoder.Frames.Add(BitmapFrame.Create(bs));
				encoder.Save(stream);
				return stream.ToArray();
			}
		}
	}
}
