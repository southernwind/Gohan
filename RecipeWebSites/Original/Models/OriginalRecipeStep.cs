
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;

namespace SandBeige.RecipeWebSites.Original.Models {
	/// <summary>
	/// レシピ手順
	/// </summary>
	public class OriginalRecipeStep : RecipeStepBase {
		public OriginalRecipeStep(IBaseSettings settings, ILogger logger) : base(settings, logger) {
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
