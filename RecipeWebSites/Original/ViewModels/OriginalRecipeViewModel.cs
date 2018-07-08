using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.RecipeWebSites.Original.Models;

namespace SandBeige.RecipeWebSites.Original.ViewModels {
	public class OriginalRecipeViewModel : RecipeViewModelBase {
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="plugin">親プラグイン</param>
		internal OriginalRecipeViewModel(IBaseSettings settings, ILogger logger, IRecipeSitePlugin plugin) : this(settings, logger, new OriginalRecipe(settings, logger, null, plugin)) {
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="recipe">レシピ</param>
		internal OriginalRecipeViewModel(IBaseSettings settings, ILogger logger, IRecipe recipe) : base(settings, logger, recipe) {
		}
	}
}
