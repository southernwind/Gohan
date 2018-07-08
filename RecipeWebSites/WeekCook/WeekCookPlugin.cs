using Livet;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.RecipeWebSites.WeekCook.Models;
using SandBeige.RecipeWebSites.WeekCook.ViewModels;

using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SandBeige.RecipeWebSites.WeekCook {
	/// <summary>
	/// クックパッドレシピプラグイン本体
	/// </summary>
	public class WeekCookRecipePlugin : NotificationObject, IRecipeSitePlugin {
		/// <summary>
		/// 対象URLパターン
		/// </summary>
		public Regex TargetUrlPattern {
			get;
		} = new Regex(@"^https://www.weekcook.jp/recipe/\d+/.*");

		/// <summary>
		/// ロゴURL
		/// </summary>
		public string LogoUrl {
			get;
		} = "https://www.weekcook.jp/common/images/header/logo.png";

		/// <summary>
		/// Model型
		/// </summary>
		public Type ModelType {
			get {
				return typeof(WeekCookRecipe);
			}
		}

		/// <summary>
		/// ViewModel型
		/// </summary>
		public Type ViewModelType {
			get {
				return typeof(WeekCookRecipeViewModel);
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
			return new WeekCookRecipeViewModel(settings, logger, this);
		}

		/// <summary>
		/// レシピViewModel生成メソッド
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="recipe"></param>
		/// <returns>レシピViewModelインスタンス</returns>
		public IRecipeViewModel CreateRecipeViewModelInstance(IBaseSettings settings, ILogger logger, IRecipe recipe) {
			return new WeekCookRecipeViewModel(settings, logger, (WeekCookRecipe)recipe);
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
			return new WeekCookRecipe(settings, logger, shoppingListOwner, this);
		}

		#endregion
	}
}
