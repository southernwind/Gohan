using System;
using System.ComponentModel;
using System.Linq;

namespace SandBeige.MealRecipes.Composition.Recipe.Search {
	/// <summary>
	/// レシピ検索条件Modelインターフェイス
	/// レシピ検索条件を追加する際はこのインターフェイスを実装したクラスを含んだDLLを
	/// アプリケーションのPluginsフォルダに追加する
	/// </summary>
	public interface IRecipeSearchConditionModel : INotifyPropertyChanged {
		/// <summary>
		/// クエリを受け取り、((そのクエリに検索条件を追加した)クエリ)を返すFuncを返す
		/// </summary>
		/// <returns>検索条件が追加されたクエリ</returns>
		Func<IQueryable<DataBase.Recipe>, IQueryable<DataBase.Recipe>> GetFilteringFunction();
	}
}
