using Livet;

using SandBeige.MealRecipes.Composition.Settings;

using System.Reactive.Disposables;

namespace SandBeige.MealRecipes.Models.Settings {
	/// <summary>
	/// 検索設定
	/// </summary>
	public class SearchSettings : NotificationObject, ISearchSettings {
		private readonly CompositeDisposable _disposable = new CompositeDisposable();
		/// <summary>
		/// 1ページあたりの表示件数
		/// </summary>
		public int ResultsPerPage {
			get;
			set;
		} = 10;

		/// <summary>
		/// レシピ詳細の自動表示
		/// </summary>
		public bool AutomaticDisplayRecipeDetail {
			get;
			set;
		} = true;

		public void Dispose() {
			this._disposable.Dispose();
		}
	}
}
