using Livet;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Recipe.Search;
using SandBeige.RecipeWebSites.Cookpad.Models;

namespace SandBeige.RecipeWebSites.Cookpad.ViewModels {
	/// <summary>
	/// アドバイス検索ViewModel
	/// </summary>
	public class AdviceSearchViewModel : ViewModel, IRecipeSearchConditionViewModel {
		/// <summary>
		/// Model
		/// </summary>
		public IRecipeSearchConditionModel RecipeSearchCondition {
			get;
		}

		public ReactiveProperty<string> Advice {
			get;
		}

		internal AdviceSearchViewModel(AdviceSearch adviceSearch) {
			this.RecipeSearchCondition = adviceSearch;
			this.Advice = adviceSearch.Advice.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
		}
	}
}
