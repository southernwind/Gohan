using Livet;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.RecipeWebSites.Rakuten.Models;
using SandBeige.RecipeWebSites.Rakuten.ViewModels;

using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SandBeige.RecipeWebSites.Rakuten {
	/// <summary>
	/// クックパッドレシピプラグイン本体
	/// </summary>
	public class RakutenRecipePlugin : NotificationObject, IRecipeSitePlugin {
		/// <summary>
		/// 対象URLパターン
		/// </summary>
		public Regex TargetUrlPattern {
			get;
		} = new Regex(@"^https://recipe\.rakuten\.co\.jp/recipe/\d+/");

		/// <summary>
		/// ロゴURL
		/// </summary>
		public string LogoUrl {
			get;
		} = "https://image.recipe.rakuten.co.jp/pc/logo_rakuten_recipe5.gif";

		/// <summary>
		/// Model型
		/// </summary>
		public Type ModelType {
			get {
				return typeof(RakutenRecipe);
			}
		}

		/// <summary>
		/// ViewModel型
		/// </summary>
		public Type ViewModelType {
			get {
				return typeof(RakutenRecipeViewModel);
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
			return new RakutenRecipeViewModel(settings, logger, this);
		}

		/// <summary>
		/// レシピViewModel生成メソッド
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="recipe"></param>
		/// <returns>レシピViewModelインスタンス</returns>
		public IRecipeViewModel CreateRecipeViewModelInstance(IBaseSettings settings, ILogger logger, IRecipe recipe) {
			return new RakutenRecipeViewModel(settings, logger, (RakutenRecipe)recipe);
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
			return new RakutenRecipe(settings, logger, shoppingListOwner, this);
		}

		#endregion
	}
}
