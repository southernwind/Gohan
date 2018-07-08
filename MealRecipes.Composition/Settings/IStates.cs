using Reactive.Bindings;

using SandBeige.MealRecipes.Composition.Recipe;

using System;
using System.ComponentModel;

namespace SandBeige.MealRecipes.Composition.Settings {
	/// <summary>
	/// 状態
	/// </summary>
	public interface IStates : INotifyPropertyChanged, IDisposable {
		/// <summary>
		/// 材料の表示モード
		/// </summary>
		IReactiveProperty<IngredientDisplayMode> IngredientDisplayMode {
			get;
		}

		/// <summary>
		/// カレンダー表示モード
		/// </summary>
		IReactiveProperty<CalendarType> CalendarType {
			get;
		}

		/// <summary>
		/// レシピビュワーに表示するレシピ情報リスト
		/// </summary>
		ReactiveCollection<RecipeInformation> RecipesInRecipeViewer {
			get;
		}
	}
}
