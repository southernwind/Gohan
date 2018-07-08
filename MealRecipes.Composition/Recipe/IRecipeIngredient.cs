using Reactive.Bindings;

using System.Collections.Generic;

namespace SandBeige.MealRecipes.Composition.Recipe {
	/// <summary>
	/// レシピ材料
	/// </summary>
	public interface IRecipeIngredient {
		IReactiveProperty<int> Id {
			get;
		}

		/// <summary>
		/// 材料名
		/// </summary>
		IReactiveProperty<string> Name {
			get;
		}

		/// <summary>
		/// 分量テキスト
		/// </summary>
		IReactiveProperty<string> AmountText {
			get;
		}

		/// <summary>
		/// 拡張
		/// </summary>
		IEnumerable<ExtensionColumn> Extensions {
			get;
			set;
		}

		/// <summary>
		/// 調整済み分量テキスト
		/// </summary>
		IReadOnlyReactiveProperty<string> AdjustedAmountText {
			get;
		}

		/// <summary>
		/// 親
		/// </summary>
		IRecipe Recipe {
			get;
		}

		/// <summary>
		/// 拡張プロパティコレクションへ登録
		/// </summary>
		void RegisterToExtensionPropertyCollection();

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		void ReadFromExtensionPropertyCollection();
	}
}
