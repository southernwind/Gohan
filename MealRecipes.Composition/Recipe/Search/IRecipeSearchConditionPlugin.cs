using System;
using System.Windows.Controls;

namespace SandBeige.MealRecipes.Composition.Recipe.Search {
	/// <summary>
	/// 検索条件プラグイン
	/// </summary>
	public interface IRecipeSearchConditionPlugin {
		/// <summary>
		/// プラグイン名
		/// </summary>
		string Name {
			get;
		}

		/// <summary>
		/// Model型
		/// </summary>
		Type ModelType {
			get;
		}

		/// <summary>
		/// 検索条件を選択するViewを作成する
		/// </summary>
		/// <param name="viewModel">DataContext</param>
		/// <returns>VIew</returns>
		UserControl CreateRecipeSearchConditionView(IRecipeSearchConditionViewModel viewModel);

		/// <summary>
		/// ViewModelのを作成する
		/// </summary>
		/// <param name="model">Model</param>
		/// <returns>ViewModel</returns>
		IRecipeSearchConditionViewModel CreateRecipeSearchConditionViewModel(IRecipeSearchConditionModel model);

		/// <summary>
		/// Modelを作成する
		/// </summary>
		/// <returns>Model</returns>
		IRecipeSearchConditionModel CreateRecipeSearchConditionModel();
	}
}
