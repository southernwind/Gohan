
using SandBeige.MealRecipes.Composition.Recipe;

namespace SandBeige.RecipeWebSites.Original.Models {
	/// <summary>
	/// 材料
	/// </summary>
	public class OriginalRecipeIngredient : RecipeIngredientBase {
		public OriginalRecipeIngredient(IRecipe recipe) : base(recipe) {
		}
		/// <summary>
		/// 拡張プロパティコレクションへ登録
		/// </summary>
		public override void RegisterToExtensionPropertyCollection() {
		}

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		public override void ReadFromExtensionPropertyCollection() {
		}
	}
}
