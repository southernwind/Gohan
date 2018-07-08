using System;
using System.ComponentModel;

namespace SandBeige.MealRecipes.Composition.Settings {
	/// <summary>
	/// 検索設定
	/// </summary>
	public interface ISearchSettings : INotifyPropertyChanged, IDisposable {
		/// <summary>
		/// 1ページあたりの表示件数
		/// </summary>
		int ResultsPerPage {
			get;
			set;
		}

		/// <summary>
		/// レシピ詳細の自動表示
		/// </summary>
		bool AutomaticDisplayRecipeDetail {
			get;
			set;
		}
	}
}
