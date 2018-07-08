using Livet;

using Reactive.Bindings;

using SandBeige.MealRecipes.Composition.Recipe.Search;
using SandBeige.MealRecipes.DataBase;
using SandBeige.MealRecipes.Extensions;

using System;
using System.Linq;

namespace SandBeige.RecipeWebSites.Cookpad.Models {
	/// <summary>
	/// アドバイス検索Model
	/// </summary>
	public class AdviceSearch : NotificationObject, IRecipeSearchConditionModel {
		/// <summary>
		/// アドバイス
		/// </summary>
		public ReactivePropertySlim<string> Advice {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 検索条件
		/// </summary>
		/// <returns>受け取ったIQueryableに絞り込み条件を付加するFunc</returns>
		public Func<IQueryable<Recipe>, IQueryable<Recipe>> GetFilteringFunction() {
			return func => {
				return func
					.Where(x =>
						this.Advice.Value == null ||
						x.Extensions
							.Any(e =>
								e.Key == nameof(this.Advice) &&
								e.Data.Deserialize<string>().Contains(this.Advice.Value)));
			};
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		internal AdviceSearch() {
			this.Advice.Subscribe(_ => this.RaisePropertyChanged());
		}
	}
}
