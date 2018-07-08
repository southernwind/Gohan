using System.ComponentModel;

namespace SandBeige.MealRecipes.Composition.Recipe.Search {
	/// <summary>
	/// 検索条件ViewModel
	/// </summary>
	public interface IRecipeSearchConditionViewModel : INotifyPropertyChanged {
		/// <summary>
		/// 検索条件Model
		/// </summary>
		IRecipeSearchConditionModel RecipeSearchCondition {
			get;
		}
	}
}
