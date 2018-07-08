using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Navigation;

namespace SandBeige.MealRecipes.Composition.Behaviors {
	/// <summary>
	/// ハイパーリンクナビゲートリクエスト時ブラウザ起動ビヘイビア
	/// </summary>
	public class HyperlinkNavigateBehavior : Behavior<Hyperlink> {
		protected override void OnAttached() {
			base.OnAttached();
			this.AssociatedObject.RequestNavigate += Navigate;
		}

		protected override void OnDetaching() {
			base.OnDetaching();
			this.AssociatedObject.RequestNavigate -= Navigate;
		}

		private static void Navigate(object sender, RequestNavigateEventArgs e) {
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
	}
}
