using Microsoft.Win32;

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SandBeige.MealRecipes.CustomControls {
	public class ImageSelector : Button {
		#region Source 依存関係プロパティ

		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register(
				nameof(Source),
				typeof(BitmapSource),
				typeof(ImageSelector),
				new PropertyMetadata(null));

		/// <summary>
		/// 画像ソース
		/// </summary>
		public BitmapSource Source {
			get { return (BitmapSource)this.GetValue(SourceProperty); }
			set { this.SetValue(SourceProperty, value); }
		}

		#endregion

		#region Path 依存関係プロパティ

		public static readonly DependencyProperty PathProperty =
			DependencyProperty.Register(
				nameof(Path),
				typeof(string),
				typeof(ImageSelector),
				new PropertyMetadata(null,
					(sender, e) => {
						if (!(sender is ImageSelector imageSelector)) {
							return;
						}
						using (var ms = new FileStream(imageSelector.Path, FileMode.Open, FileAccess.Read)) {
							var bi = new BitmapImage();
							bi.BeginInit();
							bi.CacheOption = BitmapCacheOption.OnLoad;
							bi.StreamSource = ms;
							bi.EndInit();
							bi.Freeze();
							imageSelector.Source = bi;
						}
					}));

		/// <summary>
		/// 画像ファイルパス
		/// </summary>
		public string Path {
			get { return (string)this.GetValue(PathProperty); }
			set { this.SetValue(PathProperty, value); }
		}

		#endregion

		static ImageSelector() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageSelector), new FrameworkPropertyMetadata(typeof(ImageSelector)));
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			this.Click += (sender, e) => {
				var ofd = new OpenFileDialog {
					Title = "画像を選択",
					Filter = "画像ファイル(*.png,*.jpg,*.bmp,*.gif)|*.png;*.jpg;*.bmp;*.gif|すべてのファイル(*.*)|*.*"
				};
				if (ofd.ShowDialog() == true) {
					this.Path = ofd.FileName;
				}
			};
		}
	}
}
