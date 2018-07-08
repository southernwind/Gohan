using Livet.Messaging;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Models.Settings;
using SandBeige.MealRecipes.Utilities;

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace SandBeige.MealRecipes.ViewModels.Recipe {
	/// <summary>
	/// レシピブックViewModel
	/// </summary>
	class RecipeBookViewModel : TabItemViewModelBase {
		private readonly ISettings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// メニュー表示用
		/// </summary>
		public override string Name {
			get;
		} = "レシピブック";

		/// <summary>
		/// タブアイテムの選択中フラグ
		/// </summary>
		public override ReactiveProperty<bool> IsSelected {
			get;
		} = new ReactiveProperty<bool>();

		public SearchRecipeViewModel SearchRecipeViewModel {
			get;
		}

		public ReadOnlyReactiveProperty<UserControl> RecipeDetailView {
			get;
		}

		public ReadOnlyReactiveProperty<IRecipeViewModel> RecipeDetailViewModel {
			get;
		}

		/// <summary>
		/// 選択中レシピ表示フラグ
		/// </summary>
		public ReactiveProperty<bool> IsRecipeDetailViewExpanded {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// レシピ追加
		/// </summary>
		public ReactiveCommand AddRecipeCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public RecipeBookViewModel(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;

			this.SearchRecipeViewModel = new SearchRecipeViewModel(settings, logger).AddTo(this.CompositeDisposable);

			// Properties
			// レシピ詳細表示
			this.RecipeDetailViewModel =
				this.SearchRecipeViewModel
					.DecidedRecipe
					.Where(x => x != null)
					.Select(x => {
						var recipe = Creator.CreateRecipeInstanceFromRecipeId(settings, logger, x.Id.Value);
						recipe.AutoReload = true;
						return Creator.CreateRecipeViewModelInstance(settings, logger, recipe);
					})
					.ToReadOnlyReactiveProperty();
			this.RecipeDetailView =
				this.RecipeDetailViewModel
					.Where(x => x != null)
					.Select(vm =>
						Creator.CreateRecipeViewerViewInstance(settings, logger, vm)
					).ToReadOnlyReactiveProperty()
					.AddTo(this.CompositeDisposable);

			this.SearchRecipeViewModel.DecidedRecipe.Where(x => x != null).Subscribe(_ => {
				if (settings.SearchSettings.AutomaticDisplayRecipeDetail) {
					this.IsRecipeDetailViewExpanded.Value = true;
				}
			}).AddTo(this.CompositeDisposable);

			// Commands
			// レシピダウンロード画面起動
			this.AddRecipeCommand.Subscribe(() => {
				using (var vm = new AddRecipeViewModel(settings, logger, Method.Original | Method.Download)) {
					this.Messenger.Raise(new TransitionMessage(vm, "OpenAddRecipeWindow"));
					if (vm.IsSelectionCompleted.Value) {
						// 結果を再読込
						this.SearchRecipeViewModel.SearchCommand.Execute();
					}
				}
			}).AddTo(this.CompositeDisposable);
		}
	}
}
