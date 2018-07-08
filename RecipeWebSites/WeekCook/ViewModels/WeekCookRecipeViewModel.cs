
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.RecipeWebSites.WeekCook.Models;

namespace SandBeige.RecipeWebSites.WeekCook.ViewModels {
	/// <summary>
	/// クックパッドレシピViewModel
	/// 共有部分はベースクラスで定義し、独自部分のみこのクラスで定義する
	/// </summary>
	public class WeekCookRecipeViewModel : RecipeViewModelBase {
		/// <summary>
		/// レシピModel
		/// </summary>
		public new WeekCookRecipe Recipe {
			get {
				return base.Recipe as WeekCookRecipe;
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="plugin">親プラグイン</param>
		internal WeekCookRecipeViewModel(IBaseSettings settings, ILogger logger, IRecipeSitePlugin plugin) : this(settings, logger, new WeekCookRecipe(settings, logger, null, plugin)) {
		}

		/// <summary>
		/// コンストラクタ
		/// 共有部分はベースクラスで定義する
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="cookpadRecipe">クックパッドレシピ</param>
		internal WeekCookRecipeViewModel(IBaseSettings settings, ILogger logger, IRecipe cookpadRecipe) : base(settings, logger, cookpadRecipe) {
		}
	}
}
