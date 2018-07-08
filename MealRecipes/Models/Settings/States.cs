
using Livet;

using Reactive.Bindings;

using SandBeige.MealRecipes.Composition;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;

using System.Reactive.Disposables;

namespace SandBeige.MealRecipes.Models.Settings {
	public class States : NotificationObject, IStates {
		private readonly CompositeDisposable _disposable = new CompositeDisposable();
		/// <summary>
		/// 材料表示モード
		/// </summary>
		public IReactiveProperty<IngredientDisplayMode> IngredientDisplayMode {
			get;
			set;
		} = new ReactiveProperty<IngredientDisplayMode>();

		/// <summary>
		/// カレンダー表示モード
		/// </summary>
		public IReactiveProperty<CalendarType> CalendarType {
			get;
			set;
		} = new ReactiveProperty<CalendarType>(Composition.CalendarType.Details);

		/// <summary>
		/// レシピビュワー表示中レシピ
		/// </summary>
		public ReactiveCollection<RecipeInformation> RecipesInRecipeViewer {
			get;
			set;
		} = new ReactiveCollection<RecipeInformation>();
		public void Dispose() {
			this._disposable.Dispose();
		}
	}
}
