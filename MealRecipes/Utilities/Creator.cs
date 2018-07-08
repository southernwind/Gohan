
using Livet;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Recipe.Search;
using SandBeige.MealRecipes.Models.Settings;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace SandBeige.MealRecipes.Utilities {
	/// <summary>
	/// プラグインインスタンス生成ユーティリティクラス
	/// </summary>
	static class Creator {
		/// <summary>
		/// レシピプラグインリスト
		/// </summary>
		public static readonly List<IRecipeSitePlugin> RecipeSitePlugins = new List<IRecipeSitePlugin>();

		/// <summary>
		/// レシピ検索条件プラグインリスト
		/// </summary>
		public static readonly List<IRecipeSearchConditionPlugin> RecipeSearchConditionPlugins = new List<IRecipeSearchConditionPlugin>();

		/// <summary>
		/// プラグインディレクトリ
		/// </summary>
		internal static string PluginDirectory;

		/// <summary>
		/// プラグインリスト読み込み
		/// </summary>
		internal static void Load() {
			var classes =
				Directory
					.GetFiles(PluginDirectory, "*.dll")
					.SelectMany(dll =>
						Assembly.LoadFrom(dll).GetTypes())
					.ToArray();

			RecipeSitePlugins.AddRange(
				classes.Where(t =>
					t.GetInterfaces().Any(x => x == typeof(IRecipeSitePlugin))
				).Select(t =>
					(IRecipeSitePlugin)Activator.CreateInstance(t)
				)
			);

			RecipeSearchConditionPlugins.AddRange(
				classes.Where(t =>
					t.GetInterfaces().Any(x => x == typeof(IRecipeSearchConditionPlugin))
				).Select(t =>
					(IRecipeSearchConditionPlugin)Activator.CreateInstance(t)
				)
			);
		}

		#region RecipeSite

		#region View

		/// <summary>
		/// レシピViewModelをキーにして表示用レシピViewの生成
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="recipeVm">レシピViewModel</param>
		/// <returns><paramref name="settings"/>をDataContextに設定した表示用レシピView</returns>
		internal static UserControl CreateRecipeViewerViewInstance(ISettings settings, ILogger logger, IRecipeViewModel recipeVm) {
			if (recipeVm == null) {
				return null;
			}
			return DispatcherHelper.UIDispatcher.Invoke(() => {
				return RecipeSitePlugins
					.Single(p => p.ViewModelType == recipeVm.GetType())
					.CreateRecipeViewerViewInstance(settings, logger, recipeVm);
			});
		}

		/// <summary>
		/// レシピViewModelをキーにして編集用レシピViewの生成
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="recipeVm">レシピViewModel</param>
		/// <returns><paramref name="settings"/>をDataContextに設定した編集用レシピView</returns>
		internal static UserControl CreateRecipeEditorViewInstance(ISettings settings, ILogger logger, IRecipeViewModel recipeVm) {
			if (recipeVm == null) {
				return null;
			}
			return DispatcherHelper.UIDispatcher.Invoke(() => {
				return RecipeSitePlugins
					.Single(p => p.ViewModelType == recipeVm.GetType())
					.CreateRecipeEditorViewInstance(settings, logger, recipeVm);
			});
		}

		#endregion

		#region ViewModel

		/// <summary>
		/// レシピモデルをキーにしてレシピViewModelを生成
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="recipe">レシピModel</param>
		/// <returns>レシピViewModel</returns>
		internal static IRecipeViewModel CreateRecipeViewModelInstance(ISettings settings, ILogger logger, IRecipe recipe) {
			if (recipe == null) {
				return null;
			}
			return DispatcherHelper.UIDispatcher.Invoke(() => {
				return RecipeSitePlugins
					.Single(p => p.ModelType == recipe.GetType())
					.CreateRecipeViewModelInstance(settings, logger, recipe);
			});
		}

		/// <summary>
		/// URLをキーにしてレシピViewModelを生成
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="url">URL</param>
		/// <returns>レシピViewModel</returns>
		internal static IRecipeViewModel CreateRecipeViewModelInstanceFromUrl(ISettings settings, ILogger logger, string url) {
			if (url == null) {
				return null;
			}
			return DispatcherHelper.UIDispatcher.Invoke(() => {
				return RecipeSitePlugins
					.SingleOrDefault(p => p.TargetUrlPattern?.IsMatch(url) ?? false)?
					.CreateRecipeViewModelInstance(settings, logger);
			});
		}

		/// <summary>
		/// オリジナルレシピViewModelを生成
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <returns>レシピViewModel</returns>
		internal static IRecipeViewModel CreateOriginalRecipeViewModelInstance(ISettings settings, ILogger logger) {
			return DispatcherHelper.UIDispatcher.Invoke(() => {
				return RecipeSitePlugins
					.SingleOrDefault(p => p.TargetUrlPattern == null)?
					.CreateRecipeViewModelInstance(settings, logger);
			});
		}

		#endregion

		#region Model

		/// <summary>
		/// URLをキーにしてレシピModelを生成
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="pluginName">URL</param>
		/// <param name="shoppingListOwner"></param>
		/// <returns>レシピModel</returns>
		internal static IRecipe CreateRecipeInstanceFromPluginName(ISettings settings, ILogger logger, string pluginName, IShoppingListOwner shoppingListOwner = null) {
			if (pluginName == null) {
				return null;
			}
			return DispatcherHelper.UIDispatcher.Invoke(() => {
				return RecipeSitePlugins
					.SingleOrDefault(p => p.GetType().FullName == pluginName)
					?.CreateRecipeModelInstance(settings, logger, shoppingListOwner);
			});
		}

		/// <summary>
		/// レシピIDをキーにしてレシピModelを作成
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="recipeId"></param>
		/// <param name="shoppingListOwner"></param>
		/// <returns></returns>
		internal static IRecipe CreateRecipeInstanceFromRecipeId(ISettings settings, ILogger logger, int recipeId, IShoppingListOwner shoppingListOwner = null) {
			using (var db = settings.GeneralSettings.GetMealRecipeDbContext()) {
				var pluginName = db.Recipes.SingleOrDefault(x => x.RecipeId == recipeId)?.PluginName;
				return DispatcherHelper.UIDispatcher.Invoke(() => {
					var recipe = CreateRecipeInstanceFromPluginName(settings, logger, pluginName, shoppingListOwner);
					recipe.Id.Value = recipeId;
					return recipe;
				});
			}
		}

		#endregion

		#region CanCreate

		internal static bool CanCreateFromUrl(string url) {
			if (url == null) {
				return false;
			}
			return RecipeSitePlugins.Any(x => x.TargetUrlPattern?.IsMatch(url) ?? false);
		}

		#endregion

		#endregion

		#region SearchCondition

		#region View

		internal static (UserControl View, IRecipeSearchConditionViewModel ViewModel) CreateRecipeSearchConditionViewAndViewModel(IRecipeSearchConditionModel model) {
			var plugin = RecipeSearchConditionPlugins.Single(p => p.ModelType == model.GetType());
			var vm = plugin.CreateRecipeSearchConditionViewModel(model);
			var v = plugin.CreateRecipeSearchConditionView(vm);
			return (v, vm);
		}

		#endregion

		#endregion
	}
}