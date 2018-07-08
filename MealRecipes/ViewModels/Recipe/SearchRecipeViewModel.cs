using Livet.Messaging;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Dialog;
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Recipe.Search;
using SandBeige.MealRecipes.Models.Recipe;
using SandBeige.MealRecipes.Models.Settings;
using SandBeige.MealRecipes.Utilities;

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace SandBeige.MealRecipes.ViewModels.Recipe {
	/// <summary>
	/// レシピ選択ViewModel
	/// </summary>
	class SearchRecipeViewModel : DecideRecipeViewModelBase {
		/// <summary>
		/// Model
		/// </summary>
		private SearchRecipe _searchRecipe;

		#region Search

		/// <summary>
		/// 検索ワード
		/// </summary>
		public ReactiveProperty<string> SearchWord {
			get;
		}

		/// <summary>
		/// 検索対象フラグ:タイトル
		/// </summary>
		public ReactiveProperty<bool> IsTitleSearchTarget {
			get;
		}

		/// <summary>
		/// 検索対象フラグ:材料
		/// </summary>
		public ReactiveProperty<bool> IsIngredientSearchTarget {
			get;
		}

		/// <summary>
		/// 検索対象フラグ:手順
		/// </summary>
		public ReactiveProperty<bool> IsStepSearchTarget {
			get;
		}

		/// <summary>
		/// 最終利用日
		/// </summary>
		public ReactiveProperty<DateTime?> LastUsedDate {
			get;
		}

		/// <summary>
		/// 最終利用日以前フラグ
		/// Falseなら以後
		/// </summary>
		public ReactiveProperty<bool> IsBeforeLastUsedDate {
			get;
		}

		/// <summary>
		/// 利用回数
		/// </summary>
		public ReactiveProperty<int?> UsageCount {
			get;
		}

		/// <summary>
		/// 利用回数以上フラグ
		/// Falseなら以下
		/// </summary>
		public ReactiveProperty<bool> IsUsageCountMoreThan {
			get;
		}

		/// <summary>
		/// アーカイブを含むかどうかのフラグ
		/// </summary>
		public ReactiveProperty<bool> IncludingArchive {
			get;
		}

		/// <summary>
		/// 検索結果
		/// 検索ワードなどで検索した結果
		/// </summary>
		public ReadOnlyReactiveProperty<IRecipeViewModel[]> SearchResult {
			get;
		}

		/// <summary>
		/// 評価
		/// </summary>
		public ReadOnlyReactiveCollection<Rating> Ratings {
			get;
		}

		/// <summary>
		/// タグサジェスト候補
		/// </summary>
		public ReadOnlyReactiveCollection<SelectableValue<string>> TagList {
			get;
		}

		/// <summary>
		/// 検索条件プラグインリスト
		/// </summary>
		public ReadOnlyReactiveCollection<IRecipeSearchConditionPlugin> SearchConditionPlugins {
			get;
		}

		/// <summary>
		/// 追加されたプラグイン検索条件
		/// </summary>
		public ReadOnlyReactiveCollection<ViewViewModelPair<UserControl, IRecipeSearchConditionViewModel>> PluginSearchConditions {
			get;
		}

		/// <summary>
		/// ロード中フラグ
		/// </summary>
		public ReadOnlyReactiveProperty<bool> IsBusy {
			get;
		}

		#endregion

		#region Sort

		/// <summary>
		/// 追加した検索条件
		/// </summary>
		public ReadOnlyReactiveCollection<SortCondition> SortConditions {
			get;
		}

		/// <summary>
		/// 検索条件リスト
		/// </summary>
		public ReadOnlyReactiveCollection<ISortItem> SortItems {
			get;
		}

		#endregion

		#region Page

		/// <summary>
		/// 現在ページ
		/// </summary>
		public ReadOnlyReactiveProperty<int> CurrentPage {
			get;
		}

		/// <summary>
		/// 最大ページ数
		/// </summary>
		public ReadOnlyReactiveProperty<int> MaxPage {
			get;
		}

		public ReadOnlyReactiveProperty<bool> IsMultiPage {
			get;
		}

		/// <summary>
		/// ページリスト
		/// </summary>
		public ReadOnlyReactivePropertySlim<int[]> PageList {
			get;
		}

		#endregion

		/// <summary>
		/// 選択結果
		/// 表示された検索結果から選択したレシピ
		/// </summary>
		public override ReactiveProperty<IRecipeViewModel> DecidedRecipe {
			get;
		} = new ReactiveProperty<IRecipeViewModel>();

		#region Command

		/// <summary>
		/// 検索コマンド
		/// </summary>
		public ReactiveCommand SearchCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// ソート条件追加
		/// </summary>
		public ReactiveCommand AddSortConditionCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// ソート条件削除
		/// </summary>
		public ReactiveCommand<SortCondition> RemoveSortConditionCommand {
			get;
		} = new ReactiveCommand<SortCondition>();

		/// <summary>
		/// プラグイン検索条件追加
		/// </summary>
		public ReactiveCommand<IRecipeSearchConditionPlugin> AddPluginSearchConditionCommand {
			get;
		} = new ReactiveCommand<IRecipeSearchConditionPlugin>();

		/// <summary>
		/// プラグイン検索条件削除
		/// </summary>
		public ReactiveCommand<IRecipeSearchConditionViewModel> RemovePluginSearchConditionCommand {
			get;
		} = new ReactiveCommand<IRecipeSearchConditionViewModel>();

		/// <summary>
		/// 前ページへ戻るコマンド
		/// </summary>
		public ReactiveCommand PreviousCommand {
			get;
		}

		/// <summary>
		/// 次ページへ進むコマンド
		/// </summary>
		public ReactiveCommand NextCommand {
			get;
		}

		/// <summary>
		/// ページ遷移コマンド
		/// </summary>
		public ReactiveCommand<int> TransitionCommand {
			get;
		} = new ReactiveCommand<int>();

		/// <summary>
		/// レシピ編集コマンド
		/// </summary>
		public ReactiveCommand<IRecipeViewModel> EditRecipeCommand {
			get;
		} = new ReactiveCommand<IRecipeViewModel>();

		/// <summary>
		/// レシピ削除コマンド
		/// </summary>
		public ReactiveCommand<IRecipeViewModel> DeleteRecipeCommand {
			get;
		} = new ReactiveCommand<IRecipeViewModel>();

		#endregion

		public SearchRecipeViewModel(ISettings settings, ILogger logger) {
			this._searchRecipe = new SearchRecipe(settings, logger).AddTo(this.CompositeDisposable);

			// Property

			// 現在ページ
			this.CurrentPage = this._searchRecipe.CurrentPage.ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);

			// 最大ページ数
			this.MaxPage = this._searchRecipe.MaxPage.ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);

			// 複数ページフラグ
			this.IsMultiPage = this.MaxPage.Select(x => x >= 2).ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);

			// ページリスト
			this.PageList =
				this.MaxPage
					.CombineLatest(this.CurrentPage, (max, current) => (max, current))
					.Select(x =>
					Enumerable
						.Range(x.current - 4, 10)
						.Where(i => i >= 1 && i <= x.max)
						.ToArray()
				)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(this.CompositeDisposable);

			// 検索ワード
			this.SearchWord = this._searchRecipe.SearchWord.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			// 検索対象フラグ:タイトル
			this.IsTitleSearchTarget = this._searchRecipe.IsTitleSearchTarget.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			// 検索対象フラグ:材料
			this.IsIngredientSearchTarget = this._searchRecipe.IsIngredientSearchTarget.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			// 検索対象フラグ:手順
			this.IsStepSearchTarget = this._searchRecipe.IsStepSearchTarget.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			// 最終利用日
			this.LastUsedDate = this._searchRecipe.LastUsedDate.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			// 最終利用日以前フラグ
			this.IsBeforeLastUsedDate = this._searchRecipe.IsBeforeLastUsedDate.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			// 利用回数
			this.UsageCount = this._searchRecipe.UsageCount.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			// 利用回数以上フラグ
			this.IsUsageCountMoreThan = this._searchRecipe.IsUsageCountMoreThan.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			// アーカイブを含むかどうかのフラグ
			this.IncludingArchive = this._searchRecipe.IncludingArchive.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			// 検索結果
			this.SearchResult =
				this._searchRecipe
					.Result
					.Select(x =>
						x.Select(r =>
							Creator.CreateRecipeViewModelInstance(settings, logger, r)
						).ToArray()
					).ToReadOnlyReactiveProperty()
					.AddTo(this.CompositeDisposable);

			// タグ候補
			this.TagList = this._searchRecipe.TagList.ToReadOnlyReactiveCollection().AddTo(this.CompositeDisposable);

			// 評価
			this.Ratings = this._searchRecipe.Ratings.ToReadOnlyReactiveCollection().AddTo(this.CompositeDisposable);

			// 追加したソート条件
			this.SortConditions = this._searchRecipe.SortConditions.ToReadOnlyReactiveCollection().AddTo(this.CompositeDisposable);

			// 検索条件リスト
			this.SortItems = this._searchRecipe.SortItems.ToReadOnlyReactiveCollection().AddTo(this.CompositeDisposable);

			// 検索条件プラグインリスト
			this.SearchConditionPlugins = this._searchRecipe.SearchConditionPlugins.ToReadOnlyReactiveCollection().AddTo(this.CompositeDisposable);

			// 追加されたプラグイン検索条件
			this.PluginSearchConditions = this._searchRecipe.PluginSearchConditions.ToReadOnlyReactiveCollection(x => {
				var (view, viewModel) = Creator.CreateRecipeSearchConditionViewAndViewModel(x);
				return new ViewViewModelPair<UserControl, IRecipeSearchConditionViewModel>(view, viewModel);
			}).AddTo(this.CompositeDisposable);

			// ロード中フラグ
			this.IsBusy = this._searchRecipe.IsBusy.ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);

			// Command
			// 検索
			this.SearchCommand.Subscribe(_ => this._searchRecipe.Search()).AddTo(this.CompositeDisposable);

			// ソート条件追加
			this.AddSortConditionCommand.Subscribe(() => this._searchRecipe.AddSortCondition()).AddTo(this.CompositeDisposable);

			// ソート条件削除
			this.RemoveSortConditionCommand.Subscribe(s => this._searchRecipe.RemoveSortCondition(s)).AddTo(this.CompositeDisposable);

			// プラグイン検索条件追加
			this.AddPluginSearchConditionCommand.Where(p => p != null).Subscribe(p =>
				this._searchRecipe
					.AddPluginSearchCondition(p)
			).AddTo(this.CompositeDisposable);

			// プラグイン検索条件削除
			this.RemovePluginSearchConditionCommand.Subscribe(c => this._searchRecipe.RemovePluginSearchCondition(c.RecipeSearchCondition)).AddTo(this.CompositeDisposable);

			// 前ページへ戻る
			this.PreviousCommand = this.CurrentPage.Select(c => c > 1).ToReactiveCommand().AddTo(this.CompositeDisposable);
			this.PreviousCommand.Subscribe(() => {
				this._searchRecipe.GoToPreviousPage();
			}).AddTo(this.CompositeDisposable);

			// 次ページへ進む
			this.NextCommand = this.CurrentPage.CombineLatest(this.MaxPage, (c, m) => c < m).ToReactiveCommand().AddTo(this.CompositeDisposable);
			this.NextCommand.Subscribe(() => {
				this._searchRecipe.GoToNextPage();
			}).AddTo(this.CompositeDisposable);

			// ページ遷移
			this.TransitionCommand.Subscribe(page => {
				this._searchRecipe.PageTransition(page);
			}).AddTo(this.CompositeDisposable);

			// レシピエディタ起動
			this.EditRecipeCommand.Subscribe(rvm => {
				var vm = new RecipeDetailViewModel(settings, logger);
				vm.RecipeViewModel.Value = rvm;
				vm.RecipeView.Value = Creator.CreateRecipeEditorViewInstance(settings, logger, rvm);
				this.Messenger.Raise(new TransitionMessage(vm, "OpenEditRecipeWindow"));
			}).AddTo(this.CompositeDisposable);

			// レシピ削除
			this.DeleteRecipeCommand.Subscribe(rvm => {
				var vm = new DialogWindowViewModel("削除確認",
					"レシピを削除します。よろしいですか。",
					DialogWindowViewModel.DialogResult.Yes,
					DialogWindowViewModel.DialogResult.No
				);
				using (vm) {
					this.Messenger.Raise(new TransitionMessage(vm, TransitionMode.Modal, "OpenDialogWindow"));
					if (vm.Result != DialogWindowViewModel.DialogResult.Yes) {
						return;
					}

					rvm.DeleteRecipeCommand.Execute(this.Messenger);
					// 結果を再読込
					this.SearchCommand.Execute();
				}
			}).AddTo(this.CompositeDisposable);
		}
	}
}