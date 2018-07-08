using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.RecipeWebSites.Rakuten.Models;

using System;
using System.Globalization;
using System.Windows.Data;

namespace SandBeige.RecipeWebSites.Rakuten.ViewModels {
	/// <summary>
	/// クックパッドレシピViewModel
	/// 共有部分はベースクラスで定義し、独自部分のみこのクラスで定義する
	/// </summary>
	public class RakutenRecipeViewModel : RecipeViewModelBase {
		/// <summary>
		/// レシピModel
		/// </summary>
		public new RakutenRecipe Recipe {
			get {
				return base.Recipe as RakutenRecipe;
			}
		}

		/// <summary>
		/// 作者
		/// </summary>
		public ReactiveProperty<RakutenRecipeAuthor> Author {
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
		public ReactiveProperty<string> RakutenRecipeId {
			get;
		}

		/// <summary>
		/// 公開日
		/// </summary>
		public ReactiveProperty<string> PublishedDate {
			get;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="plugin">親プラグイン</param>
		internal RakutenRecipeViewModel(IBaseSettings settings, ILogger logger, IRecipeSitePlugin plugin) : this(settings, logger, new RakutenRecipe(settings, logger, null, plugin)) {
		}

		/// <summary>
		/// コンストラクタ
		/// 共有部分はベースクラスで定義する
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="RakutenRecipe">クックパッドレシピ</param>
		internal RakutenRecipeViewModel(IBaseSettings settings, ILogger logger, IRecipe RakutenRecipe) : base(settings, logger, RakutenRecipe) {
			// Properties
			this.Author = this.Recipe.Author.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.Advice = this.Recipe.Advice.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.History = this.Recipe.History.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.RakutenRecipeId = this.Recipe.RakutenRecipeId.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.PublishedDate = this.Recipe.PublishedDate.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			// Collection Views
			var view = CollectionViewSource.GetDefaultView(this.Ingredients);
			view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(RakutenRecipeIngredient.Category), new ReactivePropertyConverter()));
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
