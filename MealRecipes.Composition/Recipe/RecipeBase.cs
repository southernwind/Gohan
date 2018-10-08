using Livet;

using Microsoft.EntityFrameworkCore;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.MealRecipes.Composition.Utilities;
using SandBeige.MealRecipes.DataBase;
using SandBeige.MealRecipes.Extensions;

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SandBeige.MealRecipes.Composition.Recipe {
	/// <summary>
	/// レシピModel基底クラス
	/// </summary>
	public abstract class RecipeBase : NotificationObject, IRecipe {
		/// <summary>
		/// プラグイン名
		/// </summary>
		private readonly string _pluginName;

		protected readonly ILogger Logger;
		protected readonly IBaseSettings Settings;

		protected readonly CompositeDisposable Disposable = new CompositeDisposable();

		/// <summary>
		/// 完了通知
		/// </summary>
		protected readonly Subject<(IRecipe Sender, Behavior Behavior)> _completedNotification =
			new Subject<(IRecipe, Behavior)>();

		/// <summary>
		/// 完了通知
		/// </summary>
		public IObservable<(IRecipe Sender, Behavior Behavior)> CompletedNotification {
			get {
				return this._completedNotification.AsObservable();
			}
		}

		/// <summary>
		/// 失敗通知
		/// </summary>
		protected readonly Subject<(IRecipe Sender, Behavior Behavior, Exception Exception)> _failedNotification =
			new Subject<(IRecipe, Behavior, Exception)>();

		/// <summary>
		/// 失敗通知
		/// </summary>
		public IObservable<(IRecipe Sender, Behavior Behavior, Exception Exception)> FailedNotification {
			get {
				return this._failedNotification.AsObservable();
			}
		}

		/// <summary>
		///  自動更新
		/// </summary>
		public bool AutoReload {
			get;
			set;
		}

		/// <summary>
		///	お買物リスト保有インスタンス
		///	食事(Meal)など、レシピを保有するクラスのインスタンスがこれに当たる
		///	お買物リスト保有インスタンスは、そのインスタンス単位でのお買物リストを保持していて、
		///	その中からこのレシピの対応するものだけをこのクラスで使用する
		/// </summary>
		public IReactiveProperty<IShoppingListOwner> ShoppingListOwner {
			get;
		} = new ReactivePropertySlim<IShoppingListOwner>();

		/// <summary>
		/// レシピID
		/// </summary>
		public IReactiveProperty<int> Id {
			get;
		} = new ReactivePropertySlim<int>();

		/// <summary>
		/// レシピURL
		/// </summary>
		public IReactiveProperty<Uri> Url {
			get;
		} = new ReactivePropertySlim<Uri>();

		/// <summary>
		/// レシピ名
		/// </summary>
		public IReactiveProperty<string> Title {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// レシピメインイメージ
		/// </summary>
		public IReactiveProperty<byte[]> Photo {
			get;
		} = new ReactivePropertySlim<byte[]>();

		/// <summary>
		/// レシピメインイメージファイルパス
		/// </summary>
		public IReactiveProperty<string> PhotoFilePath {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// サムネイル
		/// </summary>
		public IReactiveProperty<byte[]> Thumbnail {
			get;
		} = new ReactivePropertySlim<byte[]>();

		/// <summary>
		/// レシピサムネイルファイルパス
		/// </summary>
		public IReactiveProperty<string> ThumbnailFilePath {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// レシピ説明
		/// </summary>
		public IReactiveProperty<string> Description {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 容量
		/// </summary>
		public IReactiveProperty<string> Yield {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 調整後容量
		/// </summary>
		public IReadOnlyReactiveProperty<string> AdjustedYeild {
			get;
		}

		/// <summary>
		/// 材料
		/// </summary>
		public ObservableCollection<IRecipeIngredient> Ingredients {
			get;
		} = new ObservableCollection<IRecipeIngredient>();

		private ReadOnlyReactivePropertySlim<IRecipeIngredient[]> _shoppingList;

		/// <summary>
		/// お買物リスト
		/// </summary>
		public ReadOnlyReactivePropertySlim<IRecipeIngredient[]> ShoppingList {
			get {
				return this._shoppingList;
			}
			private set {
				if (this._shoppingList == value) {
					return;
				}
				this._shoppingList = value;
				this.RaisePropertyChanged();
			}
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
		public ObservableCollection<IRecipeStep> Steps {
			get;
		} = new ObservableCollection<IRecipeStep>();

		/// <summary>
		/// 調整比率
		/// </summary>
		public IReactiveProperty<double> Adjustment {
			get;
		} = new ReactivePropertySlim<double>(1);

		/// <summary>
		/// メモ
		/// </summary>
		public IReactiveProperty<string> Memo {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 所要時間
		/// </summary>
		public IReactiveProperty<TimeSpan> RequiredTime {
			get;
		} = new ReactivePropertySlim<TimeSpan>();

		/// <summary>
		/// 登録日
		/// </summary>
		public IReactiveProperty<DateTime> RegistrationDate {
			get;
		} = new ReactivePropertySlim<DateTime>();

		/// <summary>
		/// アーカイブ済みフラグ
		/// </summary>
		public IReactiveProperty<bool> IsArchived {
			get;
		} = new ReactivePropertySlim<bool>();

		/// <summary>
		/// タグリスト
		/// </summary>
		public IReactiveProperty<string[]> Tags {
			get;
		} = new ReactivePropertySlim<string[]>(new string[] { });

		/// <summary>
		/// 評価一覧
		/// </summary>
		public ReadOnlyReactiveCollection<Rating> Ratings {
			get;
			private set;
		}

		/// <summary>
		/// 拡張プロパティコレクション
		/// </summary>
		protected ObservableCollection<ExtensionColumn> Extensions {
			get;
		} = new ObservableCollection<ExtensionColumn>();

		protected RecipeBase(IBaseSettings settings, ILogger logger, IShoppingListOwner shoppingListOwner, IRecipeSitePlugin plugin) {
			this.Settings = settings;
			this.Logger = logger;

			this._pluginName = plugin.GetType().FullName;

			// 比率調整
			this.AdjustedYeild =
				this.Yield
					.CombineLatest(this.Adjustment, (y, a) => y.Apply(a))
					.ToReadOnlyReactivePropertySlim()
					.AddTo(this.Disposable);

			// お買物リスト保有インスタンス変更時
			this.ShoppingListOwner.Subscribe(owner => {
				if (owner == null) {
					this.ShoppingList?.Dispose();
					this.ShoppingList = Observable.Empty<IRecipeIngredient[]>().Publish().ToReadOnlyReactivePropertySlim(new IRecipeIngredient[] { }).AddTo(this.Disposable);
					return;
				}

				this.ShoppingList =
					Observable.Merge(
						this.Ingredients.CollectionChangedAsObservable(),
						owner.ShoppingList.CollectionChangedAsObservable()
					).Select(_ =>
						this.Ingredients.Where(i =>
							owner.ShoppingList.Any(sl =>
								sl.Recipe.Id.Value == i.Recipe.Id.Value &&
								sl.Id.Value == i.Id.Value
							)
						).ToArray()
					).ToReadOnlyReactivePropertySlim(new IRecipeIngredient[] { })
					.AddTo(this.Disposable);
			});
			this.ShoppingListOwner.Value = shoppingListOwner;

			// お買物情報付き材料リスト
			this.ShoppingInformationIncludedIngredients =
				this.ShoppingList.CombineLatest(
					this.ShoppingListOwner,
						this.Ingredients.CollectionChangedAsObservable(),
						(_, __, ___) => Unit.Default)
					.Merge(Observable.Return(Unit.Default))
					.Select(
						_ =>
							this.Ingredients.Select(x =>
								new ShoppingInformationIncludedIngredient(x, this.ShoppingList.Value.Contains(x), this.ShoppingListOwner.Value)
							).ToArray())
					.ToReadOnlyReactiveProperty()
					.AddTo(this.Disposable);

			// アーカイブ状況の変化は即時DB反映
			this.IsArchived.Subscribe(val => {
				using (var db = this.Settings.GeneralSettings.GetMealRecipeDbContext()) {
					var recipe = db.Recipes.SingleOrDefault(x => x.RecipeId == this.Id.Value);
					if (recipe == null) {
						return;
					}
					recipe.IsArchived = val;
					db.Update(recipe);
					db.SaveChanges();
					this.Settings.DbChangeNotifier.Notify(nameof(db.Recipes));
				}
			}).AddTo(this.Disposable);

			// 評価
			this.Ratings =
				this.Settings
					.Caches
					.Users
					.ToReadOnlyReactiveCollection(user => {
						var rating = new Rating(user, 0);
						var loaded = false;
						// ID確定時点で初期値取得
						this.Id.Where(x => x != 0).Select(x => Unit.Default)
							.Merge(
								this.Settings
								.DbChangeNotifier
								.Received
								.Where(x => x.TableNames.Any(t =>
									new[] {
										nameof(MealRecipeDbContext.Ratings),
										nameof(MealRecipeDbContext.Users)
									}.Contains(t))
								).Select(x => Unit.Default)
								.Where(x => this.Id.Value != 0)
							).Throttle(TimeSpan.FromMilliseconds(50))
							.Subscribe(_ => {
								loaded = false;
								// 初期値のDB取得
								using (var db = this.Settings.GeneralSettings.GetMealRecipeDbContext()) {
									rating.Value.Value = db
										.Recipes
										.Where(x => x.RecipeId == this.Id.Value)
										.SelectMany(x => x.Ratings)
										.SingleOrDefault(x => x.UserId == user.Id.Value)
										?.Value
										?? 0;
								}
								loaded = true;
							}).AddToRatingCompositeDisposable(rating);

						// ID確定時点以降で変更があれば即時DB反映
						rating.Value.Subscribe(e => {
							if (!loaded) {
								return;
							}
							using (var db = this.Settings.GeneralSettings.GetMealRecipeDbContext())
							using (var transaction = db.Database.BeginTransaction()) {
								db.Database.ExecuteSqlCommandAsync(
									$"DELETE FROM {nameof(db.Ratings)} WHERE {nameof(DataBase.Rating.RecipeId)}={{0}} AND {nameof(DataBase.Rating.UserId)}={{1}}",
									this.Id.Value,
									user.Id.Value
								);
								db.Ratings.Add(new DataBase.Rating {
									RecipeId = this.Id.Value,
									UserId = user.Id.Value,
									Value = rating.Value.Value
								});
								db.SaveChanges();
								transaction.Commit();
								this.Settings.DbChangeNotifier.Notify(nameof(db.Ratings));
							}
						}).AddToRatingCompositeDisposable(rating);
						return rating;
					}).AddTo(this.Disposable);

			// 破棄
			this.Ratings
				.CollectionChangedAsObservable()
				.Where(x =>
					new[] {
						NotifyCollectionChangedAction.Replace,
						NotifyCollectionChangedAction.Remove,
						NotifyCollectionChangedAction.Reset
					}.Contains(x.Action)
					&& (x.OldItems?.Count ?? 0) != 0
				).Subscribe(x => {
					foreach (var item in x.OldItems.Cast<Rating>()) {
						item.Value.Dispose();
					}
				}).AddTo(this.Disposable);

			var tables = new[] {
				nameof(MealRecipeDbContext.Recipes),
				nameof(MealRecipeDbContext.Ingredients),
				nameof(MealRecipeDbContext.IngredientExtensions),
				nameof(MealRecipeDbContext.RecipeExtensions),
				nameof(MealRecipeDbContext.Recipes),
				nameof(MealRecipeDbContext.RecipeTags),
				nameof(MealRecipeDbContext.StepExtensions),
				nameof(MealRecipeDbContext.Steps),
				nameof(MealRecipeDbContext.Tags)
			};

			this.Settings
				.DbChangeNotifier
				.Received
				.Where(x => x.TableNames.Any(t => tables.Contains(t)))
				.Throttle(TimeSpan.FromMilliseconds(50))
				.Select(x => Unit.Default)
				.ObserveOnUIDispatcher()
				.Subscribe(_ => {
					if (this.AutoReload) {
						this.Load();
					}
				}).AddTo(this.Disposable);

			// 画像同期
			// 画像の状態に関して、以下のパターンが存在する
			//	・レシピダウンロード直後の場合
			//		プロパティにのみ存在する
			//		この場合、次回起動時に表示する必要があるため、プロパティ→画像フォルダにコピー
			//	・他のクライアントからレシピダウンロード後、一度アプリケーションを終了し、再度起動してレシピを表示した場合
			//		画像フォルダにのみ存在する
			//		表示のために画像をファイルからプロパティにコピーする
			//		更に、次回のために画像キャッシュフォルダに画像をキャッシュしておく(ネットワークドライブが画像フォルダであった場合に画像の読み込みに時間がかかるため。)
			//	・前回の操作で画像がキャッシュフォルダに存在する場合
			//		キャッシュフォルダからプロパティにコピーする
			// 処理としては、以下のようになる
			// プロパティにあって画像フォルダになければ、プロパティ→画像フォルダへコピー
			// 画像フォルダにあって、プロパティになければ、画像フォルダ→プロパティ,キャッシュフォルダにコピー
			// キャッシュフォルダにあって、プロパティになければ、キャッシュフォルダ→プロパティにコピー
			// プロパティ→コピーはしないことにする(プロパティ→画像フォルダ→キャッシュフォルダとコピーされていくため、2回目表示時にキャッシュされるので、大きな問題にはならない)
			// DBに保存されるのはファイル名のみで、ファイル名は(画像のハッシュ).pngとなる
			// DBから読み込み時にPhotoFilePathプロパティに代入されるので、その変更通知を受けてPhotoプロパティにコピーする
			// Photoプロパティで
			this.Photo.Where(x => x != null).Subscribe(x => {
				using (var crypto = new SHA256CryptoServiceProvider()) {
					var filePath = string.Join("", crypto.ComputeHash(x).Select(b => $"{b:X2}")) + ".png";
					this.PhotoFilePath.Value = filePath;

					this.Thumbnail.Value = ThumbnailCreator.Create(x, 100, 100);
					var thumbFilePath = string.Join("", crypto.ComputeHash(this.Thumbnail.Value).Select(b => $"{b:X2}")) + ".png";
					this.ThumbnailFilePath.Value = thumbFilePath;
				}
			});

			// 共通化用ローカル関数
			void imageFunc(IReactiveProperty<byte[]> photo,string path){
				var fullpath = Path.Combine(this.Settings.GeneralSettings.ImageDirectoryPath, path);
				var cachePath = Path.Combine(this.Settings.GeneralSettings.CachesDirectoryPath, path);

				// キャッシュにあればキャッシュ利用
				if (File.Exists(cachePath)) {
					photo.Value = File.ReadAllBytes(cachePath);
					return;
				}

				if (!File.Exists(fullpath)) {
					// キャッシュになく、画像ディレクトリにもない場合、プロパティ→画像ディレクトリ
					File.WriteAllBytes(fullpath, photo.Value);
				} else {
					// キャッシュになく、画像ディレクトリにある場合、画像ディレクトリ→プロパティ
					photo.Value = File.ReadAllBytes(fullpath);
					// 次回のためにキャッシュしておく
					File.Copy(fullpath, cachePath);
				}
			};

			this.PhotoFilePath.Where(x => x != null).Subscribe(x => {
				imageFunc(this.Photo, x);
			});

			this.ThumbnailFilePath.Where(x => x != null).Subscribe(x => {
				imageFunc(this.Thumbnail, x);
			});
		}

		/// <summary>
		/// レシピデータダウンロード
		/// </summary>
		/// <returns><see cref="Task"/></returns>
		public abstract Task DownloadRecipeAsync();



		/// <summary>
		/// レシピDB登録
		/// </summary>
		/// <returns><see cref="Task"/></returns>
		public virtual async Task RegistAsync() {
			this.Logger.Log(LogLevel.Notice, $"レシピ登録開始 ID={this.Id.Value}, TITLE={this.Title.Value}");
			// 拡張プロパティコレクションへ登録
			this.RegisterToExtensionPropertyCollection();
			foreach (var ingredient in this.Ingredients) {
				ingredient.RegisterToExtensionPropertyCollection();
			}
			foreach (var step in this.Steps) {
				step.RegisterToExtensionPropertyCollection();
			}

			using (var db = this.Settings.GeneralSettings.GetMealRecipeDbContext())
			using (var transaction = db.Database.BeginTransaction()) {
				try {
					DataBase.Recipe recipe;
					if (this.Id.Value != 0) {
						// 子テーブルの削除
						var targetTables = new[] {
						nameof(db.Ingredients),
						nameof(db.IngredientExtensions),
						nameof(db.Steps),
						nameof(db.StepExtensions),
						nameof(db.RecipeExtensions),
						nameof(db.RecipeTags),
						nameof(db.Ratings)
					};
						foreach (var table in targetTables) {
							await db.Database.ExecuteSqlCommandAsync($"DELETE FROM {table} WHERE RecipeId={{0}}", this.Id.Value);
						}
						recipe = db.Recipes.Single(r => r.RecipeId == this.Id.Value);
					} else {
						recipe = new DataBase.Recipe();
						db.Recipes.Add(recipe);
					}

					recipe.Url = this.Url.ToString();
					recipe.Title = this.Title.Value;
					recipe.PhotoFilePath = this.PhotoFilePath.Value;
					recipe.ThumbnailFilePath = this.ThumbnailFilePath.Value;
					recipe.Description = this.Description.Value;
					recipe.Yield = this.Yield.Value;
					recipe.IsArchived = this.IsArchived.Value;
					recipe.RecipeTags = this.Tags.Value?.Select(x => {
						// タグが元々存在した場合は存在するタグを使用する
						// なければ作成する
						var tag = db.Tags?.FirstOrDefault(t => t.TagName == x) ?? new Tag {
							TagName = x
						};
						var rt = new RecipeTag {
							Tag = tag
						};
						return rt;
					}).ToList();
					recipe.Ingretients = this.Ingredients.Select(x => {
						return new Ingredient {
							IngredientId = x.Id.Value,
							Name = x.Name.Value,
							Amount = x.AmountText.Value,
							Extensions = x.Extensions?.Select(ex => new IngredientExtension {
								Key = ex.Key,
								Index = ex.Index,
								Data = ex.Data
							}).ToList()
						};
					}).ToList();
					recipe.Steps = this.Steps.Select(x => {
						return new Step {
							StepId = x.Number.Value,
							PhotoFilePath = x.PhotoFilePath.Value,
							ThumbnailFilePath = x.ThumbnailFilePath.Value,
							StepText = x.StepText.Value,
							Extensions = x.Extensions?.Select(ex => new StepExtension {
								Key = ex.Key,
								Index = ex.Index,
								Data = ex.Data
							}).ToList()
						};
					}).ToList();
					recipe.Ratings = this.Ratings.Select(x => {
						return new DataBase.Rating {
							UserId = x.User.Value.Id.Value,
							Value = x.Value.Value
						};
					}).ToList();
					recipe.Extensions = this.Extensions.Select(x => new RecipeExtension {
						Key = x.Key,
						Index = x.Index,
						Data = x.Data
					}).ToList();
					recipe.PluginName = this._pluginName;

					await db.SaveChangesAsync();

					transaction.Commit();

					this.Id.Value = recipe.RecipeId;

					this.Settings.DbChangeNotifier.Notify(
						nameof(db.Recipes),
						nameof(db.Ingredients),
						nameof(db.IngredientExtensions),
						nameof(db.Ratings),
						nameof(db.RecipeExtensions),
						nameof(db.Recipes),
						nameof(db.RecipeTags),
						nameof(db.StepExtensions),
						nameof(db.Steps),
						nameof(db.Tags),
						nameof(db.Users)
					);
				} catch (DbException ex) {
					this._failedNotification.OnNext((this, Behavior.Register, ex));
					return;
				}
				this._completedNotification.OnNext((this, Behavior.Register));
			}
			this.Logger.Log(LogLevel.Notice, $"レシピ登録完了 ID={this.Id.Value}, TITLE={this.Title.Value}");
		}

		/// <summary>
		/// レシピ読み込み
		/// </summary>
		public virtual void Load() {
			this.Logger.Log(LogLevel.Notice, $"レシピ読み込み ID={this.Id.Value}");
			using (var dataContext = this.Settings.GeneralSettings.GetMealRecipeDbContext()) {
				try {
					var recipe = dataContext
						.Recipes
						.Include(x => x.Ingretients).ThenInclude(ingredient => ingredient.Extensions)
						.Include(x => x.Steps).ThenInclude(step => step.Extensions)
						.Include(x => x.Extensions)
						.Include(x => x.RecipeTags).ThenInclude(rt => rt.Tag)
						.Single(x => x.RecipeId == this.Id.Value);
					if (Uri.TryCreate(recipe.Url, UriKind.Absolute, out var uri)) {
						this.Url.Value = uri;
					}
					this.Title.Value = recipe.Title;
					this.PhotoFilePath.Value = recipe.PhotoFilePath;
					this.Description.Value = recipe.Description;
					this.Yield.Value = recipe.Yield;
					this.Tags.Value = recipe.RecipeTags.Select(x => x.Tag.TagName).ToArray();
					this.IsArchived.Value = recipe.IsArchived;
					this.Ingredients.Clear();
					this.Ingredients.AddRange(recipe.Ingretients
						.OrderBy(x => x.IngredientId)
						.Select(x => {
							var ingredient = this.CreateIngredientInstance(x.IngredientId, x.Name, x.Amount);
							ingredient.Extensions = x.Extensions?.Select(ex => new ExtensionColumn {
								Key = ex.Key,
								Index = ex.Index,
								Data = ex.Data
							});
							return ingredient;
						}));
					this.Steps.Clear();
					this.Steps.AddRange(recipe.Steps
						.OrderBy(x => x.StepId)
						.Select(x => {
							var step = this.CreateStepInstance(x.StepId, x.PhotoFilePath, x.StepText);
							step.Extensions = x.Extensions?.Select(ex => new ExtensionColumn {
								Key = ex.Key,
								Index = ex.Index,
								Data = ex.Data
							});
							return step;
						}));
					this.Extensions.Clear();
					this.Extensions.AddRange(recipe.Extensions?.Select(x => new ExtensionColumn {
						Key = x.Key,
						Index = x.Index,
						Data = x.Data
					}));

					// 拡張プロパティコレクションから読み込み
					this.ReadFromExtensionPropertyCollection();
					foreach (var ingredient in this.Ingredients) {
						ingredient.ReadFromExtensionPropertyCollection();
					}
					foreach (var step in this.Steps) {
						step.ReadFromExtensionPropertyCollection();
					}
				} catch (DbException ex) {
					this._failedNotification.OnNext((this, Behavior.Load, ex));
					return;
				}
			}
			this._completedNotification.OnNext((this, Behavior.Load));
			this.Logger.Log(LogLevel.Notice, $"レシピ読み込み完了 ID={this.Id.Value}");
		}

		public virtual async Task DeleteAsync() {
			this.Logger.Log(LogLevel.Notice, $"レシピ削除 ID={this.Id.Value} TITLE={this.Title.Value}");
			using (var db = this.Settings.GeneralSettings.GetMealRecipeDbContext())
			using (var transaction = db.Database.BeginTransaction()) {
				try {
					await db.Database.ExecuteSqlCommandAsync(new RawSqlString($"DELETE FROM {nameof(db.Recipes)} WHERE RecipeId={{0}}"), this.Id.Value);
					transaction.Commit();

					this.Settings.DbChangeNotifier.Notify(
						nameof(db.Recipes),
						nameof(db.Ingredients),
						nameof(db.IngredientExtensions),
						nameof(db.Ratings),
						nameof(db.RecipeExtensions),
						nameof(db.Recipes),
						nameof(db.RecipeTags),
						nameof(db.StepExtensions),
						nameof(db.Steps),
						nameof(db.Tags),
						nameof(db.Users)
					);
				} catch (DbException ex) {
					this._failedNotification.OnNext((this, Behavior.Delete, ex));
					return;
				}
			}
			this._completedNotification.OnNext((this, Behavior.Delete));
			this.Logger.Log(LogLevel.Notice, $"レシピ削除完了 ID={this.Id.Value} TITLE={this.Title.Value}");
		}

		/// <summary>
		/// 材料挿入
		/// </summary>
		/// <param name="index">挿入する位置</param>
		public void InsertIngredient(int index) {
			foreach (var ingredient in this.Ingredients.Skip(index)) {
				ingredient.Id.Value++;
			}
			this.Ingredients.Insert(index, this.CreateIngredientInstance(index + 1, "", ""));
		}

		/// <summary>
		/// 手順挿入
		/// </summary>
		/// <param name="index">挿入する位置</param>
		public void InsertStep(int index) {
			foreach (var step in this.Steps.Skip(index)) {
				step.Number.Value++;
			}
			this.Steps.Insert(index, this.CreateStepInstance(index + 1, null, ""));
		}

		/// <summary>
		/// 材料削除
		/// </summary>
		/// <param name="index">削除する要素のインデックス</param>
		public void RemoveIngredientAt(int index) {
			foreach (var ingredient in this.Ingredients.Skip(index)) {
				ingredient.Id.Value--;
			}
			this.Ingredients.RemoveAt(index);
		}

		/// <summary>
		/// 手順削除
		/// </summary>
		/// <param name="index">削除する要素のインデックス</param>
		public void RemoveStepAt(int index) {
			foreach (var step in this.Steps.Skip(index)) {
				step.Number.Value--;
			}
			this.Steps.RemoveAt(index);
		}

		/// <summary>
		/// アーカイブ
		/// </summary>
		public void Archive() {
			this.IsArchived.Value = true;
		}

		/// <summary>
		/// アーカイブ解除
		/// </summary>
		public void Unarchive() {
			this.IsArchived.Value = false;
		}

		/// <summary>
		/// 拡張プロパティコレクションへ登録
		/// </summary>
		protected abstract void RegisterToExtensionPropertyCollection();

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		protected abstract void ReadFromExtensionPropertyCollection();

		/// <summary>
		/// レシピ材料インスタンス作成
		/// </summary>
		/// <param name="id">材料Id</param>
		/// <param name="name">材料名</param>
		/// <param name="amount">分量</param>
		/// <returns>レシピ材料インスタンス</returns>
		public abstract IRecipeIngredient CreateIngredientInstance(int id, string name, string amount);

		/// <summary>
		/// レシピ手順インスタンス作成
		/// </summary>
		/// <param name="number">番号</param>
		/// <param name="photoFilePath">写真ファイルパス</param>
		/// <param name="text">手順テキスト</param>
		/// <returns>レシピ手順インスタンス</returns>
		public abstract IRecipeStep CreateStepInstance(int number, string photoFilePath, string text);

		public void Dispose() {
			this.Disposable.Dispose();
		}
	}

	/// <summary>
	/// お買物情報付きレシピ材料
	/// </summary>
	public class ShoppingInformationIncludedIngredient {
		/// <summary>
		/// レシピ材料
		/// </summary>
		public ReactiveProperty<IRecipeIngredient> Ingredient {
			get;
		} = new ReactiveProperty<IRecipeIngredient>();

		/// <summary>
		/// お買い物リストに存在する/しない
		/// </summary>
		public ReactiveProperty<bool> ExistsInShoppingList {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="ingredient">材料インスタンス</param>
		/// <param name="existsInShoppingList">材料がお買物リストに含まれるかどうか</param>
		/// <param name="owner">お買物リスト保有インスタンス</param>
		public ShoppingInformationIncludedIngredient(IRecipeIngredient ingredient, bool existsInShoppingList, IShoppingListOwner owner) {
			this.Ingredient.Value = ingredient;
			this.ExistsInShoppingList.Value = existsInShoppingList;

			if (owner == null) {
				return;
			}

			// このインスタンスの「お買物リストに存在する/しない」が切り替えられた時にお買物リストを保有しているインスタンスのお買物リストを同期させる
			// 既にできている場合は何もせず、
			this.ExistsInShoppingList.Subscribe(x => {
				// 親のお買い物リストに材料が存在するかどうか
				var exists = owner.ShoppingList.Contains(this.Ingredient.Value);

				// 自 : 存在する
				// 親 : 存在しない
				if (x && !exists) {
					// 親に材料を追加する
					owner.ShoppingList.Add(this.Ingredient.Value);
				}

				// 自 : 存在しない
				// 親 : 存在する
				if (!x && exists) {
					// 親から材料を削除する
					owner.ShoppingList.Remove(this.Ingredient.Value);
				}
			});
		}
	}
}
