
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.MealRecipes.Models.Settings;
using SandBeige.MealRecipes.Utilities;

namespace SandBeige.MealRecipes.ViewModels.Recipe {
	/// <summary>
	/// </summary>
	sealed class OriginalRecipeDetailViewModel : RecipeDetailViewModel {
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public OriginalRecipeDetailViewModel(ISettings settings, ILogger logger) : base(settings, logger) {
			// Property
			var vm = Creator.CreateOriginalRecipeViewModelInstance(settings, logger);
			this.RecipeViewModel.Value = vm;
			this.RecipeView.Value = Creator.CreateRecipeEditorViewInstance(settings, logger, vm);
		}
	}
}
