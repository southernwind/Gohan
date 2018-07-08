using Livet;

using Reactive.Bindings;

namespace SandBeige.MealRecipes.ViewModels {
	/// <summary>
	/// タブアイテム用ViewModel
	/// </summary>
	abstract class TabItemViewModelBase : ViewModel {
		/// <summary>
		/// アイテム名
		/// </summary>
		public abstract string Name {
			get;
		}

		/// <summary>
		/// アイテム選択状態
		/// </summary>
		public abstract ReactiveProperty<bool> IsSelected {
			get;
		}
	}
}
