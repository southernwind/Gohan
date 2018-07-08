using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.RecipeWebSites.Cookpad.Models;

using System;
using System.Globalization;
using System.Windows.Data;

namespace SandBeige.RecipeWebSites.Cookpad.ViewModels {
	/// <summary>
	/// クックパッドレシピViewModel
	/// 共有部分はベースクラスで定義し、独自部分のみこのクラスで定義する
	/// </summary>
	public class CookpadRecipeViewModel : RecipeViewModelBase {
		/// <summary>
		/// レシピModel
		/// </summary>
		public new CookpadRecipe Recipe {
			get {
				return base.Recipe as CookpadRecipe;
			}
		}

		/// <summary>
		/// 作者
		/// </summary>
		public ReactiveProperty<CookpadRecipeAuthor> Author {
			get;
		}

		/// <summary>
		/// コツ・ポイント
		/// </summary>
		public ReactiveProperty<string> Advice {
			get;
		}

		/// <summary>
		/// 生い立ち
		/// </summary>
		public ReactiveProperty<string> History {
			get;
		}

		/// <summary>
		/// クックパッドレシピID
		/// クックパッド内で一意の値
		/// </summary>
		public ReactiveProperty<string> CookpadRecipeId {
			get;
		}

		/// <summary>
		/// 公開日
		/// </summary>
		public ReactiveProperty<string> PublishedDate {
			get;
		}

		/// <summary>
		/// 更新日
		/// </summary>
		public ReactiveProperty<string> UpdateDate {
			get;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="plugin">親プラグイン</param>
		internal CookpadRecipeViewModel(IBaseSettings settings, ILogger logger, IRecipeSitePlugin plugin) : this(settings, logger, new CookpadRecipe(settings, logger, null, plugin)) {
		}

		/// <summary>
		/// コンストラクタ
		/// 共有部分はベースクラスで定義する
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="cookpadRecipe">クックパッドレシピ</param>
		internal CookpadRecipeViewModel(IBaseSettings settings, ILogger logger, IRecipe cookpadRecipe) : base(settings, logger, cookpadRecipe) {
			// Properties
			this.Author = this.Recipe.Author.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.Advice = this.Recipe.Advice.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.History = this.Recipe.History.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.CookpadRecipeId = this.Recipe.CookpadRecipeId.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.PublishedDate = this.Recipe.PublishedDate.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.UpdateDate = this.Recipe.UpdateDate.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			// Collection Views
			var view = CollectionViewSource.GetDefaultView(this.Ingredients);
			view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(CookpadRecipeIngredient.Category), new ReactivePropertyConverter()));
		}

		private class ReactivePropertyConverter : IValueConverter {
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
				if (value is IReactiveProperty rp) {
					return rp.Value;
				}
				return value;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
				throw new NotImplementedException();
			}
		}
	}
}
