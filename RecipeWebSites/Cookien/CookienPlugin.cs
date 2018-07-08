using Livet;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.RecipeWebSites.Cookien.Models;
using SandBeige.RecipeWebSites.Cookien.ViewModels;

using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SandBeige.RecipeWebSites.Cookien {
	/// <summary>
	/// クックパッドレシピプラグイン本体
	/// </summary>
	public class CookienRecipePlugin : NotificationObject, IRecipeSitePlugin {
		/// <summary>
		/// 対象URLパターン
		/// </summary>
		public Regex TargetUrlPattern {
			get;
		} = new Regex(@"^https://cookien\.com/recipe/\d+");

		/// <summary>
		/// ロゴURL
		/// </summary>
		public string LogoUrl {
			get;
		} = "https://cookien.com/wp-content/themes/tsukuoki/img/tsukuoki_logo_pc.png";

		/// <summary>
		/// Model型
		/// </summary>
		public Type ModelType {
			get {
				return typeof(CookienRecipe);
			}
		}

		/// <summary>
		/// ViewModel型
		/// </summary>
		public Type ViewModelType {
			get {
				return typeof(CookienRecipeViewModel);
			}
		}

		#region View

		/// <summary>
		/// 表示用View
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="viewModel">DataContext</param>
		/// <returns></returns>
		public UserControl CreateRecipeEditorViewInstance(IBaseSettings settings, ILogger logger, IRecipeViewModel viewModel) {
			return new Views.RecipeEditor {
				DataContext = viewModel
			};
		}

		/// <summary>
		/// 編集用View
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="viewModel">DataContext</param>
		/// <returns></returns>
		public UserControl CreateRecipeViewerViewInstance(IBaseSettings settings, ILogger logger, IRecipeViewModel viewModel) {
			return new Views.RecipeViewer {
				DataContext = viewModel
			};
		}

		#endregion

		#region ViewModel

		/// <summary>
		/// レシピViewModel生成メソッド
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <returns>レシピViewModelインスタンス</returns>
		public IRecipeViewModel CreateRecipeViewModelInstance(IBaseSettings settings, ILogger logger) {
			return new CookienRecipeViewModel(settings, logger, this);
		}

		/// <summary>
		/// レシピViewModel生成メソッド
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="recipe"></param>
		/// <returns>レシピViewModelインスタンス</returns>
		public IRecipeViewModel CreateRecipeViewModelInstance(IBaseSettings settings, ILogger logger, IRecipe recipe) {
			return new CookienRecipeViewModel(settings, logger, (CookienRecipe)recipe);
		}

		#endregion

		#region Model

		/// <summary>
		/// レシピModel生成メソッド
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="shoppingListOwner">お買物リスト保持インスタンス</param>
		/// <returns>レシピModelインスタンス</returns>
		public IRecipe CreateRecipeModelInstance(IBaseSettings settings, ILogger logger, IShoppingListOwner shoppingListOwner) {
			return new CookienRecipe(settings, logger, shoppingListOwner, this);
		}

		#endregion
	}
}
