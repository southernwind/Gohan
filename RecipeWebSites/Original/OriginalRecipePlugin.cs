
using Livet;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.RecipeWebSites.Original.Models;
using SandBeige.RecipeWebSites.Original.ViewModels;

using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SandBeige.RecipeWebSites.Original {
	/// <summary>
	/// オリジナルレシピプラグイン本体
	/// </summary>
	public class OriginalRecipePlugin : NotificationObject, IRecipeSitePlugin {
		/// <summary>
		/// 対象URLパターン
		/// </summary>
		public Regex TargetUrlPattern {
			get;
		} = null;

		/// <summary>
		/// ロゴURL
		/// </summary>
		public string LogoUrl {
			get;
		}

		/// <summary>
		/// Model型
		/// </summary>
		public Type ModelType {
			get {
				return typeof(OriginalRecipe);
			}
		}

		/// <summary>
		/// ViewModel型
		/// </summary>
		public Type ViewModelType {
			get {
				return typeof(OriginalRecipeViewModel);
			}
		}

		#region View

		/// <summary>
		/// 表示用View
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="viewModel"></param>
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
		/// <param name="viewModel"></param>
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
			return new OriginalRecipeViewModel(settings, logger, this);
		}

		/// <summary>
		/// レシピViewModel生成メソッド
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="recipe"></param>
		/// <returns>レシピViewModelインスタンス</returns>
		public IRecipeViewModel CreateRecipeViewModelInstance(IBaseSettings settings, ILogger logger, IRecipe recipe) {
			return new OriginalRecipeViewModel(settings, logger, (OriginalRecipe)recipe);
		}

		#endregion

		#region Model

		/// <summary>
		/// レシピModel生成メソッド
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="shoppingListOwner"></param>
		/// <returns>レシピModelインスタンス</returns>
		public IRecipe CreateRecipeModelInstance(IBaseSettings settings, ILogger logger, IShoppingListOwner shoppingListOwner) {
			return new OriginalRecipe(settings, logger, shoppingListOwner, this);
		}

		#endregion
	}
}
