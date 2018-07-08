using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Models.Settings;
using SandBeige.MealRecipes.Utilities;

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace SandBeige.MealRecipes.ViewModels.Recipe {
	class RecipeViewerContainerViewModel : TabItemViewModelBase {
		private readonly ISettings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// タブアイテムの名前
		/// </summary>
		public override string Name {
			get;
		} = "レシピビュワー";

		/// <summary>
		/// レシピリスト
		/// ViewとViewModelのペアを保持する
		/// </summary>
		public ReadOnlyReactiveCollection<ViewViewModelPair<UserControl, IRecipeViewModel>> Recipes {
			get;
		}

		/// <summary>
		/// このViewModelを選択中かどうかのフラグ
		/// </summary>
		public override ReactiveProperty<bool> IsSelected {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// 選択中レシピ
		/// </summary>
		public ReactiveProperty<ViewViewModelPair<UserControl, IRecipeViewModel>> SelectedRecipe {
			get;
		} = new ReactiveProperty<ViewViewModelPair<UserControl, IRecipeViewModel>>();

		public ReactiveCommand<ViewViewModelPair<UserControl, IRecipeViewModel>> RemoveRecipeCommand {
			get;
		} = new ReactiveCommand<ViewViewModelPair<UserControl, IRecipeViewModel>>();

		public RecipeViewerContainerViewModel(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;

			this.Recipes =
				this._settings.States.RecipesInRecipeViewer
					.ToReadOnlyReactiveCollection(recipeInformation => {
						var recipe = Creator.CreateRecipeInstanceFromRecipeId(settings, logger, recipeInformation.RecipeId);
						if (recipe == null) {
							return null;
						}
						recipe.Adjustment.Value = recipeInformation.Adjustment;
						recipe.AutoReload = true;
						recipe.Load();
						var vm = Creator.CreateRecipeViewModelInstance(settings, logger, recipe);
						var v = Creator.CreateRecipeViewerViewInstance(settings, logger, vm);
						return new ViewViewModelPair<UserControl, IRecipeViewModel>(v, vm);
					}).AddTo(this.CompositeDisposable);

			// DB上に存在しないレシピが記憶されていた場合削除
			if (this.Recipes.Contains(null)) {
				foreach (var rirv in this._settings.States.RecipesInRecipeViewer.Where(x => Creator.CreateRecipeInstanceFromRecipeId(settings, logger, x.RecipeId) == null).ToArray()) {
					this._settings.States.RecipesInRecipeViewer.Remove(rirv);
				}
			}

			this.Recipes
				.CollectionChangedAsObservable()
				.Where(cc => cc.Action == NotifyCollectionChangedAction.Add)
				.Subscribe(cc => {
					this.SelectedRecipe.Value = (ViewViewModelPair<UserControl, IRecipeViewModel>)cc.NewItems[0];
					this.IsSelected.Value = true;
				}).AddTo(this.CompositeDisposable);

			this.SelectedRecipe.Value = this.Recipes.FirstOrDefault();

			this.RemoveRecipeCommand.Subscribe(vvmp => {
				this._settings
					.States
					.RecipesInRecipeViewer
					.Remove(
						this._settings
							.States
							.RecipesInRecipeViewer
							.First(x =>
								x.RecipeId == vvmp.ViewModel.Recipe.Id.Value &&
								x.Adjustment == vvmp.ViewModel.Recipe.Adjustment.Value
							)
					);
			}).AddTo(this.CompositeDisposable);
		}
	}
}
