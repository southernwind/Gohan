using Livet.Messaging.Windows;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Models.Settings;
using SandBeige.MealRecipes.Utilities;

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SandBeige.MealRecipes.ViewModels.Recipe {
	/// <summary>
	/// レシピ表示・編集ViewModel
	/// 枠組みだけ用意して、表示する内容に関しては各レシピに対応するアドオンに任せる。
	/// Viewが<see cref="RecipeView"/>で、ViewModelが<see cref="RecipeViewModel"/>
	/// </summary>
	class RecipeDetailViewModel : DecideRecipeViewModelBase {
		private Subject<Unit> _registerCompleted = new Subject<Unit>();
		/// <summary>
		/// 登録完了通知
		/// </summary>
		public IObservable<Unit> RegisterCompleted {
			get {
				return this._registerCompleted.AsObservable();
			}
		}

		/// <summary>
		/// レシピダウンロード元URL
		/// </summary>
		public ReactiveProperty<string> TargetUrl {
			get;
		}

		/// <summary>
		/// サイトプラグインリスト
		/// </summary>
		public ReactiveProperty<Site[]> Sites {
			get;
		} = new ReactiveProperty<Site[]>();

		/// <summary>
		/// 作成したレシピView
		/// </summary>
		public ReactiveProperty<UserControl> RecipeView {
			get;
		}

		/// <inheritdoc />
		/// <summary>
		/// 作成したレシピ
		/// </summary>
		public override ReactiveProperty<IRecipeViewModel> DecidedRecipe {
			get;
		}

		/// <summary>
		/// 作成したレシピVM
		/// </summary>
		public ReactiveProperty<IRecipeViewModel> RecipeViewModel {
			get;
		} = new ReactiveProperty<IRecipeViewModel>();

		/// <summary>
		/// ダウンロード中フラグ
		/// </summary>
		public ReactiveProperty<bool> IsDownaloding {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// 編集中フラグ
		/// </summary>
		public ReactiveProperty<bool> IsEditing {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// ビジー中フラグ
		/// </summary>
		public ReactiveProperty<bool> IsBusy {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// ダウンロードコマンド
		/// </summary>
		public ReactiveCommand DownloadCommand {
			get;
		}

		/// <summary>
		/// 登録コマンド
		/// </summary>
		public ReactiveCommand RegisterRecipeCommand {
			get;
		}

		/// <summary>
		/// 元に戻すコマンド
		/// </summary>
		public ReactiveCommand RevertRecipeCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// 編集モード切り替えコマンド
		/// </summary>
		public ReactiveCommand ChangeModeToEditorCommand {
			get;
		}

		/// <summary>
		/// 編集完了コマンド
		/// </summary>
		public ReactiveCommand CompleteEditCommand {
			get;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public RecipeDetailViewModel(ISettings settings, ILogger logger) {
			// Property

			// 作成したレシピ
			this.DecidedRecipe =
				this.RecipeViewModel
					.CombineLatest(
						// ダウンロード完了後に値をセットする
						this.IsDownaloding,
						(recipe, isDownloading) => isDownloading ? null : recipe)
					.ToReactiveProperty()
					.AddTo(this.CompositeDisposable);

			this.TargetUrl =
				new ReactiveProperty<string>()
					.SetValidateNotifyError(x => x != null ? null : "必須")
					.SetValidateNotifyError(x => Creator.CanCreateFromUrl(x) ? null : "対応サイトのURLではありません。")
					.AddTo(this.CompositeDisposable);

			this.Sites.Value = Creator.RecipeSitePlugins.Select(x => new Site {
				Logo = x.LogoUrl,
				TargetUrlPattern = x.TargetUrlPattern,
				IsValid = this.TargetUrl.Select(url => url != null && (x.TargetUrlPattern?.IsMatch(url) ?? false)).ToReactiveProperty()
			}).ToArray();


			this.RecipeView =
				this.RecipeViewModel
					.CombineLatest(this.IsEditing, (vm, isEditing) => {
						if (isEditing) {
							return Creator.CreateRecipeEditorViewInstance(settings, logger, vm);
						} else {
							return Creator.CreateRecipeViewerViewInstance(settings, logger, vm);
						}
					})
					.ToReactiveProperty()
					.AddTo(this.CompositeDisposable);
			// Command

			// レシピダウンロードコマンド
			this.DownloadCommand =
				new[] {
					this.TargetUrl.ObserveHasErrors.Select(x => !x),
					this.IsDownaloding.Select(x => !x),
					this.RecipeViewModel.Select(x => x == null)
				}.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.AddTo(this.CompositeDisposable);
			this.DownloadCommand.Subscribe(url => {
				this.IsDownaloding.Value = true;
				var vm = Creator.CreateRecipeViewModelInstanceFromUrl(settings, logger, this.TargetUrl.Value);
				this.RecipeViewModel.Value = vm;
				vm.Url.Value = new Uri(this.TargetUrl.Value);

				vm.Recipe.FailedNotification.Where(x => x.Behavior == Composition.Behavior.Download).Subscribe(x => {
					this.RecipeViewModel.Value = null;
					this.IsDownaloding.Value = false;
				});
				vm.Recipe.CompletedNotification.Where(x => x.Behavior == Composition.Behavior.Download).Subscribe(x => {
					this.IsDownaloding.Value = false;
				});
				vm.DownloadCommand.Execute(this.Messenger);

			}).AddTo(this.CompositeDisposable);

			// レシピ登録コマンド
			this.RegisterRecipeCommand =
				new[] {
					this.IsDownaloding.Select(x => !x),
					this.RecipeViewModel.Select(x=>x != null),
					this.IsEditing.Select(x => !x)
				}.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.AddTo(this.CompositeDisposable);
			this.RegisterRecipeCommand.ObserveOnDispatcher(DispatcherPriority.Background).Subscribe(_ => {
				this.IsBusy.Value = true;
				this.RecipeViewModel.Value.RegisterRecipeCommand.Execute(this.Messenger);
				this._registerCompleted.Next();
				this.Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
			}).AddTo(this.CompositeDisposable);

			// レシピ破棄コマンド
			this.RevertRecipeCommand.ObserveOnDispatcher(DispatcherPriority.Background).Subscribe(_ => {
				this.IsBusy.Value = true;
				// 変更前の状態を再読込
				this.RecipeViewModel.Value.LoadRecipeCommand.Execute(this.Messenger);
				this.Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
			}).AddTo(this.CompositeDisposable);

			// レシピ編集モード切り替えコマンド
			this.ChangeModeToEditorCommand =
				new[] {
					this.IsDownaloding.Select(x => !x),
					this.RecipeViewModel.Select(x=>x != null),
					this.IsEditing.Select(x => !x)
				}.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.AddTo(this.CompositeDisposable);
			this.ChangeModeToEditorCommand.Subscribe(() => {
				this.IsEditing.Value = true;
			}).AddTo(this.CompositeDisposable);

			// 編集終了コマンド
			this.CompleteEditCommand =
				new[] {
					this.IsDownaloding.Select(x => !x),
					this.RecipeViewModel.Select(x=>x != null),
					this.IsEditing
				}.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.AddTo(this.CompositeDisposable);
			this.CompleteEditCommand.Subscribe(() => {
				this.IsEditing.Value = false;
			}).AddTo(this.CompositeDisposable);
		}

		public class Site {
			/// <summary>
			/// ロゴ画像URL
			/// </summary>
			public string Logo {
				get;
				set;
			}

			/// <summary>
			/// URLパターン正規表現
			/// </summary>
			public Regex TargetUrlPattern {
				get;
				set;
			}

			public ReactiveProperty<bool> IsValid {
				get;
				set;
			}
		}
	}
}
