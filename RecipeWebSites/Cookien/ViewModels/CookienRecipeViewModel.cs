using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.RecipeWebSites.Cookien.Models;

using System;

namespace SandBeige.RecipeWebSites.Cookien.ViewModels {
	/// <summary>
	/// クックパッドレシピViewModel
	/// 共有部分はベースクラスで定義し、独自部分のみこのクラスで定義する
	/// </summary>
	public class CookienRecipeViewModel : RecipeViewModelBase {
		/// <summary>
		/// レシピModel
		/// </summary>
		public new CookienRecipe Recipe {
			get {
				return base.Recipe as CookienRecipe;
			}
		}

		/// <summary>
		/// 保存期間
		/// </summary>
		public ReactiveProperty<TimeSpan> ShelfLife {
			get;
		}

		/// <summary>
		/// メモ
		/// </summary>
		public ReadOnlyReactiveCollection<CookienMemo> CookienMemos {
			get;
		}

		/// <summary>
		/// つくおきメモ挿入コマンド
		/// </summary>
		public ReactiveCommand<int> InsertCookienMemoCommand {
			get;
		} = new ReactiveCommand<int>();

		/// <summary>
		/// つくおきメモ削除コマンド
		/// </summary>
		public ReactiveCommand<int> RemoveCookienMemoCommand {
			get;
		} = new ReactiveCommand<int>();


		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="plugin">親プラグイン</param>
		internal CookienRecipeViewModel(IBaseSettings settings, ILogger logger, IRecipeSitePlugin plugin) : this(settings, logger, new CookienRecipe(settings, logger, null, plugin)) {
		}

		/// <summary>
		/// コンストラクタ
		/// 共有部分はベースクラスで定義する
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="cookpadRecipe">クックパッドレシピ</param>
		internal CookienRecipeViewModel(IBaseSettings settings, ILogger logger, IRecipe cookpadRecipe) : base(settings, logger, cookpadRecipe) {
			// Properties
			this.ShelfLife = this.Recipe.ShelfLife.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.CookienMemos = this.Recipe.CookienMemos.ToReadOnlyReactiveCollection();

			// レシピ材料追加
			this.InsertCookienMemoCommand.Subscribe(index => {
				this.Recipe.InsertCookienMemo(index);
			});

			// レシピ材料削除
			this.RemoveCookienMemoCommand.Subscribe(index => {
				this.Recipe.RemoveCookienMemoAt(index);
			});
		}
	}
}
