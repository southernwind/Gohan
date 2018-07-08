using Livet;
using Livet.Messaging;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Dialog;
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Settings;

using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace SandBeige.MealRecipes.Composition.Recipe {
	/// <summary>
	/// レシピViewModel基底クラス
	/// </summary>
	public abstract class RecipeViewModelBase : ViewModel, IRecipeViewModel {
		private readonly IBaseSettings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// レシピモデル
		/// </summary>
		public virtual IRecipe Recipe {
			get;
			set;
		}

		/// <summary>
		/// 材料表示モード
		/// </summary>
		public IReadOnlyReactiveProperty<IngredientDisplayMode> IngredientDisplayMode {
			get;
		}

		/// <summary>
		/// レシピID
		/// </summary>
		public IReactiveProperty<int> Id {
			get;
		}

		/// <summary>
		/// レシピURL
		/// </summary>
		public IReactiveProperty<Uri> Url {
			get;
		}

		/// <summary>
		/// レシピ名
		/// </summary>
		public IReactiveProperty<string> Title {
			get;
		}

		/// <summary>
		/// レシピメインイメージ
		/// </summary>
		public IReactiveProperty<byte[]> Photo {
			get;
		}

		/// <summary>
		/// レシピメインイメージファイルパス
		/// </summary>
		public IReadOnlyReactiveProperty<string> PhotoFilePath {
			get;
		}

		/// <summary>
		/// サムネイル
		/// </summary>
		public IReactiveProperty<byte[]> Thumbnail {
			get;
		}

		/// <summary>
		/// サムネイルファイルパス
		/// </summary>
		public IReadOnlyReactiveProperty<string> ThumbnailFilePath {
			get;
		}

		/// <summary>
		/// レシピ説明
		/// </summary>
		public IReactiveProperty<string> Description {
			get;
		}

		/// <summary>
		/// 容量(XX人分)
		/// </summary>
		public IReactiveProperty<string> Yield {
			get;
		}

		/// <summary>
		/// 比率調整後容量(XX人分)
		/// </summary>
		public IReadOnlyReactiveProperty<string> AdjustmentedYield {
			get;
		}

		/// <summary>
		/// 材料
		/// </summary>
		public ReadOnlyReactiveCollection<IRecipeIngredient> Ingredients {
			get;
		}

		/// <summary>
		/// お買物リスト
		/// </summary>
		public IReadOnlyReactiveProperty<IRecipeIngredient[]> ShoppingList {
			get;
		}

		/// <summary>
		/// お買物リスト情報付きレシピ材料リスト
		/// </summary>
		public IReadOnlyReactiveProperty<ShoppingInformationIncludedIngredient[]> ShoppingInformationIncludedIngredients {
			get;
		}

		/// <summary>
		/// 手順
		/// </summary>
		public ReadOnlyReactiveCollection<IRecipeStep> Steps {
			get;
		}

		/// <summary>
		/// 調整比率
		/// </summary>
		public IReactiveProperty<double> Adjustment {
			get;
		}

		/// <summary>
		/// メモ
		/// </summary>
		public IReactiveProperty<string> Memo {
			get;
		}

		/// <summary>
		/// 所要時間
		/// </summary>
		public IReactiveProperty<TimeSpan> RequiredTime {
			get;
		}

		/// <summary>
		/// 登録日
		/// </summary>
		public IReactiveProperty<DateTime> RegistrationDate {
			get;
		}

		/// <summary>
		/// アーカイブ済みフラグ
		/// </summary>
		public IReactiveProperty<bool> IsArchived {
			get;
		}

		/// <summary>
		/// タグリスト
		/// </summary>
		public IReactiveProperty<string[]> Tags {
			get;
		}

		/// <summary>
		/// 評価一覧
		/// </summary>
		public ReadOnlyReactiveCollection<Rating> Ratings {
			get;
		}

		/// <summary>
		/// ダウンロードコマンド
		/// </summary>
		public AsyncReactiveCommand<InteractionMessenger> DownloadCommand {
			get;
		} = new AsyncReactiveCommand<InteractionMessenger>();

		/// <summary>
		/// レシピ登録
		/// </summary>
		public AsyncReactiveCommand<InteractionMessenger> RegisterRecipeCommand {
			get;
		} = new AsyncReactiveCommand<InteractionMessenger>();

		/// <summary>
		/// レシピ削除
		/// </summary>
		public AsyncReactiveCommand<InteractionMessenger> DeleteRecipeCommand {
			get;
		} = new AsyncReactiveCommand<InteractionMessenger>();

		/// <summary>
		/// レシピビュワー起動
		/// </summary>
		public ReactiveCommand OpenRecipeDetailCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// レシピ読み込みコマンド
		/// </summary>
		public ReactiveCommand LoadRecipeCommand {
			get;
		}

		/// <summary>
		/// 材料挿入コマンド
		/// </summary>
		public ReactiveCommand<int> InsertIngredientCommand {
			get;
		} = new ReactiveCommand<int>();

		/// <summary>
		/// 手順挿入コマンド
		/// </summary>
		public ReactiveCommand<int> InsertStepCommand {
			get;
		} = new ReactiveCommand<int>();

		/// <summary>
		/// 材料削除コマンド
		/// </summary>
		public ReactiveCommand<int> RemoveIngredientCommand {
			get;
		} = new ReactiveCommand<int>();

		/// <summary>
		/// 手順削除コマンド
		/// </summary>
		public ReactiveCommand<int> RemoveStepCommand {
			get;
		} = new ReactiveCommand<int>();

		/// <summary>
		/// アーカイブコマンド
		/// </summary>
		public ReactiveCommand ArchiveCommand {
			get;
		}

		/// <summary>
		/// アーカイブ解除コマンド
		/// </summary>
		public ReactiveCommand UnarchiveCommand {
			get;
		}

		[Obsolete]
		protected internal RecipeViewModelBase() {
		}

		///  <summary>
		///  コンストラクタ
		///  </summary>
		///  <example>
		/// 	<code>
		/// 	public ConcreteViewModel() : base(new ConcreteRecipe()) {
		/// 	}
		/// 	</code>
		///  </example>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="recipe">具象クラスで生成したレシピモデルインスタンス</param>
		protected RecipeViewModelBase(IBaseSettings settings, ILogger logger, IRecipe recipe) {
			this._settings = settings;
			this._logger = logger;
			this.Recipe = recipe.AddTo(this.CompositeDisposable);

			// Property
			this.IngredientDisplayMode = this._settings.States.IngredientDisplayMode.ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);
			this.Id = this.Recipe.Id.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.Url = this.Recipe.Url.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.Title = this.Recipe.Title.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.Photo = this.Recipe.Photo.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.PhotoFilePath =
				this.Recipe
					.PhotoFilePath
					.Where(x => x != null)
					.Select(x => Path.Combine(this._settings.GeneralSettings.ImageDirectoryPath, x))
					.ToReadOnlyReactiveProperty()
					.AddTo(this.CompositeDisposable);
			this.Thumbnail = this.Recipe.Thumbnail.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.ThumbnailFilePath =
				this.Recipe
					.ThumbnailFilePath
					.Where(x => x != null)
					.Select(x => Path.Combine(this._settings.GeneralSettings.ImageDirectoryPath, x))
					.ToReadOnlyReactiveProperty()
					.AddTo(this.CompositeDisposable);
			this.Description = this.Recipe.Description.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.Yield = this.Recipe.Yield.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.AdjustmentedYield = this.Recipe.AdjustedYeild.ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);
			this.Ingredients = this.Recipe.Ingredients.ToReadOnlyReactiveCollection().AddTo(this.CompositeDisposable);
			this.ShoppingList = this.Recipe.ShoppingList.ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);
			this.ShoppingInformationIncludedIngredients = this.Recipe.ShoppingInformationIncludedIngredients.ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);
			this.Steps = this.Recipe.Steps.ToReadOnlyReactiveCollection().AddTo(this.CompositeDisposable);
			this.Adjustment = this.Recipe.Adjustment.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.Memo = this.Recipe.Memo.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.RequiredTime = this.Recipe.RequiredTime.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.RegistrationDate = this.Recipe.RegistrationDate.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.Tags = this.Recipe.Tags.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			this.Ratings = this.Recipe.Ratings.ToReadOnlyReactiveCollection().AddTo(this.CompositeDisposable);

			this.IsArchived = this.Recipe.IsArchived.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			// Command
			// レシピダウンロード
			this.DownloadCommand.Subscribe(async messenger => {
				var disposable =
					this.Recipe.FailedNotification
						.Where(x => x.Behavior == Behavior.Download)
						.Subscribe(x => {
							this._logger.Log(LogLevel.Warning, $"レシピダウンロード失敗 レシピURL={x.Sender.Url.Value}", x.Exception);
							using (var vm = new DialogWindowViewModel("失敗通知", "レシピのダウンロードに失敗しました。", DialogWindowViewModel.DialogResult.Ok)) {
								(messenger ?? this.Messenger).Raise(new TransitionMessage(vm, "OpenDialogWindow"));
							}
						});
				using (disposable) {
					await this.Recipe.DownloadRecipeAsync();
				}
			}).AddTo(this.CompositeDisposable);

			// レシピ登録
			this.RegisterRecipeCommand.Subscribe(async messenger => {
				var disposable =
					this.Recipe.FailedNotification
						.Where(x => x.Behavior == Behavior.Register)
						.Subscribe(x => {
							this._logger.Log(LogLevel.Warning, $"レシピ登録失敗 レシピID={x.Sender.Id.Value}", x.Exception);
							using (var vm = new DialogWindowViewModel("失敗通知", "レシピの登録に失敗しました。", DialogWindowViewModel.DialogResult.Ok)) {
								(messenger ?? this.Messenger).Raise(new TransitionMessage(vm, "OpenDialogWindow"));
							}
						});
				using (disposable) {
					await this.Recipe.RegistAsync();
				}
			}).AddTo(this.CompositeDisposable);

			// レシピ削除
			this.DeleteRecipeCommand.Subscribe(async messenger => {
				var disposable =
					this.Recipe.FailedNotification
						.Where(x => x.Behavior == Behavior.Delete)
						.Subscribe(x => {
							this._logger.Log(LogLevel.Warning, $"レシピ削除失敗 レシピID={x.Sender.Id.Value}", x.Exception);
							using (var vm = new NotifyWindowViewModel("このレシピを使用している食事があるため、レシピの削除に失敗しました。", 5)) {
								(messenger ?? this.Messenger).Raise(new TransitionMessage(vm, "OpenNotifyWindow"));
							}
						});
				using (disposable) {
					await this.Recipe.DeleteAsync();
				}
			}).AddTo(this.CompositeDisposable);

			// レシピビュワーで表示
			this.OpenRecipeDetailCommand.Subscribe(() => {
				settings.States.RecipesInRecipeViewer.Add(new RecipeInformation(this.Recipe.Id.Value, this.Adjustment.Value));
			}).AddTo(this.CompositeDisposable);

			// レシピ読み込み
			this.LoadRecipeCommand = this.Id.Select(x => x != 0).ToReactiveCommand().AddTo(this.CompositeDisposable);
			this.LoadRecipeCommand.ObserveOnUIDispatcher().Subscribe(_ => this.Recipe.Load()).AddTo(this.CompositeDisposable);

			// レシピ材料追加
			this.InsertIngredientCommand.Subscribe(index => {
				this.Recipe.InsertIngredient(index);
			}).AddTo(this.CompositeDisposable);

			// レシピ手順追加
			this.InsertStepCommand.Subscribe(index => {
				this.Recipe.InsertStep(index);
			}).AddTo(this.CompositeDisposable);

			// レシピ材料削除
			this.RemoveIngredientCommand.Subscribe(index => {
				this.Recipe.RemoveIngredientAt(index);
			}).AddTo(this.CompositeDisposable);

			// レシピ手順削除
			this.RemoveStepCommand.Subscribe(index => {
				this.Recipe.RemoveStepAt(index);
			}).AddTo(this.CompositeDisposable);

			// アーカイブ
			this.ArchiveCommand = this.IsArchived.Select(x => !x).ToReactiveCommand().AddTo(this.CompositeDisposable);
			this.ArchiveCommand.Subscribe(this.Recipe.Archive).AddTo(this.CompositeDisposable);

			// アーカイブ解除
			this.UnarchiveCommand = this.IsArchived.ToReactiveCommand().AddTo(this.CompositeDisposable);
			this.UnarchiveCommand.Subscribe(this.Recipe.Unarchive).AddTo(this.CompositeDisposable);


			// エラー時対処
			this.Recipe.FailedNotification.Where(x => x.Behavior == Behavior.Load).Subscribe(x => {
				// 抽象化の諦め
				// 他クライアントからの変更通知で例外が発生した場合、メッセージを表示するための画面を特定できず、LivetのMessengerを渡すのが困難なため。
				this._logger.Log(LogLevel.Warning, $"レシピ読み込み失敗 レシピID={x.Sender.Id.Value}", x.Exception);
				using (var vm = new DialogWindowViewModel("失敗通知", "レシピの読み込みに失敗しました。", DialogWindowViewModel.DialogResult.Ok)) {
					var dialog = new DialogWindow() {
						DataContext = vm
					};
					DispatcherHelper.UIDispatcher.Invoke(() => {
						dialog.ShowDialog();
					});
				}
			}).AddTo(this.CompositeDisposable);
		}
	}
}