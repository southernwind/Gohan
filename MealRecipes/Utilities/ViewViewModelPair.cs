using System.ComponentModel;
using System.Windows.Controls;

namespace SandBeige.MealRecipes.Utilities {
	/// <summary>
	/// ViewとViewModelを保持するクラス
	/// TはViewModel型
	/// </summary>
	public class ViewViewModelPair<TView, TViewModel> where TView : Control where TViewModel : INotifyPropertyChanged {
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="view">View</param>
		/// <param name="viewModel">ViewModel</param>
		public ViewViewModelPair(TView view, TViewModel viewModel) {
			this.View = view;
			this.ViewModel = viewModel;
		}

		/// <summary>
		/// View
		/// </summary>
		public TView View {
			get;
		}

		/// <summary>
		/// ViewModel
		/// </summary>
		public TViewModel ViewModel {
			get;
		}
	}
}
