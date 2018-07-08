using Livet;
using Livet.Messaging;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Dialog;
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.DataBase;
using SandBeige.MealRecipes.Models.Meal;
using SandBeige.MealRecipes.Models.Settings;
using SandBeige.MealRecipes.Utilities;
using SandBeige.MealRecipes.ViewModels.Recipe;

using System;
using System.Reactive.Linq;

namespace SandBeige.MealRecipes.ViewModels.Meal {
	/// <summary>
	/// 食事ViewModel
	/// </summary>
	class MealViewModel : ViewModel {
		private ISettings _settings;
		private ILogger _logger;

		/// <summary>
		/// 食事モデルインスタンス
		/// </summary>
		public MealModel Meal {
			get;
		}

		/// <summary>
		/// 食事ID
		/// </summary>
		public ReactiveProperty<int> MealId {
			get;
			set;
		}

		/// <summary>
		/// 食事タイプ
		/// </summary>
		public ReactiveProperty<MealType> MealType {
			get;
			set;
		}

		/// <summary>
		/// 食事タイプリスト
		/// </summary>
		public ReadOnlyReactiveCollection<MealType> MealTypes {
			get;
		}

		/// <summary>
		/// レシピリスト
		/// </summary>
		public ReadOnlyReactiveCollection<IRecipeViewModel> RecipeList {
			get;
		}

		/// <summary>
		/// 食事削除コマンド
		/// </summary>
		public ReactiveCommand RemoveMealCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// レシピ追加コマンド
		/// </summary>
		public ReactiveCommand AddRecipeCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// レシピ削除コマンド
		/// </summary>
		public ReactiveCommand<IRecipeViewModel> RemoveRecipeCommand {
			get;
		} = new ReactiveCommand<IRecipeViewModel>();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="meal">食事モデル</param>
		public MealViewModel(ISettings settings, ILogger logger, MealModel meal) {
			this._settings = settings;
			this._logger = logger;
			this.Meal = meal.AddTo(this.CompositeDisposable);

			// Property
			// 食事ID
			this.MealId = this.Meal.MealId.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			// レシピリスト
			this.RecipeList = this.Meal.Recipes.ToReadOnlyReactiveCollection(recipe => Creator.CreateRecipeViewModelInstance(settings, logger, recipe)).AddTo(this.CompositeDisposable);
			// 食事タイプ
			this.MealType = this.Meal.MealType.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			// 食事タイプリスト
			this.MealTypes = this.Meal.MealTypes.ToReadOnlyReactiveCollection().AddTo(this.CompositeDisposable);

			// Command
			// レシピ追加コマンド
			this.AddRecipeCommand.Subscribe(() => {
				using (var vm = new AddRecipeViewModel(this._settings, this._logger, Method.HistorySearch | Method.Download | Method.Original)) {
					this.Messenger.Raise(new TransitionMessage(vm, "OpenSearchRecipeWindow"));
					if (vm.IsSelectionCompleted.Value) {
						this.Meal.AddRecipe(vm.SelectionResult.Value.Recipe);
					}
				}
			}).AddTo(this.CompositeDisposable);
			// レシピ削除コマンド
			this.RemoveRecipeCommand.Subscribe(rvm => {
				using (var vm = new DialogWindowViewModel(
					"削除確認",
					"食事からレシピを削除します。よろしいですか。",
					DialogWindowViewModel.DialogResult.Yes,
					DialogWindowViewModel.DialogResult.No
					)) {
					this.Messenger.Raise(new TransitionMessage(vm, TransitionMode.Modal, "OpenDialogWindow"));
					if (vm.Result == DialogWindowViewModel.DialogResult.Yes) {
						this.Meal.RemoveRecipe(rvm.Recipe);
					}
				}
			}).AddTo(this.CompositeDisposable);

			// 食事削除コマンド
			this.RemoveMealCommand.Subscribe(() => {
				using (var vm = new DialogWindowViewModel(
					"削除確認",
					"食事を削除します。よろしいですか。",
					DialogWindowViewModel.DialogResult.Yes,
					DialogWindowViewModel.DialogResult.No
					)) {
					this.Messenger.Raise(new TransitionMessage(vm, TransitionMode.Modal, "OpenDialogWindow"));
					if (vm.Result == DialogWindowViewModel.DialogResult.Yes) {
						this.Meal.RemoveMeal();
					}
				}
			}).AddTo(this.CompositeDisposable);
		}
	}
}
