using Reactive.Bindings;

namespace SandBeige.MealRecipes.ViewModels.Settings {
	abstract class SettingsPageViewModelBase : TabItemViewModelBase {
		/// <summary>
		/// 値検証結果
		/// </summary>
		public abstract ReactiveProperty<bool> IsValidated {
			get;
		}
	}
}
