
using Reactive.Bindings;

namespace SandBeige.MealRecipes.Composition.Recipe {
	/// <summary>
	/// お買物リスト保有インターフェイス
	/// </summary>
	public interface IShoppingListOwner {
		/// <summary>
		/// お買物リスト
		/// </summary>
		ReactiveCollection<IRecipeIngredient> ShoppingList {
			get;
		}
	}
}
