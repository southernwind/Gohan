using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Settings;

using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SandBeige.MealRecipes.Composition.Recipe {
	/// <summary>
	/// レシピサイトインターフェイス
	/// レシピサイトを追加する際はこのインターフェイスを実装したクラスを含んだDLLを
	/// アプリケーションのPluginsフォルダに追加する
	/// </summary>
	public interface IRecipeSitePlugin : INotifyPropertyChanged {
		#region Key

		/// <summary>
		/// 対象サイトURLパターン
		/// </summary>
		Regex TargetUrlPattern {
			get;
		}

		/// <summary>
		/// ロゴURL
		/// </summary>
		string LogoUrl {
			get;
		}

		/// <summary>
		/// Model型
		/// </summary>
		Type ModelType {
			get;
		}

		/// <summary>
		/// ViewModel型
		/// </summary>
		Type ViewModelType {
			get;
		}

		#endregion

		#region View

		/// <summary>
		/// 表示用View
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="viewModel">DataContext</param>
		/// <returns>表示用View</returns>
		UserControl CreateRecipeViewerViewInstance(IBaseSettings settings, ILogger logger, IRecipeViewModel viewModel);

		/// <summary>
		/// 編集用View
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="viewModel">DataContext</param>
		/// <returns>編集用View</returns>
		UserControl CreateRecipeEditorViewInstance(IBaseSettings settings, ILogger logger, IRecipeViewModel viewModel);

		#endregion

		#region ViewModel

		/// <summary>
		/// レシピViewModel生成メソッド
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <returns>レシピViewModelインスタンス</returns>
		IRecipeViewModel CreateRecipeViewModelInstance(IBaseSettings settings, ILogger logger);

		/// <summary>
		/// レシピViewModel生成メソッド
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="recipe"></param>
		/// <returns>レシピViewModelインスタンス</returns>
		IRecipeViewModel CreateRecipeViewModelInstance(IBaseSettings settings, ILogger logger, IRecipe recipe);

		#endregion

		#region Model

		/// <summary>
		/// レシピModel生成メソッド
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="shoppingListOwner"></param>
		/// <returns>レシピModelインスタンス</returns>
		IRecipe CreateRecipeModelInstance(IBaseSettings settings, ILogger logger, IShoppingListOwner shoppingListOwner);

		#endregion
	}
}
