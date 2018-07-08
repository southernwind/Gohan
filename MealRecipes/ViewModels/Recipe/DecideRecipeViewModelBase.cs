using Livet;

using Reactive.Bindings;

using SandBeige.MealRecipes.Composition.Recipe;

namespace SandBeige.MealRecipes.ViewModels.Recipe {
	abstract class DecideRecipeViewModelBase : ViewModel {
		/// <summary>
		/// 決定したレシピ
		/// </summary>
		public abstract ReactiveProperty<IRecipeViewModel> DecidedRecipe {
			get;
		}
	}
}
