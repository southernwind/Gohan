
using Reactive.Bindings;

using SandBeige.MealRecipes.Composition.Utilities;

using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace SandBeige.MealRecipes.Composition.Recipe {
	public abstract class RecipeIngredientBase : IRecipeIngredient {
		/// <summary>
		/// 材料ID
		/// </summary>
		public IReactiveProperty<int> Id {
			get;
		} = new ReactivePropertySlim<int>();

		/// <summary>
		/// カテゴリ
		/// </summary>
		public IReactiveProperty<string> Category {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 名前
		/// </summary>
		public IReactiveProperty<string> Name {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 分量テキスト
		/// </summary>
		public IReactiveProperty<string> AmountText {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 調整済み分量テキスト
		/// </summary>
		public IReadOnlyReactiveProperty<string> AdjustedAmountText {
			get;
		}

		/// <summary>
		/// 拡張
		/// </summary>
		public IEnumerable<ExtensionColumn> Extensions {
			get;
			set;
		}

		/// <summary>
		/// 親
		/// </summary>
		public IRecipe Recipe {
			get;
			private set;
		}

		protected RecipeIngredientBase(IRecipe recipe) {
			this.Recipe = recipe;
			this.AdjustedAmountText =
				this.AmountText.Where(x => x != null).CombineLatest(
					this.Recipe.Adjustment,
					(amount, adjustment) => amount.Apply(adjustment)
				).ToReactiveProperty();
		}

		/// <summary>
		/// 拡張プロパティコレクションへ登録
		/// </summary>
		public abstract void RegisterToExtensionPropertyCollection();

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		public abstract void ReadFromExtensionPropertyCollection();

	}
}
