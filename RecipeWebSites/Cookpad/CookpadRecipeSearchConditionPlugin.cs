using SandBeige.MealRecipes.Composition.Recipe.Search;
using SandBeige.RecipeWebSites.Cookpad.Models;
using SandBeige.RecipeWebSites.Cookpad.ViewModels;

using System;
using System.Windows.Controls;

namespace SandBeige.RecipeWebSites.Cookpad {
	/// <summary>
	/// アドバイス検索プラグイン
	/// </summary>
	public class CookpadRecipeSearchConditionPlugin : IRecipeSearchConditionPlugin {
		/// <summary>
		/// プラグイン名
		/// </summary>
		public string Name {
			get;
		} = "アドバイス検索";

		/// <summary>
		/// Model型
		/// </summary>
		public Type ModelType {
			get;
		} = typeof(AdviceSearch);

		/// <summary>
		/// Modelの取得
		/// </summary>
		/// <returns>Model</returns>
		public IRecipeSearchConditionModel CreateRecipeSearchConditionModel() {
			return new AdviceSearch();
		}

		/// <summary>
		/// Viewの取得
		/// </summary>
		/// <param name="viewModel">DataContext</param>
		/// <returns>View</returns>
		public UserControl CreateRecipeSearchConditionView(IRecipeSearchConditionViewModel viewModel) {
			return new Views.SearchConditions.AdviceSearch {
				DataContext = viewModel
			};
		}

		/// <summary>
		/// ViewModelの取得
		/// </summary>
		/// <param name="model">Model</param>
		/// <returns>ViewModel</returns>
		public IRecipeSearchConditionViewModel CreateRecipeSearchConditionViewModel(IRecipeSearchConditionModel model) {
			return new AdviceSearchViewModel((AdviceSearch)model);
		}
	}
}
