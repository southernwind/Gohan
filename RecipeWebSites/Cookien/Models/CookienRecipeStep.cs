
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;

namespace SandBeige.RecipeWebSites.Cookien.Models {
	/// <summary>
	/// レシピ手順
	/// </summary>
	public class CookienRecipeStep : RecipeStepBase {
		public CookienRecipeStep(IBaseSettings settings, ILogger logger) : base(settings, logger) {

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
