using Livet;
using Livet.Messaging.Windows;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Models.Settings;

using System;
using System.Reactive.Linq;
using System.Windows.Threading;

namespace SandBeige.MealRecipes.ViewModels.Recipe {
	[Flags]
	public enum Method {
		/// <summary>
		/// オリジナルレシピ作成モード
		/// </summary>
		Original = 1,

		/// <summary>
		/// レシピダウンロードモード
		/// </summary>
		Download = 1 << 1,

		/// <summary>
		/// 登録レシピ検索モード
		/// </summary>
		HistorySearch = 1 << 2
	}

	/// <summary>
	/// レシピ選択ViewModel
	/// </summary>
	class AddRecipeViewModel : ViewModel {
		/// <summary>
		/// 表示コンテンツ
		/// </summary>
		public ReactiveProperty<DecideRecipeViewModelBase> SelectedMethod {
			get;
		} = new ReactiveProperty<DecideRecipeViewModelBase>();

		/// <summary>
		/// 戻る可能フラグ
		/// </summary>
		public ReadOnlyReactiveProperty<bool> CanBack {
			get;
		}

		/// 選択方式リスト
		public ReactiveCollection<DecideRecipeViewModelBase> MethodList {
			get;
		} = new ReactiveCollection<DecideRecipeViewModelBase>();

		private ReadOnlyReactiveProperty<IRecipeViewModel> _selectionResult;

		/// <summary>
		/// 選択結果
		/// 検索し、その中から最終的に選択されたアイテム
		/// </summary>
		public ReadOnlyReactiveProperty<IRecipeViewModel> SelectionResult {
			get {
				return this._selectionResult;
			}
			private set {
				if (this._selectionResult == value) {
					return;
				}
				this._selectionResult = value;
				this.RaisePropertyChanged();
			}
		}

		public ReactivePropertySlim<bool> IsBusy {
			get;
		} = new ReactivePropertySlim<bool>();

		/// <summary>
		/// 選択済みフラグ
		/// </summary>
		public ReactiveProperty<bool> IsSelectionCompleted {
			get;
		} = new ReactiveProperty<bool>();

		private ReactiveCommand _selectCommand;

		/// <summary>
		/// 選択コマンド
		/// </summary>
		public ReactiveCommand SelectCommand {
			get {
				return this._selectCommand;
			}
			private set {
				if (this._selectCommand == value) {
					return;
				}
				this._selectCommand = value;
				this.RaisePropertyChanged();
			}
		}

		/// <summary>
		/// 閉じる
		/// </summary>
		public ReactiveCommand CloseCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// 選択方式選択に戻る
		/// </summary>
		public ReactiveCommand BackCommand {
			get;
			set;
		}

		/// <summary>
		/// 選択方式変更コマンド
		/// </summary>
		public ReactiveCommand<DecideRecipeViewModelBase> ChangeMethodCommand {
			get;
		} = new ReactiveCommand<DecideRecipeViewModelBase>();

		public AddRecipeViewModel(ISettings settings, ILogger logger, Method methods) {
			// 追加方法のリスト
			if ((methods & Method.Original) != 0) {
				this.MethodList.Add(new OriginalRecipeDetailViewModel(settings, logger).AddTo(this.CompositeDisposable));
			}
			if ((methods & Method.Download) != 0) {
				var rdvm = new RecipeDetailViewModel(settings, logger).AddTo(this.CompositeDisposable);
				rdvm.IsDownaloding.Subscribe(x => {
					this.IsBusy.Value = x;
				});
				this.MethodList.Add(rdvm);
			}
			if ((methods & Method.HistorySearch) != 0) {
				this.MethodList.Add(new SearchRecipeViewModel(settings, logger).AddTo(this.CompositeDisposable));
			}

			// 表示コンテンツ切り替わり
			this.SelectedMethod.Subscribe(vm => {
				if (vm == null) {
					this.SelectCommand = new ReactiveProperty<bool>(false).ToReactiveCommand().AddTo(this.CompositeDisposable);
					return;
				}
				this.SelectionResult = this.SelectedMethod.Value.DecidedRecipe.ToReadOnlyReactiveProperty();

				this.SelectCommand =
					new[] {
						this.SelectionResult.Select(x => x != null),
						this.IsBusy.Select(x => !x)
					}.CombineLatestValuesAreAllTrue()
					.ToReactiveCommand()
					.AddTo(this.CompositeDisposable);
				this.SelectCommand.ObserveOnDispatcher(DispatcherPriority.Background).Subscribe(_ => {
					this.IsBusy.Value = true;
					// 新規レシピ作成の場合は登録してから終了
					if (this.SelectedMethod.Value is RecipeDetailViewModel revm) {
						revm.DecidedRecipe.Value.RegisterRecipeCommand.Execute(this.Messenger);
					}
					this.IsSelectionCompleted.Value = true;
					this.Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
				}).AddTo(this.CompositeDisposable);
			}).AddTo(this.CompositeDisposable);

			// Command
			this.CloseCommand.Subscribe(() => {
				this.Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
			});

			// 選択方式変更
			this.ChangeMethodCommand.Subscribe(method => {
				this.SelectedMethod.Value = method;
			}).AddTo(this.CompositeDisposable);

			// 選択方式選択に戻る
			this.CanBack = this.SelectedMethod.Select(x => x != null).ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);
			this.BackCommand = this.CanBack.ToReactiveCommand().AddTo(this.CompositeDisposable);
			this.BackCommand.Subscribe(() => {
				this.SelectedMethod.Value = null;
			}).AddTo(this.CompositeDisposable);
		}
	}
}